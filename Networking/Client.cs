using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;
using StbImageSharp;
using VoxelGame.Util;

namespace VoxelGame.Networking;

public class Client
{
    private EventBasedNetListener _listener;
    public NetManager _client;
    public NetDataWriter _writer;
    public NetPeer ClientPeer;

    private World _world;
    private WorldGenerator _worldGenerator;

    public MoveableCamera Camera;
    
    Stopwatch sw; // = Stopwatch.StartNew();
    Stopwatch timer; // = Stopwatch.StartNew();
    List<double> frameTimes = new();
    
    ChunkVertex[] vertices = new ChunkVertex[6];
    private int vbo, vao = 0;
    private Shader shad;
    public Client()
    {
        _listener = new EventBasedNetListener();
        _client = new NetManager(_listener);
        _writer = new NetDataWriter();
    }

    public Client Start()
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
        
        ToolkitOptions options = new ToolkitOptions();
        options.ApplicationName = "Voxel Game";
        options.Logger = null;
        
        Toolkit.Init(options);

        OpenGLGraphicsApiHints contextSettings = new OpenGLGraphicsApiHints();
        contextSettings.Version = new Version(4, 1);
        contextSettings.Profile = OpenGLProfile.Core;
        contextSettings.DebugFlag = true;
        contextSettings.DepthBits = ContextDepthBits.Depth24;
        contextSettings.StencilBits = ContextStencilBits.Stencil8;
        
        Config.Window = Toolkit.Window.Create(contextSettings);
        Config.OpenGLContext = Toolkit.OpenGL.CreateFromWindow(Config.Window);
        
        Toolkit.Window.SetTitle(Config.Window, "Voxel Game");
        Toolkit.Window.SetSize(Config.Window, (Config.Width, Config.Height));
        Toolkit.Window.SetMode(Config.Window, WindowMode.Normal);
        Toolkit.Window.SetCursor(Config.Window, null);
        Toolkit.Window.SetCursorCaptureMode(Config.Window, CursorCaptureMode.Locked);
        Toolkit.Window.SetTransparencyMode(Config.Window, WindowTransparencyMode.TransparentFramebuffer);
        
        EventQueue.EventRaised += EventRaised;

        Toolkit.OpenGL.SetCurrentContext(Config.OpenGLContext);
        GLLoader.LoadBindings(Toolkit.OpenGL.GetBindingsContext(Config.OpenGLContext));
        
        Config.Atlas = new DynamicAtlas("resources/textures/blocks").Generate();
        Config.Register.RegisterBlock("air", new Block());
        Config.Register.RegisterBlock("grass", 
            new Block()
                .SetBlockModel(new BlockModel()
                    .AddCube(new Cube())
                    .SetTextureFace(0, Direction.Top, "grass_top")
                    .SetTextureSides(0, "grass_side")
                    .SetTextureFace(0, Direction.Bottom, "dirt")));
        Config.Register.RegisterBlock("sand",
            new Block()
                .SetBlockModel(new BlockModel()
                    .AddCube(new Cube())
                    .SetAllTextureFaces(0, "sand")));
        Config.Register.RegisterBlock("pumpkin",
            new Block()
                .SetBlockModel(new BlockModel()
                    .AddCube(new Cube())
                    .SetTextureFace(0, Direction.Top, "pumpkin_top")
                    .SetTextureFace(0, Direction.Bottom, "pumpkin_bottom")
                    .SetTextureSides(0, "pumpkin_face")));
        
