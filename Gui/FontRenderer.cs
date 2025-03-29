using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Game.Core.TexturePack;
using Game.Util;
using FreeTypeSharp;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform.Native.X11;

namespace Game.Core.GuiRendering;

public struct GlyphData
{

    public Vector2i Advance;
    public Vector2i Bearing;
    public Vector2i Size;

}
public struct FaceData
{

    public Dictionary<char, GlyphData> Glyphs = new();
    public FontArrayTexture FaceTexture;
    public int Ascender;
    public int Descender;
    public int LineHeight;
    public FaceData() {}

}

public struct GlyphVertex
{

    public Vector2 Position;
    public Vector2 TextureCoordinates;
    public Color4<Rgba> Color;
    public int TextureIndex;

}

public enum JustifyMode
{

    HorizontalLeftVerticalCentered,
    HorizontalRightVerticalCentered,
    Left,
    Right,
    Centered

}
public unsafe class FontRenderer
{

    static FT_Error _error;
    static GCHandle _libraryHandle = GCHandle.Alloc(new FT_LibraryRec_(), GCHandleType.Pinned);
    static GCHandle _faceHandle = GCHandle.Alloc(new FT_FaceRec_(), GCHandleType.Pinned);
    static FT_LibraryRec_* _library;
    static FT_FaceRec_* _face;
    static Dictionary<uint, FaceData> _faces = new();
    static string _fontFile;
    static Shader _glyphShader;
    static Camera _glyphCamera = new Camera((0, 0, 1), (0, 0, -1), (0, 1, 0), CameraType.Orthographic, 90);
    public static string FontFile 
    {
        get => _fontFile;

        set
        {
            FT_FaceRec_* face;

            _fontFile = value;
            _error = FT.FT_New_Face(_library, (byte*)Marshal.StringToHGlobalAnsi(_fontFile), 0, &face);
            _face = face;
        }
    }

