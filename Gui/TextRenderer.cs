using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

using Blockgame_OpenTK.Util;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

namespace Blockgame_OpenTK.Gui
{

    struct GUIVertex 
    {

        public Vector3 Position;
        public Vector3 Color;
        public Vector2 TextureCoordinates;
        public float isWiggle = 0;
        public float isItalics = 0;
        public GUIVertex(Vector3 position, Vector2 textureCoordinates, Vector3 color)
        {

            Position = position;
            Color = color;
            TextureCoordinates = textureCoordinates;

        }
    
    }

    internal class TextRenderer
    {

        private static char[] InternalChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!?[]{}/.,<>()\"':- _\\".ToArray();
        static Vector2 CharDimension = (8, 8);
        static Vector2 CharRenderDimension;
        static Vector2 RelativePosition;
        static int Vbo, Vao;
        static GUIVertex[] Vertices;
        static Texture FontTexture;
        static Shader FontShader;
        static Vector3[] TextColors;
        static bool[] isItalics;
        static bool[] isWiggle;
        static bool[] isTag;
        static bool IsInputtingFancyText = false;
        // NOTE can be in Globals class
        public static Camera Camera = new Camera((0.0f, 0.0f, 1.0f), (0.0f, 0.0f, -1.0f), (0.0f, 1.0f, 0.0f), CameraType.Orthographic, 90);

        public static void InitTextRenderer()
        {

            FontTexture = new Texture("fatlas.png");
            FontShader = new Shader("font.vert", "font.frag");

        }

        public static string FilterText(string text)
        {

            TextColors = new Vector3[text.Length];
            isItalics = new bool[text.Length];
            isWiggle = new bool[text.Length];
            isTag = new bool[text.Length];
            // KEEP IN MIND -> Convert.FromHexString(text);
            Vector3 targetColor = (-1, -1, -1);
            bool isWiggleTrail = false;
            bool isItalicsTrail = false;
            for (int i = 0; i < text.Length; i++)
            {

                TextColors[i] = targetColor;
                isWiggle[i] = isWiggleTrail;
                isItalics[i] = isItalicsTrail;

                if (text[i] == '<')
                {

                    if (text[i+1] == '0' && text[i+2] == 'x')
                    {

                        // Console.WriteLine("found a color change");

                        string hex = new string(new char[] { text[i+3], text[i+4], text[i+5], text[i+6], text[i+7], text[i+8] });
                        float r = Convert.ToByte(hex.Substring(0, 2), 16) / 255f;
                        float g = Convert.ToByte(hex.Substring(2, 2), 16) / 255f;
                        float b = Convert.ToByte(hex.Substring(4, 2), 16) / 255f;

                        targetColor = (r, g, b);
                        TextColors[i] = targetColor;

                        isTag[i] = true;
                        isTag[i + 1] = true;
                        isTag[i + 2] = true;
                        isTag[i + 3] = true;
                        isTag[i + 4] = true;
                        isTag[i + 5] = true;
                        isTag[i + 6] = true;
                        isTag[i + 7] = true;
                        isTag[i + 8] = true;
                        isTag[i + 9] = true;

                    }

                    if (text[i+1] == '/' && text[i+2] == '0' && text[i+3] == 'x')
                    {

                        TextColors[i] = targetColor;
                        targetColor = (-1, -1, -1);

                        isTag[i] = true;
                        isTag[i + 1] = true;
                        isTag[i + 2] = true;
                        isTag[i + 3] = true;
                        isTag[i + 4] = true;
                        isTag[i + 5] = true;
                        isTag[i + 6] = true;
                        isTag[i + 7] = true;
                        isTag[i + 8] = true;
                        isTag[i + 9] = true;
                        isTag[i + 10] = true;

                    }
                    if (text[i+1] == 'w')
                    {

                        isWiggle[i] = true;
                        isWiggleTrail = true;

                        isTag[i] = true;
                        isTag[i + 1] = true;
                        isTag[i + 2] = true;
                        isTag[i + 3] = true;
                        isTag[i + 4] = true;
                        isTag[i + 5] = true;
                        isTag[i + 6] = true;
                        isTag[i + 7] = true;

                    }
                    if (text[i+1] == '/' && text[i+2] == 'w')
                    {

                        isWiggle[i] = true;
                        isWiggleTrail = false;

                        isTag[i] = true;
                        isTag[i + 1] = true;
                        isTag[i + 2] = true;
                        isTag[i + 3] = true;
                        isTag[i + 4] = true;
                        isTag[i + 5] = true;
                        isTag[i + 6] = true;
                        isTag[i + 7] = true;
                        isTag[i + 8] = true;

                    }

                    if (text[i+1] == 'i')
                    {

                        isItalics[i] = true;
                        isItalicsTrail = true;

                        isTag[i] = true;
                        isTag[i + 1] = true;
                        isTag[i + 2] = true;
                        isTag[i + 3] = true;
                        isTag[i + 4] = true;
                        isTag[i + 5] = true;
                        isTag[i + 6] = true;
                        isTag[i + 7] = true;

                    }
                    
                    if (text[i+1] == '/' && text[i+2] == 'i')
                    {

                        isItalics[i] = true;
                        isItalicsTrail = false;

                        isTag[i] = true;
                        isTag[i + 1] = true;
                        isTag[i + 2] = true;
                        isTag[i + 3] = true;
                        isTag[i + 4] = true;
                        isTag[i + 5] = true;
                        isTag[i + 6] = true;
                        isTag[i + 7] = true;
                        isTag[i + 8] = true;

                    }

                }

            }

            List<char> chars = new List<char>();
            List<bool> isItalicsList = new List<bool>();
            List<bool> isWiggleList = new List<bool>();

            for (int i = 0; i < text.Length; i++)
            {

                if (isTag[i] == false)
                {

                    chars.Add(text[i]);
                    isItalicsList.Add(isItalics[i]);
                    isWiggleList.Add(isWiggle[i]);

                }

            }

            isItalics = isItalicsList.ToArray();
            isWiggle = isWiggleList.ToArray();

            IsInputtingFancyText = true;

            return new string(chars.ToArray());

        }

