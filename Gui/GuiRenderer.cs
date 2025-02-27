using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Blockgame_OpenTK.Gui
{
    public struct GuiNewVertex
    {

        public Vector2 Position = Vector2.Zero;
        public Vector2 TextureCoordinate = Vector2.Zero;
        public float Layer = 0;
        public GuiNewVertex(Vector2 position, Vector2 textureCoordinate, float layer)
        {

            Position = position;
            TextureCoordinate = textureCoordinate;
            Layer = layer;

        }

    }
    public class GuiRendere
    {

        public static Camera GuiCamera = new Camera((0, 0, 1), (0, 0, -1), (0, 1, 0), CameraType.Orthographic, 90);
        private static Shader _shader;
        public static List<GuiElement> Elements = new List<GuiElement>();

        private static int _vbo, _vao, _ibo;

        public static void Init()
        {

            _shader = new Shader("gui.vert", "gui.frag");

        }
        public static void RenderElement(GuiElement element)
        {

            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ibo);
            GL.DeleteVertexArray(_vao);

            Vector2 topLeft = element.Position - (element.Dimensions * element.Origin);

            GuiNewVertex[] vertices =
            {

                new GuiNewVertex(topLeft, (0, 0), element.Layer),
                new GuiNewVertex(topLeft + (0.0f, element.Dimensions.Y), (0, 0), element.Layer),
                new GuiNewVertex(topLeft + element.Dimensions, (0, 0), element.Layer),
                new GuiNewVertex(topLeft + (element.Dimensions.X, 0.0f), (0, 0), element.Layer)

            };

            int[] indices =
            {

                0, 1, 2,
                2, 3, 0

            };

            _vao = GL.GenVertexArray();

            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Marshal.SizeOf<GuiNewVertex>(), vertices, BufferUsage.StaticDraw);

            _ibo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsage.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<GuiNewVertex>(), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<GuiNewVertex>(), Marshal.OffsetOf<GuiNewVertex>(nameof(GuiNewVertex.TextureCoordinate)));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, Marshal.SizeOf<GuiNewVertex>(), Marshal.OffsetOf<GuiNewVertex>(nameof(GuiNewVertex.Layer)));
            GL.EnableVertexAttribArray(2);

            _shader.Use();

            Vector4 color = (Vector4)element.Color;

            GL.UniformMatrix4f(0, 1, true, in GuiCamera.ViewMatrix);
            GL.UniformMatrix4f(1, 1, true, in GuiCamera.ProjectionMatrix);
            GL.Uniform4f(2, 1, in color);

            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

        }
        
    }
}
