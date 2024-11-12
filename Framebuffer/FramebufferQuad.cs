using OpenTK.Graphics.OpenGL;

using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK.FramebufferUtil
{
    internal class FramebufferQuad
    {

        float[] vertices =
        {

            -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
            1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
            1.0f, 1.0f, 0.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 0.0f, 1.0f, 1.0f,
            -1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
            -1.0f, -1.0f, 0.0f, 0.0f, 0.0f

        };

        int vbo, vao;
        Shader shader;

        public FramebufferQuad()
        {

            shader = new Shader("framebuffer.vert", "framebuffer.frag");

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsage.StaticDraw);
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0 * sizeof(float)); // this is the vertices
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float)); // UVs 
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

        }

        public void Draw(Framebuffer framebuffer, float time)
        {

            GL.Disable(EnableCap.DepthTest);

            shader.Use();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2d, framebuffer.textureColorBufferId);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2d, framebuffer.textureDepthStencilBufferId);

            GL.Uniform1f(GL.GetUniformLocation(shader.getID(), "time"), (float)time);
            GL.Uniform1i(GL.GetUniformLocation(shader.getID(), "framebufferColorTexture"), 0);
            GL.Uniform1i(GL.GetUniformLocation(shader.getID(), "framebufferDepthStencilTexture"), 1);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length/5);
            // GL.BindVertexArray(0);

            // GL.BindTexture(TextureTarget.Texture2d, 0);
            // shader.UnUse();

            GL.Enable(EnableCap.DepthTest);

        }

    }
}
