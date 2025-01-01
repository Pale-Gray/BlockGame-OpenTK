using static FreeTypeSharp.FT;
using OpenTK.Mathematics;
using System.Collections.Generic;
using FreeTypeSharp;
using System.Runtime.InteropServices;
using System;
using OpenTK.Graphics.OpenGL;
using Blockgame_OpenTK.Util;
using System.IO;
using OpenTK.Graphics.Vulkan;
using System.Runtime.CompilerServices;

namespace Blockgame_OpenTK.Font
{
    class CachedFontRenderer
    {

        struct CachedFontVertex
        {

            public Vector3 Position;
            public Vector2 TextureCoordinate;
            public float TextureIndex = 0;
            public Vector4 Color = Vector4.One;

            public CachedFontVertex(Vector3 position, Vector2 textureCoordinate, float textureIndex)
            {

                Position = position;
                TextureCoordinate = textureCoordinate;
                TextureIndex = textureIndex;

            }

        }

        struct CachedGlyphData
        {

            public float TextureIndex;
            public Vector2 Size;
            public Vector2 Bearing;
            public Vector2 Advance;

        }

        // private static Dictionary<char, CachedGlyphData> _glyphData = new Dictionary<char, CachedGlyphData>();
        private static int _vao, _vbo;
        // private static float _fontSize = 16.0f;
        // private static float _textureClearanceScale = 2.0f;

        private static Dictionary<(char, int), CachedGlyphData> _variableSizedGlyphData = new();
        private static Dictionary<int, ArrayTexture> _variabledSizedArrayTexture = new();

        private static float _ascent = 0.0f;
        private static float _descent = 0.0f;

        // private static ArrayTexture _glyphArrays;

        struct CharFormatData
        {

            public Color3<Rgb> Color;
            public bool IsWavy;
            public bool IsItalics;
            public int Start;
            public int Length;

        }

        public static void Init()
        {

            // _glyphArrays = new ArrayTexture((int)(_fontSize * _textureClearanceScale), (int) (_fontSize*_textureClearanceScale));
            // _glyphArrays.Init();

        }

        private static unsafe FT_LibraryRec_* _library;
        private static unsafe FT_FaceRec_* _face;
        private static FT_Error _error;

        public static unsafe void Initialize(string pathToFont)
        {



        }

