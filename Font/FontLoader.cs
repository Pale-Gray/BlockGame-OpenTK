using System;
using System.Collections.Generic;

using System.Runtime.InteropServices;
using Blockgame_OpenTK.Util;
using FreeTypeSharp;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using static FreeTypeSharp.FT;

namespace Blockgame_OpenTK.Font
{
    internal unsafe class FontLoader
    {
        public static void InitializeFontLoader()
        {


        }

        public struct CharacterInformation
        {

            public Vector2 TextureAtlasStart;
            public Vector2 Size;
            public Vector2 Bearing;
            public int Advance;

        }

        private static Dictionary<char, CharacterInformation> Characters = new Dictionary<char, CharacterInformation>();
        private static int texture;
        private static int TextureWidth;
        private static int TextureHeight;
        private static Shader FontShader;
        private static int fontSize = 96;
        public static void LoadFont()
        {

            FT_LibraryRec_* library;
            FT_FaceRec_* face;
            FT_Error error = FT_Init_FreeType(&library);

            error = FT_New_Face(library, (byte*)Marshal.StringToHGlobalAnsi("Resources/Fonts/Daydream.ttf"), 0, &face);
            if (error == FT_Error.FT_Err_Unknown_File_Format)
            {

                Console.WriteLine("could not read the file format");

            }

            int maxTextureWidth = GL.GetInteger(GetPName.MaxTextureSize);
            int width = 0;
            int height = 0;
            int rowHeight = 0;
            Console.WriteLine($"the max texture size is {GL.GetInteger(GetPName.MaxTextureSize)}");
            error = FT_Set_Pixel_Sizes(face, 0, (uint) fontSize);
            for (int i = 32; i < 255; i++)
            {

                error = FT_Load_Char(face, (uint)i, FT_LOAD.FT_LOAD_RENDER);

                rowHeight = Math.Max(rowHeight, (int)face->glyph->bitmap.rows);

                if (width > maxTextureWidth)
                {

                    height += rowHeight;

                } else
                {

                    width += (int)face->glyph->bitmap.width;
                    // height = Math.Max(height, (int)face->glyph->bitmap.rows);

                }

                // height = Math.Max(height, (int)face->glyph->bitmap.rows);

            }
            if (height == 0) { height = rowHeight; }

            TextureWidth = width;
            TextureHeight = height;

            GL.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);
            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2d, texture);

