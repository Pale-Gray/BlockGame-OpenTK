using Blockgame_OpenTK.Util;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.PlayerUtil;
using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.FramebufferUtil;
using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.Worlds;
using OpenTK.Mathematics;
using System;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using System.Diagnostics;
using Blockgame_OpenTK.Font;
using System.Runtime.InteropServices;
using System.IO;
using Blockgame_OpenTK.ChunkUtil;

namespace Blockgame_OpenTK
{
    internal class BlockGame
    {

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

        static float[] xyz_verts =
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

        static float[] v =
        {

             -0.5f, 0.5f, 0.0f, 0.0f, 1.0f,
             -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
             0.5f, 0.5f, 0.0f, 1.0f, 1.0f,
             -0.5f, 0.5f, 0.0f, 0.0f, 1.0f

        };

        static float[] skybox =
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

        static Shader shader;
        static Shader ChunkShader;
        static Texture texture;
        static Texture emtexture;

        static int vbo;
        static int vao;

        static float speed = 2.0f;
        // Vector3 cposition = new Vector3(10000000.0f, 0.0f, 10000000.0f);
        static Vector3 cposition = (0, 0, 0);
        static Vector3 cfront = new Vector3(0.0f, 0.0f, -1.0f);
        static Vector3 cup = new Vector3(0.0f, 1.0f, 0.0f);

        static float pitch; // x rotation
        static float yaw; // y rotation
        static float roll; // z rotation

        static float sens = 0.8f;

        static double time = 0;

        static Vector2 lmpos = new Vector2(0.0f, 0.0f);

        static bool firstmove = true;

        static double delay = 0;

        public static Model rmodel;
        // Chunk chunk;
        static Camera camera;

        static Model xyz_display;
        static Model hitdisplay;
        static Model Skybox;
        static Model e;
        // Model Sun;

        static NakedModel nakedmodel;

        static BoundingBox boundingbox = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(0.5f, 0.5f, 0.5f));
        static NakedModel boundmodel;
        // GUIElement TestElement;
        // GUIClickable GUIClick;
        static  TextRenderer text;

        // Ray ray = new Ray(0, 0, 0, 0, 0, 0);

        public static Framebuffer frameBuffer;
        static FramebufferQuad framebufferQuad;

        static ArrayTexture TextureArray = new ArrayTexture();

        static bool debug = false;

        static bool IsGrabbed = true;

        static double ft = 0;
        static double fs = 0;
        // Chunk c;
        static Sun Sun;

        static Player Player;

        static GuiElement Element;
        static GuiButton ButtonElement;
        static GuiWindow GWindow;
        // GUIElement texx;

        static World World = new World("nofile");
        static GuiWindow Window;

        static GuiElement uiTest;
        static GuiButton GenerateButton;

