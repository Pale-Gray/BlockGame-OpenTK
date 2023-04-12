using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace opentk_proj
{
    internal class Game : GameWindow
    {

        float[] verts = 
        { 
        
            -0.5f, -0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            0.0f, 0.5f, 0.0f

        };

        int vbo;
        int vao;

        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title}) {}

        Shader shader;

        protected override void OnUpdateFrame(FrameEventArgs args)
        {

            base.OnUpdateFrame(args);

            KeyboardState k = KeyboardState;

            if (k.IsKeyDown(Keys.Escape))
            {

                Close();

            }

        }

        protected override void OnLoad()
        {

            base.OnLoad();

            GL.ClearColor(0.0f, 0.2f, 0.6f, 1.0f);

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);
            // this portion is not required
            // GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // GL.DeleteBuffer(vbo);

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            // defining attributes
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vbo);

            shader = new Shader("../../../res/shaders/default.vert", "../../../res/shaders/default.frag");
            shader.Use();

        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {

            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // etc
            shader.Use();
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            SwapBuffers();

        }

        protected override void OnUnload()
        {

            base.OnUnload();

            shader.Dispose();

        }
        protected override void OnResize(ResizeEventArgs e)
        {

            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);

        }

    }

}
