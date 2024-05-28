using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK
{
    internal class NakedModel
    {

        public Matrix4 model;

        public Matrix4 mscale;
        public Matrix4 mrotation;
        public Matrix4 mposition;

        public Vector3d position;

        Shader shader;

        int vbo, vao;

        public float[] vertices;

        public static float[] Tri =
        {

            -0.5f, -0.5f, 0.5f,
            0.5f, -0.5f, -0.5f,
            -0.5f, 0.5f, -0.5f

        };

        public NakedModel(float[] vertices)
        {

            if (vertices == Tri)
            {

                this.vertices = Tri;

            } else
            {

                this.vertices = vertices;

            }

            shader = new Shader("nakedmodel.vert", "nakedmodel.frag");

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0 * sizeof(float)); // this is the vertices
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            SetScale(1, 1, 1);
            SetRotation(0, 0, 0);
            SetPosition(0, 0, 0);

        }
        public void Draw(Camera camera)
        {

            model = mrotation * mscale * mposition;

            shader.Use();

            //GL.Disable(EnableCap.CullFace);
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "model"), true, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "view"), true, ref camera.view);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "projection"), true, ref camera.projection);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);
            GL.BindVertexArray(0);

            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            //GL.Enable(EnableCap.CullFace);

            shader.UnUse();

        }
        public void SetScale(float x, float y, float z)
        {

            mscale = Matrix4.CreateScale(x, y, z);

        }
        public void SetRotation(float pitch, float yaw, float roll)
        {

            mrotation = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(pitch));
            mrotation *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(yaw));
            mrotation *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(roll));

        }
        public void SetPosition(float x, float y, float z)
        {

            mposition = Matrix4.CreateTranslation(x, y, z);
            position = new Vector3d((double)x, (double)y, (double)z);

        }

    }
}
