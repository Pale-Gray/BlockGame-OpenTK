using static FreeTypeSharp.FT;
using OpenTK.Mathematics;
using System.Collections.Generic;
using FreeTypeSharp;
using System.Runtime.InteropServices;
using System;
using OpenTK.Graphics.OpenGL;
using Blockgame_OpenTK.Util;
using System.IO;

namespace Blockgame_OpenTK.Font
{
    class CachedFontRenderer
    {

        struct CachedFontVertex
        {

            public Vector3 Position;
            public Vector2 TextureCoordinate;
            public ulong TextureHandle;

            public CachedFontVertex(Vector3 position, Vector2 textureCoordinate, ulong textureHandle)
            {

                Position = position;
                TextureCoordinate = textureCoordinate;
                TextureHandle = textureHandle;

            }

        }

        struct CachedGlyphData
        {

            public Texture GlyphTexture;
            public ulong TextureHandle;
            public bool IsTextureHandleResident;
            public Vector2 Size;
            public Vector2 Bearing;
            public Vector2 Advance;

        }

        private static Dictionary<char, CachedGlyphData> _glyphData = new Dictionary<char, CachedGlyphData>();
        private static int _vao, _vbo;

        public static void RenderFont(Vector2 position, int layer, float size, string text, string fontName)
        {

            Vector3 p = (position.X, position.Y, layer);
            List<CachedFontVertex> textVertices = new List<CachedFontVertex>();
            Vector3 currentGlyphPosition = Vector3.Zero;
            for (int i = 0; i < text.Length; i++)
            {

                if (!_glyphData.ContainsKey(text[i]))
                {

                    GenerateGlyphData(text[i], size, fontName);

                }
                if (_glyphData[text[i]].Size != Vector2.Zero)
                {

                    CachedGlyphData data = _glyphData[text[i]];
                    CachedFontVertex[] currentGlyphVertices =
                    {

                        // new CachedFontVertex(currentGlyphPosition + (_glyphData]]), (0, 1), _glyphData[text[i]].TextureHandle),
                        new CachedFontVertex(currentGlyphPosition + (data.Bearing.X, -data.Size.Y + (data.Size.Y - data.Bearing.Y), 0), (0, 0), data.TextureHandle),
                        new CachedFontVertex(currentGlyphPosition + (data.Bearing.X, (data.Size.Y - data.Bearing.Y), 0), (0, 1), data.TextureHandle),
                        new CachedFontVertex(currentGlyphPosition + (data.Bearing.X + data.Size.X, (data.Size.Y - data.Bearing.Y), 0), (1, 1), data.TextureHandle),
                        new CachedFontVertex(currentGlyphPosition + (data.Bearing.X + data.Size.X, (data.Size.Y - data.Bearing.Y), 0), (1, 1), data.TextureHandle),
                        new CachedFontVertex(currentGlyphPosition + (data.Bearing.X + data.Size.X, -data.Size.Y + (data.Size.Y - data.Bearing.Y), 0), (1, 0), data.TextureHandle),
                        new CachedFontVertex(currentGlyphPosition + (data.Bearing.X, -data.Size.Y + (data.Size.Y - data.Bearing.Y), 0), (0, 0), data.TextureHandle)
                        // new CachedFontVertex(currentGlyphPosition + (_glyphData[text[i]].Bearing.X, 0, 0), (0, 1), _glyphData[text[i]].TextureHandle),
                        // new CachedFontVertex((0, _glyphData[text[i]].Size.Y, 0) + currentGlyphPosition + (_glyphData[text[i]].Bearing.X, 0, 0), (0, 0), _glyphData[text[i]].TextureHandle),
                        // new CachedFontVertex((_glyphData[text[i]].Size.X, _glyphData[text[i]].Size.Y, 0) + currentGlyphPosition + (_glyphData[text[i]].Bearing.X, 0, 0), (1, 0), _glyphData[text[i]].TextureHandle),
                        // new CachedFontVertex((_glyphData[text[i]].Size.X, _glyphData[text[i]].Size.Y, 0) + currentGlyphPosition + (_glyphData[text[i]].Bearing.X, 0, 0), (1, 0), _glyphData[text[i]].TextureHandle),
                        // new CachedFontVertex((_glyphData[text[i]].Size.X, 0, 0) + currentGlyphPosition + (_glyphData[text[i]].Bearing.X, 0, 0), (1, 1), _glyphData[text[i]].TextureHandle),
                        // new CachedFontVertex(currentGlyphPosition + (_glyphData[text[i]].Bearing.X, 0, 0), (0, 1), _glyphData[text[i]].TextureHandle),

                    };
                    for (int v = 0; v < currentGlyphVertices.Length; v++)
                    {
                        // currentGlyphVertices[v].Position.Y *= -1.0f;
                        currentGlyphVertices[v].Position += p;
                    }
                    textVertices.AddRange(currentGlyphVertices);

                }
                currentGlyphPosition.X += (int)_glyphData[text[i]].Advance.X >> 6;

            }
            CachedFontVertex[] textVerticesArray = textVertices.ToArray();
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
            GL.VertexAttribLPointer(2, 1, (VertexAttribLType) VertexAttribPointerType.UnsignedInt64Arb, Marshal.SizeOf<CachedFontVertex>(), Marshal.OffsetOf<CachedFontVertex>(nameof(CachedFontVertex.TextureHandle)));
            GL.EnableVertexAttribArray(2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            GlobalValues.CachedFontShader.Use();

            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "view"), 1, true, GlobalValues.GuiCamera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.CachedFontShader.id, "projection"), 1, true, GlobalValues.GuiCamera.ProjectionMatrix);

