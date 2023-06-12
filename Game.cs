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

        Chunk c1;
        Chunk c2;

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

            Blocks.RegisterIDs();

            // verts = c.getvertdata();

            GL.ClearColor(0.0f, 0.2f, 0.6f, 1.0f);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.Enable(EnableCap.Texture2D);
            GL.ActiveTexture(TextureUnit.Texture0);
            // GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            // GL.LineWidth(10);

            shader = new Shader("../../../res/shaders/default.vert", "../../../res/shaders/default.frag");
            shader.Use();

            texture = new Texture("../../../res/textures/atlas.png");

            c1 = new Chunk(0, 0, 0);
            c2 = new Chunk(1, 0, 0);

            /* vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            // how it will be
            GL.VertexAttribPointer(0, 1, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0 * sizeof(float)); // this is the blocktype data
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 1 * sizeof(float)); // this is the vertices
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 4 * sizeof(float)); // this is the normals
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 7 * sizeof(float)); // UVs 
            GL.EnableVertexAttribArray(3); */

        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {

            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 transformation = Matrix4.CreateScale(1.0f);// rot; // translation, rotation, then scale

           
            Matrix4 model = Matrix4.CreateScale(1.0f);// Matrix4.CreateRotationX(MathHelper.DegreesToRadians(90.0f * (float)GLFW.GetTime())) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(90.0f * (float)GLFW.GetTime()));
            // Matrix4 view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float) Constants.WIDTH / (float) Constants.HEIGHT, 0.1f, 100.0f);


            cfront.X = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(yaw));
            cfront.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            cfront.Z = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(yaw));
            cfront = Vector3.Normalize(cfront);

            // Matrix4 view = Matrix4.LookAt(new Vector3(0.0f, 0.0f, 3.0f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitY); ;
            Matrix4 view = Matrix4.LookAt(cposition, cposition + cfront, cup);

            // etc
            GL.BindTexture(TextureTarget.Texture2D, texture.getID());
            shader.Use();

            MouseState mouse = MouseState;

            if (mouse.IsButtonPressed(MouseButton.Left))
            {
                Console.WriteLine("pressed");
                c1.allzeros();

            }
            c1.Draw(shader, view, (float)time);
            // c2.Draw(shader, view, (float)time);

            /* GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "model"), true, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "view"), true, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "projection"), true, ref projection);
            GL.Uniform1(GL.GetUniformLocation(shader.getID(), "time"), (float) time);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, verts.Length);
            GL.BindTexture(TextureTarget.Texture2D, 0); */
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