        static GuiElement TestElement = new GuiElement();
        static GuiContainer TestContainer = new GuiContainer();
        static double TickTime = 0;

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
        public static void Load()
        {

            GLDebugProc debugMessageDel = OnDebugMessage;

            Util.Debugger.Log($"Platform: {RuntimeInformation.OSDescription}", Severity.Info);
            Util.Debugger.Log($"Architecture: {RuntimeInformation.OSArchitecture.ToString().ToLower()}", Severity.Info);
            Util.Debugger.Log(RuntimeInformation.FrameworkDescription, Severity.Info);
            Util.Debugger.Log(GL.GetString(StringName.Vendor), Severity.Info);
            Util.Debugger.Log(GL.GetString(StringName.Renderer), Severity.Info); // gpu specs
            Util.Debugger.Log(GL.GetString(StringName.Version), Severity.Info); 
            

            if (!Directory.Exists("Chunks"))
            {

                Util.Debugger.Log("Created Chunks folder because it did not exist.", Severity.Info);
                Directory.CreateDirectory("Chunks");

            }

            //  GL.DebugMessageCallback(debugMessageDel, IntPtr.Zero);
            // GL.Enable(EnableCap.DebugOutput);
            // GL.Enable(EnableCap.DebugOutputSynchronous);
            // Console.WriteLine(GL.GetInteger(GetPName.MaxVaryingVectors));

            Translator.LoadGameSettings();
            Input.Initialize();
            CachedFontRenderer.Init();

            // GL.Viewport(0, 0, (int)GlobalValues.WIDTH, (int)GlobalValues.HEIGHT);

            int major = GL.GetInteger(GetPName.MajorVersion);
            int minor = GL.GetInteger(GetPName.MinorVersion);

            Console.WriteLine($"Running OpenGL Version {major}.{minor}");

            TextRenderer.InitTextRenderer();
            Translator.LoadKeymap();

            GlobalValues.ArrayTexture.Load();
            Blocks.Load();
            // GlobalValues.AtlasTexture = new TextureAtlas("atlas.png", 32);
            GlobalValues.GuiBlockShader = new Shader("guiblock.vert", "guiblock.frag");
            GlobalValues.ChunkShader = new Shader("chunk.vert", "chunk.frag");
            GlobalValues.DefaultShader = new Shader("default.vert", "default.frag");
            GlobalValues.GuiShader = new Shader("gui.vert", "gui.frag");
            GlobalValues.CachedFontShader = new Shader("cachedFont.vert", "cachedFont.frag");
            // GlobalValues.Mouse = MouseState;
            // GlobalValues.Keyboard = KeyboardState;

            // CursorState = CursorState.Grabbed;

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
            GL.DepthFunc(DepthFunction.Less);
            // GL.DepthFunc(DepthFunction.Lequal);
            // GL.Enable(EnableCap.StencilTest);
            GL.Enable(EnableCap.CullFace);
            // GL.Enable(EnableCap.FramebufferSrgb);
            // GL.FrontFace(FrontFaceDirection.Ccw);
            // GL.Enable(EnableCap.TextureCubeMap);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Enable(EnableCap.PolygonOffsetFill);

            // texture = new Texture("atlas.png");
            // emtexture = new Texture("atlas_em.png");
            Texture t = new Texture("cubemap/cubemap_test.png");

            ChunkShader = new Shader("chunk.vert", "chunk.frag");

            frameBuffer = new Framebuffer();
            framebufferQuad = new FramebufferQuad();

            Sun = new Sun("sun.png", 10);

            Player = new Player();
            Player.SetHeight(1.8f);
            Player.SetPosition((16, 68, 16));

            // uiTest = new GuiElement((8, 8), GuiElement.Center);
            // uiTest.SetRelativePosition(0.5f, 0.5f);
            // uiTest.Rotate(45);

            // Element = new GuiElement((50, 50), GuiElement.TopLeft);
            // Element.SetRelativePosition(0, 1);

            // ButtonElement = new GuiButton((75, 75), GuiElement.Center);
            // ButtonElement.SetRelativePosition(0.5f, 0.5f);
            // ButtonElement.OnButtonClick = () => { Console.WriteLine("I was clicked!"); };
            // ButtonElement.IsMoveable = true;

            // GWindow = new GuiWindow((100, 80), GuiWindow.DecorationMode.Decorated);
            // GWindow.SetRelativePosition(0.5f, 0.5f);

            // GuiButton button = new GuiButton((50, 50), GuiElement.BottomLeft);
            // button.SetRelativePosition(0, 1);
            // GWindow.AddElement(button);

            // GenerateButton = new GuiButton((100, 20), GuiElement.Center);
            // GenerateButton.SetRelativePosition(0.2f, 0.2f);

            // GenerateButton.OnButtonClick = () => { World.DebugReset(); };

            // TestElement.AbsolutePosition = (0, 0);
            // Console.WriteLine((GlobalValues.WIDTH, GlobalValues.HEIGHT));
            // TestElement.Dimensions = (12, 12);
            // TestElement.Origin = (0.0f, 0.0f);
            // TestElement.RelativePosition = (0.0f, 0.0f);
            // TestContainer.TextureName = "Test.png";
            // TestContainer.TextureMode = TextureMode.Tile;
            // TestContainer.TileSize = 48;

            //TestContainer.Dimensions = (120, 80);
            // TestContainer.Origin = (0.5f, 0.5f);
            // TestContainer.RelativePosition = (0.5f, 0.5f);
            // TestContainer.Dimensions = (120, 80);
            // TestContainer.Color = Color3.Red;
            // TestContainer.IsVisible = true;
            // TestContainer.RelativePosition = (0.5f, 0.5f);
            GuiContainer container = new GuiContainer();
            container.Dimensions = (120, 120);
            container.RelativePosition = (0.5f, 0.5f);
            container.Origin = (0.5f, 0.5f);
            container.Color = Color3.White;

            GuiElement element = new GuiElement();
            element.Dimensions = (25, 25);
            element.Origin = (0, 0);
            element.RelativePosition = (0, 0);
            element.Color = Color3.Red;
            // element.IsVisible = true;

            container.AddElement(element);

            // container.IsVisible = true;

            // GuiContainer testContainer = new GuiContainer();
            // testContainer.Dimensions = (50, 50);
            // GuiElement testElement = new GuiElement();
            // testElement.Origin = (0.5f, 0.5f);
            // testElement.Dimensions = (25, 25);
            // testElement.RelativePosition = (0.5f, 0.5f);
            // testElement.Color = Color3.Red;
            // testContainer.AddElement(testElement);
            // TestContainer.AddElement(testContainer);

            // TestContainer.IsVisible = true;

            // Window = new GuiWindow((100, 80), GuiWindow.DecorationMode.Decorated);

            e = new Model(v, "missing.png", "billboard.vert", "billboard.frag");

            // FontLoader.LoadFont();

        }

