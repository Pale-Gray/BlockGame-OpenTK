using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
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
        public static readonly Vector2 TopLeft = (0.0f, 0.0f);
        public static readonly Vector2 TopRight = (1.0f, 0.0f);
        public static readonly Vector2 BottomLeft = (0.0f, 1.0f);
        public static readonly Vector2 BottomRight = (1.0f, 1.0f);

        public int Vao, Vbo;

        public Vector2 Position;
        public Vector2 AbsolutePosition;
        public Vector2 RelativeReference;
        public Vector2 RelativePosition;
        public Vector2 Origin;
        public Vector2 OriginOffset;
        public Vector2 Dimensions;
        public GuiVertex[] GuiMesh;

        public Matrix4 TranslationMatrix;
        public Matrix4 RotationMatrix;
        public Matrix4 ScaleMatrix;
        public Matrix4 ModelMatrix;

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

                new GuiVertex((Dimensions.X, Dimensions.Y, 0), (0, 0)),
                new GuiVertex((Dimensions.X, 0, 0), (0, 0)),
                new GuiVertex((0, 0, 0), (0, 0)),
                new GuiVertex((0, 0, 0), (0, 0)),
                new GuiVertex((0, Dimensions.Y, 0), (0, 0)),
                new GuiVertex((Dimensions.X, Dimensions.Y, 0), (0, 0))

            };

        }

        private void CallOpenGL()
        {

            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);
            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, GuiMesh.Length * Marshal.SizeOf<GuiVertex>(), GuiMesh, BufferUsage.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<GuiVertex>(), Marshal.OffsetOf<GuiVertex>(nameof(GuiVertex.Position)));
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<GuiVertex>(), Marshal.OffsetOf<GuiVertex>(nameof(GuiVertex.TextureCoordinates)));
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

        }

        public virtual void Draw(float time)
        {

            Position = (AbsolutePosition + RelativePosition);
            TranslationMatrix = Matrix4.CreateTranslation(Position.X, Position.Y, 0);
            ModelMatrix = Matrix4.CreateTranslation(-OriginOffset.X, -OriginOffset.Y, 0) * RotationMatrix * TranslationMatrix * ScaleMatrix;

            GlobalValues.GuiShader.Use();

            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.GuiShader.id, "model"), 1, true, ref ModelMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.GuiShader.id, "view"), 1, true, ref GlobalValues.GuiCamera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.GuiShader.id, "projection"), 1, true, ref GlobalValues.GuiCamera.ProjectionMatrix);

            GL.BindVertexArray(Vao);

            GL.DrawArrays(PrimitiveType.Triangles, 0, GuiMesh.Length);

            GL.BindVertexArray(0);

            GlobalValues.GuiShader.UnUse();

        }

        public void OnScreenResize()
        {

            SetRelativePosition(RelativeReference.X, RelativeReference.Y);

        }

    }
}
