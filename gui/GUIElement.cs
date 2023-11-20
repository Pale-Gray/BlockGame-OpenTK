using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using opentk_proj.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj.gui
{

    public enum OriginType
    {

        Center,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight

    };
    internal class GUIElement
    {

        public Vector2 RelativePosition; // note that this is in PERCENTAGE (0 to 100)
        public Vector2 Dimensions; // note that this is in PIXELS, not relative screen coordinates (0 to 1, percentages, etc)

        public Vector2 Position;
        public Vector2 PositionNoOffset;
        public Vector2 OriginOffset;

        public Vector2 PositionOffsetInPixels = (0,0);
        public Vector2 PositionOffsetInPercentage = (0,0);

        public Vector2 CoordinateOffset = (Constants.WIDTH / 2, Constants.HEIGHT / 2);

        public static Vector4i Null = (-1, -1, -1, -1);
        public static Texture NullTexture = new Texture("../../../res/textures/missing.png");

        // NOTE
        // This instance is temporary, move to a global variables class
        Camera Camera;// = new Camera((0.0f, 0.0f, 0.0f), (0.0f, 0.0f, -1.0f), (0.0f, 1.0f, 0.0f), CameraType.Orthographic, 90);

        Shader GUIShader = new Shader("../../../res/shaders/gui.vert", "../../../res/shaders/gui.frag");
        Texture Texture;

        Matrix4 Model;

        int vbo, vao;
        float[] vertices;
        private float x;
        private float y;
        private float w;
        private float h;
        private OriginType originType;

        public GUIElement(float x, float y, float w, float h, OriginType originType, Texture texture, Vector4i texturePortion)
        {

            if (texturePortion == Null)
            {

                Texture = texture;

            } else
            {

                Texture = Texture.GetPortion(false, texture, texturePortion.X, texturePortion.Y, texturePortion.Z, texturePortion.W);

            }

            switch(originType)
            {

                case OriginType.BottomLeft:
                    OriginOffset = new Vector2(0f, 0f);
                    break;
                case OriginType.BottomRight:
                    OriginOffset = new Vector2(1f, 0f);
                    break;
                case OriginType.TopLeft:
                    OriginOffset = new Vector2(0f, 1f);
                    break;
                case OriginType.TopRight:
                    OriginOffset = new Vector2(1f, 1f);
                    break;
                case OriginType.Center:
                    OriginOffset = new Vector2(0.5f, 0.5f);
                    break;
                default:
                    OriginOffset = new Vector2(0f, 0f);
                    break;

            }

            Camera = new Camera((0.0f, 0.0f, 1.0f), (0.0f, 0.0f, -1.0f), (0.0f, 1.0f, 0.0f), CameraType.Orthographic, 90);

            RelativePosition = (x, y);
            Dimensions = (w, h);

            PositionNoOffset = AbsolutePositionFromRelative((x, y));
            Position = PositionNoOffset - CoordinateOffset;

            Model = Matrix4.CreateTranslation(Position.X + PositionOffsetInPercentage.X + PositionOffsetInPixels.X, Position.Y + PositionOffsetInPercentage.Y + PositionOffsetInPixels.Y, 0.0f);

            Vector2 Offset = OriginOffset * Dimensions;

            vertices = new float[] {

                0f - Offset.X, 0f - Offset.Y, 0f, 0f, 0f,
                Dimensions.X - Offset.X, 0f - Offset.Y, 0f, 1f, 0f,
                Dimensions.X - Offset.X, Dimensions.Y - Offset.Y, 0f, 1f, 1f,
                Dimensions.X - Offset.X, Dimensions.Y - Offset.Y, 0f, 1f, 1f,
                0f - Offset.X, Dimensions.Y - Offset.Y, 0f, 0f, 1f,
                0f - Offset.X, 0f - Offset.Y, 0f, 0f, 0f

            };

            vbo = Vbo.Generate(vertices, BufferUsageHint.StaticDraw);
            vao = Vao.Generate(AttribPointerMode.VertexTexcoord);
            Vbo.Unbind();
            Vao.Unbind();

        }

        public void Draw()
        {

            GUIShader.Use();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Texture.id);

            GL.UniformMatrix4(GL.GetUniformLocation(GUIShader.getID(), "model"), true, ref Model);
            GL.UniformMatrix4(GL.GetUniformLocation(GUIShader.getID(), "view"), true, ref Camera.view);
            GL.UniformMatrix4(GL.GetUniformLocation(GUIShader.getID(), "projection"), true, ref Camera.projection);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GUIShader.UnUse();

        }
        public void Update()
        {

            CoordinateOffset = (Constants.WIDTH / 2, Constants.HEIGHT / 2);

            Camera.UpdateProjectionMatrix();
            PositionNoOffset = AbsolutePositionFromRelative(RelativePosition);
            Position = AbsolutePositionFromRelative(RelativePosition) - CoordinateOffset;
            Model = Matrix4.CreateTranslation(Position.X + PositionOffsetInPercentage.X + PositionOffsetInPixels.X, Position.Y + PositionOffsetInPercentage.Y + PositionOffsetInPixels.Y, 0.0f);

        }

        public void SetRelativePosition(Vector2 relativePositionInPercentage)
        {

            RelativePosition = relativePositionInPercentage;
            Update();

        }

        public void SetRelativePositionOffset(Vector2 relativePositionOffsetInPercentage)
        {

            PositionOffsetInPercentage = AbsolutePositionFromRelative(relativePositionOffsetInPercentage);
            Update();

        }

        public void SetAbsolutePosition(Vector2 positionInPixels)
        {

            Position = positionInPixels;
            Model = Matrix4.CreateTranslation(Position.X + PositionOffsetInPercentage.X + PositionOffsetInPixels.X, Position.Y + PositionOffsetInPercentage.Y + PositionOffsetInPixels.Y, 0.0f);

        }

        public void SetAbsolutePositionOffset(Vector2 positionOffsetInPixels)
        {

            PositionOffsetInPixels += positionOffsetInPixels;
            Update();

        }

        public Vector2 AbsolutePositionFromRelative(Vector2 relativePositionAmount)
        {

            return ((relativePositionAmount / 100) * (Constants.WIDTH, Constants.HEIGHT));

        }

    }
}