            GL.BindVertexArray(_vao);

            // GL.Disable(EnableCap.CullFace);

            GL.DrawArrays(PrimitiveType.Triangles, 0, textVerticesArray.Length);

            // GL.Enable(EnableCap.CullFace);

            GL.BindVertexArray(0);

            GlobalValues.CachedFontShader.UnUse();

        }

        private unsafe static void GenerateGlyphData(char character, float size, string pathToFont)
        {

            FT_LibraryRec_* library;
            FT_FaceRec_* face;
            FT_Error error = FT_Init_FreeType(&library);

            error = FT_New_Face(library, (byte*)Marshal.StringToHGlobalAnsi(pathToFont), 0, &face);
            error = FT_Set_Pixel_Sizes(face, 0, (uint)size);
            error = FT_Load_Char(face, character, FT_LOAD.FT_LOAD_RENDER);
            CachedGlyphData glyphData = new CachedGlyphData();
            if ((int)face->glyph->bitmap.width != 0 && (int)face->glyph->bitmap.rows != 0)
            {

                glyphData.GlyphTexture = new Texture((nint)face->glyph->bitmap.buffer, (int)face->glyph->bitmap.width, (int)face->glyph->bitmap.rows);
                glyphData.Size = (face->glyph->bitmap.width, face->glyph->bitmap.rows);
                // Console.WriteLine(glyphData.TextureHandle); // TODO: maybe after vao stuff?
                glyphData.TextureHandle = GL.ARB.GetTextureHandleARB(glyphData.GlyphTexture.GetID());
                if (glyphData.IsTextureHandleResident == false)
                {

                    GL.ARB.MakeTextureHandleResidentARB(glyphData.TextureHandle);
                    glyphData.IsTextureHandleResident = true;

                }

            }
            Console.WriteLine($"Texture handle for glyph {character} is {glyphData.TextureHandle}{(glyphData.TextureHandle == 0 ? " (no texture creation)": "")} with dimensions {glyphData.Size}");
            glyphData.Size = (face->glyph->bitmap.width, face->glyph->bitmap.rows);
            glyphData.Bearing = (face->glyph->bitmap_left, face->glyph->bitmap_top);
            glyphData.Advance = (face->glyph->advance.x, face->glyph->advance.y);
            _glyphData.Add(character, glyphData);

        }

    }
}
