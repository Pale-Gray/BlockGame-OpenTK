using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Game.Audio;
using Game.Core.Chrono;
using Game.Core.Chunks;
using Game.Core.Generation;
using Game.Core.GuiRendering;
using Game.Core.Language;
using Game.Core.Modding;
using Game.Core.PlayerUtil;
using Game.Core.TexturePack;
using Game.Core.Worlds;
using Game.FramebufferUtil;
using Game.GuiRendering;
using Game.Util;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Game.Core.Networking;

public class Client
{

    private NetManager _manager;
    private EventBasedNetListener _listener;
    private NetDataWriter _writer = new NetDataWriter();
    public bool IsNetworked { get; private set; } = false;
    public Player Player { get; private set; }
    private Framebuffer _terrainBuffer;
    private FramebufferQuad _terrainBufferQuad;
    private int _id;
    public void Load()
    {

        TextRenderer.Initialize();
        Translator.LoadKeymap();

        GlobalValues.GuiBlockShader = new Shader("guiblock.vert", "guiblock.frag");
        GlobalValues.ChunkShader = new Shader("chunk.vert", "chunk.frag");
        GlobalValues.DefaultShader = new Shader("default.vert", "default.frag");
        GlobalValues.GuiShader = new Shader("gui.vert", "gui.frag");
        GlobalValues.CachedFontShader = new Shader("cachedFont.vert", "cachedFont.frag");
        GlobalValues.PackedChunkShader = new Shader("chunkTerrain.vert", "chunkTerrain.frag");
        GlobalValues.SkyboxShader = new Shader("skybox.vert", "skybox.frag");

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.Enable(EnableCap.PolygonOffsetFill);

        Gui.Initialize();
        FontRenderer.Initialize();

        TexturePackManager.IterateAvailableTexturePacks();
        TexturePackManager.LoadTexturePack(TexturePackManager.AvailableTexturePacks["Default"]);

        ModLoader.Load();

        _terrainBuffer = new Framebuffer();
        _terrainBufferQuad = new FramebufferQuad();

    }

    private static GLDebugProc _delegate = OnDebugMessage;