    static int _vbo, _vao, _ibo;
    static List<GlyphVertex> _vertices = new();
    static List<int> _indices = new();
    public static void Text(Vector2 position, Vector2 containerDimensions, uint pixelSize, Color4<Rgba> color, string text, bool doKerning = false, JustifyMode justifyMode = JustifyMode.Left, bool wrapped = false)
    {

        if (text.Length == 0) return;

        if (!_faces.ContainsKey(pixelSize)) _faces.Add(pixelSize, new FaceData() { FaceTexture = new FontArrayTexture((int)pixelSize * 2, (int)pixelSize * 2) });

        string formatted = string.Empty;

        float currentWidth = 0;

        for (int i = 0; i < text.Length; i++)
        {

            if (!_faces[pixelSize].Glyphs.ContainsKey(text[i]))
            {
                GrabGlyph(pixelSize, text[i]);
            }

        }

        for (int i = 0; i < text.Length; i++)
        {

            currentWidth += _faces[pixelSize].Glyphs[text[i]].Advance.X;

            if (wrapped)
            {

                if (i + 1 < text.Length)
                {

                    if (currentWidth + _faces[pixelSize].Glyphs[text[i+1]].Advance.X > containerDimensions.X)
                    {

                        currentWidth = _faces[pixelSize].Glyphs[text[i]].Advance.X;
                        formatted += '\n';

                    }

                }

            }

            formatted += text[i];
        }

        string[] lines = formatted.Split('\n');

        _vertices.Clear();
        _indices.Clear();

        position.Y += _faces[pixelSize].Ascender;
        Vector2 pos = Vector2.Zero;
        Vector2 textBlockBounds = (0, 0); 
        float textLineWidth;

        int index = 0;
        int currentCharacterIndex = 0;

        foreach (string line in lines)
        {

            pos.X = 0;
            textBlockBounds.Y += _faces[pixelSize].LineHeight;
            textLineWidth = 0;

            for (int i = 0; i < line.Length; i++)
            {

                currentCharacterIndex++; 
                textLineWidth += _faces[pixelSize].Glyphs[line[i]].Advance.X;

                Vector2 bearing = (_faces[pixelSize].Glyphs[line[i]].Bearing.X, _faces[pixelSize].Glyphs[line[i]].Size.Y - _faces[pixelSize].Glyphs[line[i]].Bearing.Y);

                float textureCoordinateY = (_faces[pixelSize].FaceTexture.Size.Y - (_faces[pixelSize].FaceTexture.Size.Y - _faces[pixelSize].Glyphs[line[i]].Size.Y)) / (float)_faces[pixelSize].FaceTexture.Size.Y;
                float textureCoordinateX = _faces[pixelSize].Glyphs[line[i]].Size.X / (float)_faces[pixelSize].FaceTexture.Size.X;

                int textureIndex = _faces[pixelSize].FaceTexture.GetTextureIndex(line[i]);

                _vertices.AddRange(
                    new GlyphVertex() {Position = position + pos + bearing + (0, -_faces[pixelSize].Glyphs[line[i]].Size.Y), TextureCoordinates = (0, 0), TextureIndex = textureIndex, Color = color},
                    new GlyphVertex() {Position = position + pos + bearing, TextureCoordinates = (0, textureCoordinateY), TextureIndex = textureIndex, Color = color},
                    new GlyphVertex() {Position = position + pos + bearing + (_faces[pixelSize].Glyphs[line[i]].Size.X, 0), TextureCoordinates = (textureCoordinateX, textureCoordinateY), TextureIndex = textureIndex, Color = color},
                    new GlyphVertex() {Position = position + pos + bearing + (_faces[pixelSize].Glyphs[line[i]].Size.X, -_faces[pixelSize].Glyphs[line[i]].Size.Y), TextureCoordinates = (textureCoordinateX, 0), TextureIndex = textureIndex, Color = color}
                );

                pos.X += _faces[pixelSize].Glyphs[line[i]].Advance.X;

            }

            for (int i = index; i < _vertices.Count; i++)
            {

                GlyphVertex vertex = _vertices[i];

                switch (justifyMode)
                {

                    case JustifyMode.HorizontalLeftVerticalCentered:
                    case JustifyMode.Left:
                        break;
                    case JustifyMode.HorizontalRightVerticalCentered:
                    case JustifyMode.Right:
                        vertex.Position.X += containerDimensions.X - textLineWidth;
                        break;
                    case JustifyMode.Centered:
                        vertex.Position.X += (containerDimensions.X - textLineWidth) / 2;
                        break;

                }

                _vertices[i] = vertex;

            }

            index = _vertices.Count;

            pos.Y += _faces[pixelSize].LineHeight;

        }

        for (int i = 0; i < _vertices.Count / 4; i++)
        {

            _indices.AddRange(
                0 + (i*4),
                1 + (i*4),
                2 + (i*4),
                2 + (i*4),
                3 + (i*4),
                0 + (i*4)
            );

        }

        for (int i = 0; i < _vertices.Count; i++)
        {

            GlyphVertex vertex = _vertices[i];
            Vector2 offsets = Vector2.Zero;

            switch (justifyMode)
            {

                case JustifyMode.Left:
                case JustifyMode.Right:
                    offsets.Y = -containerDimensions.Y;
                    break;
                case JustifyMode.Centered:
                case JustifyMode.HorizontalLeftVerticalCentered:
                case JustifyMode.HorizontalRightVerticalCentered:
                    offsets.Y = -containerDimensions.Y + ((containerDimensions.Y - textBlockBounds.Y) / 2);
                    break;

            }

            vertex.Position += offsets;

            _vertices[i] = vertex;

        }

        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ibo);
        GL.DeleteVertexArray(_vao);

        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData<GlyphVertex>(BufferTarget.ArrayBuffer, _vertices.Count * Marshal.SizeOf<GlyphVertex>(), CollectionsMarshal.AsSpan(_vertices), BufferUsage.DynamicDraw);
        
