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
using System.Globalization;

namespace Blockgame_OpenTK.Font
{
    class CachedFontRenderer
    {

        struct CachedFontVertex
        {

            public Vector3 Position;
            public Vector2 TextureCoordinate;
            public float TextureIndex = 0;
            public float CharIndex = 0;
            public Vector4 WiggleWaggleParameters = Vector4.Zero;
            public float WiggleWaggleSmoothnessFactor = 1.0f;
            public Vector4 Color = Vector4.One;
            public uint FormatFlags = 0; // 0000, rainbow, underline flag, wiggle, italics, LTR.
            public float ItalicPortion = 0;

            public CachedFontVertex(Vector3 position, Vector2 textureCoordinate, float textureIndex, CharFormattingData formattingData, float italicPortion)
            {

                Position = position;
                TextureCoordinate = textureCoordinate;
                TextureIndex = textureIndex;
                FormatFlags = FormatFlags | (formattingData.IsItalics ? 1u : 0u); // equivalent of first bit
                FormatFlags = FormatFlags | (formattingData.IsWiggleWaggle ? 2u : 0u); // equivalent of second bit
                FormatFlags = FormatFlags | (formattingData.IsUnderlined ? 4u : 0u); // equivalent of third bit
                FormatFlags = FormatFlags | (formattingData.IsRainbow ? 8u : 0u); // equivalent of fourth bit
                WiggleWaggleParameters = formattingData.WiggleWaggleParameters;
                WiggleWaggleSmoothnessFactor = formattingData.WiggleWaggleSmoothnessFactor;
                CharIndex = formattingData.CharIndex;
                Color = (Vector4)formattingData.Color;
                ItalicPortion = italicPortion;

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
        private static string _fontPath = "";
        public static string FontPath
        {
            get => _fontPath;
            set
            {
                _fontPath = value;
                foreach (ArrayTexture tex in _variabledSizedArrayTexture.Values) tex.Dispose();
                _variabledSizedArrayTexture.Clear();
                _variableSizedGlyphData.Clear();

            }
        }
        // private static float _fontSize = 16.0f;
        // private static float _textureClearanceScale = 2.0f;

        private static Dictionary<(char, int), CachedGlyphData> _variableSizedGlyphData = new();
        private static Dictionary<int, ArrayTexture> _variabledSizedArrayTexture = new();
        private static Dictionary<int, (float, float, float)> _variableSizedUnderlineAndLinegap = new();

        private static float _ascent = 0.0f;
        private static float _descent = 0.0f;
        private static float _linegap = 0.0f;

        // private static ArrayTexture _glyphArrays;

        struct CharFormattingData
        {

            public bool ShouldRender = true;
            public bool IsWiggleWaggle = false;
            public bool IsItalics = false;
            public bool IsUnderlined = false;
            public bool IsRainbow = false;
            public float CharIndex = 0;
            public Vector4 WiggleWaggleParameters = Vector4.Zero;
            public float WiggleWaggleSmoothnessFactor = 1.0f;
            public Color4<Rgba> Color;
            public CharFormattingData(Color4<Rgba> color)
            {
                Color = color;
            }

        }

        public static void Init()
        {

            // _glyphArrays = new ArrayTexture((int)(_fontSize * _textureClearanceScale), (int) (_fontSize*_textureClearanceScale));
            // _glyphArrays.Init();

        }

        private static unsafe FT_LibraryRec_* _library;
        private static unsafe FT_FaceRec_* _face;
        private static FT_Error _error;
        private static List<CharFormattingData> _currentTextFormattingData = new();

        public static void RenderFont(Vector2 position, Vector2 origin, float layer, int size, string text, Color4<Rgba>? color = null, Vector2? bounds = null, float lineSpacing = 1.0f)
        {

            if (color == null) color = Color4.Black;

            if (_fontPath == null || _fontPath == string.Empty)
            {

                Debugger.Log("Font path isn't set. Cannot render text", Severity.Error);
                return;

            }

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

                    GenerateGlyphData(text[i], size);

                }

            }

