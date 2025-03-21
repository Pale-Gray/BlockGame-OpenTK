using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Blockgame_OpenTK.Audio;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Gui;
using Blockgame_OpenTK.Core.Image;
using Blockgame_OpenTK.Core.PlayerUtil;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.FramebufferUtil;
using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.Util;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Blockgame_OpenTK.Core.Networking;

public class NewClient
{

    private NetManager _manager;
    private EventBasedNetListener _listener;
    private NetDataWriter _writer = new NetDataWriter();
    public World World;
    public bool IsNetworked { get; private set; } = false;
    public NewPlayer Player { get; private set; }
    private Framebuffer _terrainBuffer;
    private FramebufferQuad _terrainBufferQuad;
    private int _id;
    public void Load()
    {

        // GL.Enable(EnableCap.DebugOutput);
        // GL.DebugMessageCallback(_delegate, IntPtr.Zero);

        BlockGame.Load();
        GlobalValues.Base.OnLoad(GlobalValues.Register);

        _terrainBuffer = new Framebuffer();
        _terrainBufferQuad = new FramebufferQuad();

        // Stopwatch sw = Stopwatch.StartNew();

        // PngImage image = new PngImage();
        // // image = ImageFile.Load("Resources/Textures/happy-super-happy1080inter.png");
        // // PngImage image = ImageFile.Load("Resources/Textures/happy-super-happy1080.png");
        // // sw.Stop();
        // // Console.WriteLine($"img load took {sw.Elapsed.TotalMilliseconds}ms");
// 
        // _id = GL.GenTexture();
        // GL.BindTexture(TextureTarget.Texture2d, _id);
        // GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        // GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        // GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        // GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
// 
        // GL.TexStorage2D(TextureTarget.Texture2d, 1, SizedInternalFormat.Rgba8, image.Width, image.Height);
        // GL.TexSubImage2D(TextureTarget.Texture2d, 0, 0, 0, image.Width, image.Height, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
// 
        // GL.BindTexture(TextureTarget.Texture2d, 0);

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
    public void JoinWorld(string worldSave, NewPlayer player)
    {

        // singleplayer
        IsNetworked = false;

        Player = player;

        NetworkingValues.Server = new NewServer();
        NetworkingValues.Server.StartSingleplayer(worldSave, Player);

    }

    public void JoinWorld(string addressOrHost, int port, NewPlayer player)
    {

        // multiplayer
        IsNetworked = true;

        Player = player;

        _listener = new EventBasedNetListener();
        _manager = new NetManager(_listener);

        World = new World("");
        PackedWorldGenerator.CurrentWorld = World;
        PackedWorldGenerator.Initialize();

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
                    World.WorldColumns.TryAdd(sendPacket.Position, new ChunkColumn(sendPacket.Position) { QueueType = ColumnQueueType.Mesh });
                    ColumnSerializer.DeserializeColumnFromBytes(World.WorldColumns[sendPacket.Position], sendPacket.Data);
                    for (int i = 0; i < PackedWorldGenerator.WorldGenerationHeight; i++)
                    {
                        World.WorldColumns[sendPacket.Position].Chunks[i].HasUpdates = true;
                    }
                    // column.QueueType = ColumnQueueType.Mesh;
                    // bool added = _world.WorldColumns.TryAdd(sendPacket.Position, column);
                    // Console.WriteLine(added);
                    PackedWorldGenerator.ColumnWorldGenerationQueue.EnqueueLast(sendPacket.Position);
                    break;

            }

            reader.Recycle();

        };

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
        if (NetworkingValues.Server?.IsNetworked ?? true) 
        {
            World?.Draw(Player);
        } else
        {
            NetworkingValues.Server?.World.Draw(Player);
        }
        _terrainBuffer.Unbind();
        _terrainBufferQuad.Draw(_terrainBuffer, 0.0f);
        
        PackedWorldGenerator.Update();

        if (!Input.IsMouseFocused)
        {

            GuiRenderer.Begin("thing");
            GuiRenderer.RenderElement(GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time), (float)Math.Cos(GlobalValues.Time)) / 2), (50, 50), (0.5f, 0.5f));
            GuiRenderer.RenderElement(GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time + 0.1), (float)Math.Cos(GlobalValues.Time + 0.1)) / 2), (50, 50), (0.5f, 0.5f), Color4.Bisque);
            GuiRenderer.RenderElement(GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time + 0.25), (float)Math.Cos(GlobalValues.Time + 0.25)) / 2), (50, 50), (0.5f, 0.5f), Color4.Purple);
            GuiRenderer.RenderTextbox(GuiMath.RelativeToAbsolute(0.5f, 0.4f), new Vector2i(200, 24), (0.5f, 0.5f), "Address", out string addressString, Color4.White);
            GuiRenderer.RenderTextbox(GuiMath.RelativeToAbsolute(0.5f, 0.6f), (200, 24), (0.5f, 0.5f), "User Id", out string userIdString, Color4.White);
            if (GuiRenderer.RenderButton(GuiMath.RelativeToAbsolute(0.5f, 0.7f), (150, 24), (0.5f, 0.5f), "Join Server", Color4.White))
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
                        NetworkingValues.Client.JoinWorld(network[0], port, new NewPlayer() { UserId = uid, DisplayName = "Poo" });
                    }

                }

            }
            if (GuiRenderer.RenderButton(GuiMath.RelativeToAbsolute(0.5f, 0.8f), (150, 24), (0.5f, 0.5f), "Start Client Instance", Color4.White))
            {

                NetworkingValues.Client.JoinWorld("Shit", new NewPlayer());
                // NetworkingValues.Server = new NewServer();
                // NetworkingValues.Server.StartSingleplayer("Shit", new NewPlayer());

            }
            // GuiRenderer.RenderTexture(GuiMath.RelativeToAbsolute(0.5f, 0.5f), (150, 150), (0.5f, 0.5f), _id);
            GuiRenderer.End();

        }

        _manager?.PollEvents();

    }

    public void Unload()
    {

        PackedWorldGenerator.Unload();

        BlockGame.Unload();

    }

    public void OnResize()
    {

        Player?.Camera.UpdateProjectionMatrix();
        _terrainBuffer.UpdateAspect();

    }

}