using OpenTK.Graphics.OpenGL;
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
using System.Linq;
using Blockgame_OpenTK.BlockUtil;
using System.Text.Json;

namespace Blockgame_OpenTK
{
    internal class Game : GameWindow
    {
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title, Flags = ContextFlags.Debug }) { }

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
        GUIElement TestElement;
        GUIClickable GUIClick;
        FontRenderer text;

        Ray ray = new Ray(0, 0, 0, 0, 0, 0);

        Framebuffer frameBuffer;
        FramebufferQuad framebufferQuad;

        TextureArray TextureArray = new TextureArray();

        bool debug = false;

        bool IsGrabbed = true;

        double ft = 0;
        double fs = 0;
        Chunk c;
        Sun Sun;

        Player Player;

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
            if (Globals.Keyboard.IsKeyPressed(Keys.Escape))
            { 

                if (!IsGrabbed)
                {

                    IsGrabbed = true;
                    CursorState = CursorState.Grabbed;
                        
                }
                else { IsGrabbed = false; CursorState = CursorState.Normal; MousePosition = Globals.Center;  }


            }
            Globals.CursorState = CursorState;

            Player.Update();

        }
        protected override void OnLoad()
        {

            base.OnLoad();

            // TextureArray.Load();
            Globals.ArrayTexture = new TextureArray();
            Globals.ArrayTexture.Load();

            // Blocks.GetBlockFromName("RandomBlock");

            // original: 8. printed: 1
            // Console.WriteLine(Convert.ToInt16(0b1000 >> 3 & 0b1111));
            // binary shift right three and cmp to full mask
            Globals.AtlasTexture = new TextureAtlas("atlas.png", 32);
            Globals.ChunkShader = new Shader("chunk.vert", "chunk.frag");
            Globals.DefaultShader = new Shader("default.vert", "default.frag");
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
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.TextureCubeMap);
            GL.Enable(EnableCap.TextureCubeMapSeamless);
            GL.ActiveTexture(TextureUnit.Texture0);

            texture = new Texture("atlas.png");
            emtexture = new Texture("atlas_em.png");
            Texture t = new Texture("cubemap/cubemap_test.png");
            cmtex = new CMTexture(t, 64);

            TestElement = new GUIElement(50, 50, 10, 10, OriginType.Center, t, GUIElement.Null);
            text = new FontRenderer(16, "");
            text.SetFontColor((0,0,0));

            ChunkShader = new Shader("chunk.vert", "chunk.frag");

            frameBuffer = new Framebuffer();
            framebufferQuad = new FramebufferQuad();

            Sun = new Sun("sun.png", 10);

            Player = new Player();
            Player.SetHeight(0);
            Player.SetPosition((0, 0, 0));
            Blocks.GetBlockFromName("Air");

        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {

            base.OnRenderFrame(args);

            Stopwatch sw = Stopwatch.StartNew();

            frameBuffer.Bind();
            GL.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);
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
            GL.Disable(EnableCap.CullFace);
            // Skybox.SetRotation((float)time*5, 0, 0);
            Skybox.Draw(cposition, camera, (float)time);
            // Sun.SetPosition(camera.position);
            float sec = 30;
            float angle = Maths.ToRadians(360)*(float)(time/sec);
            float angle2 = Maths.Lerp(Maths.ToRadians(45), Maths.ToRadians(-45), (float) ((Math.Cos(Maths.ToRadians(360) * (float) (time / sec))) / 2) + 0.5f);

            Vector3 rotationPosition = (0.0f, (float)Math.Cos(angle) * Sun.RadiusFromCamera, (float)Math.Sin(angle) * Sun.RadiusFromCamera);

            Sun.SetRotation((Maths.ToRadians(-45),0,Maths.ToRadians(-35)));
            // Sun.SetPosition((0, (float)(4*Math.Sin(time)), 0));
            Sun.Draw(camera);
            Globals.ChunkShader.Use();
            GL.UniformMatrix4(GL.GetUniformLocation(Globals.ChunkShader.getID(), "rot"), true, ref Sun.RotationMatrix);
            Globals.ChunkShader.UnUse();
            // Console.WriteLine(Sun.GetNormalToCamera(camera));

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            // ChunkLoader.GenerateThreadedFilledColumns(16, camera.position);

            ChunkLoader.GenerateThreaded(25, Player.Camera.Position);
            ChunkLoader.DrawAllReadyChunks(Globals.ChunkShader, Player.Camera, (float)time);

            // Player.GetBlockLooking(5);
            // DDA.Trace(ChunkLoader.ChunkDictionary, Player.Camera.Position, Player.Camera.ForwardVector, 10);

            DDA.TraceChunks(ChunkLoader.ChunkDictionary, Player.Camera.Position, Player.Camera.ForwardVector, 5);
            //  Console.WriteLine(DDA.PositionAtHit);
            // Console.WriteLine("cpos: {0}, blpos: {1}, bgpos: {2}, blposfrombgpos: {3}", ChunkUtils.PositionToChunk(Player.Camera.Position), ChunkUtils.PositionToBlockLocal(Player.Camera.Position), ChunkUtils.PositionToBlockGlobal(Player.Camera.Position), ChunkUtils.PositionToBlockLocal(ChunkUtils.PositionToBlockGlobal(Player.Camera.Position)));
            // Console.WriteLine("prtb: {0}, chnp: {1}, hit: {2}", DDA.RoundedPosition,DDA.ChunkAtHit, DDA.ChunkAtHit);
            // Console.WriteLine("cam gl pos: {0} chunk pos from glblpos: {1}", ChunkUtils.PositionToBlockGlobal(Player.Camera.Position), ChunkUtils.PositionToChunk(ChunkUtils.PositionToBlockGlobal(Player.Camera.Position)));
            // Console.WriteLine("hitpos: {0}, prevpos: {1}, prevpovloc: {2}", DDA.PositionAtHit, DDA.PreviousPositionAtHit, ChunkUtils.PositionToBlockLocal(DDA.PreviousPositionAtHit));

            GL.Disable(EnableCap.DepthTest);
            rmodel.SetScale(1, 1, 1);
            //rmodel.Draw((Vector3)DDA.PositionAtHit + (0.5f,0.5f,0.5f), Player.Camera, (float)time);
            //rmodel.Draw((Vector3)DDA.PreviousPositionAtHit + (0.5f, 0.5f, 0.5f), Player.Camera, (float)time);
            //rmodel.SetScale(1, 1, 1);
            //rmodel.Draw((Vector3)DDA.PositionAtHit + (0.5f,0.5f,0.5f), Player.Camera, (float)time);
            rmodel.SetScale(0.2f,0.2f,0.2f);
            //rmodel.Draw(DDA.SmoothPosition, Player.Camera, (float)time);
            // hitdisplay.Draw((0,0,0), camera, (float)time);
            TestElement.Draw();
            GL.Enable(EnableCap.DepthTest);

            // Console.WriteLine(ChunkUtils.PositionToBlockPositionRelativeToChunk(camera.position));
            // Console.WriteLine(DDA.HitPoint);
            Vector3 CameraAtChunk = ChunkUtils.PositionToChunk(camera.Position);
            Vector3 CameraAtBlockLocal = ChunkUtils.PositionToBlockLocal(camera.Position);
            Vector3 CameraAtBlockGlobal = ChunkUtils.PositionToBlockGlobal(camera.Position);

            text.UpdateText($"Player Position: {ChunkUtils.PositionToBlockGlobal(Player.Position)}");
            text.Draw();

            if (debug)
            {

                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.DepthTest);
                // GL.LineWidth(5f);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                xyz_display.Draw(Player.Camera.Position + (Player.Camera.ForwardVector * 5), Player.Camera, (float)time);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.Enable(EnableCap.CullFace);
                GL.Enable(EnableCap.DepthTest);

            }
            frameBuffer.Unbind();

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            framebufferQuad.Draw(frameBuffer, (float)time);
            KeyboardState ks = KeyboardState;
            if (ks.IsKeyPressed(Keys.F1))
            {

                // StbImageWrite.stbi_flip_vertically_on_write(1);
                // /Stream str = File.OpenWrite("../../../res/ss/ss1.png");
                // StreamReader sr = new StreamReader(File.OpenRead("../../../res/textures/testatlas.png"));
                // ImageWriter wr = new ImageWriter();
                // byte[] pixels = new byte[(640 * 480) * 4];
                // GL.ReadPixels(0, 0, 640, 480, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
                // wr.WritePng(pixels, 640, 480, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, str);

            }
            if (ks.IsKeyPressed(Keys.F5))
            {

                // chunk.Save("../../../res/cdat/chunk.cdat");
                for (int i = 0; i < ChunkLoader.GetAllChunks().Count; i++)
                {
                    // string key = ChunkLoader.GetAllChunks().ElementAt(i).Key;

                    // ChunkLoader.GetAllChunks()[key].Save("../../../res/cdat/"+key+".cdat");


                }

            }
            if (ks.IsKeyPressed(Keys.F6))
            {

                // chunk.Load("../../../res/cdat/chunk.cdat");

                for (int i = 0; i < ChunkLoader.GetAllChunks().Count; i++)
                {
                    // string key = ChunkLoader.GetAllChunks().ElementAt(i).Key;

                    // ChunkLoader.GetAllChunks()[key].Load("../../../res/cdat/" + key + ".cdat");


                }

            }
            if (ks.IsKeyPressed(Keys.F7))
            {

                // chunk.Rewrite();

                for (int i = 0; i < ChunkLoader.GetAllChunks().Count; i++)
                {
                    
                    // string key = ChunkLoader.GetAllChunks().ElementAt(i).Key;

                    // ChunkLoader.GetAllChunks()[key].Rewrite();


                }

            }

            sw.Stop();
            // Console.WriteLine("Finished frame in " + sw.ElapsedMilliseconds + " ms. FPS: " + (1000f/sw.ElapsedMilliseconds));
            Globals.FrameInformation = "Ft: " + sw.ElapsedMilliseconds + "ms. campos: " + (Vector3i) cposition;

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
            TestElement.Update();
            text.Update();
            frameBuffer.UpdateAspect();
            // GUIClick.Update();

        }

    }

}