            bool shouldBeHidden = false;
            CharFormattingData charFormatData = new CharFormattingData(color ?? Color4.Black);
            _currentTextFormattingData.Clear();
            bool hasGradient = false;
            bool startGradient = false;
            Color4<Rgba> gradientStart = Color4.Black;
            Color4<Rgba> gradientEnd = Color4.Black;
            float gradientTextLength = 0;
            float currentCount = 0;
            int currentIndex = 0;
            for (int i = 0; i < text.Length; i++)
            {
                
                if (text[i] == '!')
                {

                    if (i+1 < text.Length && text[i+1] == '[')
                    {

                        if (i+2 < text.Length)
                        {

                            if (text[i+2] == 'i')
                            {

                                if (i + 3 < text.Length && text[i + 3] == ']')
                                {

                                    charFormatData.IsItalics = true;
                                    shouldBeHidden = true;

                                }

                            }

                            if (text[i+2] == 'd')
                            {

                                if (i+3 < text.Length && text[i+3] == ']')
                                {

                                    charFormatData = new CharFormattingData(color ?? Color4.Black);
                                    shouldBeHidden = true;

                                }

                            }

                            if (text[i+2] == 'w')
                            {

                                for (int s = i+2; s < text.Length; s++)
                                {

                                    if (text[s] == ']')
                                    {

                                        string[] parameters = text.Substring(i + 2, s - (i + 2)).Split(',');
                                        // Console.WriteLine(parameters.Length);
                                        if (parameters.Length == 6) // w, xa, xs, ya, ys, s
                                        {

                                            if (float.TryParse(parameters[1], out float xAmp) &&
                                                float.TryParse(parameters[2], out float xTime) &&
                                                float.TryParse(parameters[3], out float yAmp) &&
                                                float.TryParse(parameters[4], out float yTime) &&
                                                float.TryParse(parameters[5], out float smooth))
                                            {

                                                charFormatData.WiggleWaggleParameters = (xAmp, xTime, yAmp, yTime);
                                                charFormatData.WiggleWaggleSmoothnessFactor = smooth;
                                                charFormatData.IsWiggleWaggle = true;
                                                shouldBeHidden = true;

                                            }

                                        }

                                        break;

                                    }

                                }

                            }

                            if (text[i+2] == 'g')
                            {

                                for (int s = i+2; s < text.Length; s++)
                                {

                                    if (text[s] == ']')
                                    {

                                        string[] parameters = text.Substring(i + 2, s - (i + 2)).Split(',');
                                        if (parameters.Length == 4)
                                        {
                                            if (uint.TryParse(parameters[1], out uint len) &&
                                                Maths.TryStringHexColorToColor4(parameters[2], out Color4<Rgba> col1) &&
                                                Maths.TryStringHexColorToColor4(parameters[3], out Color4<Rgba> col2))
                                            {
                                                hasGradient = true;
                                                gradientStart = col1;
                                                gradientEnd = col2;
                                                gradientTextLength = len;
                                                shouldBeHidden = true;
                                            }
                                        }

                                        break;

                                    }

                                }

                            }

                            if (text[i+2] == '0')
                            {

                                if (i+3 < text.Length && text[i+3] == 'x')
                                {

                                    if (i+12 < text.Length)
                                    {

                                        if (text[i+12] == ']')
                                        {

                                            string stringBytes = text.Substring(i + 4, 8);
                                            if (Maths.TryStringHexColorToColor4(text.Substring(i+2, 10), out Color4<Rgba> c))
                                            {

                                                charFormatData.Color = c;
                                                shouldBeHidden = true;

                                            }

                                        }

                                    }

                                }

                            }

                        }

                    }

                }

                if (shouldBeHidden)
                {
                    charFormatData.ShouldRender = false;
                } else
                {
                    charFormatData.ShouldRender = true;
                }

                // escape character overrides
                if (text[i] == '\n')
                {
                    charFormatData.ShouldRender = false;
                }
                if (text[i] == '\r')
                {
                    charFormatData.ShouldRender = false;
                }

                if (shouldBeHidden == false && startGradient)
                {

                    charFormatData.Color = Maths.Mix(gradientStart, gradientEnd, currentCount / (gradientTextLength - 1));

                    if (currentCount == gradientTextLength)
                    {

                        charFormatData.Color = color ?? Color4.Black;
                        startGradient = false;

                    }

                    currentCount++;

                    if (startGradient == false)
                    {
                        currentCount = 0;
                    }

                }

                _currentTextFormattingData.Add(charFormatData);

                if (shouldBeHidden && text[i] == ']')
                {

                    shouldBeHidden = false;

                    if (hasGradient)
                    {

                        startGradient = true;
                        hasGradient = false;

                    }

                }

                if (charFormatData.ShouldRender)
                {

                    charFormatData.CharIndex++;

                }

            }