        public static void RenderFont(Vector2 position, Vector2 origin, float layer, int size, string text, string fontName, Color4<Rgba> color, Vector2? bounds = null)
        {

            if (!_variabledSizedArrayTexture.ContainsKey(size))
            {

                _variabledSizedArrayTexture.Add(size, new ArrayTexture(size * 2, size * 2));
                _variabledSizedArrayTexture[size].Init();

            }

            Vector3 p = (position.X, position.Y, layer);
            List<CachedFontVertex> textVertices = new List<CachedFontVertex>();
            List<ulong> textSamplers = new List<ulong>();
            Vector3 currentGlyphPosition = Vector3.Zero;
            float aspect = 1.0f;
            for (int i = 0; i < text.Length; i++)
            {

                if (!_variableSizedGlyphData.ContainsKey((text[i], size)))
                {

                    GenerateGlyphData(text[i], size, fontName);

                }
                if (_variableSizedGlyphData[(text[i], size)].Size != Vector2.Zero)
                {

                    CachedGlyphData data = _variableSizedGlyphData[(text[i], size)];
                    Vector2 texCoordOffset = Vector2.One - (data.Size / (size * 2));// Vector2.One - (data.Size / data.Size); //Vector2.One;// - (data.Size / (size * 2));
                    CachedFontVertex[] currentGlyphVertices =
                    {

                        new CachedFontVertex(currentGlyphPosition + (data.Bearing.X * aspect, (-data.Size.Y + (data.Size.Y - data.Bearing.Y)) * aspect, 0), (0, 0), data.TextureIndex),
                        new CachedFontVertex(currentGlyphPosition + (data.Bearing.X * aspect, (data.Size.Y - data.Bearing.Y) * aspect, 0), (0, 1 - texCoordOffset.Y), data.TextureIndex),
                        new CachedFontVertex(currentGlyphPosition + ((data.Bearing.X + data.Size.X) * aspect, (data.Size.Y - data.Bearing.Y) * aspect, 0), (1 - texCoordOffset.X, 1 - texCoordOffset.Y), data.TextureIndex),
                        new CachedFontVertex(currentGlyphPosition + ((data.Bearing.X + data.Size.X) * aspect, (data.Size.Y - data.Bearing.Y) * aspect, 0), (1 - texCoordOffset.X, 1 - texCoordOffset.Y), data.TextureIndex),
                        new CachedFontVertex(currentGlyphPosition + ((data.Bearing.X + data.Size.X) * aspect, (-data.Size.Y + (data.Size.Y - data.Bearing.Y)) * aspect, 0), (1 - texCoordOffset.X, 0), data.TextureIndex),
                        new CachedFontVertex(currentGlyphPosition + (data.Bearing.X * aspect, (-data.Size.Y + (data.Size.Y - data.Bearing.Y)) * aspect, 0), (0, 0), data.TextureIndex)

                    };
                    for (int v = 0; v < currentGlyphVertices.Length; v++)
                    {

                        currentGlyphVertices[v].Position += p;

                    }
                    textVertices.AddRange(currentGlyphVertices);

                }
                currentGlyphPosition.X += ((int)_variableSizedGlyphData[(text[i], size)].Advance.X >> 6) * aspect;

            }
            float width = 0;
            float height = size;
            for (int i = 0; i < textVertices.Count; i++)
            {

                width = Math.Max(width, textVertices[i].Position.X - p.X);
                // height = Math.Abs(Math.Min(height, textVertices[i].Position.Y - p.Y));

            }

            Color3<Hsv> hsv = new Color3<Hsv>();
            hsv.X = (float) (GlobalValues.Time/5.0 % 1.0);
            hsv.Y = 1;
            hsv.Z = 1;
            // Console.WriteLine($"{width}, {height}");
            CachedFontVertex[] textVerticesArray = textVertices.ToArray();
            for (int i = 0; i < textVerticesArray.Length; i++)
            {

                textVerticesArray[i].Position -= (width * origin.X, -height * origin.Y, 0.0f);
                textVerticesArray[i].Color = (Vector4) color;

            }
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, textVerticesArray.Length * Marshal.SizeOf<CachedFontVertex>(), textVerticesArray, BufferUsage.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<CachedFontVertex>(), Marshal.OffsetOf<CachedFontVertex>(nameof(CachedFontVertex.Position)));
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<CachedFontVertex>(), Marshal.OffsetOf<CachedFontVertex>(nameof(CachedFontVertex.TextureCoordinate)));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<CachedFontVertex>(), Marshal.OffsetOf<CachedFontVertex>(nameof(CachedFontVertex.Color)));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, Marshal.SizeOf<CachedFontVertex>(), Marshal.OffsetOf<CachedFontVertex>(nameof(CachedFontVertex.TextureIndex)));
            GL.EnableVertexAttribArray(3);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            GlobalValues.CachedFontShader.Use();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2dArray, _variabledSizedArrayTexture[size].TextureID);

            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "view"), 1, true, ref GlobalValues.GuiCamera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "projection"), 1, true, ref GlobalValues.GuiCamera.ProjectionMatrix);
            GL.Uniform4f(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "color"), 1, (Vector4)color);
            GL.Uniform1f(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "time"), 1, (float) GlobalValues.Time);
            GL.BindVertexArray(_vao);

            GL.DrawArrays(PrimitiveType.Triangles, 0, textVerticesArray.Length);

        }

        private unsafe static void GenerateGlyphData(char character, int size, string pathToFont)
        {

            FT_LibraryRec_* library;
            FT_FaceRec_* face;
            FT_Error error = FT_Init_FreeType(&library);

            // _ascent = face->ascender;
            // _descent = face->descender;

            // Console.WriteLine($"trying to render {character}");

            error = FT_New_Face(library, (byte*)Marshal.StringToHGlobalAnsi(pathToFont), 0, &face);
            error = FT_Set_Pixel_Sizes(face, 0, (uint)size);
            error = FT_Load_Char(face, character, FT_LOAD.FT_LOAD_RENDER);

            _ascent = face->ascender;
            _descent = face->descender;

            CachedGlyphData glyphData = new CachedGlyphData();
            // Console.WriteLine(_glyphArrays==null);

            _variabledSizedArrayTexture[size].AddTexture((nint)face->glyph->bitmap.buffer, (int)face->glyph->bitmap.width, (int)face->glyph->bitmap.rows, out float index);
            // Console.WriteLine($"character {character} has a texture index of {index}, dimensions {face->glyph->bitmap.width}, {face->glyph->bitmap.rows} with error of {error.ToString()}");
            // glyphData.GlyphTexture = new Texture((nint)face->glyph->bitmap.buffer, (int)face->glyph->bitmap.width, (int)face->glyph->bitmap.rows);
            glyphData.TextureIndex = index;
            glyphData.Size = (face->glyph->bitmap.width, face->glyph->bitmap.rows);
            // glyphData.Size = (face->glyph->bitmap.width * (uint)_textureClearanceScale, face->glyph->bitmap.rows * (uint)_textureClearanceScale);
            glyphData.Bearing = (face->glyph->bitmap_left, face->glyph->bitmap_top);
            glyphData.Advance = (face->glyph->advance.x, face->glyph->advance.y);

            _variableSizedGlyphData.Add((character, size), glyphData);

        }

    }
}
