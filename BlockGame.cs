using Blockgame_OpenTK.Util;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.PlayerUtil;
using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.FramebufferUtil;
using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.Worlds;
using OpenTK.Mathematics;
using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using System.Diagnostics;
using Blockgame_OpenTK.Font;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using Blockgame_OpenTK.Audio;
using Blockgame_OpenTK.Core.TexturePack;
using Blockgame_OpenTK.BlockProperty;
using Tomlet;
using Blockgame_OpenTK.Core.Language;
using System.IO.Compression;

namespace Blockgame_OpenTK
{
    public class BlockGame
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


        static double time = 0;

        static Vector2 lmpos = new Vector2(0.0f, 0.0f);

        public static Model rmodel;
        // Chunk chunk;
        static Camera camera;

        static Model xyz_display;
        static Model hitdisplay;
        static Model Skybox;
        static Model e;

        static BoundingBox boundingbox = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(0.5f, 0.5f, 0.5f));
        static NakedModel boundmodel;
        // GUIElement TestElement;
        // GUIClickable GUIClick;

        // Ray ray = new Ray(0, 0, 0, 0, 0, 0);

        public static Framebuffer frameBuffer;
        static FramebufferQuad framebufferQuad;

        static ArrayTexture TextureArray = new ArrayTexture();

        static bool debug = false;
        // Chunk c;

        static Player Player;
        // GUIElement texx;

        static double TickTime = 0;

        static List<double> times = new List<double>();

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

        public struct Str
        {

            public int Hello = 16;
            public int hi = 32;

            public Str() {}

        }
        public static void Load()
        {

            int extensionCount = GL.GetInteger(GetPName.NumExtensions);
            List<string> requestedExtensions = [ "GL_ATI_meminfo", "GL_NVX_gpu_memory_info" ];
            for (int i = 0; i < extensionCount; i++)
            {
                string extension = GL.GetStringi(StringName.Extensions, (uint)i);
                if (requestedExtensions.Contains(extension)) GlobalValues.AvailableExtensions.Add(extension);
            }   
            Console.WriteLine("Requested extensions");
            foreach (string requestedExtension in requestedExtensions) Console.WriteLine($"\t{requestedExtension}");
            Console.WriteLine("Supported extensions");
            foreach (string supportedExtension in GlobalValues.AvailableExtensions) Console.WriteLine($"\t{supportedExtension}");

            GlobalValues.MissingTexturePackIcon = new Texture(Path.Combine("Resources", "Textures", "MissingIcon.png"));
            TexturePackManager.IterateAvailableTexturePacks();
            TexturePackManager.LoadTexturePack(TexturePackManager.AvailableTexturePacks["Archive"]);
            foreach (KeyValuePair<string, TexturePackInfo> texturePack in TexturePackManager.AvailableTexturePacks)
            {

                Console.WriteLine(texturePack.Value);

            }

            NewProperties prop = new NewProperties();
            using (DataWriter writer = DataWriter.Open("data.dat"))
            {

                prop.ToBytes(writer);

            }

            LanguageManager.LoadLanguage(Path.Combine("Resources", "Data", "Languages", "english_us.toml"));

            GLDebugProc debugMessageDel = OnDebugMessage;
            GuiRenderer.Init();
            AudioPlayer.Initialize();

            GameLogger.Log($"Platform: {RuntimeInformation.OSDescription}", Severity.Info);
            GameLogger.Log($"Architecture: {RuntimeInformation.OSArchitecture.ToString().ToLower()}", Severity.Info);
            GameLogger.Log($"Runtime: {RuntimeInformation.FrameworkDescription}", Severity.Info);
            GameLogger.Log($"Gpu Vendor: {GL.GetString(StringName.Vendor)}", Severity.Info);
            GameLogger.Log($"Gpu Renderer: {GL.GetString(StringName.Renderer)}", Severity.Info);
            GameLogger.Log($"OpenGL Version: {GL.GetString(StringName.Version)}", Severity.Info); 
            GameLogger.Log($"Max texture size: {GL.GetInteger(GetPName.MaxTextureSize)}", Severity.Info);
            GameLogger.Log($"Max array texture layers: {GL.GetInteger(GetPName.MaxArrayTextureLayers)}");

            if (!Directory.Exists("Chunks"))
            {

                GameLogger.Log("Created Chunks folder because it did not exist.", Severity.Info);
                Directory.CreateDirectory("Chunks");

            }

            Translator.LoadGameSettings();
            Input.Initialize();
            CachedFontRenderer.Init();

            int major = GL.GetInteger(GetPName.MajorVersion);
            int minor = GL.GetInteger(GetPName.MinorVersion);

            TextRenderer.InitTextRenderer();
            Translator.LoadKeymap();

            GlobalValues.GuiBlockShader = new Shader("guiblock.vert", "guiblock.frag");
            GlobalValues.ChunkShader = new Shader("chunk.vert", "chunk.frag");
            GlobalValues.DefaultShader = new Shader("default.vert", "default.frag");
            GlobalValues.GuiShader = new Shader("gui.vert", "gui.frag");
            GlobalValues.CachedFontShader = new Shader("cachedFont.vert", "cachedFont.frag");

            // camera = new Camera(cposition, cfront, cup, CameraType.Perspective, 45.0f);
            rmodel = new Model(verts, Path.Combine("Resources", "Textures", "hitbox.png"), "hitbox.vert", "hitbox.frag");
            hitdisplay = new Model(verts, Path.Combine("Resources", "Textures", "debug.png"), "model.vert", "model.frag");
            xyz_display = new Model(xyz_verts, null, "debug.vert", "debug.frag");

            boundmodel = new NakedModel(boundingbox.triangles);

            Skybox = new Model(verts, Path.Combine("Resources", "Textures", "cubemap", "cubemap_test.png"), "model.vert", "model.frag");

            xyz_display.SetScale(0.25f, 0.25f, 0.25f);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.CullFace);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Enable(EnableCap.PolygonOffsetFill);

            Texture t = new Texture(Path.Combine("Resources", "Textures", "cubemap", "cubemap_test.png"));

            frameBuffer = new Framebuffer();
            framebufferQuad = new FramebufferQuad();

            Player = new Player();
            Player.SetHeight(1.8f);
            Player.SetPosition((0, 68, 0));


            FontFamily font = new FontFamily();
            font.AddFontPath(Path.Combine("Resources", "Fonts", "LanaPixel", "LanaPixel.ttf"));
            CachedFontRenderer.FontFamily = font;
            
            e = new Model(v, null, "billboard.vert", "billboard.frag");

            // CachedFontRenderer.FontPath = Path.Combine("Resources", "Fonts", "alagard.ttf");

            GuiTextbox box = new GuiTextbox();
            box.AbsoluteDimensions = (500, 400);
            box.Origin = (0.5f, 0.5f);
            box.RelativePosition = (0.5f, 0.5f);
            // box.IsVisible = true;
            box.Text = "";
            box.Color = Color4.White;

            // WorldGenerator.InitThreads();
            GlobalValues.PackedChunkShader = new Shader("packedChunk.vert", "packedChunk.frag");
            // WorldGenerator.StartThreads(World);
            PackedWorldGenerator.CurrentWorld = new PackedChunkWorld();
            PackedWorldGenerator.Initialize();
            
            GlobalValues.GuiLineShader = new Shader("lines2.vert", "lines2.frag");
            
            NewBlock block = new NewBlock();
            Cube cube = new Cube();
            cube.Start = (0, 0, 0);
            cube.End = (32, 32, 32);
            cube.TopTextureName = "GrassBlockTop";
            cube.BottomTextureName = "GrassBlockTop";
            cube.RightTextureName = "GrassBlockTop";
            cube.LeftTextureName = "GrassBlockTop";
            cube.FrontTextureName = "GrassBlockTop";
            cube.BackTextureName = "GrassBlockTop";
            NewBlockModel blockModel = NewBlockModel.FromCubes([ cube ]);
            block.BlockModel = NewBlockModel.FromToml("grass_block.toml");
            GlobalValues.NewRegister.RegisterBlock(new Namespace("Game", "Air"), new NewBlock());
            GlobalValues.NewRegister.RegisterBlock(new Namespace("Game", "GrassBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "grass_block.toml")));
            GlobalValues.NewRegister.RegisterBlock(new Namespace("Game", "DirtBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "dirt_block.toml")));
            GlobalValues.NewRegister.RegisterBlock(new Namespace("Game", "StoneBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "stone_block.toml")));
            GlobalValues.NewRegister.RegisterBlock(new Namespace("Game", "BrickBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "bricks.toml")));
            GlobalValues.NewRegister.RegisterBlock(new Namespace("Game", "PlimboBlock"), NewBlock.FromToml<PlimboBlock>(Path.Combine("Resources", "Data", "Blocks", "plimbo_block.toml")));
            GlobalValues.NewRegister.RegisterBlock(new Namespace("Game", "EmptyCrate"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "empty_crate.toml")));
            GlobalValues.NewRegister.RegisterBlock(new Namespace("Game", "TomatoCrate"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "tomato_crate.toml")));
            GlobalValues.NewRegister.RegisterBlock(new Namespace("Game", "RedLightBlock"), NewBlock.FromToml<RedLightBlock>(Path.Combine("Resources", "Data", "Blocks", "red_light_block.toml")));
            GlobalValues.NewRegister.RegisterBlock(new Namespace("Game", "GreenLightBlock"), NewBlock.FromToml<GreenLightBlock>(Path.Combine("Resources", "Data", "Blocks", "green_light_block.toml")));
            GlobalValues.NewRegister.RegisterBlock(new Namespace("Game", "BlueLightBlock"), NewBlock.FromToml<BlueLightBlock>(Path.Combine("Resources", "Data", "Blocks", "blue_light_block.toml")));
            GlobalValues.NewRegister.RegisterBlock(new Namespace("Game", "LightBlock"), NewBlock.FromToml<LightBlock>(Path.Combine("Resources", "Data", "Blocks", "light_block.toml")));
            GlobalValues.NewRegister.RegisterBlock(new Namespace("Game", "LeafBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "leaf_block.toml")));

            Core.Gui.GuiRenderer.Initialize();

        }

        public static void Render()
        {

            Stopwatch sw = Stopwatch.StartNew();
            AudioPlayer.ListenerPosition = Player.Camera.Position;
            AudioPlayer.SetOrientation(Player.Camera.ForwardVector, Player.Camera.UpVector);

            double frames = 60.0;
            if (TickTime >= 1 / frames)
            {

                TickTime -= 1 / frames;
                

            }
            Player.Update(PackedWorldGenerator.CurrentWorld);
            TickTime += GlobalValues.DeltaTime;

            frameBuffer.Bind();

            GL.PolygonOffset(0, 0);

            GL.ClearColor(new OpenTK.Mathematics.Color4<OpenTK.Mathematics.Rgba>(0, 0, 0, 1));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.Disable(EnableCap.DepthTest);
            Skybox.Draw(Player.Camera.Position, (new Vector4(0, -1, 0, 0)).Xyz, Player.Camera, (float)GlobalValues.Time);

            // GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            if (Input.IsKeyDown(Key.LeftControl))
            {

                if (Input.IsKeyPressed(Key.R))
                {

                    // Input.CheckForController(0);
                    // GlobalValues.Toggle = !GlobalValues.Toggle;
                    CachedFontRenderer.FontPath = Path.Combine("Resources", "Fonts", "JetBrainsMono-Bold.ttf");

                }

                if (Input.IsKeyPressed(Key.S))
                {

                    

                }

                if (Input.IsKeyPressed(Key.W))
                {



                }

                if (Input.IsKeyPressed(Key.C))
                {

                    PackedWorldGenerator.CurrentWorld.PackedWorldChunks[ChunkUtils.PositionToChunk(Player.Camera.Position)].QueueType = PackedChunkQueueType.PassOne;

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

                if (Input.IsKeyPressed(Key.F1))
                {

                    GlobalValues.ShouldHideHud = !GlobalValues.ShouldHideHud;

                }

                GlobalValues.FogOffset = Math.Clamp(GlobalValues.FogOffset, 0, 1);

            }

            PackedWorldGenerator.Tick(Player);
            PackedWorldGenerator.QueueGeneration();

            string gpuUsage = "err";
            if (GlobalValues.AvailableExtensions.Contains("GL_NVX_gpu_memory_info"))
            {
                gpuUsage = Math.Round((GL.GetInteger((GetPName)All.GpuMemoryInfoDedicatedVidmemNvx) - GL.GetInteger((GetPName)All.GpuMemoryInfoCurrentAvailableVidmemNvx)) / 1024.0 / 1024.0, 2).ToString();
            }
            
            CachedFontRenderer.RenderFont(out var _, (40, 40), (0, 0), 0, 24, $"{Math.Floor(Player.Position.X)}, {Math.Floor(Player.Position.Y)}, {Math.Floor(Player.Position.Z)}", Color4.Black);
            CachedFontRenderer.RenderFont(out var _, (40, 40 + 50), (0, 0), 0, 24, $"{ChunkUtils.PositionToChunk(Player.Position)}", Color4.Black);
            CachedFontRenderer.RenderFont(out var _, (40, 40 + 50 + 50), (0, 0), 0, 24, $"Usage (GB): {Math.Round(Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0 / 1024.0, 2)}", Color4.White);
            CachedFontRenderer.RenderFont(out var _, (40, 40 + 50 + 50 + 50), (0, 0), 0, 24, $"GPU Usage (GB): {gpuUsage}", Color4.White);

            GL.Enable(EnableCap.DepthTest);

            if (debug)
            {

                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

            }

            Dda.TraceChunks(PackedWorldGenerator.CurrentWorld.WorldColumns, Player.Camera.Position, Player.Camera.ForwardVector, GlobalValues.PlayerRange);

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

            // TextRenderer.RenderText((4, 4, -100), TextRenderer.TopLeft, (1, 1, 1), 18, $"{GlobalValues.Phase} {GlobalValues.Version}");
            // TextRenderer.RenderText((10, 10, -900), TextRenderer.TopLeft, (1,0,0), 18, $"{GlobalValues.Phase} {GlobalValues.Version}");

            if (Input.IsKeyPressed(Key.DownArrow)) GlobalValues.BlockSelectorID--;
            if (Input.IsKeyPressed(Key.UpArrow)) GlobalValues.BlockSelectorID++;
            if (Input.IsJoystickButtonPressed(JoystickButton.RightShoulder)) GlobalValues.BlockSelectorID++;
            if (Input.IsJoystickButtonPressed(JoystickButton.LeftShoulder)) GlobalValues.BlockSelectorID--;
            GlobalValues.BlockSelectorID = (ushort)((int)GlobalValues.BlockSelectorID + Input.ScrollDelta.Y);
            if (GlobalValues.BlockSelectorID > GlobalValues.NewRegister.BlockCount - 1) GlobalValues.BlockSelectorID = (ushort) 1;
            if (GlobalValues.BlockSelectorID <= 0) GlobalValues.BlockSelectorID = (ushort) (GlobalValues.NewRegister.BlockCount - 1);
            CachedFontRenderer.RenderFont(out (Vector2, float, float) c, GuiMath.RelativeToAbsolute(0.5f, 0.8f), (0.5f, 0), 100, 48, $"Currently holding ![w,5,2,5,2,0.25]![g,{GlobalValues.NewRegister.GetBlockFromId(GlobalValues.BlockSelectorID).DisplayName.Length},0x8004d9FF,0x533c63FF]{GlobalValues.NewRegister.GetBlockFromId(GlobalValues.BlockSelectorID).DisplayName}");
            // GL.Enable(EnableCap.DepthTest);
            // GL.Disable(EnableCap.DepthTest);
            // GlobalValues.Register.GetBlockFromID(GlobalValues.BlockSelectorID).GuiRenderableBlockModel.Draw(GuiMaths.RelativeToAbsolute((1.0f, 0.5f, 0.0f)) - (50, 0, 50), 40, (float)GlobalValues.Time);
            // CachedFontRenderer.RenderFont(GuiMath.RelativeToAbsolute(0.5f, 0.5f), 0, 48.0f + (float)(30.0 * Math.Sin(GlobalValues.Time)), "", Path.Combine("Resources", "Fonts", "NotoSansJP-Regular.ttf"), Color3.Darkred);
            // CachedFontRenderer.RenderFont(GuiMath.RelativeToAbsolute(0.5f, 0.5f), 0, 48, "Hello World!", Path.Combine("Resources", "Fonts", "NotoSansJP-Regular.ttf"), Color3.Black);
            // GL.Disable(EnableCap.ScissorTest);
            // GL.Enable(EnableCap.DepthTest);

            if (Dda.hit && !GlobalValues.ShouldHideHud)
            {

                GL.Disable(EnableCap.CullFace);
                GL.PolygonOffset(-1, 1);
                rmodel.SetScale(1, 1, 1);
                rmodel.Draw(Dda.PositionAtHit + new Vector3(0.5f, 0.5f, 0.5f), (0, 0, 0), Player.Camera, (float) GlobalValues.Time);
                GL.PolygonOffset(1, 1);
                GL.Enable(EnableCap.CullFace);

            }

            frameBuffer.Unbind();

            framebufferQuad.Draw(frameBuffer, (float)time);

            GL.Clear(ClearBufferMask.DepthBufferBit);

            AudioPlayer.Poll();

            Core.Gui.GuiRenderer.GuiBegin();
            Core.Gui.GuiRenderer.RenderElement(0, GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time), (float)Math.Cos(GlobalValues.Time)) / 2), (50, 50), Vector2.Zero);
            Core.Gui.GuiRenderer.RenderElement(0, GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time + 0.25), (float)Math.Cos(GlobalValues.Time + 0.25)) / 2), (50, 50), Vector2.Zero, Color4.Bisque);
            Core.Gui.GuiRenderer.RenderElement(0, GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time + 0.1), (float)Math.Cos(GlobalValues.Time + 0.1)) / 2), (50, 50), Vector2.Zero, Color4.Purple);
            Core.Gui.GuiRenderer.GuiEnd();

            /*
            foreach (GuiElement element in GuiRenderer.Elements)
            {

                element.Draw();

            }
            */

            // CachedFontRenderer.RenderFont(out Tuple<Vector2, float, float> cursorParams1, GuiMath.RelativeToAbsolute(0.5f, 1.0f) - (0, 20), (0.5f, 0.5f), 1, 24, $"{Math.Round(Player.Position.X, 2)}, {Math.Round(Player.Position.Y, 2)}, {Math.Round(Player.Position.Z, 2)}", Color4.Black);
            // CachedFontRenderer.RenderFont(out Tuple<Vector2, float, float> cursorParams2, GuiMath.RelativeToAbsolute(0.5f, 1.0f) - (0, 46), (0.5f, 0.5f), 1, 24, GlobalValues.Register.GetBlockFromID(GlobalValues.BlockSelectorID).DisplayName, Color4.Black);

            /*CachedFontRenderer.RenderFont(GuiMath.RelativeToAbsolute(0.5f, 0.5f) - (0, 80), (0.5f, 0.5f), 1, 24, """
                This is
                A raw string literal.
                Which means I don't have to 
                ![i]worry about adding any newline![d]
                ![g,20,0xFFFFFFFF,0xAA00FFFF]![w,0,0,2,4,0.5]This!! is a gradient![d]
                characters!!
                    This is a tab
                """);
            */
            // GL.Disable(EnableCap.DepthTest);
            // GL.Enable(EnableCap.ScissorTest);

        }

        public static void UpdateScreenSize(WindowResizeEventArgs args)
        {

            Toolkit.Window.GetFramebufferSize(args.Window, out Vector2i framebufferSize);
            GL.Viewport(0, 0, framebufferSize.X, framebufferSize.Y);
            GlobalValues.WIDTH = args.NewClientSize.X;
            GlobalValues.HEIGHT = args.NewClientSize.Y;
            // Console.WriteLine((GlobalValues.WIDTH, GlobalValues.HEIGHT));
            GlobalValues.Center = (GlobalValues.WIDTH / 2f, GlobalValues.HEIGHT / 2f);

            Player.Camera.UpdateProjectionMatrix();
            GlobalValues.GuiCamera.UpdateProjectionMatrix();
            GuiRenderer.GuiCamera.UpdateProjectionMatrix();
            Core.Gui.GuiRenderer.UpdateProjectionMatrix();

            foreach (GuiElement element in GlobalValues.GlobalGuiElements)
            {

                // element.Recalculate();

            }

            TextRenderer.Camera.UpdateProjectionMatrix();
            frameBuffer.UpdateAspect();

            Render();

        }

        public static void Unload()
        {

            /*
            foreach (Chunk c in World.WorldChunks.Values)
            {

                c.SaveToFile();

            }
            */

            TexturePackManager.Free();

            // this portion is not required

        }

    }
}