            for (int i = 0; i < text.Length; i++)
            {

                if (_currentTextFormattingData[i].ShouldRender)
                {

                    if (_variableSizedGlyphData[(text[i], size)].Size != Vector2.Zero)
                    {

                        CachedGlyphData data = _variableSizedGlyphData[(text[i], size)];
                        Vector2 texCoordOffset = Vector2.One - (data.Size / (size * 2));// Vector2.One - (data.Size / data.Size); //Vector2.One;// - (data.Size / (size * 2));
                        CachedFontVertex[] currentGlyphVertices =
                        {

                            new CachedFontVertex(currentGlyphPosition + (data.Bearing.X * aspect, (-data.Size.Y + (data.Size.Y - data.Bearing.Y)) * aspect, 0), (0, 0), data.TextureIndex, _currentTextFormattingData[i], 1),
                            new CachedFontVertex(currentGlyphPosition + (data.Bearing.X * aspect, (data.Size.Y - data.Bearing.Y) * aspect, 0), (0, 1 - texCoordOffset.Y), data.TextureIndex, _currentTextFormattingData[i], 0),
                            new CachedFontVertex(currentGlyphPosition + ((data.Bearing.X + data.Size.X) * aspect, (data.Size.Y - data.Bearing.Y) * aspect, 0), (1 - texCoordOffset.X, 1 - texCoordOffset.Y), data.TextureIndex, _currentTextFormattingData[i], 0),
                            new CachedFontVertex(currentGlyphPosition + ((data.Bearing.X + data.Size.X) * aspect, (data.Size.Y - data.Bearing.Y) * aspect, 0), (1 - texCoordOffset.X, 1 - texCoordOffset.Y), data.TextureIndex, _currentTextFormattingData[i], 0),
                            new CachedFontVertex(currentGlyphPosition + ((data.Bearing.X + data.Size.X) * aspect, (-data.Size.Y + (data.Size.Y - data.Bearing.Y)) * aspect, 0), (1 - texCoordOffset.X, 0), data.TextureIndex, _currentTextFormattingData[i], 1),
                            new CachedFontVertex(currentGlyphPosition + (data.Bearing.X * aspect, (-data.Size.Y + (data.Size.Y - data.Bearing.Y)) * aspect, 0), (0, 0), data.TextureIndex, _currentTextFormattingData[i], 1),

                            // underline portion
                            // new CachedFontVertex(currentGlyphPosition + (0, _variableSizedUnderlineAndLinegap[size].Item1, 0), (0, 0), 0, _currentTextFormattingData[i]),
                            // new CachedFontVertex(currentGlyphPosition + (0, _variableSizedUnderlineAndLinegap[size].Item1 + (_variableSizedUnderlineAndLinegap[size].Item2*2), 0) - (0, _variableSizedUnderlineAndLinegap[size].Item2, 0), (0, 0), 0, _currentTextFormattingData[i]),
                            // new CachedFontVertex(currentGlyphPosition + ((int)_variableSizedGlyphData[(text[i], size)].Advance.X >> 6, _variableSizedUnderlineAndLinegap[size].Item1 + (_variableSizedUnderlineAndLinegap[size].Item2*2), 0) - (0, _variableSizedUnderlineAndLinegap[size].Item2, 0), (0, 0), 0, _currentTextFormattingData[i]),
                            // new CachedFontVertex(currentGlyphPosition + ((int)_variableSizedGlyphData[(text[i], size)].Advance.X >> 6, _variableSizedUnderlineAndLinegap[size].Item1 + (_variableSizedUnderlineAndLinegap[size].Item2*2), 0) - (0, _variableSizedUnderlineAndLinegap[size].Item2, 0), (0, 0), 0, _currentTextFormattingData[i]),
                            // new CachedFontVertex(currentGlyphPosition + ((int)_variableSizedGlyphData[(text[i], size)].Advance.X >> 6, _variableSizedUnderlineAndLinegap[size].Item1, 0) - (0, _variableSizedUnderlineAndLinegap[size].Item2, 0), (0, 0), 0, _currentTextFormattingData[i]),
                            // new CachedFontVertex(currentGlyphPosition + (0, _variableSizedUnderlineAndLinegap[size].Item1, 0) - (0, _variableSizedUnderlineAndLinegap[size].Item2, 0), (0, 0), 0, _currentTextFormattingData[i])
                            
                        };
                        for (int v = 0; v < currentGlyphVertices.Length; v++)
                        {

                            currentGlyphVertices[v].Position += p;

                        }
                        textVertices.AddRange(currentGlyphVertices);

                    }
                    currentGlyphPosition.X += ((int)_variableSizedGlyphData[(text[i], size)].Advance.X >> 6) * aspect;
                    // _currentTextFormattingData[i].CharIndex++;

                }

                if (i + 1 < text.Length)
                {

                    if (text[i + 1] == '\n')
                    {

                        currentGlyphPosition.X = 0;
                        currentGlyphPosition.Y += _variableSizedUnderlineAndLinegap[size].Item3;

                    }

                }

                if (bounds != null)
                {

                    if (i + 1 < text.Length)
                    {

                        if (currentGlyphPosition.X + ((int)_variableSizedGlyphData[(text[i + 1], size)].Advance.X >> 6) + _variableSizedGlyphData[(text[i + 1], size)].Size.X > bounds?.X)
                        {

                            currentGlyphPosition.X = 0;
                            // currentGlyphPosition.Y += _ascent - _descent + (_linegap);
                            currentGlyphPosition.Y += _variableSizedUnderlineAndLinegap[size].Item3;

                        }
                        else
                        {

                        }

                    }

                }

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
                // textVerticesArray[i].Color = (Vector4) color;

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
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<CachedFontVertex>(), Marshal.OffsetOf<CachedFontVertex>(nameof(CachedFontVertex.Color)));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, Marshal.SizeOf<CachedFontVertex>(), Marshal.OffsetOf<CachedFontVertex>(nameof(CachedFontVertex.TextureIndex)));
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<CachedFontVertex>(), Marshal.OffsetOf<CachedFontVertex>(nameof(CachedFontVertex.WiggleWaggleParameters)));
            GL.EnableVertexAttribArray(4);
            GL.VertexAttribIPointer(5, 1, VertexAttribIType.UnsignedInt, Marshal.SizeOf<CachedFontVertex>(), Marshal.OffsetOf<CachedFontVertex>(nameof(CachedFontVertex.FormatFlags)));
            GL.EnableVertexAttribArray(5);
            GL.VertexAttribPointer(6, 1, VertexAttribPointerType.Float, false, Marshal.SizeOf<CachedFontVertex>(), Marshal.OffsetOf<CachedFontVertex>(nameof(CachedFontVertex.CharIndex)));
            GL.EnableVertexAttribArray(6);
            GL.VertexAttribPointer(7, 1, VertexAttribPointerType.Float, false, Marshal.SizeOf<CachedFontVertex>(), Marshal.OffsetOf<CachedFontVertex>(nameof(CachedFontVertex.WiggleWaggleSmoothnessFactor)));
            GL.EnableVertexAttribArray(7);
            GL.VertexAttribPointer(8, 1, VertexAttribPointerType.Float, false, Marshal.SizeOf<CachedFontVertex>(), Marshal.OffsetOf<CachedFontVertex>(nameof(CachedFontVertex.ItalicPortion)));
            GL.EnableVertexAttribArray(8);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            GlobalValues.CachedFontShader.Use();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2dArray, _variabledSizedArrayTexture[size].TextureID);

            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "view"), 1, true, ref GlobalValues.GuiCamera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "projection"), 1, true, ref GlobalValues.GuiCamera.ProjectionMatrix);
            GL.Uniform1f(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "time"), 1, (float) GlobalValues.Time);
            GL.Uniform1f(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "textSize"), 1, size);
            GL.BindVertexArray(_vao);

            GL.DrawArrays(PrimitiveType.Triangles, 0, textVerticesArray.Length);

        }

        private unsafe static void GenerateGlyphData(char character, int size)
        {

            FT_LibraryRec_* library;
            FT_FaceRec_* face;
            FT_Error error = FT_Init_FreeType(&library);

            error = FT_New_Face(library, (byte*)Marshal.StringToHGlobalAnsi(_fontPath), 0, &face);
            error = FT_Set_Pixel_Sizes(face, 0, (uint)size);
            error = FT_Load_Char(face, character, FT_LOAD.FT_LOAD_RENDER);

            // _ascent = face->size->metrics.ascender >> 6;
            // _descent = face->size->metrics.descender >> 6;
            // _linegap = face->size->metrics.height >> 6;

            // float yScale = face->size->metrics.p
            // float yScale = face->size->metrics.pixel
            float xScale = face->size->metrics.x_scale >> 6;
            float yScale = face->size->metrics.y_scale >> 6;

            // Console.WriteLine(FT_MulFix(face->underline_thickness, face->size->metrics.y_scale) >> 6);

            if (!_variableSizedUnderlineAndLinegap.ContainsKey(size))
            {

                _variableSizedUnderlineAndLinegap.Add(size, (-1 * (FT_MulFix(face->underline_position, face->size->metrics.y_scale) >> 6), FT_MulFix(face->underline_thickness, face->size->metrics.y_scale) >> 6, (face->size->metrics.height >> 6)));

            }

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
