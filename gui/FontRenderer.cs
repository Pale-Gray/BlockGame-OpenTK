using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK.Gui
{

    struct GUIVertex 
    {

        public Vector3 Position;
        public Vector2 TextureCoordinates;
        public GUIVertex(float x, float y, float u, float v)
        {

            Position = (x, y, 0);
            TextureCoordinates = (u, v);

        }
    
    }

    
    internal class FontRenderer
    {

        char[] InternalChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!?[]{}/.,<>()\"': ".ToArray();
        Vector2 CharDimension = (8, 8);
        Vector2 CharRenderDimension;
        Vector2 RelativePosition;
        int Vbo, Vao;
        GUIVertex[] Vertices;
        Texture FontTexture;
        Shader FontShader;
        // NOTE can be in Globals class
        Camera Camera = new Camera((0.0f, 0.0f, 1.0f), (0.0f, 0.0f, -1.0f), (0.0f, 1.0f, 0.0f), CameraType.Orthographic, 90);
        Vector3 FontColor = (1, 1, 1);
        Vector2 CoordinateOffset = (Globals.WIDTH / 2, Globals.HEIGHT / 2);

        Matrix4 model;
        public FontRenderer(int characterDimension, string text)
        {

            CharRenderDimension = (characterDimension, characterDimension);

            FontTexture = new Texture("fatlas.png");
            FontShader = new Shader("font.vert", "font.frag");

            GenerateMesh(text);
            model = Matrix4.CreateTranslation(-300, 200, 0);

            ProcessToRender();

        }

        public void ProcessToRender()
        {

            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);
            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * Marshal.SizeOf<GUIVertex>(), Vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<GUIVertex>(), Marshal.OffsetOf<GUIVertex>(nameof(GUIVertex.Position)));
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<GUIVertex>(), Marshal.OffsetOf<GUIVertex>(nameof(GUIVertex.TextureCoordinates)));
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        }

        public void UpdateText(String newText)
        {

            GenerateMesh(newText);
            ProcessToRender();

        }

        public Vector2 AbsoluteToRelative(Vector2 absolutePosition)
        {

            return (absolutePosition.X / Globals.WIDTH, absolutePosition.Y / Globals.HEIGHT);

        }

        public void SetFontColor(Vector3 color)
        {

            FontColor = color;

        }

        public void Draw()
        {

            GL.Disable(EnableCap.DepthTest);

            FontShader.Use();

            GL.Uniform1(GL.GetUniformLocation(FontShader.getID(), "fatlas"), 0);
            // GL.Uniform3(GL.GetUniformLocation(ChunkShader.id, "cameraPosition"), camera.position);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, FontTexture.getID());
            GL.UniformMatrix4(GL.GetUniformLocation(FontShader.getID(), "model"), true, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(FontShader.getID(), "view"), true, ref Camera.view);
            GL.UniformMatrix4(GL.GetUniformLocation(FontShader.getID(), "projection"), true, ref Camera.projection);
            GL.Uniform1(GL.GetUniformLocation(FontShader.getID(), "time"), (float)Globals.Time);
            GL.Uniform3(GL.GetUniformLocation(FontShader.getID(), "fontColor"), ref FontColor);
            // GL.Uniform3(GL.GetUniformLocation(shader.getID(), "cpos"), ref ChunkPosition);
            GL.BindVertexArray(Vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Length);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            FontShader.UnUse();

            GL.Enable(EnableCap.DepthTest);

        }

        private void GenerateMesh(string text)
        {

            List<GUIVertex> Mesh = new List<GUIVertex>();

            for (int i = 0; i < text.Length; i++)
            {

                // Console.WriteLine(GetIndexFromChar(text[i], InternalChars));
                Vector2[] TextureCoordinates = GetTextureCoordinatesFromIndex(GetIndexFromChar(text[i], InternalChars));

                // Console.WriteLine(TextureCoordinates[0].X);

                Mesh.Add(new GUIVertex((i * CharRenderDimension.X), 0, TextureCoordinates[2].X, TextureCoordinates[2].Y));
                Mesh.Add(new GUIVertex(CharRenderDimension.X + (i * CharRenderDimension.X), 0, TextureCoordinates[3].X, TextureCoordinates[3].Y));
                Mesh.Add(new GUIVertex(CharRenderDimension.X + (i * CharRenderDimension.X), CharRenderDimension.Y, TextureCoordinates[1].X, TextureCoordinates[1].Y));
                Mesh.Add(new GUIVertex(CharRenderDimension.X + (i * CharRenderDimension.X), CharRenderDimension.Y, TextureCoordinates[1].X, TextureCoordinates[1].Y));
                Mesh.Add(new GUIVertex((i * CharRenderDimension.X), CharRenderDimension.Y, TextureCoordinates[0].X, TextureCoordinates[0].Y));
                Mesh.Add(new GUIVertex((i * CharRenderDimension.X), 0, TextureCoordinates[2].X, TextureCoordinates[2].Y));

            }

            Vertices = Mesh.ToArray();

        }

        private int GetIndexFromChar(char character, char[] reference)
        {

            int index = reference.ToList().IndexOf(character);
            if (index == -1)
            {

                return 255;

            }
            else
            {

                return index;

            }

        }

        private Vector2[] GetTextureCoordinatesFromIndex(int index)
        {

            Vector2 TextureDimensions = (FontTexture.getImage().Width, FontTexture.getImage().Height);
            Vector2 TextureCoordinateRatio = CharDimension / TextureDimensions;

            Vector2 AmountOfCharsPerDimension = TextureDimensions / CharDimension;

            float Xcoordinate = (index%AmountOfCharsPerDimension.X) * TextureCoordinateRatio.X;
            float Ycoordinate = 1 - ((float)Math.Floor(((float)index / AmountOfCharsPerDimension.X)) * TextureCoordinateRatio.Y);

            Vector2[] Vectors =
            {
                (Xcoordinate, Ycoordinate),
                (Xcoordinate+TextureCoordinateRatio.X,Ycoordinate),
                (Xcoordinate,Ycoordinate-TextureCoordinateRatio.Y),
                (Xcoordinate+TextureCoordinateRatio.X,Ycoordinate-TextureCoordinateRatio.Y),
            };

            return Vectors;

        }

        public void Update()
        {

            Camera.UpdateProjectionMatrix();
            model = Matrix4.CreateTranslation(-300, 200, 0);

        }

    }
}
