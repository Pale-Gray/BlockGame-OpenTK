using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace opentk_proj
{
    internal class Model
    {

        public float[] vertices;
        Shader shader;
        Texture texture;

        Matrix4 model;

        int vbo;
        int vao;
        public Model(float[] vertices, string vertexshader, string fragmentshader)
        {

            this.vertices = vertices;

            shader = new Shader(vertexshader, fragmentshader);
            texture = new Texture("../../../res/textures/debug.png");

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

        }

        public void Draw(Vector3 position, Matrix4 projection, Matrix4 view, float time)
        {

            model = Matrix4.CreateTranslation(position);

            shader.Use();

            GL.BindTexture(TextureTarget.Texture2D, texture.getID());

            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "model"), true, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "view"), true, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "projection"), true, ref projection);
            GL.Uniform1(GL.GetUniformLocation(shader.getID(), "time"), (float)time);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            shader.UnUse();

        }

    }
}
