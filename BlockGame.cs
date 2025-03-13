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
using Blockgame_OpenTK.Core.Language;
using Blockgame_OpenTK.Core.Gui;
using System.Drawing;
using Blockgame_OpenTK.Core.Networking;
using Blockgame_OpenTK.Core.PlayerUtil;
using LiteNetLib.Utils;

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

        static NewPlayer Player;
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

        public static void LoadResources()
        {

            Translator.LoadGameSettings();
            GlobalValues.Base.OnLoad(GlobalValues.Register);

        }
        
        public static void Load()
        {

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

            // Player = new Player();
            // Player.SetHeight(1.8f);
            // Player.SetPosition((0, 68, 0));
            Player = new NewPlayer() { Camera = new PlayerCamera(90.0f) };
            Console.WriteLine(Player.Camera.FieldOfView);

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
            // PackedWorldGenerator.CurrentWorld = NetworkingValues.Client.World;
            // PackedWorldGenerator.Initialize();
            
            GlobalValues.GuiLineShader = new Shader("lines2.vert", "lines2.frag");

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
            // TexturePackManager.IterateAvailableTexturePacks();
            // TexturePackManager.LoadTexturePack(TexturePackManager.AvailableTexturePacks["Archive"]);
            // foreach (KeyValuePair<string, TexturePackInfo> texturePack in TexturePackManager.AvailableTexturePacks)
            // {
// 
            //     Console.WriteLine(texturePack.Value);
// 
            // }

            NewProperties prop = new NewProperties();
            using (DataWriter writer = DataWriter.OpenFile("data.dat"))
            {

                prop.ToBytes(writer);

            }

            GLDebugProc debugMessageDel = OnDebugMessage;
            GuiRendere.Init();
            AudioPlayer.Initialize();

            CachedFontRenderer.Init();
            GuiRenderer.Initialize();
            FontRenderer.Initialize();

            TexturePackManager.IterateAvailableTexturePacks();
            TexturePackManager.LoadTexturePack(TexturePackManager.AvailableTexturePacks["Archive"]);


            LanguageManager.LoadLanguage(Path.Combine("Resources", "Data", "Languages", "english_us.toml"));

            // GameLogger.Log($"Platform: {RuntimeInformation.OSDescription}", Severity.Info);
            // GameLogger.Log($"Architecture: {RuntimeInformation.OSArchitecture.ToString().ToLower()}", Severity.Info);
            // GameLogger.Log($"Runtime: {RuntimeInformation.FrameworkDescription}", Severity.Info);
            // GameLogger.Log($"Gpu Vendor: {GL.GetString(StringName.Vendor)}", Severity.Info);
            // GameLogger.Log($"Gpu Renderer: {GL.GetString(StringName.Renderer)}", Severity.Info);
            // GameLogger.Log($"OpenGL Version: {GL.GetString(StringName.Version)}", Severity.Info); 
            // GameLogger.Log($"Max texture size: {GL.GetInteger(GetPName.MaxTextureSize)}", Severity.Info);
            // GameLogger.Log($"Max array texture layers: {GL.GetInteger(GetPName.MaxArrayTextureLayers)}");

            if (!Directory.Exists("Chunks"))
            {

                GameLogger.Log("Created Chunks folder because it did not exist.", Severity.Info);
                Directory.CreateDirectory("Chunks");

            }

            Translator.LoadGameSettings();

            GameLogger.Log("loading blocks");

            // GlobalValues.Register.RegisterBlock(new Namespace("Game", "Air"), new NewBlock());
            // GlobalValues.Register.RegisterBlock(new Namespace("Game", "GrassBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "grass_block.toml")));
            // GlobalValues.Register.RegisterBlock(new Namespace("Game", "DirtBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "dirt_block.toml")));
            // GlobalValues.Register.RegisterBlock(new Namespace("Game", "StoneBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "stone_block.toml")));
            // GlobalValues.Register.RegisterBlock(new Namespace("Game", "BrickBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "bricks.toml")));
            // GlobalValues.Register.RegisterBlock(new Namespace("Game", "PlimboBlock"), NewBlock.FromToml<PlimboBlock>(Path.Combine("Resources", "Data", "Blocks", "plimbo_block.toml")));
            // GlobalValues.Register.RegisterBlock(new Namespace("Game", "EmptyCrate"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "empty_crate.toml")));
            // GlobalValues.Register.RegisterBlock(new Namespace("Game", "TomatoCrate"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "tomato_crate.toml")));
            // GlobalValues.Register.RegisterBlock(new Namespace("Game", "RedLightBlock"), NewBlock.FromToml<RedLightBlock>(Path.Combine("Resources", "Data", "Blocks", "red_light_block.toml")));
            // GlobalValues.Register.RegisterBlock(new Namespace("Game", "GreenLightBlock"), NewBlock.FromToml<GreenLightBlock>(Path.Combine("Resources", "Data", "Blocks", "green_light_block.toml")));
            // GlobalValues.Register.RegisterBlock(new Namespace("Game", "BlueLightBlock"), NewBlock.FromToml<BlueLightBlock>(Path.Combine("Resources", "Data", "Blocks", "blue_light_block.toml")));
            // GlobalValues.Register.RegisterBlock(new Namespace("Game", "LightBlock"), NewBlock.FromToml<LightBlock>(Path.Combine("Resources", "Data", "Blocks", "light_block.toml")));
            // GlobalValues.Register.RegisterBlock(new Namespace("Game", "LeafBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "leaf_block.toml")));


        }

        public static void Render()
        {

            // TODO: NO CLIENT CALLS SHOULD BE IN THIS
            // INCLUDING PLAYER IN PUT AND RESPONSE
            // ONLY RENDER CODE

            // TODO: maybe render calls actually just are separately implemented?

            // NetworkingValues.Client.Update();

            // Player.UpdateInputs();
            // Console.WriteLine(Player.Camera.ForwardVector);

            // AudioPlayer.ListenerPosition = Player.Camera.Position;
            // AudioPlayer.SetOrientation(Player.Camera.ForwardVector, Player.Camera.UpVector);

            // Player.Update(PackedWorldGenerator.CurrentWorld);
            TickTime += GlobalValues.DeltaTime;

            frameBuffer.Bind();

            GL.PolygonOffset(0, 0);

            GL.ClearColor(new OpenTK.Mathematics.Color4<OpenTK.Mathematics.Rgba>(0, 0, 0, 1));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.Disable(EnableCap.DepthTest);
            // Skybox.Draw(Player.Camera.Position, (new Vector4(0, -1, 0, 0)).Xyz, Player.Camera, (float)GlobalValues.Time);
            GL.Enable(EnableCap.DepthTest);

            // PackedWorldGenerator.Poll();
            // NetworkingValues.Client.World.Draw(Player);
            // PackedWorldGenerator.QueueGeneration();

            string gpuUsage = "err";
            if (GlobalValues.AvailableExtensions.Contains("GL_NVX_gpu_memory_info"))
            {
                gpuUsage = Math.Round((GL.GetInteger((GetPName)All.GpuMemoryInfoDedicatedVidmemNvx) - GL.GetInteger((GetPName)All.GpuMemoryInfoCurrentAvailableVidmemNvx)) / 1024.0 / 1024.0, 2).ToString();
            }

            GL.Enable(EnableCap.DepthTest);

            if (debug)
            {

                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

            }

            // Dda.TraceChunks(PackedWorldGenerator.CurrentWorld.WorldColumns, Player.Camera.Position, Player.Camera.ForwardVector, GlobalValues.PlayerRange);

            frameBuffer.Unbind();

            framebufferQuad.Draw(frameBuffer, (float)time);

            GL.Clear(ClearBufferMask.DepthBufferBit);

            GuiRenderer.Begin("thing");
            GuiRenderer.RenderElement(GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time), (float)Math.Cos(GlobalValues.Time)) / 2), (50, 50), (0.5f, 0.5f));
            GuiRenderer.RenderElement(GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time + 0.1), (float)Math.Cos(GlobalValues.Time + 0.1)) / 2), (50, 50), (0.5f, 0.5f), Color4.Bisque);
            GuiRenderer.RenderElement(GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time + 0.25), (float)Math.Cos(GlobalValues.Time + 0.25)) / 2), (50, 50), (0.5f, 0.5f), Color4.Purple);
            GuiRenderer.RenderTextbox(GuiMath.RelativeToAbsolute(0.5f, 0.4f), new Vector2i(200, 24), (0.5f, 0.5f), "Address", out string addressString, Color4.White);
            GuiRenderer.RenderTextbox(GuiMath.RelativeToAbsolute(0.5f, 0.6f), (200, 24), (0.5f, 0.5f), "User Id", out string userIdString, Color4.White);
            if (GuiRenderer.RenderButton(GuiMath.RelativeToAbsolute(0.5f, 0.7f), (150, 24), (0.5f, 0.5f), "Join Server", Color4.White))
            {

                string[] network = addressString.Split(':');

                if (!int.TryParse(network[1], out int port))
                {
                    GameLogger.Log("port is invalid");
                } else if (!long.TryParse(userIdString, out long uid))
                {
                    GameLogger.Log("uid is invalid");
                } else
                {
                    // NetworkingValues.Client = new PhysicalClient();
                    // NetworkingValues.Client.Start(network[0], port, new NewPlayer() { UserId = uid, DisplayName = "Poo" });
                }
                // NetworkingValues.Client.JoinWorld(network[0], int.Parse(network[1]), long.Parse(userIdString));

            }
            GuiRenderer.End();

            // if (Input.IsKeyPressed(Key.H)) FontRenderer.ClearCache();

            AudioPlayer.Poll();

        }

        public static void UpdateScreenSize(WindowResizeEventArgs args)
        {

            Toolkit.Window.GetFramebufferSize(args.Window, out Vector2i framebufferSize);
            GL.Viewport(0, 0, framebufferSize.X, framebufferSize.Y);
            GlobalValues.Width = args.NewClientSize.X;
            GlobalValues.Height = args.NewClientSize.Y;
            // Console.WriteLine((GlobalValues.WIDTH, GlobalValues.HEIGHT));
            GlobalValues.Center = (GlobalValues.Width / 2f, GlobalValues.Height / 2f);

            Player.Camera.UpdateProjectionMatrix();
            GlobalValues.GuiCamera.UpdateProjectionMatrix();
            GuiRendere.GuiCamera.UpdateProjectionMatrix();
            GuiRenderer.UpdateProjectionMatrix();
            FontRenderer.Update();

            TextRenderer.Camera.UpdateProjectionMatrix();
            frameBuffer.UpdateAspect();

            // this shitty call
            // Render();

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
            FontRenderer.Free();

            // this portion is not required

        }

    }
}