    private static void OnDebugMessage(
    DebugSource source,     // Source of the debugging message.
    DebugType type,         // Type of the debugging message.
    uint id,                 // ID associated with the message.
    DebugSeverity severity, // Severity of the message.
    int length,             // Length of the string in pMessage.
    nint pMessage,        // Pointer to message string.
    nint pUserParam)      // The pointer you gave to OpenGL, explained later.
    {
        // In order to access the string pointed to by pMessage, you can use Marshal
        // class to copy its contents to a C# string without unsafe code. You can
        // also use the new function Marshal.PtrToStringUTF8 since .NET Core 1.1.
        string message = Marshal.PtrToStringAnsi(pMessage, length);

        // The rest of the function is up to you to implement, however a debug output
        // is always useful.
        Console.WriteLine("[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);

        // Potentially, you may want to throw from the function for certain severity
        // messages.
        if (type == DebugType.DebugTypeError)
        {
            // throw new Exception(message);
        }
    }
    public void StartSingleplayer(string worldSave, Player player)
    {

        IsNetworked = false;

        NetworkingValues.Server = new Server();
        NetworkingValues.Server.StartSingleplayer(worldSave, player);

        // hook
        Player = NetworkingValues.Server.ConnectedPlayers[0];
        NetworkingValues.Client.Player = player;

    }

    public void StartMultiplayer(string addressOrHost, int port, Player player)
    {

        IsNetworked = true;

        Player = player;

        _listener = new EventBasedNetListener();
        _manager = new NetManager(_listener);

        GameState.World = new World("");
        WorldGenerator.Initialize();

        _manager.Start();
        _manager.Connect(addressOrHost, port, "BlockGame");

        _listener.PeerConnectedEvent += (peer) =>
        {

            // GameLogger.Log("Connected to server.");

        };

        _listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {

            PacketType packetType = default;//(PacketType)disconnectInfo.AdditionalData.GetUShort();

            switch (packetType)
            {
                case PacketType.ConnectRejectPacket:
                    // ConnectRejectPacket packet = new ConnectRejectPacket();
                    // packet.Deserialize(disconnectInfo.AdditionalData);
                    // GameLogger.Log($"{packet.Type}: {packet.Reason}");
                    break;
            }

            disconnectInfo.AdditionalData.Recycle();

        };

        _listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
        {

            PacketType packetType = (PacketType)reader.GetUShort();

            switch (packetType)
            {
            
                case PacketType.PlayerDataRequestPacket:
                    PlayerDataSendPacket packet = new PlayerDataSendPacket();
                    packet.UserId = Player.UserId;
                    packet.DisplayName = Player.DisplayName;
                    packet.Serialize(_writer);
                    peer.Send(_writer, DeliveryMethod.ReliableOrdered);
                    break;
                case PacketType.ConnectSuccessPacket:
                    GameLogger.Log("Connected to server.");
                    break;
                case PacketType.ConnectRejectPacket:
                    ConnectRejectPacket rejectPacket = new ConnectRejectPacket();
                    rejectPacket.Deserialize(reader);
                    GameLogger.Log($"{rejectPacket.Type}: {rejectPacket.Reason}");
                    _manager.DisconnectAll();
                    break;
                case PacketType.ChunkSendPacket:
                    ChunkSendPacket sendPacket = new ChunkSendPacket();
                    sendPacket.Deserialize(reader);
                    GameState.World.WorldColumns.TryAdd(sendPacket.Position, new ChunkColumn(sendPacket.Position) { QueueType = QueueType.Mesh });
                    ColumnSerializer.DeserializeColumnFromBytes(GameState.World.WorldColumns[sendPacket.Position], sendPacket.Data);
                    for (int i = 0; i < WorldGenerator.WorldGenerationHeight; i++)
                    {
                        GameState.World.WorldColumns[sendPacket.Position].Chunks[i].HasUpdates = true;
                    }
                    // column.QueueType = ColumnQueueType.Mesh;
                    // bool added = _world.WorldColumns.TryAdd(sendPacket.Position, column);
                    // Console.WriteLine(added);
                    WorldGenerator.LowPriorityWorldGenerationQueue.Enqueue(sendPacket.Position);
                    break;

            }

            reader.Recycle();

        };

    }

    public void TickUpdate()
    {

        AnimatedTextureManager.Update();

    }
    public void Update()
    {

        GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
        GL.ClearColor(Color4.Black);

        Player?.UpdateInputs();
        if (Input.IsMouseFocused) Player?.Camera.Update();

        _terrainBuffer.Bind();
        GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
        GL.ClearColor(Color4.Black);
        GameState.World?.Draw(Player);
        _terrainBuffer.Unbind();
        _terrainBufferQuad.Draw(_terrainBuffer, 0.0f);

        if (Player != null)
        {

            if (GameState.World != null)
            {

                if (Input.IsKeyDown(Key.LeftControl) && Input.IsKeyPressed(Key.F))
                {

                    GlobalValues.Register.GetStructureFromNamespace("Game.RedMushroomTree")?.OnStructurePlace(GameState.World, VectorMath.Floor(NetworkingValues.Client.Player.Position));

                }

                if (Input.IsKeyDown(Key.LeftControl) && Input.IsKeyPressed(Key.G))
                {

                    string command;
                    do
                    {
                        command = Console.ReadLine();
                    } while (command == null);
                    GameLogger.Log($"Command {command} written.");
                    string[] values = command.Split(' ');
                    Vector3i globalStart = (int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]));
                    Vector3i globalEnd = (int.Parse(values[3]), int.Parse(values[4]), int.Parse(values[5]));
                    Vector3i localOrigin = (int.Parse(values[6]), int.Parse(values[7]), int.Parse(values[8]));
                    string fileName = values[9];

                    Structure.WriteDataToStructFile(GameState.World, globalStart, globalEnd, localOrigin, fileName);
                    

                }

                Dda.TraceChunks(GameState.World.WorldColumns, Player.Camera.Position, -Player.Camera.ForwardVector, 20);

                if (Input.IsKeyPressed(Key.DownArrow))
                {

                    NetworkingValues.Client.Player.CurrentSelectedIndex--;
                    if (NetworkingValues.Client.Player.CurrentSelectedIndex <= 0)
                    {

                        NetworkingValues.Client.Player.CurrentSelectedIndex = GlobalValues.Register.BlockCount - 1;

                    }

                }

                if (Input.IsKeyPressed(Key.UpArrow))
                {

                    NetworkingValues.Client.Player.CurrentSelectedIndex++;
                    if (NetworkingValues.Client.Player.CurrentSelectedIndex >= GlobalValues.Register.BlockCount)
                    {

                        NetworkingValues.Client.Player.CurrentSelectedIndex = 1;

                    }

                }

                if (Dda.DidHit && Input.IsMouseFocused)
                {

                    if (Input.IsMouseButtonPressed(MouseButton.Button2))
                    {

                        if (Input.IsKeyDown(Key.LeftControl))
                        {

                            GlobalValues.Register.GetBlockFromId(GameState.World.GetBlockId(Dda.PositionAtHit)).OnRandomTick(GameState.World, Dda.PositionAtHit, true, true);

                        } else
                        {

                            GlobalValues.Register.GetBlockFromId((ushort)NetworkingValues.Client.Player.CurrentSelectedIndex).OnBlockPlace(GameState.World, Dda.PreviousPositionAtHit, true, true);

                        }

                    }

                    if (Input.IsMouseButtonPressed(MouseButton.Button1))
                    {

                        GlobalValues.Register.GetBlockFromId((ushort)NetworkingValues.Client.Player.CurrentSelectedIndex).OnBlockDestroy(GameState.World, Dda.PositionAtHit, true, true);

                    }
                    
                }                

            }

        }