        public static void Render()
        {

            Stopwatch sw = Stopwatch.StartNew();

            double frames = 60.0;
            if (TickTime >= 1 / frames)
            {

                TickTime -= 1 / frames;
                // Player.PhysicsUpdate(World);
                // tick
                // Console.WriteLine("tick");

                // Player.Update(World);
            }
            Player.Update(World);
            TickTime += GlobalValues.DeltaTime;

            frameBuffer.Bind();

            GL.PolygonOffset(0, 0);

            GL.ClearColor(new OpenTK.Mathematics.Color4<OpenTK.Mathematics.Rgba>(0, 0, 0, 1));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.Disable(EnableCap.DepthTest);
            Skybox.Draw(Player.Camera.Position, (new Vector4(0, -1, 0, 0) * Sun.RotationMatrix).Xyz, Player.Camera, (float)GlobalValues.Time);

            // GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            if (Input.IsKeyDown(Key.LeftControl))
            {

                if (Input.IsKeyPressed(Key.R))
                {

                    Input.CheckForController(0);

                }

                if (Input.IsKeyPressed(Key.S))
                {

                    if (World.WorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)].QueueType == QueueType.Done)
                    {

                        // World.WorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)].SaveToFile();
                        ChunkSerializer.Serialize(World.WorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)]);
                        ChunkSerializer.Deserialize(World.WorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)]);

                    }
                    else
                    {

                        Console.WriteLine($"The chunk at {ChunkUtils.PositionToChunk(Player.Camera.Position)} either is not used or is not ready.");

                    }

                }

                if (Input.IsKeyPressed(Key.W))
                {

                    if (World.WorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)].QueueType == QueueType.Done)
                    {

                        World.WorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)].TryLoad();

                    }

                }

                if (Input.IsKeyPressed(Key.C))
                {

                    Console.WriteLine("Reloading chunks for debug purposes");
                    World.DebugReset();

                }

                if (Input.IsKeyPressed(Key.F))
                {

                    Console.WriteLine("Toggling fog");
                    GlobalValues.ShouldRenderFog = !GlobalValues.ShouldRenderFog;

                }

                if (Input.IsKeyPressed(Key.B))
                {

                    Console.WriteLine("Toggling chunk bounds");
                    GlobalValues.ShouldRenderBounds = !GlobalValues.ShouldRenderBounds;

                }

                if (Input.IsKeyPressed(Key.M))
                {

                    GlobalValues.ShouldRenderWireframe = !GlobalValues.ShouldRenderWireframe;

                }

                if (Input.IsKeyPressed(Key.O))
                {

                    Console.WriteLine("Toggling AO");
                    GlobalValues.RenderAmbientOcclusion = !GlobalValues.RenderAmbientOcclusion;

                }

                // GlobalValues.FogOffset += (0.1f * GlobalValues.Mouse.ScrollDelta.Y);
                GlobalValues.FogOffset = Math.Clamp(GlobalValues.FogOffset, 0, 1);

            }

            World.Generate(Player.Position);
            World.Draw(Player.Camera);

            GL.Enable(EnableCap.DepthTest);

            if (debug)
            {

                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

            }

            Dda.TraceChunks(World.WorldChunks, Player.Camera.Position, Player.Camera.ForwardVector, GlobalValues.PlayerRange);

            if (debug)
            {

                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.DepthTest);

                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
                xyz_display.Draw(Player.Camera.Position + (Player.Camera.ForwardVector * 5), Vector3.Zero, Player.Camera, (float)time);
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
                GL.Enable(EnableCap.CullFace);
                GL.Enable(EnableCap.DepthTest);

            }

            TextRenderer.RenderText((4, 4, -100), TextRenderer.TopLeft, (1, 1, 1), 18, $"{GlobalValues.Phase} {GlobalValues.Version}");
            TextRenderer.RenderText((10, 10, -900), TextRenderer.TopLeft, (1,0,0), 18, $"{GlobalValues.Phase} {GlobalValues.Version}");

            if (Input.IsKeyPressed(Key.DownArrow)) GlobalValues.BlockSelectorID--;
            if (Input.IsKeyPressed(Key.UpArrow)) GlobalValues.BlockSelectorID++;
            if (Input.IsJoystickButtonPressed(JoystickButton.RightShoulder)) GlobalValues.BlockSelectorID++;
            if (Input.IsJoystickButtonPressed(JoystickButton.LeftShoulder)) GlobalValues.BlockSelectorID--;
            GlobalValues.BlockSelectorID = (ushort)((int)GlobalValues.BlockSelectorID + Input.ScrollDelta.Y);
            if (GlobalValues.BlockSelectorID > GlobalValues.Register.Blocks.Keys.Last()) GlobalValues.BlockSelectorID = (ushort) (GlobalValues.Register.Blocks.Keys.First() + 1);
            if (GlobalValues.BlockSelectorID < GlobalValues.Register.Blocks.Keys.First() + 1) GlobalValues.BlockSelectorID = GlobalValues.Register.Blocks.Keys.Last();

            // GL.Enable(EnableCap.DepthTest);
            // GL.Disable(EnableCap.DepthTest);
            GlobalValues.Register.GetBlockFromID(GlobalValues.BlockSelectorID).GuiRenderableBlockModel.Draw(GuiMaths.RelativeToAbsolute((1.0f, 0.5f, 0.0f)) - (50, 0, 50), 40, (float)GlobalValues.Time);
            // CachedFontRenderer.RenderFont(GuiMath.RelativeToAbsolute(0.5f, 0.5f), 0, 48.0f + (float)(30.0 * Math.Sin(GlobalValues.Time)), "", Path.Combine("Resources", "Fonts", "NotoSansJP-Regular.ttf"), Color3.Darkred);
            // CachedFontRenderer.RenderFont(GuiMath.RelativeToAbsolute(0.5f, 0.5f), 0, 48, "Hello World!", Path.Combine("Resources", "Fonts", "NotoSansJP-Regular.ttf"), Color3.Black);
            CachedFontRenderer.RenderFont(GuiMath.RelativeToAbsolute(0.5f, 1.0f) - (0, 20), (0.5f, 0.5f), 0, 24, $"{Math.Round(Player.Position.X, 2)}, {Math.Round(Player.Position.Y, 2)}, {Math.Round(Player.Position.Z, 2)}", Path.Combine("Resources", "Fonts", "NotoSansJP-Regular.ttf"), Color3.Black);
            CachedFontRenderer.RenderFont(GuiMath.RelativeToAbsolute(0.5f, 1.0f) - (0, 46), (0.5f, 0.5f), 0, 24, GlobalValues.Register.GetBlockFromID(GlobalValues.BlockSelectorID).DisplayName, Path.Combine("Resources", "Fonts", "NotoSansJP-Regular.ttf"), Color3.Black);

            foreach (GuiElement element in GlobalValues.GlobalGuiElements)
            {

                element.Draw();

            }

            frameBuffer.Unbind();

            //GL.ClearColor(new OpenTK.Mathematics.Color4<OpenTK.Mathematics.Rgba>(0, 0, 0, 1));
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            framebufferQuad.Draw(frameBuffer, (float)time);

            frameBuffer.Bind();
            GL.PolygonOffset(1, 1);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            // .Draw(Player.Camera);
            World.DrawRadius(Player, 1);
            frameBuffer.Unbind();
            if (Dda.hit)
            {

                float lineThickness = 12.0f;

                GL.FrontFace(FrontFaceDirection.Cw);
                rmodel.SetScale(1, 1, 1);
                // GL.PolygonOffset(-1, 1);
                GL.Disable(EnableCap.DepthTest);
                // LineRenderer.DrawLine((0, 0, 0) + Dda.PositionAtHit, (0, 1, 0) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                LineRenderer.Draw(frameBuffer, 4, Vector3.Zero, Player.Camera,
                    (0, 0, 0) + Dda.PositionAtHit, (0, 1, 0) + Dda.PositionAtHit,
                    (1, 0, 0) + Dda.PositionAtHit, (1, 1, 0) + Dda.PositionAtHit,
                    (0, 0, 1) + Dda.PositionAtHit, (0, 1, 1) + Dda.PositionAtHit,
                    (1, 0, 1) + Dda.PositionAtHit, (1, 1, 1) + Dda.PositionAtHit,
                    (0, 0, 0) + Dda.PositionAtHit, (1, 0, 0) + Dda.PositionAtHit,
                    (0, 1, 0) + Dda.PositionAtHit, (1, 1, 0) + Dda.PositionAtHit,
                    (0, 0, 1) + Dda.PositionAtHit, (1, 0, 1) + Dda.PositionAtHit,
                    (0, 1, 1) + Dda.PositionAtHit, (1, 1, 1) + Dda.PositionAtHit,
                    (1, 0, 0) + Dda.PositionAtHit, (1, 0, 1) + Dda.PositionAtHit,
                    (1, 1, 0) + Dda.PositionAtHit, (1, 1, 1) + Dda.PositionAtHit,
                    (0, 0, 0) + Dda.PositionAtHit, (0, 0, 1) + Dda.PositionAtHit,
                    (0, 1, 0) + Dda.PositionAtHit, (0, 1, 1) + Dda.PositionAtHit);
                // LineRenderer.DrawLine((1, 0, 0) + Dda.PositionAtHit, (1, 1, 0) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                // LineRenderer.DrawLine((0, 0, 1) + Dda.PositionAtHit, (0, 1, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                // LineRenderer.DrawLine((1, 0, 1) + Dda.PositionAtHit, (1, 1, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);

                // LineRenderer.DrawLine((0, 0, 0) + Dda.PositionAtHit, (1, 0, 0) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                // LineRenderer.DrawLine((0, 1, 0) + Dda.PositionAtHit, (1, 1, 0) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                // LineRenderer.DrawLine((0, 0, 1) + Dda.PositionAtHit, (1, 0, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                // LineRenderer.DrawLine((0, 1, 1) + Dda.PositionAtHit, (1, 1, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);

                // LineRenderer.DrawLine((1, 0, 0) + Dda.PositionAtHit, (1, 0, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                // LineRenderer.DrawLine((1, 1, 0) + Dda.PositionAtHit, (1, 1, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                // LineRenderer.DrawLine((0, 0, 0) + Dda.PositionAtHit, (0, 0, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                // LineRenderer.DrawLine((0, 1, 0) + Dda.PositionAtHit, (0, 1, 1) + Dda.PositionAtHit, lineThickness, (0, 0, 0), Player.Camera);
                GL.Enable(EnableCap.DepthTest);
                GL.FrontFace(FrontFaceDirection.Ccw);

            }
            // Console.WriteLine($"frametime (ms): {sw.ElapsedMilliseconds}");

        }

        public static void UpdateScreenSize(WindowResizeEventArgs args)
        {

            // Console.WriteLine($"Framebuffer size: {FramebufferSize} window size: {e.Width}. {e.Height}, size with e.Size: {e.Size}");

            // GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
            // Toolkit.Window.GetFramebufferSize(window, out int fw, out int fh);
            // Toolkit.Window.GetSize(window, out int w, out int h);

            // GL.Viewport(0, 0, fw, fh);
            // GL.Viewport(0, 0, args.NewClientSize.X, args.NewClientSize.Y);
            Toolkit.Window.GetFramebufferSize(args.Window, out Vector2i framebufferSize);
            GL.Viewport(0, 0, framebufferSize.X, framebufferSize.Y);
            // GlobalValues.WIDTH = e.Width;
            // GlobalValues.HEIGHT = e.Height;
            GlobalValues.WIDTH = args.NewClientSize.X;
            GlobalValues.HEIGHT = args.NewClientSize.Y;
            Console.WriteLine((GlobalValues.WIDTH, GlobalValues.HEIGHT));
            GlobalValues.Center = (GlobalValues.WIDTH / 2f, GlobalValues.HEIGHT / 2f);

            camera.UpdateProjectionMatrix();
            Player.Camera.UpdateProjectionMatrix();
            GlobalValues.GuiCamera.UpdateProjectionMatrix();

            foreach (GuiElement element in GlobalValues.GlobalGuiElements)
            {

                element.Recalculate();

            }

            // TestElement.RecalculatePosition();
            // TestContainer.Recalculate();
            // uiTest.OnScreenResize();
            // TestElement.Update();
            TextRenderer.Camera.UpdateProjectionMatrix();
            frameBuffer.UpdateAspect();

        }

        public static void Unload()
        {

            foreach (Chunk c in World.WorldChunks.Values)
            {

                c.SaveToFile();

            }

            // this portion is not required
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vbo);

        }

    }
}
