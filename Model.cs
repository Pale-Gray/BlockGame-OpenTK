using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK
{
    internal class Model
    {

        public float[] vertices;
        Shader shader;
        Texture texture;

        Matrix4 model;

        Matrix4 mscale;
        Matrix4 mrotation;
        Matrix4 mposition;

        bool isCulled = true;

        int vbo;
        int vao;
        public Model(float[] vertices, string texture, string vertexshader, string fragmentshader)
        {

            this.vertices = vertices;

            shader = new Shader(vertexshader, fragmentshader);
            this.texture = new Texture(texture);

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0 * sizeof(float)); // this is the vertices
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float)); // UVs 
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            SetScale(1,1,1);
            SetRotation(0,0,0);
            SetPosition(0,0,0);

        }

        public void Draw(Vector3 position, Vector3 sunVec, Camera camera, float time)
        {

            SetPosition(position.X, position.Y, position.Z);
            model = mrotation * mscale * mposition;

            shader.Use();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.getID());

            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "model"), true, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "view"), true, ref camera.ViewMatrix);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "projection"), true, ref camera.ProjectionMatrix);
            GL.Uniform3(GL.GetUniformLocation(shader.getID(), "sunVec"), sunVec);
            GL.Uniform1(GL.GetUniformLocation(shader.getID(), "time"), (float)time);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);

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

        }

    }
}