        public static void RenderTextWithShadow(Vector3 position, Vector3 shadowOffset, Vector3 color, Vector3 shadowColor, int size, string text)
        {

            bool keepFancy = false;
            if (IsInputtingFancyText) keepFancy = true; 

            RenderText(position + shadowOffset, color * (0.2f,0.2f,0.2f), size, text);
            IsInputtingFancyText = keepFancy;
            RenderText(position, color, size, text);

        }
        public static void RenderText(Vector3 position, Vector3 color, int size, string text)
        {

            List<GUIVertex> textVertices = new List<GUIVertex>();
            float stepSize = 1f/(InternalChars.Length+1);

            for (int i = 0; i < text.Length; i++)
            {

                int charIndex = Array.IndexOf(InternalChars, text[i]);
                GUIVertex[] vertices =
                {

                    new GUIVertex(position + (size * i, size, 0), (charIndex * stepSize, 1), color),
                    new GUIVertex(position + (size*i, 0, 0), (charIndex * stepSize, 0), color),
                    new GUIVertex(position + (size + size*i, 0, 0), (stepSize + (charIndex * stepSize), 0), color),
                    new GUIVertex(position + (size + size * i, 0, 0), (stepSize + (charIndex * stepSize), 0), color),
                    new GUIVertex(position + (size + size * i, size, 0), (stepSize + (charIndex * stepSize), 1), color),
                    new GUIVertex(position + (size * i, size, 0), (charIndex * stepSize, 1), color),

                };

                // Console.WriteLine($"tag: {isTag[i]}, italics: {isItalics[i]}, wiggle: {isWiggle[i]}");

                if (IsInputtingFancyText)
                {

                    // Console.WriteLine($"tag: {isTag[i]}, italics: {isItalics[i]}, wiggle: {isWiggle[i]}");

                    if (TextColors[i] != (-1,-1,-1))
                    {

                        for (int c = 0; c < vertices.Length; c++)
                        {

                            vertices[c].Color = TextColors[i];

                        }

                    }

                    if (isWiggle[i])
                    {

                        for (int w = 0; w < vertices.Length; w++)
                        {

                            vertices[w].isWiggle = 1;

                        }

                    }

                    if (isItalics[i])
                    {

                        for (int w = 0; w < vertices.Length; w++)
                        {
                            vertices[w].isItalics = 1;

                        }

                    }

                }

                textVertices.AddRange(vertices);

            }

            if (IsInputtingFancyText)
            {

                IsInputtingFancyText = false;

            }

            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);
            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, textVertices.Count() * Marshal.SizeOf<GUIVertex>(), textVertices.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<GUIVertex>(), Marshal.OffsetOf<GUIVertex>(nameof(GUIVertex.Position)));
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<GUIVertex>(), Marshal.OffsetOf<GUIVertex>(nameof(GUIVertex.Color)));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<GUIVertex>(), Marshal.OffsetOf<GUIVertex>(nameof(GUIVertex.TextureCoordinates)));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, Marshal.SizeOf<GUIVertex>(), Marshal.OffsetOf<GUIVertex>(nameof(GUIVertex.isWiggle)));
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, Marshal.SizeOf<GUIVertex>(), Marshal.OffsetOf<GUIVertex>(nameof(GUIVertex.isItalics)));
            GL.EnableVertexAttribArray(4);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            FontShader.Use();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, FontTexture.getID());

            GL.UniformMatrix4(GL.GetUniformLocation(FontShader.id, "view"), true, ref Camera.ViewMatrix);
            GL.UniformMatrix4(GL.GetUniformLocation(FontShader.id, "projection"), true, ref Camera.ProjectionMatrix);
            GL.Uniform3(GL.GetUniformLocation(FontShader.id, "textPosition"), position);
            GL.Uniform1(GL.GetUniformLocation(FontShader.id, "fontTexture"), 0);
            // Console.WriteLine(Globals.Time);
            GL.Uniform1(GL.GetUniformLocation(FontShader.id, "time"), (float) Globals.Time);

            GL.BindVertexArray(Vao);

            GL.DrawArrays(PrimitiveType.Triangles, 0, textVertices.Count());

            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.DeleteVertexArray(Vao);
            GL.DeleteBuffer(Vbo);

            FontShader.UnUse();

        }

    }
}
