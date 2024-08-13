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
using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.PlayerUtil;
using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.FramebufferUtil;
using Blockgame_OpenTK.BlockUtil;
using OpenTK.Windowing.Common.Input;
using Image = OpenTK.Windowing.Common.Input.Image;
using System.Linq;


namespace Blockgame_OpenTK
{
    internal class Game : GameWindow
    {
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title, Flags = ContextFlags.Debug}) { }

        public static float[] verts = {

            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, // front
             0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

            0.5f, -0.5f, -0.5f,  0.0f, 0.0f, // right
            0.5f, -0.5f, 0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, 0.5f,  1.0f, 1.0f,
            0.5f,  0.5f, 0.5f,  1.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

            0.5f, -0.5f, 0.5f, 0.0f, 0.0f, // back 
            -0.5f, -0.5f, 0.5f, 1.0f, 0.0f,
            -0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            -0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 0.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 0.0f, 0.0f,

            -0.5f, -0.5f, 0.5f,  0.0f, 0.0f, // left
            -0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, 0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, 0.5f,  0.0f, 0.0f,

            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, // top
            0.5f, 0.5f, -0.5f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            -0.5f, 0.5f, 0.5f, 0.0f, 1.0f,
            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f,

            0.5f, -0.5f, -0.5f, 0.0f, 0.0f, // bottom
            -0.5f, -0.5f, -0.5f, 1.0f, 0.0f,
            -0.5f, -0.5f, 0.5f, 1.0f, 1.0f,
            -0.5f, -0.5f, 0.5f, 1.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 0.0f, 1.0f,
            0.5f, -0.5f, -0.5f, 0.0f, 0.0f

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

        Model rmodel;
        // Chunk chunk;
        Camera camera;

        Model xyz_display;
        Model hitdisplay;
        Model Skybox;
        // Model Sun;

        NakedModel nakedmodel;

        BoundingBox boundingbox = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(0.5f, 0.5f, 0.5f));
        NakedModel boundmodel;
        // GUIElement TestElement;
        // GUIClickable GUIClick;
        TextRenderer text;

        Ray ray = new Ray(0, 0, 0, 0, 0, 0);

        public Framebuffer frameBuffer;
        FramebufferQuad framebufferQuad;

        ArrayTexture TextureArray = new ArrayTexture();

        bool debug = false;

        bool IsGrabbed = true;

        double ft = 0;
        double fs = 0;
        // Chunk c;
        NewChunk nc = new NewChunk((0,0,0));
        Sun Sun;

        Player Player;
        // GUIElement texx;

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

            Globals.DeltaTime = args.Time;
            Globals.Time += Globals.DeltaTime;
            Globals.Mouse = MouseState;
            Globals.Keyboard = KeyboardState;

            // Console.WriteLine($"Mouse Delta X: {Globals.Mouse.Delta.X}, Computed mouse delta X: {Globals.Mouse.X - Globals.Mouse.PreviousX}");
            // Console.WriteLine(UInt32.MaxValue);
            
            if (Globals.Keyboard.IsKeyPressed(Keys.Escape))
            { 

                if (!IsGrabbed)
                {

                    IsGrabbed = true;
                    CursorState = CursorState.Grabbed;
                        
                }
                else { IsGrabbed = false; CursorState = CursorState.Normal; MousePosition = Globals.Center;  }


            }
            if (Globals.Keyboard.IsKeyPressed(Keys.F1))
            {

                if (!debug)
                {

                    debug = true;

                }
                else { debug = false; }


            }
            Globals.CursorState = CursorState;

        }

        GuiElement uiTest;
        protected override void OnLoad()
        {

            base.OnLoad();

            TextRenderer.InitTextRenderer();

            // Texture iconTexture = new Texture("icon.png", 0);
            // Icon = new WindowIcon(new Image(32, 32, iconTexture.Data));

            // Console.WriteLine($"Max array texture layers: {GL.GetInteger(GetPName.MaxArrayTextureLayers)}, Max texture 2d size: {GL.GetInteger(GetPName.MaxTextureSize)}");
            // GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
            // GL.Enable(EnableCap.DebugOutput);

            // TextureArray.Load();
            //BlockModel bm = new BlockModel();
            // BlockModel bm = BlockModel.Load("GrassBlockNew");
            // Console.WriteLine(bm.BlockFaces.Count);
            Console.WriteLine(GL.GetInteger(GetPName.MajorVersion) + "." + GL.GetInteger(GetPName.MinorVersion));

            Globals.ArrayTexture.Load();
            Blocks.Load();
            // Blocks.GetBlockFromName("RandomBlock");

            // original: 8. printed: 1
            // Console.WriteLine(Convert.ToInt16(0b1000 >> 3 & 0b1111));
            // binary shift right three and cmp to full mask
            Globals.AtlasTexture = new TextureAtlas("atlas.png", 32);
            Globals.ChunkShader = new Shader("chunk.vert", "chunk.frag");
            Globals.DefaultShader = new Shader("default.vert", "default.frag");
            Globals.GuiShader = new Shader("gui.vert", "gui.frag");
            Globals.Mouse = MouseState;
            Globals.Keyboard = KeyboardState;
            CursorState = CursorState.Grabbed;

            BinaryWriter bw = new BinaryWriter(File.Open("../../../Resources/cdat/1.cdat", FileMode.OpenOrCreate));

            bw.Write((uint)500);

            camera = new Camera(cposition, cfront, cup, CameraType.Perspective, 45.0f);
            rmodel = new Model(verts, "hitbox.png", "hitbox.vert", "hitbox.frag");
            hitdisplay = new Model(verts, "debug.png", "model.vert", "model.frag");
            xyz_display = new Model(xyz_verts, null, "debug.vert", "debug.frag");

            nakedmodel = new NakedModel(NakedModel.Tri);

            boundmodel = new NakedModel(boundingbox.triangles);

            Skybox = new Model(skybox, "cubemap/cubemap_test.png", "model.vert", "model.frag");

            xyz_display.SetScale(0.25f, 0.25f, 0.25f);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Enable(EnableCap.DepthTest);
            // GL.DepthFunc(DepthFunction.Equal);
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
            Player.SetPosition((0, 0, 0));

            byte[] dataExample = new byte[] { 255, 220 };

            Console.WriteLine(TextureLoader.Sub(dataExample, 1));

            Blockgame_OpenTK.Util.Image Image = Blockgame_OpenTK.Util.Image.LoadPng("../../../Resources/Textures/skybox.png", true);
            Console.WriteLine($"width: {Image.Width}, height: {Image.Height}");

            // byte[] textureData = File.ReadAllBytes("../../../Resources/Textures/TextureArray/DirtBlock.png");
            // textureData = TextureLoader.DecompressPng(textureData);
            // textureData = TextureLoader.Flip(textureData);

            // texx = new GUIElement(50, 50, 120, 120, OriginType.Center, new Texture(Image.ImageData, Image.Width, Image.Height), GUIElement.Null);

            uiTest = new GuiElement((8, 8), GuiElement.Center);
            uiTest.SetRelativePosition(0.5f, 0.5f);
            uiTest.Rotate(45);

        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {

            base.OnRenderFrame(args);

            Player.Update();

            Stopwatch sw = Stopwatch.StartNew();

            frameBuffer.Bind();

            GL.ClearColor(1,1,1,1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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
            Vector3 rotation = ((float)Globals.Time / 5f, 0, 0);
            rotation = Vector3.Zero;
            // Console.WriteLine(Vector3.Dot((0, 1, 0), (new Vector4(0, -1, 0, 0) * Sun.RotationMatrix).Xyz));
            Sun.SetRotation(rotation);
            Skybox.Draw(Player.Camera.Position, (new Vector4(0, -1, 0, 0) * Sun.RotationMatrix).Xyz, Player.Camera, (float)Globals.Time);
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

            // texx.Draw();

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            if (Globals.Keyboard.IsKeyDown(Keys.LeftControl))
            {

                if (Globals.Keyboard.IsKeyPressed(Keys.R))
                {

                    Globals.Register.GetBlockFromID(552);

                }

                if (Globals.Keyboard.IsKeyPressed(Keys.S))
                {

                    ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(Player.Camera.Position)).SaveToFile();

                }

                if (Globals.Keyboard.IsKeyPressed(Keys.W))
                {

                    ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(Player.Camera.Position)).TryLoad();
                    ChunkLoader.RemeshQueue.Add(ChunkUtils.PositionToChunk(Player.Camera.Position));

                }

            }

            // Console.WriteLine(Vector3.Dot(Vector3.UnitY, (new Vector4(0, -1, 0, 0) * Sun.RotationMatrix).Xyz));
            ChunkLoader.LoadChunks(Player.Camera.Position);
            ChunkLoader.UpdateChunkQueue();
            // ChunkLoader.UpdateSaveAndRemoveQueue();

            ChunkLoader.DrawReadyChunks((new Vector4(0, -1, 0, 0) * Sun.RotationMatrix).Xyz, Player.Camera);

            /*
            foreach (Vector3i chunkPosition in ChunkLoader.Chunks.Keys)
            {

                if (ChunkLoader.Chunks[chunkPosition].GetChunkState() == ChunkState.Ready && ChunkLoader.Chunks[chunkPosition].IsExposed && !ChunkLoader.Chunks[chunkPosition].IsEmpty)
                {

                    GL.FrontFace(FrontFaceDirection.Cw);
                    rmodel.SetScale(32, 32, 32);
                    rmodel.Draw(((Vector3)chunkPosition + (0.5f, 0.5f, 0.5f)) * Globals.ChunkSize, (0, 0, 0), Player.Camera, 0);
                    rmodel.SetScale(1, 1, 1);
                    GL.FrontFace(FrontFaceDirection.Ccw);

                }

            }
            */



            GL.Disable(EnableCap.DepthTest);
            TextRenderer.RenderText(GuiMaths.RelativeToAbsolute((0.0f, 1.0f, 0)) - (0, 21, 0), (0,0,0), 18, Globals.FrameInformation);

            uiTest.Draw(0);
            GL.Enable(EnableCap.DepthTest);

            if (debug)
            {

                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            }

            DDA.TraceChunks(ChunkLoader.Chunks, Player.Camera.Position, Player.Camera.ForwardVector, Globals.PlayerRange);

            if (DDA.hit)
            {

                GL.FrontFace(FrontFaceDirection.Cw);
                rmodel.SetScale(1, 1, 1);
                GL.PolygonOffset(-1, 1);
                rmodel.Draw((Vector3)DDA.PositionAtHit + (0.5f, 0.5f, 0.5f), Vector3.Zero, Player.Camera, (float)time);
                //rmodel.Draw((Vector3)DDA.PreviousPositionAtHit + (0.5f, 0.5f, 0.5f), Player.Camera, (float)time);
                //rmodel.SetScale(1, 1, 1);
                //rmodel.Draw((Vector3)DDA.PositionAtHit + (0.5f,0.5f,0.5f), Player.Camera, (float)time);
                rmodel.SetScale(0.2f, 0.2f, 0.2f);
                rmodel.Draw(DDA.SmoothPosition, Vector3.Zero, Player.Camera, (float)time);
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
            // TestElement.Draw();
            // texx.Draw();

            frameBuffer.Unbind();

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            framebufferQuad.Draw(frameBuffer, (float)time);
            KeyboardState ks = KeyboardState;

            sw.Stop();
            // Console.WriteLine("Finished frame in " + sw.ElapsedMilliseconds + " ms. FPS: " + (1000f/sw.ElapsedMilliseconds));
            Globals.FrameInformation = "Ft: " + (sw.ElapsedMilliseconds.ToString().Length != 2 ? "0" + sw.ElapsedMilliseconds : sw.ElapsedMilliseconds) + "ms. campos: " + ChunkUtils.PositionToBlockGlobal(Player.Camera.Position);

            SwapBuffers();

        }
        protected override void OnUnload()
        {

            base.OnUnload();

            // this portion is not required
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vbo);

            // shader.Dispose();

        }
        protected override void OnResize(ResizeEventArgs e)
        {

            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
            Globals.WIDTH = e.Width;
            Globals.HEIGHT = e.Height;
            Globals.Center = (Globals.WIDTH / 2f, Globals.HEIGHT / 2f);

            camera.UpdateProjectionMatrix();
            Player.Camera.UpdateProjectionMatrix();
            Globals.GuiCamera.UpdateProjectionMatrix();
            uiTest.OnScreenResize();
            // TestElement.Update();
            TextRenderer.Camera.UpdateProjectionMatrix();
            frameBuffer.UpdateAspect();

        }

    }

}