        _ibo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
        GL.BufferData<int>(BufferTarget.ElementArrayBuffer, _indices.Count * sizeof(int), CollectionsMarshal.AsSpan(_indices), BufferUsage.DynamicDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<GlyphVertex>(), Marshal.OffsetOf<GlyphVertex>(nameof(GlyphVertex.Position)));
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<GlyphVertex>(), Marshal.OffsetOf<GlyphVertex>(nameof(GlyphVertex.TextureCoordinates)));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribIPointer(2, 1, VertexAttribIType.Int, Marshal.SizeOf<GlyphVertex>(), Marshal.OffsetOf<GlyphVertex>(nameof(GlyphVertex.TextureIndex)));
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<GlyphVertex>(), Marshal.OffsetOf<GlyphVertex>(nameof(GlyphVertex.Color)));
        GL.EnableVertexAttribArray(3);

        _glyphShader.Use();
        GL.BindTextureUnit(0, _faces[pixelSize].FaceTexture.TextureId);

        GL.UniformMatrix4f(GL.GetUniformLocation(_glyphShader.Handle, "uView"), 1, true, ref _glyphCamera.ViewMatrix);
        GL.UniformMatrix4f(GL.GetUniformLocation(_glyphShader.Handle, "uProjection"), 1, true, ref _glyphCamera.ProjectionMatrix);

        GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);

    }

    public static void Initialize()
    {

        FT_LibraryRec_* library;
        FT.FT_Init_FreeType(&library);
        _library = library;

        _glyphShader = new Shader("glyph.vert", "glyph.frag");

        FontFile = Path.Combine("Resources", "Fonts", "NotoSansJP-Regular.ttf");

        long flags = _face->face_flags;

        Console.WriteLine($"Has kerning? {(flags & (long)FT_FACE_FLAG.FT_FACE_FLAG_KERNING) != 0}");

    }

    public static void Free()
    {

        _libraryHandle.Free();
        _faceHandle.Free();

    }

    private static void CheckError()
    {

        if (_error != FT_Error.FT_Err_Ok)
        {
            GameLogger.ThrowError($"Freetype method failed with error {_error}");
        } else
        {
            GameLogger.Log("No freetype errors present");
        }

    }
    public static void GrabGlyph(uint pixelSize, char character)
    {

        // GameLogger.Log($"{character} was grabbed with pixel size {pixelSize}");

        _error = FT.FT_Set_Pixel_Sizes(_face, 0, pixelSize);
        
        uint charIndex = FT.FT_Get_Char_Index(_face, character);

        FT.FT_Load_Glyph(_face, charIndex, FT_LOAD.FT_LOAD_DEFAULT);
        FT.FT_Render_Glyph(_face->glyph, FT_Render_Mode_.FT_RENDER_MODE_NORMAL);

        GlyphData glyphData = new GlyphData();

        glyphData.Advance = ((int)_face->glyph->advance.x >> 6, (int)_face->glyph->advance.y >> 6);
        glyphData.Bearing = (_face->glyph->bitmap_left, _face->glyph->bitmap_top);
        glyphData.Size = ((int)_face->glyph->bitmap.width, (int)_face->glyph->bitmap.rows);

        int ascent = (int) _face->size->metrics.ascender >> 6;
        int descent = (int) _face->size->metrics.descender >> 6;

        FaceData faceData = _faces[pixelSize];
        faceData.Ascender = ascent;
        faceData.Descender = descent;
        faceData.LineHeight = (int) _face->size->metrics.height >> 6;
        faceData.FaceTexture.AddGlyph(character, _face->glyph->bitmap);
        faceData.Glyphs.Add(character, glyphData);

        _faces[pixelSize] = faceData;

    }

    public static void Resize()
    {

        _glyphCamera.UpdateProjectionMatrix();

    }

    public static void ClearCache()
    {

        foreach (FaceData faceData in _faces.Values)
        {

            faceData.FaceTexture.Free();

        }
        _faces.Clear();

    }

}