using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Blockgame_OpenTK.Gui
{
    public struct GuiVertex
    {

        public Vector3 Position;
        public Vector2 TextureCoordinates;

        public GuiVertex(Vector3 position, Vector2 textureCoordinates)
        {

            Position = position;
            TextureCoordinates = textureCoordinates;

        }

    }

    internal class GuiElement
    {

        public static readonly Vector2 Center = (0.5f, 0.5f);
        public static readonly Vector2 TopLeft = (0.0f, 1.0f);

        private int Vao, Vbo;

        Vector2 Position;
        Vector2 AbsolutePosition;
        Vector2 RelativeReference;
        Vector2 RelativePosition;
        Vector2 Origin;
        Vector2 OriginOffset;
        Vector2 Dimensions;
        GuiVertex[] GuiMesh;

        Matrix4 TranslationMatrix;
        Matrix4 RotationMatrix;
        Matrix4 ScaleMatrix;
        Matrix4 ModelMatrix;

        public GuiElement(Vector2 dimensions, Vector2 origin)
        {

            Dimensions = dimensions;
            Origin = (origin.X, origin.Y);

            ModelMatrix = Matrix4.Identity;
            OriginOffset = dimensions * origin;

            // ModelMatrix = Matrix4.CreateTranslation(((position.X - OriginOffset.X, position.Y - OriginOffset.Y, 0)));
            TranslationMatrix = Matrix4.Identity;
            RotationMatrix = Matrix4.Identity;
            ScaleMatrix = Matrix4.Identity;
            ModelMatrix = TranslationMatrix * RotationMatrix * ScaleMatrix;

            GenerateMesh();
            CallOpenGL();

        }

        public void SetRelativePosition(float x, float y)
        {

            RelativeReference = (x, y);
            RelativePosition = GuiMath.RelativeToAbsolute(RelativeReference.X, RelativeReference.Y);

        }

        public void SetAbsolutePosition(float x, float y)
        {

            AbsolutePosition = (x, y);

        }

        public void Rotate(float angle)
        {

            RotationMatrix = Matrix4.CreateRotationZ(Maths.ToRadians(angle));

        }

        private void GenerateMesh()
        {

            GuiMesh = new GuiVertex[] {

                new GuiVertex((0, Dimensions.Y, 0), (0, 0)),
                new GuiVertex((0, 0, 0), (0, 0)),
                new GuiVertex((Dimensions.X, 0, 0), (0, 0)),
                new GuiVertex((Dimensions.X, 0, 0), (0, 0)),
                new GuiVertex((Dimensions.X, Dimensions.Y, 0), (0, 0)),
                new GuiVertex((0, Dimensions.Y, 0), (0, 0)),

            };

        }

        private void CallOpenGL()
        {

            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);
            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, GuiMesh.Length * Marshal.SizeOf<GuiVertex>(), GuiMesh, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<GuiVertex>(), Marshal.OffsetOf<GuiVertex>(nameof(GuiVertex.Position)));
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<GuiVertex>(), Marshal.OffsetOf<GuiVertex>(nameof(GuiVertex.TextureCoordinates)));
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

        }

        public void Draw(float time)
        {

            Position = (AbsolutePosition + RelativePosition);
            TranslationMatrix = Matrix4.CreateTranslation(Position.X, Position.Y, 0);
            ModelMatrix = Matrix4.CreateTranslation(-OriginOffset.X, -OriginOffset.Y, 0) * RotationMatrix * TranslationMatrix * ScaleMatrix;

            Globals.GuiShader.Use();

            GL.UniformMatrix4(GL.GetUniformLocation(Globals.GuiShader.id, "model"), true, ref ModelMatrix);
            GL.UniformMatrix4(GL.GetUniformLocation(Globals.GuiShader.id, "view"), true, ref Globals.GuiCamera.ViewMatrix);
            GL.UniformMatrix4(GL.GetUniformLocation(Globals.GuiShader.id, "projection"), true, ref Globals.GuiCamera.ProjectionMatrix);

            GL.BindVertexArray(Vao);

            GL.DrawArrays(PrimitiveType.Triangles, 0, GuiMesh.Length);

            GL.BindVertexArray(0);

            Globals.GuiShader.UnUse();

        }

        public void OnScreenResize()
        {

            SetRelativePosition(RelativeReference.X, RelativeReference.Y);

        }

    }
}
