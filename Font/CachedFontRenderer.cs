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
            public Vector3 Color;

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

        private static Dictionary<char, CachedGlyphData> _glyphData = new Dictionary<char, CachedGlyphData>();
        private static int _vao, _vbo;
        private static float _fontSize = 48.0f;

        private static ArrayTexture _glyphArrays;
        public static void Init()
        {

            _glyphArrays = new ArrayTexture((int)_fontSize, (int)_fontSize);
            _glyphArrays.Init();

        }

        struct CharFormatData
        {

            public Color3<Rgb> Color;
            public bool IsWavy;
            public bool IsItalics;
            public int Start;
            public int Length;

        }
        public static void RenderFontSpecialText(Vector2 position, Vector2 origin, int layer, float size, string text, string fontName, Color3<Rgb> color)
        {

            List<CharFormatData> charData = new List<CharFormatData>();
            for (int i = 0; i < text.Length; i++)
            {

                if (text[i] == '[')
                {

                    CharFormatData formatData = new CharFormatData();
                    int start = i;
                    int end = i;
                    for (int startIndex = i+1; startIndex < text.Length; startIndex++)
                    {

                        if (text[startIndex] == ']')
                        {

                            end = startIndex;
                            int startingString = startIndex+1;
                            for (int stringStartingIndex = i+1;  stringStartingIndex < text.Length; stringStartingIndex++)
                            {

                                if (text[stringStartingIndex] == '[' || stringStartingIndex == text.Length - 1)
                                {

                                    formatData.Start = startingString;
                                    formatData.Length = stringStartingIndex - startingString + 1;

                                }

                            }

                        }

                    }
                    string data = text.Substring(start+1, (end - start - 1)).ToLower().Replace(" ", "");
                    string formatText = text.Substring(formatData.Start, formatData.Length);

                    string[] parameters = data.Split(',');
                    for (int c = 0; c < parameters.Length; c++)
                    {

                        switch (parameters[c])
                        {

                            case "wavy":
                                formatData.IsWavy = true;
                                break;
                            case "italics":
                                formatData.IsItalics = true;
                                break;
                            case "red":
                                formatData.Color = Color3.Red;
                                break;
                            case "blue":
                                formatData.Color = Color3.Blue;
                                break;
                            case "green":
                                formatData.Color = Color3.Green;
                                break;
                            case "yellow":
                                formatData.Color = Color3.Yellow;
                                break;
                            case "orange":
                                formatData.Color = Color3.Orange;
                                break;
                            case "purple":
                                formatData.Color = Color3.Purple;
                                break;
                            default:
                                if (parameters[i].StartsWith("0x"))
                                {

                                    byte r = byte.Parse(parameters[i].Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                                    byte g = byte.Parse(parameters[i].Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                                    byte b = byte.Parse(parameters[i].Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

                                    float rFloat = r / (float)255;
                                    float gFloat = g / (float)255;
                                    float bFloat = b / (float)255;

                                    formatData.Color = new Color3<Rgb>(rFloat,gFloat,bFloat);

                                }
                                break;

                        }

                    }
                    charData.Add(formatData);

                }

            }

            foreach (CharFormatData data in charData)
            {

                Console.WriteLine($"{data.Color}, {data.Start}, {data.Length}, {data.IsWavy}, {data.IsItalics}");

            }

            RenderFont(position, origin, layer, size, text, fontName, color);

        }

        public static void RenderFont(Vector2 position, Vector2 origin, int layer, float size, string text, string fontName, Color3<Rgb> color)
        {

            Vector3 p = (position.X, position.Y, -layer);
            List<CachedFontVertex> textVertices = new List<CachedFontVertex>();
            List<ulong> textSamplers = new List<ulong>();
            Vector3 currentGlyphPosition = Vector3.Zero;
            float aspect = size / _fontSize;
            for (int i = 0; i < text.Length; i++)
            {

                if (!_glyphData.ContainsKey(text[i]))
                {

                    GenerateGlyphData(text[i], size, fontName);

                }
                if (_glyphData[text[i]].Size != Vector2.Zero)
                {

                    CachedGlyphData data = _glyphData[text[i]];
                    Vector2 texCoordOffset = Vector2.One - (data.Size / _fontSize);
                    CachedFontVertex[] currentGlyphVertices =
                    {

                        new CachedFontVertex(currentGlyphPosition + (data.Bearing.X * aspect, (-data.Size.Y + (data.Size.Y - data.Bearing.Y)) * aspect, 0), (0, 0), _glyphData[text[i]].TextureIndex),
                        new CachedFontVertex(currentGlyphPosition + (data.Bearing.X * aspect, (data.Size.Y - data.Bearing.Y) * aspect, 0), (0, 1 - texCoordOffset.Y), _glyphData[text[i]].TextureIndex),
                        new CachedFontVertex(currentGlyphPosition + ((data.Bearing.X + data.Size.X) * aspect, (data.Size.Y - data.Bearing.Y) * aspect, 0), (1 - texCoordOffset.X, 1 - texCoordOffset.Y), _glyphData[text[i]].TextureIndex),
                        new CachedFontVertex(currentGlyphPosition + ((data.Bearing.X + data.Size.X) * aspect, (data.Size.Y - data.Bearing.Y) * aspect, 0), (1 - texCoordOffset.X, 1 - texCoordOffset.Y), _glyphData[text[i]].TextureIndex),
                        new CachedFontVertex(currentGlyphPosition + ((data.Bearing.X + data.Size.X) * aspect, (-data.Size.Y + (data.Size.Y - data.Bearing.Y)) * aspect, 0), (1 - texCoordOffset.X, 0), _glyphData[text[i]].TextureIndex),
                        new CachedFontVertex(currentGlyphPosition + (data.Bearing.X * aspect, (-data.Size.Y + (data.Size.Y - data.Bearing.Y)) * aspect, 0), (0, 0), _glyphData[text[i]].TextureIndex)

                    };
                    for (int v = 0; v < currentGlyphVertices.Length; v++)
                    {

                        currentGlyphVertices[v].Position += p;

                    }
                    textVertices.AddRange(currentGlyphVertices);

                }
                currentGlyphPosition.X += ((int)_glyphData[text[i]].Advance.X >> 6) * aspect;

            }
            float width = 0;
            float height = 0;
            for (int i = 0; i < textVertices.Count; i++)
            {

                width = Math.Max(width, textVertices[i].Position.X - p.X);
                height = Math.Abs(Math.Min(height, textVertices[i].Position.Y - p.Y));

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
            GL.BindTexture(TextureTarget.Texture2dArray, _glyphArrays.TextureID);

            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "view"), 1, true, GlobalValues.GuiCamera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "projection"), 1, true, GlobalValues.GuiCamera.ProjectionMatrix);
            GL.Uniform3f(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "color"), 1, (Vector3)color);
            GL.Uniform1f(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "time"), 1, (float) GlobalValues.Time);
            // GL.ARB.UniformHandleui64vARB(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "glyphSamplers"), textSamplers.Count, textSamplers.ToArray());

            GL.BindVertexArray(_vao);

            // GL.Disable(EnableCap.CullFace);

            GL.DrawArrays(PrimitiveType.Triangles, 0, textVerticesArray.Length);

            // GL.Enable(EnableCap.CullFace);

            // GL.BindVertexArray(0);

            // GlobalValues.CachedFontShader.UnUse();

        }

        private unsafe static void GenerateGlyphData(char character, float size, string pathToFont)
        {

            FT_LibraryRec_* library;
            FT_FaceRec_* face;
            FT_Error error = FT_Init_FreeType(&library);

            error = FT_New_Face(library, (byte*)Marshal.StringToHGlobalAnsi(pathToFont), 0, &face);
            error = FT_Set_Pixel_Sizes(face, 0, (uint)_fontSize);
            error = FT_Load_Char(face, character, FT_LOAD.FT_LOAD_RENDER);

            CachedGlyphData glyphData = new CachedGlyphData();
            _glyphArrays.AddTexture((nint)face->glyph->bitmap.buffer, (int)face->glyph->bitmap.width, (int)face->glyph->bitmap.rows, out float index);
            Console.WriteLine($"character {character} has a texture index of {index}");
            // glyphData.GlyphTexture = new Texture((nint)face->glyph->bitmap.buffer, (int)face->glyph->bitmap.width, (int)face->glyph->bitmap.rows);
            glyphData.TextureIndex = index;
            glyphData.Size = (face->glyph->bitmap.width, face->glyph->bitmap.rows);
            glyphData.Size = (face->glyph->bitmap.width, face->glyph->bitmap.rows);
            glyphData.Bearing = (face->glyph->bitmap_left, face->glyph->bitmap_top);
            glyphData.Advance = (face->glyph->advance.x, face->glyph->advance.y);
            _glyphData.Add(character, glyphData);

        }

    }
}
