using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Linq;

namespace opentk_proj
{
    internal class Game : GameWindow
    {
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title}) {}

        float[] verts =
        {

            -0.5f, -0.5f, 0.0f, 0.0f, 1.0f,
            0.5f, -0.5f, 0.0f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.0f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.0f, 1.0f, 0.0f,
            -0.5f, 0.5f, 0.0f, 0.0f, 0.0f,
            -0.5f, -0.5f, 0.0f, 0.0f, 1.0f

        };

        Shader shader;
        Texture texture;

        int vbo;
        int vao;

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

            GL.Enable(EnableCap.Texture2D);
            GL.ActiveTexture(TextureUnit.Texture0);

            shader = new Shader("../../../res/shaders/default.vert", "../../../res/shaders/default.frag");
            shader.Use();
            // GL.ActiveTexture(TextureUnit.Texture0);
            texture = new Texture("../../../res/textures/grass.png");
            GL.BindTexture(TextureTarget.Texture2D, texture.getID());

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3);
            GL.EnableVertexAttribArray(1);

        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {

            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // etc
            GL.BindTexture(TextureTarget.Texture2D, texture.getID());
            shader.Use();
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindTexture(TextureTarget.Texture2D, 0);

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

        }

    }

}
