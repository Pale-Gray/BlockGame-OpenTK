using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGLES2;
using OpenTK.Mathematics;

namespace VoxelGame.Util;

public struct GuiRectangle()
{
    public Vector2 Position = Vector2.Zero;
    public Vector2 Size = Vector2.One;
    public Vector2 TextureCoordinatePosition = Vector2.Zero;
    public Vector2 TextureCoordinateSize = Vector2.One;
}

public struct GuiVertex
{
    public Vector2 Position;
    public Vector2 TextureCoordinate;

    public GuiVertex(Vector2 position, Vector2 textureCoordinate)
    {
        Position = position;
        TextureCoordinate = textureCoordinate;
    }
}

public static class Gui
{
    private static Shader _guiShader;
    private static Texture _fontTexture;

    private static List<GuiRectangle> _rectangles = new();
    private static List<GuiVertex> _vertices = new();
    private static List<int> _indices = new();
    private static int Vbo, Vao, Ibo;
    private static Texture _activeTexture => _fontTexture;
    public static float PixelsPerUnit = 2.0f;
    
    public static void Init()
    {
        _guiShader = new Shader("resources/shaders/gui.vert", "resources/shaders/gui.frag").Compile();
        _fontTexture = new Texture("resources/fonts/default.png").Generate();
    }

    public static void DrawRectangles()
    {
        foreach (GuiRectangle rectangle in _rectangles)
        {
            _vertices.AddRange
            (
                new GuiVertex(rectangle.Position, rectangle.TextureCoordinatePosition),
                new GuiVertex(rectangle.Position + (0, rectangle.Size.Y), rectangle.TextureCoordinatePosition + (0, rectangle.TextureCoordinateSize.Y)),
                new GuiVertex(rectangle.Position + rectangle.Size, rectangle.TextureCoordinatePosition + rectangle.TextureCoordinateSize),
                new GuiVertex(rectangle.Position + (rectangle.Size.X, 0), rectangle.TextureCoordinatePosition + (rectangle.TextureCoordinateSize.X, 0))
            );
        }

        for (int i = 0; i < _rectangles.Count; i++)
        {
            int idx = i * 4;
            
            _indices.AddRange
            (
                0 + idx, 1 + idx, 2 + idx,
                2 + idx, 3 + idx, 0 + idx
            );
        }
        
        Vao = GL.GenVertexArray();
        GL.BindVertexArray(Vao);

        Vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
        GL.BufferData<GuiVertex>(BufferTarget.ArrayBuffer, Marshal.SizeOf<GuiVertex>() * _vertices.Count, CollectionsMarshal.AsSpan(_vertices), BufferUsage.StaticDraw);

        Ibo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ibo);
        GL.BufferData<int>(BufferTarget.ElementArrayBuffer, sizeof(uint) * _indices.Count, CollectionsMarshal.AsSpan(_indices), BufferUsage.StaticDraw);
        
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<GuiVertex>(), Marshal.OffsetOf<GuiVertex>(nameof(GuiVertex.Position)));
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<GuiVertex>(), Marshal.OffsetOf<GuiVertex>(nameof(GuiVertex.TextureCoordinate)));
        GL.EnableVertexAttribArray(1);

        GL.Disable(EnableCap.DepthTest);
        // GL.Disable(EnableCap.CullFace);
        _guiShader.Bind();
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, Config.Width, Config.Height, 0, 0.1f, 100.0f);
        GL.UniformMatrix4f(_guiShader.GetUniformLocation("uProjection"), 1, true, in projection);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2d, _activeTexture.Id);
        GL.Uniform1i(_guiShader.GetUniformLocation("uTexture"), 0);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
        GL.Enable(EnableCap.DepthTest);
        
        GL.DeleteVertexArray(Vao);
        GL.DeleteBuffer(Vbo);
        GL.DeleteBuffer(Ibo);
        _vertices.Clear();
        _indices.Clear();
        _rectangles.Clear();
    }

    private static Vector2 _glyphTextureSize = (1.0f / 64.0f, -1.0f / 64.0f);
    private static Vector2 _glyphSize = (6.0f, 14.0f);
    
    public static void Text(string text)
    {
        Vector2 position = Vector2.Zero;
        Vector2 textureCoordinateIndex = Vector2.Zero;
        
        foreach (char character in text)
        {
            textureCoordinateIndex = GlyphIndexToCoordinates(character);
            
            _rectangles.Add(new GuiRectangle() { Position = Vector2.Zero + (position * (6, 0)) * PixelsPerUnit, Size = new Vector2(6, 14) * PixelsPerUnit, TextureCoordinatePosition = (0, 1) + (_glyphTextureSize * textureCoordinateIndex), TextureCoordinateSize = _glyphTextureSize});
            position.X += 1;
        }
        DrawRectangles();
    }

    public static Vector2 GlyphIndexToCoordinates(int index)
    {
        return (index % 64, (int) (index / 64.0f));
    }
}