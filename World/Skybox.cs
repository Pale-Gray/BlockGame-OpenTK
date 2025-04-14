using System.ComponentModel;
using System.IO;
using System.Linq;
using Game.Core.Graphics;
using Game.Core.PlayerUtil;
using Game.Util;
using OpenTK.Graphics.OpenGL;

namespace Game.Core.Worlds;

public class Skybox
{

    private float[] _vertices = 
    {

        // top
        0.5f, 0.5f, -0.5f,
        0.5f, 0.5f, 0.5f,
        -0.5f, 0.5f, 0.5f,
        -0.5f, 0.5f, -0.5f,

        // back
        0.5f, 0.5f, 0.5f,
        0.5f, -0.5f, 0.5f,
        -0.5f, -0.5f, 0.5f,
        -0.5f, 0.5f, 0.5f,

        // left
        0.5f, 0.5f, -0.5f,
        0.5f, -0.5f, -0.5f,
        0.5f, -0.5f, 0.5f,
        0.5f, 0.5f, 0.5f,

        // front
        -0.5f, 0.5f, -0.5f,
        -0.5f, -0.5f, -0.5f,
        0.5f, -0.5f, -0.5f,
        0.5f, 0.5f, -0.5f,

        // right
        -0.5f, 0.5f, 0.5f,
        -0.5f, -0.5f, 0.5f,
        -0.5f, -0.5f, -0.5f,
        -0.5f, 0.5f, -0.5f,

        // bottom
        -0.5f, -0.5f, -0.5f,
        -0.5f, -0.5f, 0.5f,
        0.5f, -0.5f, 0.5f,
        0.5f, -0.5f, -0.5f

    };

    private int[] _indices = 
    {

        0, 1, 2,
        2, 3, 0,

        4, 5, 6,
        6, 7, 4,

        8, 9, 10,
        10, 11, 8,

        12, 13, 14,
        14, 15, 12,

        16, 17, 18,
        18, 19, 16,

        20, 21, 22,
        22, 23, 20

    };

    private int _vao, _vbo, _ibo;
    private NewTexture _texture;

    public Skybox()
    {

        _texture = new NewTexture(Path.Combine("Resources", "Textures", "sky.png"), TextureMinFilter.Linear, TextureMagFilter.Linear);

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ibo = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _vertices.Length, _vertices, BufferUsage.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * _indices.Length, _indices, BufferUsage.StaticDraw);

    }

    public void Draw(Player player)
    {

        GL.BindVertexArray(_vao);

        GlobalValues.SkyboxShader.Use();
        _texture.Bind(0);
        GL.Uniform1f(GlobalValues.SkyboxShader.GetUniformLocation("uSkyColor"), 0);
        GL.Uniform3f(GlobalValues.SkyboxShader.GetUniformLocation("uCameraPosition"), 1, in player.Camera.Position);
        GL.Uniform1i(GlobalValues.SkyboxShader.GetUniformLocation("uTickTime"), (int) GameState.World.TickTime);
        GL.UniformMatrix4f(GlobalValues.SkyboxShader.GetUniformLocation("uProjectionMatrix"), 1, true, player.Camera.ProjectionMatrix);
        GL.UniformMatrix4f(GlobalValues.SkyboxShader.GetUniformLocation("uViewMatrix"), 1, true, player.Camera.ViewMatrix);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

    }

    public void Unload()
    {

        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ibo);
        GL.DeleteVertexArray(_vao);

    }

}