using OpenTK.Graphics.OpenGL;
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
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsage.StaticDraw);
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0 * sizeof(float)); // this is the vertices
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float)); // UVs 
            GL.EnableVertexAttribArray(2);
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
            GL.BindTexture(TextureTarget.Texture2d, texture.GetID());

            GL.UniformMatrix4f(GL.GetUniformLocation(shader.getID(), "model"), 1, true, ref model);
            GL.UniformMatrix4f(GL.GetUniformLocation(shader.getID(), "view"), 1, true, ref camera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(shader.getID(), "projection"), 1, true, ref camera.ProjectionMatrix);
            GL.Uniform3f(GL.GetUniformLocation(shader.getID(), "sunVec"), 1, ref sunVec);
            GL.Uniform1f(GL.GetUniformLocation(shader.getID(), "time"), (float)GlobalValues.Time);
            GL.Uniform3f(GL.GetUniformLocation(shader.getID(), "cameraPosition"), 1, ref camera.Position);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);
            // GL.BindVertexArray(0);

            // GL.BindTexture(TextureTarget.Texture2d, 0);

            // shader.UnUse();

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