        GL.Viewport(0, 0, Config.Width, Config.Height);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);
        GL.FrontFace(FrontFaceDirection.Ccw);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        Camera = new MoveableCamera(60.0f, Config.Width, Config.Height, CameraMode.Perspective);
        
        Config.ChunkShader = new Shader("resources/shaders/shad.vert", "resources/shaders/shad.frag").Compile();
        shad = new Shader("resources/shaders/vshad.vert", "resources/shaders/vshad.frag").Compile();

        Input.Init();
        
        sw = Stopwatch.StartNew();
        timer = Stopwatch.StartNew();
        
        vertices = new ChunkVertex[]
        {
            new ChunkVertex((0, 1, 1), (0, 1, 0), (0, 0)),
            new ChunkVertex((0, 1, 0), (0, 1, 0), (0, 0)),
            new ChunkVertex((1, 1, 0), (0, 1, 0), (0, 0)),
            new ChunkVertex((1, 1, 0), (0, 1, 0), (0, 0)),
            new ChunkVertex((1, 1, 1), (0, 1, 0), (0, 0)),
            new ChunkVertex((0, 1, 1), (0, 1, 0), (0, 0))
        };

        vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Marshal.SizeOf<ChunkVertex>() * vertices.Length, vertices, BufferUsage.StaticDraw);
        
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.Position)));
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.Normal)));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.TextureCoordinate)));
        GL.EnableVertexAttribArray(2);
        
        return this;
    }
    
    public void JoinServer(string hostOrIp, int port)
    {
        _client.Start();
        ClientPeer = _client.Connect(hostOrIp, port, "hello");
        
        _world = new World();
        _worldGenerator = new WorldGenerator(_world).Start();

        _listener.NetworkReceiveEvent += (fromPeer, dataReader, channel, deliveryMethod) =>
        {
            PacketType type = (PacketType)dataReader.GetInt();
            switch (type)
            {
                case PacketType.ChunkData:
                    ChunkDataPacket chunkData = (ChunkDataPacket) new ChunkDataPacket().Deserialize(dataReader);
                    chunkData.Column.Status = ChunkStatus.Mesh;
                    _world.ChunkColumns.TryAdd(chunkData.Position, chunkData.Column);
                    _worldGenerator.GenerationQueue.Enqueue(chunkData.Position);
                    break;
                case PacketType.BlockDestroy:
                    BlockDestroyPacket blockDestroy = (BlockDestroyPacket)new BlockDestroyPacket().Deserialize(dataReader);
                    Console.WriteLine($"CLIENT: block needs to be destroyed: {blockDestroy.GlobalBlockPosition}");
                    _world.SetBlockId(blockDestroy.GlobalBlockPosition, 0);
                    Vector3i chunkPosition = ChunkMath.GlobalToChunk(blockDestroy.GlobalBlockPosition);
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            for (int z = -1; z <= 1; z++)
                            {
                                if (_world.ChunkColumns.ContainsKey(chunkPosition.Xz + (x, z)))
                                {
                                    if (chunkPosition.Y + y >= 0 && chunkPosition.Y + y < Config.ColumnSize)
                                    {
                                        _world.ChunkColumns[chunkPosition.Xz + (x, z)].ChunkMeshes[chunkPosition.Y + y]
                                            .NeedsUpdates = true;
                                    }

                                    _world.ChunkColumns[chunkPosition.Xz + (x, z)].Status = ChunkStatus.Mesh;
                                    _worldGenerator.GenerationQueue.Enqueue(chunkPosition.Xz + (x, z));
                                }
                            }
                        }
                    }
                    break;
            }
            
            dataReader.Recycle();
        };

        _listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine("Connected.");
        };

        _listener.PeerDisconnectedEvent += (peer, info) =>
        {
            Console.WriteLine($"Connection disconnected. Reason: {info.Reason}");
        };
    }
    
    public void Poll()
    {
        Input.Poll();
        Toolkit.Window.ProcessEvents(false);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        GL.ClearColor(100.0f / 255.0f, 129.0f / 255.0f, 237.0f / 255.0f, 1.0f);

        if (Toolkit.Window.IsWindowDestroyed(Config.Window))
        {
            Config.IsRunning = false;
            return;
        }

        if (Input.IsKeyPressed(Key.J))
        {
            Console.WriteLine();
        }

        if (Input.IsKeyPressed(Key.F))
        {
            if (Toolkit.Window.GetMode(Config.Window) != WindowMode.WindowedFullscreen)
            {
                Toolkit.Window.SetMode(Config.Window, WindowMode.WindowedFullscreen);
            }
            else
            {
                Toolkit.Window.SetMode(Config.Window, WindowMode.Normal);
            }
        }
        
        Camera.Poll();
        _world.Draw(Camera);
        _worldGenerator.Poll();

        Ray ray = new Ray();
        ray.Origin = Camera.Position;
        ray.Direction = (Matrix3.CreateRotationY(float.DegreesToRadians(Camera.Rotation.Y)) * Matrix3.CreateRotationX(float.DegreesToRadians(-Camera.Rotation.X))).Column2 * (-1, 1, 1);
        
        if (ray.TryHit(_world, 10))
        {
            if (Input.IsMouseButtonPressed(MouseButton.Button1))
            {
                _writer.Reset();
                BlockDestroyPacket blockDestroyPacket = new BlockDestroyPacket();
                blockDestroyPacket.GlobalBlockPosition = ray.HitBlockPosition;
                blockDestroyPacket.Serialize(_writer);
                Console.WriteLine($"sending block break packet: {blockDestroyPacket.GlobalBlockPosition}");
                _client.SendToAll(_writer, DeliveryMethod.ReliableOrdered);
            }
        }
        
        Toolkit.OpenGL.SwapBuffers(Config.OpenGLContext);
        frameTimes.Add(sw.Elapsed.TotalMilliseconds);
        if (timer.Elapsed.Seconds >= 1)
        {
            double average = 0;
            double low = frameTimes.Max();
            double high = frameTimes.Min();
            foreach (double time in frameTimes) average += time;
            average = double.Round(16.6 / (average / frameTimes.Count) * 60.0);
            low = double.Round(16.6 / low * 60.0);
            high = double.Round(16.6 / high * 60.0);
            
            Toolkit.Window.SetTitle(Config.Window, $"Size: {Config.Width}, {Config.Height} | Avg FPS: {average} | Low: {low} | High: {high} | Memory usage: {double.Round(GC.GetTotalMemory(false) * 9.3132257461548E-10, 2)}GB");
            frameTimes.Clear();
            timer.Restart();
        }

        Config.DeltaTime = (float) sw.Elapsed.TotalSeconds;
        sw.Restart();
        
        _client.PollEvents();
    }

    public void Disconnect()
    {
        _client.Stop();
        _worldGenerator.Stop();
    }
    
    void EventRaised(PalHandle? handle, PlatformEventType eventType, EventArgs args)
    {
        if (args is CloseEventArgs closeEvent)
        {
            Toolkit.Window.Destroy(closeEvent.Window);
        }

        if (args is MouseMoveEventArgs mouseMove)
        {
            Input.OnMouseMove(mouseMove.ClientPosition);
        }

        if (args is MouseButtonDownEventArgs mouseButtonDown)
        {
            Input.OnMouseButtonDown(mouseButtonDown.Button);
        }

        if (args is MouseButtonUpEventArgs mouseButtonUp)
        {
            Input.OnMouseButtonUp(mouseButtonUp.Button);
        }

        if (args is KeyDownEventArgs keyDownEvent)
        {
            Input.OnKeyDown(keyDownEvent.Key);
        }

        if (args is KeyUpEventArgs keyUpEvent)
        {
            Input.OnKeyUp(keyUpEvent.Key);
        }

        if (args is WindowFramebufferResizeEventArgs windowFramebufferResize)
        {
            Config.Width = windowFramebufferResize.NewFramebufferSize.X;
            Config.Height = windowFramebufferResize.NewFramebufferSize.Y;

            Camera.Width = Config.Width;
            Camera.Height = Config.Height;
            
            GL.Viewport(0, 0, Config.Width, Config.Height);
        }
    }
}