using OpenTK.Graphics.OpenGL;
using VoxelGame.Util;

namespace VoxelGame.Rendering;

public class FramebufferQuad
{
    private int _vao, _vbo;
    private Shader _framebufferShader = new Shader("resources/shaders/framebuffer.vert", "resources/shaders/framebuffer.frag");
    private float[] _quad =
    {
        -1.0f, 1.0f,
        -1.0f, -1.0f,
        1.0f, -1.0f,
        1.0f, -1.0f,
        1.0f, 1.0f,
        -1.0f, 1.0f
    };
    
    public void Create()
    {
        _framebufferShader.Compile();

        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _quad.Length * sizeof(float), _quad, BufferUsage.StaticDraw);
        
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }

    public void Draw(DeferredFramebuffer framebuffer)
    {
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        _framebufferShader.Bind();
        framebuffer.BindTextures();
        GL.Uniform1i(_framebufferShader.GetUniformLocation("uAlbedo"), 0);
        GL.Uniform1i(_framebufferShader.GetUniformLocation("uNormal"), 1);
        GL.Uniform1i(_framebufferShader.GetUniformLocation("uDepthStencil"), 2);
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
    }
}