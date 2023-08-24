using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using opentk_proj.util;
using System;
using System.Collections.Generic;
using System.Linq;
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

        Vector2 RelativePosition; // note that this is in PERCENTAGE (0 to 100)
        Vector2 Dimensions; // note that this is in PIXELS, not relative screen coordinates (0 to 1, percentages, etc)

        Vector2 Position;
        Vector2 OriginOffset;

        // NOTE
        // This instance is temporary, move to a global variables class
        Camera Camera;// = new Camera((0.0f, 0.0f, 0.0f), (0.0f, 0.0f, -1.0f), (0.0f, 1.0f, 0.0f), CameraType.Orthographic, 90);

        Shader GUIShader = new Shader("../../../res/shaders/gui.vert", "../../../res/shaders/gui.frag");

        Matrix4 Model;

        int vbo, vao;
        float[] vertices;
        public GUIElement(float x, float y, float w, float h, OriginType originType)
        {

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

            Position = ((x / 100f) * Constants.WIDTH - (Constants.WIDTH / 2), (y / 100f) * Constants.HEIGHT - (Constants.HEIGHT / 2));

            Model = Matrix4.CreateTranslation(Position.X, Position.Y, 0.0f);

            Vector2 Offset = OriginOffset * Dimensions;

            vertices = new float[] {

                0f - Offset.X, 0f - Offset.Y, 0f,
                Dimensions.X - Offset.X, 0f - Offset.Y, 0f,
                Dimensions.X - Offset.X, Dimensions.Y - Offset.Y, 0f,
                Dimensions.X - Offset.X, Dimensions.Y - Offset.Y, 0f,
                0f - Offset.X, Dimensions.Y - Offset.Y, 0f,
                0f - Offset.X, 0f - Offset.Y, 0f

            };

            vbo = Vbo.Generate(vertices, BufferUsageHint.StaticDraw);
            vao = Vao.Generate(AttribPointerMode.Vertex);
            Vbo.Unbind();
            Vao.Unbind();

        }

        public void Draw()
        {

            GUIShader.Use();

            GL.UniformMatrix4(GL.GetUniformLocation(GUIShader.getID(), "model"), true, ref Model);
            GL.UniformMatrix4(GL.GetUniformLocation(GUIShader.getID(), "view"), true, ref Camera.view);
            GL.UniformMatrix4(GL.GetUniformLocation(GUIShader.getID(), "projection"), true, ref Camera.projection);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);
            GL.BindVertexArray(0);

            GUIShader.UnUse();

        }
        public void Update()
        {

            Camera.UpdateProjectionMatrix();
            Position = ((RelativePosition.X / 100f) * Constants.WIDTH - (Constants.WIDTH / 2), (RelativePosition.Y / 100f) * Constants.HEIGHT - (Constants.HEIGHT / 2));
            Model = Matrix4.CreateTranslation(Position.X, Position.Y, 0.0f);

        }

    }
}
