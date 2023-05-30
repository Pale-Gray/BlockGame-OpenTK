using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace opentk_proj
{
    internal class Game : GameWindow
    {
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title}) {}

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

        protected override void OnUpdateFrame(FrameEventArgs args)
        {

            base.OnUpdateFrame(args);

            time = GLFW.GetTime();

            MouseState mouse = MouseState;

            if (firstmove)
            {

                lmpos = new Vector2(mouse.X, mouse.Y);
                firstmove = false;

            } else
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

            Chunk c = new Chunk();
            c.initialize();
            Console.WriteLine(verts.Length);
            verts = c.getvertdata();

            GL.ClearColor(0.0f, 0.2f, 0.6f, 1.0f);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);
            GL.Enable(EnableCap.Texture2D);
            GL.ActiveTexture(TextureUnit.Texture0);

            shader = new Shader("../../../res/shaders/default.vert", "../../../res/shaders/default.frag");
            shader.Use();
            // GL.ActiveTexture(TextureUnit.Texture0);
            // texture = new Texture("../../../res/textures/bowser_propose.jpg");
            texture = new Texture("../../../res/textures/atlas.png");
            // GL.BindTexture(TextureTarget.Texture2D, texture.getID());

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 6 * sizeof(float), 5 * sizeof(float));
            GL.EnableVertexAttribArray(2);

        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {

            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Matrix4 rot = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(45.0f * (float)GLFW.GetTime()));
            Matrix4 transformation = Matrix4.CreateScale(1.0f);// rot; // translation, rotation, then scale

            // Matrix4 model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-55.0f));
            Matrix4 model = Matrix4.CreateScale(1.0f);// Matrix4.CreateRotationX(MathHelper.DegreesToRadians(90.0f * (float)GLFW.GetTime())) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(90.0f * (float)GLFW.GetTime()));
            // Matrix4 view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float) Constants.WIDTH / (float) Constants.HEIGHT, 0.1f, 100.0f);

            // Vector3 position = new Vector3(0.0f, 0.0f, 0.0f);
            // Vector3 camTarget = Vector3.Zero;
            // Vector3 camDirection = Vector3.Normalize(position - camTarget);

            // Vector3 up = Vector3.UnitY;
            // Vector3 right = Vector3.Normalize(Vector3.Cross(up, camDirection));
            // Vector3 camup = Vector3.Cross(camDirection, right);

            cfront.X = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(yaw));
            cfront.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            cfront.Z = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(yaw));
            cfront = Vector3.Normalize(cfront);

            // Matrix4 view = Matrix4.LookAt(new Vector3(0.0f, 0.0f, 3.0f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitY); ;
            Matrix4 view = Matrix4.LookAt(cposition, cposition + cfront, cup);

            // etc
            GL.BindTexture(TextureTarget.Texture2D, texture.getID());
            shader.Use();
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "model"), true, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "view"), true, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "projection"), true, ref projection);
            GL.Uniform1(GL.GetUniformLocation(shader.getID(), "time"), (float) time);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, verts.Length);
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
            Constants.WIDTH = e.Width;
            Constants.HEIGHT = e.Height;

        }

    }

}
