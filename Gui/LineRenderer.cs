using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Blockgame_OpenTK.Gui
{

    struct LineVertex
    {

        public Vector3 Position;
        public Vector3 PointAPosition;
        public Vector3 PointBPosition;
        public Vector3 Color;

        public LineVertex(Vector3 position, Vector3 pointAPosition, Vector3 pointBPosition, Vector3 color)
        {

            Position = position;
            PointAPosition = pointAPosition;
            PointBPosition = pointBPosition;
            Color = color;

        }

    }

    // implemented based on https://wwwtyro.net/2019/11/18/instanced-lines.html
    internal class LineRenderer
    {

        static Shader LineShader = new Shader("lines.vert", "lines.frag");
        static int Vao, Vbo;

        public static void DrawLine(Vector3 pointA, Vector3 pointB, float thickness, Vector3 color, Camera camera)
        {

            GL.DeleteVertexArray(Vao);
            GL.DeleteBuffer(Vbo);

            List<LineVertex> vertices = new List<LineVertex>();
            Vector3 distance = pointB - pointA;

            vertices.Add(new LineVertex((0.0f, 0.5f, 0.0f), pointA, pointB, color));
            vertices.Add(new LineVertex((0.0f, -0.5f, 0.0f), pointA, pointB, color));
            vertices.Add(new LineVertex((1.0f, -0.5f, 0.0f), pointB, pointB+distance, color));
            vertices.Add(new LineVertex((1.0f, -0.5f, 0.0f), pointB, pointB+distance, color));
            vertices.Add(new LineVertex((1.0f, 0.5f, 0.0f), pointB, pointB+distance, color));
            vertices.Add(new LineVertex((0.0f, 0.5f, 0.0f), pointA, pointB, color));

            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);
            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * Marshal.SizeOf<LineVertex>(), vertices.ToArray(), BufferUsage.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<LineVertex>(), Marshal.OffsetOf<LineVertex>(nameof(LineVertex.Position)));
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<LineVertex>(), Marshal.OffsetOf<LineVertex>(nameof(LineVertex.PointAPosition)));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<LineVertex>(), Marshal.OffsetOf<LineVertex>(nameof(LineVertex.PointBPosition)));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<LineVertex>(), Marshal.OffsetOf<LineVertex>(nameof(LineVertex.Color)));
            GL.EnableVertexAttribArray(3);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            LineShader.Use();

            GL.Disable(EnableCap.CullFace);

            GL.UniformMatrix4f(GL.GetUniformLocation(LineShader.id, "view"), 1, true, ref camera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(LineShader.id, "projection"), 1, true, ref camera.ProjectionMatrix);
            GL.Uniform1f(GL.GetUniformLocation(LineShader.id, "thickness"), thickness);
            GL.Uniform2f(GL.GetUniformLocation(LineShader.id, "screenDimensions"), 1, (GlobalValues.WIDTH, GlobalValues.HEIGHT));
            GL.Uniform3f(GL.GetUniformLocation(LineShader.id, "pointAPoint"), 1, pointA);
            GL.Uniform3f(GL.GetUniformLocation(LineShader.id, "pointBPoint"), 1, pointB);

            GL.BindVertexArray(Vao);

            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Count);

            GL.Enable(EnableCap.CullFace);

            GL.BindVertexArray(0);

            LineShader.UnUse();

        }

    }
}
