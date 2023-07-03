using OpenTK.Graphics.OpenGL;
// using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Transactions;

using opentk_proj.chunk;
using opentk_proj.block;
using opentk_proj.util;

namespace opentk_proj
{
    internal class Game : GameWindow
    {
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title, Flags = ContextFlags.Debug }) { }

        float[] verts = {

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

        float[] v =
        {

             -0.5f, 0.5f, 0.0f, 0.0f, 1.0f,
             -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
             0.5f, 0.5f, 0.0f, 1.0f, 1.0f,
             -0.5f, 0.5f, 0.0f, 0.0f, 1.0f

        };

        Shader shader;
        Texture texture;

        int vbo;
        int vao;

        float speed = 15.0f;
        Vector3 cposition = new Vector3(0.0f, 0.0f, 0.0f);
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
        Chunk chunk;
        Camera camera;

        double ft = 0;
        double fs = 0;
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
            /* if (type == DebugType.DebugTypeError)
            {
                throw new Exception(message);
            } */
        }

        private static DebugProc DebugMessageDelegate = OnDebugMessage;
        protected override void OnUpdateFrame(FrameEventArgs args)
        {

            base.OnUpdateFrame(args);

            time = GLFW.GetTime();

            delay += args.Time;
            ft += 1d / args.Time;
            fs++;

            if (delay > 1d)
            {

                Title = "fps [" + fs + "]";
                ft = 0;
                fs = 0;
                delay = 0;

            }

            MouseState mouse = MouseState;

            if (firstmove)
            {

                lmpos = new Vector2(mouse.X, mouse.Y);
                firstmove = false;

            }
            else
            {

                CursorState = CursorState.Grabbed;
                float deltaX = mouse.X - lmpos.X;
                float deltaY = mouse.Y - lmpos.Y;
                lmpos = new Vector2(mouse.X, mouse.Y);
                yaw += deltaX * sens;
                pitch -= deltaY * sens;

            }

            // Console.WriteLine(verts.Length);

            KeyboardState k = KeyboardState;

            if (k.IsKeyDown(Keys.W))
            {

                cposition += cfront * speed * (float)args.Time;

            }
            if (k.IsKeyDown(Keys.S))
            {

                cposition -= cfront * speed * (float)args.Time;

            }
            if (k.IsKeyDown(Keys.A))
            {

                cposition -= Vector3.Normalize(Vector3.Cross(cfront, cup)) * (speed * (float)args.Time);

            }
            if (k.IsKeyDown(Keys.D))
            {

                cposition += Vector3.Normalize(Vector3.Cross(cfront, cup)) * (speed * (float)args.Time);

            }
            if (k.IsKeyDown(Keys.E))
            {

                cposition += cup * speed * (float)args.Time;

            }
            if (k.IsKeyDown(Keys.Q))
            {

                cposition -= cup * speed * (float)args.Time;

            }

            if (k.IsKeyDown(Keys.Escape))
            {

                Close();

            }

        }
        protected override void OnLoad()
        {

            base.OnLoad();

            Blocks.RegisterIDs();

            chunk = new Chunk(0, 0, 0);
            camera = new Camera(cposition, cfront, cup, Camera.Perspective, 45.0f);
            rmodel = new Model(verts, "../../../res/shaders/model.vert", "../../../res/shaders/model.frag");

            GL.ClearColor(0.0f, 0.2f, 0.6f, 1.0f);

            GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // GL.LineWidth(25f);
            // GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            GL.Enable(EnableCap.DepthTest);
            // GL.Enable(EnableCap.CullFace);
            // GL.FrontFace(FrontFaceDirection.Ccw);
            GL.Enable(EnableCap.Texture2D);
            GL.ActiveTexture(TextureUnit.Texture0);

            shader = new Shader("../../../res/shaders/default.vert", "../../../res/shaders/default.frag");
            shader.Use();

            texture = new Texture("../../../res/textures/atlas.png");
            shader.UnUse();

        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {

            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 transformation = Matrix4.CreateScale(1.0f);
            Matrix4 model = Matrix4.CreateScale(1.0f);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), Constants.WIDTH / (float)Constants.HEIGHT, 0.1f, 100.0f);

            cfront.X = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(yaw));
            cfront.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            cfront.Z = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(yaw));
            cfront = Vector3.Normalize(cfront);

            Matrix4 view = Matrix4.LookAt(cposition, cposition + cfront, cup);

            camera.Update(cposition, cfront, cup, yaw, pitch, roll);
            // etc
            GL.BindTexture(TextureTarget.Texture2D, texture.getID());
            shader.Use();

            MouseState mouse = MouseState;

            if (mouse.IsButtonPressed(MouseButton.Left))
            {
                Console.WriteLine("pressed");
                chunk.allzeros();

            }
            chunk.Draw(shader, camera.projection, camera.view, (float)time);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            shader.UnUse();

            rmodel.Draw(cposition + (cfront * 5), projection, view, (float)time);

            SwapBuffers();

        }
        protected override void OnUnload()
        {

            base.OnUnload();

            // this portion is not required
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vbo);

            shader.Dispose();

        }
        protected override void OnResize(ResizeEventArgs e)
        {

            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
            Constants.WIDTH = e.Width;
            Constants.HEIGHT = e.Height;

        }

    }

}
