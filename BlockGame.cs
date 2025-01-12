﻿using Blockgame_OpenTK.Util;
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
using System.Collections.Generic;
using Blockgame_OpenTK.BlockProperty;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

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
        // GUIElement texx;

        static World World = new World("nofile");

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
        public static void Load()
        {

            GLDebugProc debugMessageDel = OnDebugMessage;
            GuiRenderer.Init();

            // GL.DebugMessageCallback(debugMessageDel, IntPtr.Zero);
            // GL.Enable(EnableCap.DebugOutput);

            Util.Debugger.Log($"Platform: {RuntimeInformation.OSDescription}", Severity.Info);
            Util.Debugger.Log($"Architecture: {RuntimeInformation.OSArchitecture.ToString().ToLower()}", Severity.Info);
            Util.Debugger.Log($"Runtime: {RuntimeInformation.FrameworkDescription}", Severity.Info);
            Util.Debugger.Log($"Gpu Vendor: {GL.GetString(StringName.Vendor)}", Severity.Info);
            Util.Debugger.Log($"Gpu Renderer: {GL.GetString(StringName.Renderer)}", Severity.Info);
            Util.Debugger.Log($"OpenGL Version: {GL.GetString(StringName.Version)}", Severity.Info); 

            if (!Directory.Exists("Chunks"))
            {

                Util.Debugger.Log("Created Chunks folder because it did not exist.", Severity.Info);
                Directory.CreateDirectory("Chunks");

            }

            Translator.LoadGameSettings();
            Input.Initialize();
            CachedFontRenderer.Init();

            int major = GL.GetInteger(GetPName.MajorVersion);
            int minor = GL.GetInteger(GetPName.MinorVersion);

            TextRenderer.InitTextRenderer();
            Translator.LoadKeymap();

            GlobalValues.ArrayTexture.Load();
            Blocks.Load();

            GlobalValues.MissingBlockModel = BlockModel.LoadFromJson("MissingModel.json");
            GlobalValues.GuiBlockShader = new Shader("guiblock.vert", "guiblock.frag");
            GlobalValues.ChunkShader = new Shader("chunk.vert", "chunk.frag");
            GlobalValues.DefaultShader = new Shader("default.vert", "default.frag");
            GlobalValues.GuiShader = new Shader("gui.vert", "gui.frag");
            GlobalValues.CachedFontShader = new Shader("cachedFont.vert", "cachedFont.frag");

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
            GL.Enable(EnableCap.CullFace);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Enable(EnableCap.PolygonOffsetFill);

            Texture t = new Texture("cubemap/cubemap_test.png");

            ChunkShader = new Shader("chunk.vert", "chunk.frag");

            frameBuffer = new Framebuffer();
            framebufferQuad = new FramebufferQuad();

            Sun = new Sun("sun.png", 10);

            Player = new Player();
            Player.SetHeight(1.8f);
            Player.SetPosition((16, 68, 16));

            GuiElement element = new GuiElement();
            element.Origin = (0.5f, 0.5f);
            element.Color = Color4.White;
            element.AbsoluteDimensions = (50, 50);
            element.Layer = 10;
            // element.IsVisible = true;

            TransitionElement transition = new TransitionElement(element);
            transition.Length = 2.0f;
            transition.StartingRelativePosition = (0.2f, 0.5f);
            transition.EndingRelativePosition = (0.5f, 0.5f);
            transition.StartingAbsoluteDimensions = (50, 50);
            transition.EndingAbsoluteDimensions = (50, 100);
            transition.StartingColor = Color4.White;
            transition.EndingColor = new Color4<Rgba>(1,1,1, 0);
            transition.InterpolationMode = InterpolationMode.EaseOutCubic;
            // transition.IsRunning = true;
            transition.ShouldLoop = true;

            GuiRandomTextDisplay textDisplay = new GuiRandomTextDisplay();
            textDisplay.AbsoluteDimensions = (50, 50);
            textDisplay.RelativePosition = (0.5f, 0.5f);
            textDisplay.Origin = (0.5f, 0.5f);
            // textDisplay.Color = Color4.White;
            textDisplay.Layer = 1;
            textDisplay.TickDelayInSeconds = 0.0f;
            textDisplay.TimeInSeconds = 2.0f;
            textDisplay.Text = "Pale_Gray";
            // textDisplay.IsVisible = true;

            e = new Model(v, "missing.png", "billboard.vert", "billboard.frag");

            CachedFontRenderer.FontPath = Path.Combine("Resources", "Fonts", "NotoSansKR-Regular.ttf");

            GuiTextbox box = new GuiTextbox();
            box.AbsoluteDimensions = (500, 400);
            box.Origin = (0.5f, 0.5f);
            box.RelativePosition = (0.5f, 0.5f);
            box.IsVisible = true;
            box.Text = "";
            box.Color = Color4.White;

            WorldGenerator.InitThreads();
            WorldGenerator.StartThreads(World);

        }

        public static void Render()
        {

            Stopwatch sw = Stopwatch.StartNew();

            double frames = 60.0;
            if (TickTime >= 1 / frames)
            {

                TickTime -= 1 / frames;
                World.Tick(Player.Position);

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

                    // Input.CheckForController(0);
                    // GlobalValues.Toggle = !GlobalValues.Toggle;
                    CachedFontRenderer.FontPath = Path.Combine("Resources", "Fonts", "JetBrainsMono-Bold.ttf");

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

                    // Console.WriteLine("Reloading chunks for debug purposes");
                    // World.DebugReset();

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
            // GL.Disable(EnableCap.ScissorTest);
            // GL.Enable(EnableCap.DepthTest);

            BitConverter.ToUInt32(SHA256.HashData(Encoding.UTF8.GetBytes("mywonderfulnamespace")));

            if (Dda.hit && !GlobalValues.ShouldHideHud)
            {

                float lineThickness = 12.0f;

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

            foreach (GuiElement element in GuiRenderer.Elements)
            {

                element.Draw();

            }

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

            camera.UpdateProjectionMatrix();
            Player.Camera.UpdateProjectionMatrix();
            GlobalValues.GuiCamera.UpdateProjectionMatrix();
            GuiRenderer.GuiCamera.UpdateProjectionMatrix();

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
