using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

using Blockgame_OpenTK.Util;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.PlayerUtil;
using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.FramebufferUtil;
using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.Worlds;

namespace Blockgame_OpenTK
{
    internal class Game : GameWindow
    {
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title, Flags = ContextFlags.Debug}) { }

        public static float[] verts = {

            -0.5f, -0.5f, -0.5f,  0, 0, -1, 0.0f, 0.0f, // front
             0.5f, -0.5f, -0.5f,  0, 0, -1, 1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  0, 0, -1, 1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  0, 0, -1, 1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0, 0, -1, 0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0, 0, -1, 0.0f, 0.0f,

            0.5f, -0.5f, -0.5f,  1, 0, 0, 0.0f, 0.0f, // right
            0.5f, -0.5f, 0.5f,  1, 0, 0, 1.0f, 0.0f,
            0.5f,  0.5f, 0.5f,  1, 0, 0, 1.0f, 1.0f,
            0.5f,  0.5f, 0.5f,  1, 0, 0, 1.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  1, 0, 0, 0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  1, 0, 0, 0.0f, 0.0f,

            0.5f, -0.5f, 0.5f, 0, 0, 1, 0.0f, 0.0f, // back 
            -0.5f, -0.5f, 0.5f, 0, 0, 1, 1.0f, 0.0f,
            -0.5f, 0.5f, 0.5f, 0, 0, 1, 1.0f, 1.0f,
            -0.5f, 0.5f, 0.5f, 0, 0, 1, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 0, 0, 1, 0.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 0, 0, 1, 0.0f, 0.0f,

            -0.5f, -0.5f, 0.5f,  -1, 0, 0, 0.0f, 0.0f, // left
            -0.5f, -0.5f, -0.5f,  -1, 0, 0, 1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  -1, 0, 0, 1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  -1, 0, 0, 1.0f, 1.0f,
            -0.5f,  0.5f, 0.5f,  -1, 0, 0, 0.0f, 1.0f,
            -0.5f, -0.5f, 0.5f,  -1, 0, 0, 0.0f, 0.0f,

            -0.5f, 0.5f, -0.5f, 0, 1, 0, 0.0f, 0.0f, // top
            0.5f, 0.5f, -0.5f, 0, 1, 0, 1.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 0, 1, 0, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 0, 1, 0, 1.0f, 1.0f,
            -0.5f, 0.5f, 0.5f, 0, 1, 0, 0.0f, 1.0f,
            -0.5f, 0.5f, -0.5f, 0, 1, 0, 0.0f, 0.0f,

            0.5f, -0.5f, -0.5f, 0, -1, 0, 0.0f, 0.0f, // bottom
            -0.5f, -0.5f, -0.5f, 0, -1, 0, 1.0f, 0.0f,
            -0.5f, -0.5f, 0.5f, 0, -1, 0, 1.0f, 1.0f,
            -0.5f, -0.5f, 0.5f, 0, -1, 0, 1.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 0, -1, 0, 0.0f, 1.0f,
            0.5f, -0.5f, -0.5f, 0, -1, 0, 0.0f, 0.0f

        };

        float[] xyz_verts =
        {

            0, 0, 0, 0, 0,
            1, 0, 0, 0, 0,
            0, 0, 0, 0, 0,

            0, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 0, 0, 0,

            0, 0, 0, 0, 0,
            0, 0, 1, 0, 0,
            0, 0, 0, 0, 0

        };

        float[] v =
        {

             -0.5f, 0.5f, 0.0f, 0.0f, 1.0f,
             -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
             0.5f, 0.5f, 0.0f, 1.0f, 1.0f,
             -0.5f, 0.5f, 0.0f, 0.0f, 1.0f

        };

        float[] skybox =
        {

            -0.5f, -0.5f, -0.5f,  0.0f, 0.5f, // front
             0.5f, -0.5f, -0.5f,  0.25f, 0.5f,
             0.5f,  0.5f, -0.5f,  0.25f, 1f,
             0.5f,  0.5f, -0.5f,  0.25f, 1f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.5f,

            0.5f, -0.5f, -0.5f,  0.25f, 0.5f, // right
            0.5f, -0.5f, 0.5f,  0.5f, 0.5f,
            0.5f,  0.5f, 0.5f,  0.5f, 1.0f,
            0.5f,  0.5f, 0.5f,  0.5f, 1.0f,
            0.5f,  0.5f, -0.5f,  0.25f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.25f, 0.5f,

            0.5f, -0.5f, 0.5f, 0.5f, 0.5f, // back 
            -0.5f, -0.5f, 0.5f, 0.75f, 0.5f,
            -0.5f, 0.5f, 0.5f, 0.75f, 1.0f,
            -0.5f, 0.5f, 0.5f, 0.75f, 1.0f,
            0.5f, 0.5f, 0.5f, 0.5f, 1.0f,
            0.5f, -0.5f, 0.5f, 0.5f, 0.5f,

            -0.5f, -0.5f, 0.5f,  0.75f, 0.5f, // left
            -0.5f, -0.5f, -0.5f,  1f, 0.5f,
            -0.5f,  0.5f, -0.5f,  1f, 1.0f,
            -0.5f,  0.5f, -0.5f,  1f, 1.0f,
            -0.5f,  0.5f, 0.5f,  0.75f, 1.0f,
            -0.5f, -0.5f, 0.5f,  0.75f, 0.5f,

            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, // top
            0.5f, 0.5f, -0.5f, 0.25f, 0.0f,
            0.5f, 0.5f, 0.5f, 0.25f, 0.5f,
            0.5f, 0.5f, 0.5f, 0.25f, 0.5f,
            -0.5f, 0.5f, 0.5f, 0.0f, 0.5f,
            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f,

            0.5f, -0.5f, -0.5f, 0.25f, 0.0f, // bottom
            -0.5f, -0.5f, -0.5f, 0.5f, 0.0f,
            -0.5f, -0.5f, 0.5f, 0.5f, 0.5f,
            -0.5f, -0.5f, 0.5f, 0.5f, 0.5f,
            0.5f, -0.5f, 0.5f, 0.25f, 0.5f,
            0.5f, -0.5f, -0.5f, 0.25f, 0.0f

        };

        Shader shader;
        Shader ChunkShader;
        Texture texture;
        Texture emtexture;
        CMTexture cmtex;

        int vbo;
        int vao;

        float speed = 2.0f;
        // Vector3 cposition = new Vector3(10000000.0f, 0.0f, 10000000.0f);
        Vector3 cposition = (0, 0, 0);
        Vector3 cfront = new Vector3(0.0f, 0.0f, -1.0f);
        Vector3 cup = new Vector3(0.0f, 1.0f, 0.0f);

        float pitch; // x rotation
        float yaw; // y rotation
        float roll; // z rotation

        float sens = 0.8f;

        double time = 0;

        Vector2 lmpos = new Vector2(0.0f, 0.0f);

        bool firstmove = true;

        double delay = 0;

        public static Model rmodel;
        // Chunk chunk;
        Camera camera;

        Model xyz_display;
        Model hitdisplay;
        Model Skybox;
        Model e;
        // Model Sun;

        NakedModel nakedmodel;

        BoundingBox boundingbox = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(0.5f, 0.5f, 0.5f));
        NakedModel boundmodel;
        // GUIElement TestElement;
        // GUIClickable GUIClick;
        TextRenderer text;

        // Ray ray = new Ray(0, 0, 0, 0, 0, 0);

        public Framebuffer frameBuffer;
        FramebufferQuad framebufferQuad;

        ArrayTexture TextureArray = new ArrayTexture();

        bool debug = false;

        bool IsGrabbed = true;

        double ft = 0;
        double fs = 0;
        // Chunk c;
        Chunk nc = new Chunk((0,0,0));
        Sun Sun;

        Player Player;

        GuiElement Element;
        GuiButton ButtonElement;
        GuiWindow GWindow;
        // GUIElement texx;

        World World = new World("nofile");
        GuiWindow Window;

        // public static Block snowb;

        private static void OnDebugMessage(
    DebugSource source,     // Source of the debugging message.
    DebugType type,         // Type of the debugging message.
    int id,                 // ID associated with the message.
    DebugSeverity severity, // Severity of the message.
    int length,             // Length of the string in pMessage.
    IntPtr pMessage,        // Pointer to message string.
    IntPtr pUserParam)      // The pointer you gave to OpenGL, explained later.
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
            
        }

        private static DebugProc DebugMessageDelegate = OnDebugMessage;
        protected override void OnUpdateFrame(FrameEventArgs args)
        {

            base.OnUpdateFrame(args);

            GlobalValues.DeltaTime = args.Time;
            GlobalValues.Time += GlobalValues.DeltaTime;
            GlobalValues.Mouse = MouseState;
            GlobalValues.Keyboard = KeyboardState;

            // Console.WriteLine($"Mouse Delta X: {Globals.Mouse.Delta.X}, Computed mouse delta X: {Globals.Mouse.X - Globals.Mouse.PreviousX}");
            // Console.WriteLine(UInt32.MaxValue);
            
            if (GlobalValues.Keyboard.IsKeyPressed(Keys.Escape))
            { 

                if (!IsGrabbed)
                {

                    IsGrabbed = true;
                    CursorState = CursorState.Grabbed;
                        
                }
                else { IsGrabbed = false; CursorState = CursorState.Normal; MousePosition = GlobalValues.Center;  }


            }
            if (GlobalValues.Keyboard.IsKeyPressed(Keys.F1))
            {

                if (!debug)
                {

                    debug = true;

                }
                else { debug = false; }


            }
            GlobalValues.CursorState = CursorState;

        }

        GuiElement uiTest;
        GuiButton GenerateButton;
        protected override void OnLoad()
        {

            base.OnLoad();

            TextRenderer.InitTextRenderer();
            // Console.WriteLine(GL.GetInteger(GetPName.MajorVersion) + "." + GL.GetInteger(GetPName.MinorVersion));

            GlobalValues.ArrayTexture.Load();
            Blocks.Load();
            GlobalValues.AtlasTexture = new TextureAtlas("atlas.png", 32);
            GlobalValues.ChunkShader = new Shader("chunk.vert", "chunk.frag");
            GlobalValues.DefaultShader = new Shader("default.vert", "default.frag");
            GlobalValues.GuiShader = new Shader("gui.vert", "gui.frag");
            GlobalValues.Mouse = MouseState;
            GlobalValues.Keyboard = KeyboardState;
            CursorState = CursorState.Grabbed;

            BinaryWriter bw = new BinaryWriter(File.Open("Resources/cdat/1.cdat", FileMode.OpenOrCreate));

            bw.Write((uint)500);

            camera = new Camera(cposition, cfront, cup, CameraType.Perspective, 45.0f);
            rmodel = new Model(verts, "hitbox.png", "hitbox.vert", "hitbox.frag");
            hitdisplay = new Model(verts, "debug.png", "model.vert", "model.frag");
            xyz_display = new Model(xyz_verts, null, "debug.vert", "debug.frag");

            nakedmodel = new NakedModel(NakedModel.Tri);

            boundmodel = new NakedModel(boundingbox.triangles);

            Skybox = new Model(verts, "cubemap/cubemap_test.png", "model.vert", "model.frag");

            xyz_display.SetScale(0.25f, 0.25f, 0.25f);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Enable(EnableCap.DepthTest);
            // GL.DepthFunc(DepthFunction.Equal);
            GL.Enable(EnableCap.StencilTest);
            // GL.DepthMask(false);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.TextureCubeMap);
            // GL.Enable(EnableCap.TextureCubeMapSeamless);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Enable(EnableCap.PolygonOffsetFill);

            texture = new Texture("atlas.png");
            emtexture = new Texture("atlas_em.png");
            Texture t = new Texture("cubemap/cubemap_test.png");
            cmtex = new CMTexture(t, 64);
            // TestElement = new GUIElement(50, 50, 10, 10, OriginType.Center, t, GUIElement.Null);
            // text = new TextRenderer(16, "");
            // text.SetFontColor((0,0,0));

            ChunkShader = new Shader("chunk.vert", "chunk.frag");

            frameBuffer = new Framebuffer();
            framebufferQuad = new FramebufferQuad();

            Sun = new Sun("sun.png", 10);

            Player = new Player();
            Player.SetHeight(0);
            Player.SetPosition((16, 16, 16));

            byte[] dataExample = new byte[] { 255, 220 };

            Console.WriteLine(TextureLoader.Sub(dataExample, 1));

            Blockgame_OpenTK.Util.Image Image = Blockgame_OpenTK.Util.Image.LoadPng("Resources/Textures/skybox.png", true);
            Console.WriteLine($"width: {Image.Width}, height: {Image.Height}");

            uiTest = new GuiElement((24, 24), GuiElement.Center);
            uiTest.SetRelativePosition(0.5f, 0.5f);
            uiTest.Rotate(45);

            Element = new GuiElement((50,50), GuiElement.TopLeft);
            Element.SetRelativePosition(0, 1);

            ButtonElement = new GuiButton((75, 75), GuiElement.Center);
            ButtonElement.SetRelativePosition(0.5f, 0.5f);
            ButtonElement.OnButtonClick = () => { Console.WriteLine("I was clicked!"); };
            ButtonElement.IsMoveable = true;

            GWindow = new GuiWindow((100, 80), GuiWindow.DecorationMode.Decorated);
            GWindow.SetRelativePosition(0.5f, 0.5f);

            GuiButton button = new GuiButton((50, 50), GuiElement.BottomLeft);
            button.SetRelativePosition(0, 1);
            GWindow.AddElement(button);

            // World world = new World();
            GenerateButton = new GuiButton((100, 20), GuiElement.Center);
            GenerateButton.SetRelativePosition(0.2f, 0.2f);

            GenerateButton.OnButtonClick = () => { World.DebugReset(); };


            Window = new GuiWindow((100, 80), GuiWindow.DecorationMode.Decorated);

            e = new Model(v, "missing.png", "billboard.vert", "billboard.frag");

        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {

            base.OnRenderFrame(args);

            Player.Update(World);

            Stopwatch sw = Stopwatch.StartNew();

            frameBuffer.Bind();

            GL.ClearColor(1,1,1,1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            cfront.X = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(yaw));
            cfront.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            cfront.Z = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(yaw));
            cfront = Vector3.Normalize(cfront);

            Matrix4 view = Matrix4.LookAt(cposition, cposition + cfront, cup);

            camera.Update(cposition, cfront, cup);
            // Player.Update();
            // etcW
            GL.Disable(EnableCap.DepthTest);
            // draw skybox
            Vector3 rotation = ((float)GlobalValues.Time / 5f, 0, 0);
            rotation = Vector3.Zero;
            // Console.WriteLine(Vector3.Dot((0, 1, 0), (new Vector4(0, -1, 0, 0) * Sun.RotationMatrix).Xyz));
            Sun.SetRotation(rotation);
            Skybox.Draw(Player.Camera.Position, (new Vector4(0, -1, 0, 0) * Sun.RotationMatrix).Xyz, Player.Camera, (float)GlobalValues.Time);
            Sun = new Sun("sun.png", 0);
            Sun.SetPosition(Player.Camera.Position);
            Sun.SetScale((0.5f, 5, 0.5f));
            Sun.SetRotation(rotation);
            Sun.Draw(Player.Camera);
            Sun = new Sun("moon.png", 0);
            Sun.SetPosition(Player.Camera.Position);
            Sun.SetScale((0.5f, 5, 0.5f));
            Sun.SetRotation((rotation.X + (Maths.ToRadians(180)), rotation.Y, rotation.Z));
            Sun.Draw(Player.Camera);

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            if (GlobalValues.Keyboard.IsKeyDown(Keys.LeftControl))
            {

                if (GlobalValues.Keyboard.IsKeyPressed(Keys.R))
                {

                    // throw new BlockNotFoundException("Forced a crash using BlockNotFoundException");

                    // Console.WriteLine($"Is empty: {World.WorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)].IsEmpty} is exposed: {World.WorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)].CheckIfExposed(World.WorldChunks)}, queue type: {World.WorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)].QueueType}, queue count: {WorldGenerator.ChunkUpdateQueue.Count}, in queue: {WorldGenerator.ChunkUpdateQueue.Where(value => value == ChunkUtils.PositionToChunk(Player.Camera.Position)) != null}");

                    Console.WriteLine("forcing a mesh");
                    // ChunkBuilder.MeshThreaded(World.WorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)], World.WorldChunks, Vector3i.Zero);
                    WorldGenerator.ChunkUpdateQueue.Enqueue(ChunkUtils.PositionToChunk(Player.Camera.Position));

                }

                if (GlobalValues.Keyboard.IsKeyPressed(Keys.S))
                {

                    // ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(Player.Camera.Position)).SaveToFile();

                    if (World.WorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)].QueueType == QueueType.Finish)
                    {

                        World.WorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)].SaveToFile();

                    } else
                    {

                        Console.WriteLine($"The chunk at {ChunkUtils.PositionToChunk(Player.Camera.Position)} either is not used or is not ready.");

                    }

                }

                if (GlobalValues.Keyboard.IsKeyPressed(Keys.W))
                {

                    if (World.WorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)].QueueType == QueueType.Finish)
                    {

                        World.WorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)].TryLoad();

                    }

                    // ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(Player.Camera.Position)).TryLoad();
                    // ChunkLoader.RemeshQueue.Add(ChunkUtils.PositionToChunk(Player.Camera.Position));

                }

                if (GlobalValues.Keyboard.IsKeyPressed(Keys.C))
                {

                    Console.WriteLine("Reloading chunks for debug purposes");
                    World.DebugReset();
                    // ChunkLoader.DebugReset();

                }

                if (GlobalValues.Keyboard.IsKeyPressed(Keys.F))
                {

                    Console.WriteLine("Toggling fog");
                    GlobalValues.ShouldRenderFog = !GlobalValues.ShouldRenderFog;

                }

                if (GlobalValues.Keyboard.IsKeyPressed(Keys.B))
                {

                    Console.WriteLine("Toggling chunk bounds");
                    GlobalValues.ShouldRenderBounds = !GlobalValues.ShouldRenderBounds;

                }

                if (GlobalValues.Keyboard.IsKeyPressed(Keys.O))
                {

                    Console.WriteLine("Toggling AO");
                    GlobalValues.RenderAmbientOcclusion = !GlobalValues.RenderAmbientOcclusion;

                }

                GlobalValues.FogOffset += (0.1f * GlobalValues.Mouse.ScrollDelta.Y);
                GlobalValues.FogOffset = Math.Clamp(GlobalValues.FogOffset, 0, 1);

            }

            World.Generate(Player.Position);
            World.Draw(Player.Camera);

            // Console.WriteLine(Globals.FogOffset);

            // GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
            // GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            // GL.StencilMask(0xFF);
            GL.FrontFace(FrontFaceDirection.Cw);
            rmodel.SetScale(10, 1, 10);
            rmodel.Draw(Vector3.Zero, Vector3.Zero, Player.Camera, (float)GlobalValues.Time/5f);
            rmodel.SetScale(0.25f, 0.25f, 0.25f);
            rmodel.Draw(Vector3.Zero, Vector3.Zero, Player.Camera, (float)time);
            GL.FrontFace(FrontFaceDirection.Ccw);
            // GL.StencilMask(0x00);
            // ChunkLoader.DrawReadyChunks((new Vector4(0, -1, 0, 0) * Sun.RotationMatrix).Xyz, Player.Camera);


            //GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            TextRenderer.RenderTextWithShadow((4, 4, 0), TextRenderer.TopLeft, (2, 2, 0), (0.8f, 0.8f, 0.8f), (0,0,0), 16, $"{Player.Position}");

            uiTest.Draw(0);
            GenerateButton.Draw(0);
            
            GL.Enable(EnableCap.DepthTest);

            if (debug)
            {

                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            }

            Dda.TraceChunks(World.WorldChunks, Player.Camera.Position, Player.Camera.ForwardVector, GlobalValues.PlayerRange);

            if (Dda.hit)
            {

                float lineThickness = 12.0f;

                GL.FrontFace(FrontFaceDirection.Cw);
                rmodel.SetScale(1, 1, 1);
                GL.PolygonOffset(-1, 1);
                GL.Disable(EnableCap.DepthTest);
                LineRenderer.DrawLine((0,0,0) + Dda.PositionAtHit, (0,1,0) + Dda.PositionAtHit, lineThickness, (0,0,0), Player.Camera);
                LineRenderer.DrawLine((1, 0, 0) + Dda.PositionAtHit, (1, 1, 0) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                LineRenderer.DrawLine((0, 0, 1) + Dda.PositionAtHit, (0, 1, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                LineRenderer.DrawLine((1, 0, 1) + Dda.PositionAtHit, (1, 1, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);

                LineRenderer.DrawLine((0, 0, 0) + Dda.PositionAtHit, (1, 0, 0) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                LineRenderer.DrawLine((0, 1, 0) + Dda.PositionAtHit, (1, 1, 0) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                LineRenderer.DrawLine((0, 0, 1) + Dda.PositionAtHit, (1, 0, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                LineRenderer.DrawLine((0, 1, 1) + Dda.PositionAtHit, (1, 1, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);

                LineRenderer.DrawLine((1, 0, 0) + Dda.PositionAtHit, (1, 0, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                LineRenderer.DrawLine((1, 1, 0) + Dda.PositionAtHit, (1, 1, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                LineRenderer.DrawLine((0, 0, 0) + Dda.PositionAtHit, (0, 0, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                LineRenderer.DrawLine((0, 1, 0) + Dda.PositionAtHit, (0, 1, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                GL.Enable(EnableCap.DepthTest);
                // rmodel.Draw((Vector3)DDA.PositionAtHit + (0.5f, 0.5f, 0.5f), Vector3.Zero, Player.Camera, (float)time);
                // rmodel.Draw((Vector3)Dda.PreviousPositionAtHit + (0.5f, 0.5f, 0.5f), Vector3.Zero, Player.Camera, (float)time);
                // rmodel.SetScale(1, 1, 1);
                // rmodel.Draw((Vector3)Dda.PositionAtHit + (0.5f,0.5f,0.5f), Vector3.Zero, Player.Camera, (float)time);
                // rmodel.SetScale(0.2f, 0.2f, 0.2f);
                // rmodel.Draw(Dda.SmoothPosition, Vector3.Zero, Player.Camera, (float)time);
                GL.FrontFace(FrontFaceDirection.Ccw);
                GL.PolygonOffset(0, 0);

            }

            if (debug)
            {

                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.DepthTest);
                // GL.LineWidth(5f);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                xyz_display.Draw(Player.Camera.Position + (Player.Camera.ForwardVector * 5), Vector3.Zero, Player.Camera, (float)time);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.Enable(EnableCap.CullFace);
                GL.Enable(EnableCap.DepthTest);

            }

            LineRenderer.DrawLine((0,0,0), (8,20,0), 25, Vector3.Zero, Player.Camera);
            // LineRenderer.DrawLine((0, 0, 0), (2, 0, 0), 20, Vector3.Zero, Player.Camera);
            // LineRenderer.DrawLine((0, 0, 0), (0, 0, 2), 20, Vector3.Zero, Player.Camera);

            frameBuffer.Unbind();

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            framebufferQuad.Draw(frameBuffer, (float)time);
            KeyboardState ks = KeyboardState;

            sw.Stop();
            // Console.WriteLine("Finished frame in " + sw.ElapsedMilliseconds + " ms. FPS: " + (1000f/sw.ElapsedMilliseconds));
            GlobalValues.FrameInformation = "Ft: " + (sw.ElapsedMilliseconds.ToString().Length < 2 ? "0" + sw.ElapsedMilliseconds : sw.ElapsedMilliseconds) + "ms. campos: " + ChunkUtils.PositionToBlockGlobal(Player.Camera.Position);

            SwapBuffers();

        }
        protected override void OnUnload()
        {

            base.OnUnload();

            foreach (Chunk c in World.WorldChunks.Values)
            {

                c.SaveToFile();

            }

            // this portion is not required
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vbo);

            // shader.Dispose();

        }
        protected override void OnResize(ResizeEventArgs e)
        {

            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
            GlobalValues.WIDTH = e.Width;
            GlobalValues.HEIGHT = e.Height;
            GlobalValues.Center = (GlobalValues.WIDTH / 2f, GlobalValues.HEIGHT / 2f);

            camera.UpdateProjectionMatrix();
            Player.Camera.UpdateProjectionMatrix();
            GlobalValues.GuiCamera.UpdateProjectionMatrix();
            uiTest.OnScreenResize();
            // TestElement.Update();
            TextRenderer.Camera.UpdateProjectionMatrix();
            frameBuffer.UpdateAspect();

        }

    }

}