            Console.WriteLine($"width: {width}, height: {height}");

            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.R8, width, height, 0, PixelFormat.Red, PixelType.UnsignedByte, 0);

            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            int xOffset = 0;
            int yOffset = 0;
            for (int i = 32; i < 255; i++)
            {

                error = FT_Load_Char(face, (uint)i, FT_LOAD.FT_LOAD_RENDER);

                CharacterInformation ci = new CharacterInformation();
                Vector2i size = ((int)face->glyph->bitmap.width, (int)face->glyph->bitmap.rows);
                Vector2i bearing = (face->glyph->bitmap_left, face->glyph->bitmap_top);
                int advance = (int)face->glyph->advance.x;
                ci.Size = size;
                ci.Bearing = bearing;
                ci.Advance = advance;
                ci.TextureAtlasStart = (xOffset, yOffset); // NOTE: this is in pixels, not NDC

                Characters.TryAdd((char)i, ci);

                GL.TexSubImage2D(TextureTarget.Texture2d, 0, xOffset, yOffset, (int)face->glyph->bitmap.width, (int)face->glyph->bitmap.rows, PixelFormat.Red, PixelType.UnsignedByte, (nint)face->glyph->bitmap.buffer);
                xOffset += (int)face->glyph->bitmap.width;
                rowHeight = Math.Max(rowHeight, (int) face->glyph->bitmap.rows);
                if (xOffset > maxTextureWidth)
                {

                    xOffset = 0;
                    yOffset += rowHeight;

                }

            }

            GL.BindTexture(TextureTarget.Texture2d, 0);

            FontShader = new Shader("newfont.vert", "newfont.frag");

            /*
            float[] vertices = 
            {
                
                0.0f, 0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 1000.0f, 0.0f, 0.0f, 0.0f,
                1000.0f, 1000.0f, 0.0f, 1.0f, 0.0f,
                1000.0f, 1000.0f, 0.0f, 1.0f, 0.0f,
                1000.0f, 0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 0.0f, 1.0f

            };

            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            Shader shader = new Shader("newfont.vert", "newfont.frag");

            shader.Use();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.UniformMatrix4(GL.GetUniformLocation(shader.id, "view"), true, ref GlobalValues.GuiCamera.ViewMatrix);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.id, "projection"), true, ref GlobalValues.GuiCamera.ProjectionMatrix);
            GL.Uniform3(GL.GetUniformLocation(shader.id, "textPosition"), (0,0,0));
            GL.Uniform1(GL.GetUniformLocation(shader.id, "fontTexture"), 0);
            // Console.Log(Globals.Time);
            GL.Uniform1(GL.GetUniformLocation(shader.id, "time"), (float)GlobalValues.Time);

            GL.BindVertexArray(vao);

            GL.Disable(EnableCap.CullFace);

            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);

            GL.Enable(EnableCap.CullFace);

            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(vbo);

            shader.UnUse();
            */

        }

        public struct TextVertex
        {

            public Vector3 Position;
            public Vector2 TextureCoordinate;

            public TextVertex(Vector2 position, Vector2 textureCoordinates)
            {

                Position = (position.X, position.Y, 0);
                TextureCoordinate = textureCoordinates;

            }

        }

        public static void RenderText(Vector2 position, float textSize, string text)
        {

            List<TextVertex> vertices = new List<TextVertex>();

            for (int i = 0; i < text.Length; i++)
            {

                CharacterInformation charInfo = Characters[text[i]];

                float aspect = textSize / fontSize;

                TextVertex[] charVertices =
                {

                    new TextVertex((position.X + (charInfo.Bearing.X * aspect), 0.0f), (charInfo.TextureAtlasStart.X / TextureWidth, (charInfo.TextureAtlasStart.Y + charInfo.Size.Y) / TextureHeight)),
                    new TextVertex((position.X + (charInfo.Bearing.X * aspect), 0.0f + charInfo.Size.Y* aspect), (charInfo.TextureAtlasStart.X / TextureWidth, charInfo.TextureAtlasStart.Y / TextureHeight)),
                    new TextVertex((position.X + (charInfo.Bearing.X* aspect) + (charInfo.Size.X * aspect), 0.0f + charInfo.Size.Y* aspect), ((charInfo.TextureAtlasStart.X + charInfo.Size.X) / TextureWidth, charInfo.TextureAtlasStart.Y / TextureHeight)),
                    new TextVertex((position.X + (charInfo.Bearing.X* aspect) + (charInfo.Size.X * aspect), 0.0f + charInfo.Size.Y* aspect), ((charInfo.TextureAtlasStart.X + charInfo.Size.X) / TextureWidth, charInfo.TextureAtlasStart.Y / TextureHeight)),
                    new TextVertex((position.X + (charInfo.Bearing.X* aspect) + (charInfo.Size.X * aspect), 0.0f), ((charInfo.TextureAtlasStart.X + charInfo.Size.X) / TextureWidth, (charInfo.TextureAtlasStart.Y + charInfo.Size.Y) / TextureHeight)),
                    new TextVertex((position.X + (charInfo.Bearing.X* aspect), 0.0f), (charInfo.TextureAtlasStart.X / TextureWidth, (charInfo.TextureAtlasStart.Y + charInfo.Size.Y) / TextureHeight))

                };

                for (int ind = 0; ind < charVertices.Length; ind++)
                {

                    charVertices[ind].Position.Y *= -1;
                    charVertices[ind].Position.Y += position.Y + ((charInfo.Size.Y - charInfo.Bearing.Y) * aspect);

                }

                vertices.AddRange(charVertices);

                position.X += (charInfo.Advance >> 6) * aspect;

            }

            TextVertex[] verticesArray = vertices.ToArray();

            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, verticesArray.Length * Marshal.SizeOf<TextVertex>(), verticesArray, BufferUsage.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<TextVertex>(), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<TextVertex>(), Marshal.OffsetOf<TextVertex>(nameof(TextVertex.TextureCoordinate)));
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            FontShader.Use();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2d, texture);

            GL.UniformMatrix4f(GL.GetUniformLocation(FontShader.id, "view"), 1, true, GlobalValues.GuiCamera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(FontShader.id, "projection"), 1, true, GlobalValues.GuiCamera.ProjectionMatrix);
            GL.Uniform1f(GL.GetUniformLocation(FontShader.id, "fontTexture"), 0);
            // Console.Log(Globals.Time);
            GL.Uniform1f(GL.GetUniformLocation(FontShader.id, "time"), (float)GlobalValues.Time);

            GL.BindVertexArray(vao);

            GL.Disable(EnableCap.CullFace);

            GL.DrawArrays(PrimitiveType.Triangles, 0, verticesArray.Length);

            GL.Enable(EnableCap.CullFace);

            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2d, 0);

            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(vbo);

            FontShader.UnUse();

        }

        public static TextVertex[] GenerateLine(Vector2 position, float textSize, string text)
        {

            List<TextVertex> vertices = new List<TextVertex>();

            for (int i = 0; i < text.Length; i++)
            {

                CharacterInformation charInfo = Characters[text[i]];

                float aspect = textSize / fontSize;

                TextVertex[] charVertices =
                {

                    new TextVertex((position.X + (charInfo.Bearing.X * aspect), 0.0f), (charInfo.TextureAtlasStart.X / TextureWidth, (charInfo.TextureAtlasStart.Y + charInfo.Size.Y) / TextureHeight)),
                    new TextVertex((position.X + (charInfo.Bearing.X * aspect), 0.0f + charInfo.Size.Y* aspect), (charInfo.TextureAtlasStart.X / TextureWidth, charInfo.TextureAtlasStart.Y / TextureHeight)),
                    new TextVertex((position.X + (charInfo.Bearing.X* aspect) + (charInfo.Size.X * aspect), 0.0f + charInfo.Size.Y* aspect), ((charInfo.TextureAtlasStart.X + charInfo.Size.X) / TextureWidth, charInfo.TextureAtlasStart.Y / TextureHeight)),
                    new TextVertex((position.X + (charInfo.Bearing.X* aspect) + (charInfo.Size.X * aspect), 0.0f + charInfo.Size.Y* aspect), ((charInfo.TextureAtlasStart.X + charInfo.Size.X) / TextureWidth, charInfo.TextureAtlasStart.Y / TextureHeight)),
                    new TextVertex((position.X + (charInfo.Bearing.X* aspect) + (charInfo.Size.X * aspect), 0.0f), ((charInfo.TextureAtlasStart.X + charInfo.Size.X) / TextureWidth, (charInfo.TextureAtlasStart.Y + charInfo.Size.Y) / TextureHeight)),
                    new TextVertex((position.X + (charInfo.Bearing.X* aspect), 0.0f), (charInfo.TextureAtlasStart.X / TextureWidth, (charInfo.TextureAtlasStart.Y + charInfo.Size.Y) / TextureHeight))

                };

                for (int ind = 0; ind < charVertices.Length; ind++)
                {

                    charVertices[ind].Position.Y *= -1;
                    charVertices[ind].Position.Y += position.Y + ((charInfo.Size.Y - charInfo.Bearing.Y) * aspect);

                }

                vertices.AddRange(charVertices);

                position.X += (charInfo.Advance >> 6) * aspect;

            }

            return vertices.ToArray();

        }

        public static void RenderLines(Vector2 position, float fontSize, float lineSpacing, params string[] lines)
        {

            List<TextVertex> vertices = new List<TextVertex>();

            for (int i = 0; i < lines.Length; i++)
            {

                // RenderText(position + (0, fontSize * i), fontSize, lines[i]);

                vertices.AddRange(GenerateLine(position + (0, (fontSize + (fontSize * (lineSpacing - 1.0f))) * i), fontSize, lines[i]));

            }

            TextVertex[] verticesArray = vertices.ToArray();

            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, verticesArray.Length * Marshal.SizeOf<TextVertex>(), verticesArray, BufferUsage.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<TextVertex>(), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<TextVertex>(), Marshal.OffsetOf<TextVertex>(nameof(TextVertex.TextureCoordinate)));
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            FontShader.Use();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2d, texture);

            GL.UniformMatrix4f(GL.GetUniformLocation(FontShader.id, "view"), 1, true, ref GlobalValues.GuiCamera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(FontShader.id, "projection"), 1, true, ref GlobalValues.GuiCamera.ProjectionMatrix);
            GL.Uniform1f(GL.GetUniformLocation(FontShader.id, "fontTexture"), 0);
            // Console.Log(Globals.Time);
            GL.Uniform1f(GL.GetUniformLocation(FontShader.id, "time"), (float)GlobalValues.Time);

            GL.BindVertexArray(vao);

            GL.Disable(EnableCap.CullFace);

            GL.DrawArrays(PrimitiveType.Triangles, 0, verticesArray.Length);

            GL.Enable(EnableCap.CullFace);

            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2d, 0);

            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(vbo);

            FontShader.UnUse();

        }

    }
}