        if (!Input.IsMouseFocused)
        {

            Gui.Begin("thing");
            Gui.RenderElement(GuiMath.RelativeToAbsolute(0.75f, 0.0f), (500, 0) + GuiMath.RelativeToAbsolute(0, 1), (0.5f, 0.0f), new Color4<Rgba>(1.0f, 1.0f, 1.0f, 0.5f));
            // Gui.RenderElement(GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time), (float)Math.Cos(GlobalValues.Time)) / 2), (50, 50), (0.5f, 0.5f));
            // Gui.RenderElement(GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time + 0.1), (float)Math.Cos(GlobalValues.Time + 0.1)) / 2), (50, 50), (0.5f, 0.5f), Color4.Bisque);
            // Gui.RenderElement(GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time + 0.25), (float)Math.Cos(GlobalValues.Time + 0.25)) / 2), (50, 50), (0.5f, 0.5f), Color4.Purple);
            Gui.RenderTextbox(GuiMath.RelativeToAbsolute(0.5f, 0.4f), new Vector2i(200, 24), (0.5f, 0.5f), "Address", out string addressString, Color4.White);
            Gui.RenderTextbox(GuiMath.RelativeToAbsolute(0.5f, 0.6f), (200, 24), (0.5f, 0.5f), "User Id", out string userIdString, Color4.White);
            if (Gui.RenderButton(GuiMath.RelativeToAbsolute(0.5f, 0.7f), (150, 24), (0.5f, 0.5f), "Join Server", Color4.White))
            {

                string[] network = addressString.Split(':');

                if (network.Length != 2) 
                {

                    GameLogger.Log("invalid address");

                } else
                {

                    if (!int.TryParse(network[1], out int port))
                    {
                        GameLogger.Log("port is invalid");
                    } else if (!long.TryParse(userIdString, out long uid))
                    {
                        GameLogger.Log("uid is invalid");
                    } else
                    {
                        NetworkingValues.Client.StartMultiplayer(network[0], port, new Player() { UserId = uid, DisplayName = "Poo" });
                    }

                }

            }
            if (Gui.RenderButton(GuiMath.RelativeToAbsolute(0.5f, 0.8f), (150, 24), (0.5f, 0.5f), "Start Client Instance", Color4.White))
            {

                NetworkingValues.Client.StartSingleplayer("Shit", new Player());

            }
            Gui.End();

        }

        FontRenderer.Text((0, 25), (25, 25), 15, Color4.White, $"Memory usage: {Math.Round(Process.GetCurrentProcess().WorkingSet64 * 9.3132257461548E-10, 2)}GB");
        FontRenderer.Text((0, 50), (25, 25), 15, Color4.White, Time.TicksToDateTime(GameState.World?.TickTime ?? 0));
        FontRenderer.Text((0, 75), (25, 25), 15, Color4.White, $"Draw time: {GameState.World?.DrawTime}");
        FontRenderer.Text((0, 100), (25, 25), 15, Color4.White, $"Chunks drawn: {GameState.World?.ChunksDrawn}");
        FontRenderer.Text((0, 125), (25, 25), 15, Color4.White, $"Looking at: {Dda.PositionAtHit}");
        if (NetworkingValues.Client.Player != null)
        {

            FontRenderer.Text((0, 150), (25, 25), 15, Color4.White, $"Holding: {GlobalValues.Register.GetBlockFromId((ushort) NetworkingValues.Client.Player.CurrentSelectedIndex).DisplayName}");

        }

        _manager?.PollEvents();

    }

    public void Unload()
    {

        WorldGenerator.Unload();
        TexturePackManager.Free();
        FontRenderer.Free();
        AudioPlayer.Unload();
        // BlockGame.Unload();

    }

    public void OnResize(WindowHandle window)
    {

        Toolkit.Window.GetFramebufferSize(window, out Vector2i framebufferSize);
        GL.Viewport(0, 0, framebufferSize.X, framebufferSize.Y);
        GlobalValues.Width = framebufferSize.X;
        GlobalValues.Height = framebufferSize.Y;
        GlobalValues.Center = (GlobalValues.Width / 2f, GlobalValues.Height / 2f);

        Player?.Camera.Resize();
        _terrainBuffer.Resize();
        Gui.Resize();
        FontRenderer.Resize();

    }

}