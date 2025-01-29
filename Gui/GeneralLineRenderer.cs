using System;
using System.Runtime.InteropServices;
using Blockgame_OpenTK.PlayerUtil;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Gui;

public struct GeneralLineVertex
{

    public Vector3 Position;
    public Vector3 Before;
    public Vector3 After;

    public GeneralLineVertex(Vector3 position, Vector3 before, Vector3 after)
    {
        Position = position;
        Before = before;
        After = after;
        
    }

}

public class GeneralLineRenderer
{
    
    public static int Vao, Vbo, Ibo;

    public static void DrawLine(Vector3 start, Vector3 end, Color4<Rgba> color, float thickness, Player player)
    {

        Vector3 diff = Vector3.Abs(start - end).Normalized();

        GeneralLineVertex[] vertices =
        {
            new GeneralLineVertex((0, 0.5f, 0), start, end),
            new GeneralLineVertex((0, -0.5f, 0), start, end),
            new GeneralLineVertex((1, -0.5f, 0), start, end),
            new GeneralLineVertex((1, 0.5f, 0), start, end),
        };

        int[] indices =
        {
            0, 1, 2,
            2, 3, 0
        };
        
        GL.DeleteVertexArray(Vao);
        GL.DeleteBuffer(Vbo);
        GL.DeleteBuffer(Ibo);

        Vao = GL.GenVertexArray();
        GL.BindVertexArray(Vao);

        Vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Marshal.SizeOf<GeneralLineVertex>(), vertices, BufferUsage.DynamicDraw);

        Ibo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ibo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsage.DynamicDraw);
        
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<GeneralLineVertex>(), Marshal.OffsetOf<GeneralLineVertex>(nameof(GeneralLineVertex.Position)));
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<GeneralLineVertex>(), Marshal.OffsetOf<GeneralLineVertex>(nameof(GeneralLineVertex.Before)));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<GeneralLineVertex>(), Marshal.OffsetOf<GeneralLineVertex>(nameof(GeneralLineVertex.After)));
        GL.EnableVertexAttribArray(2);
        
        GlobalValues.GuiLineShader.Use();
        GL.Disable(EnableCap.CullFace);

        Matrix4 modelMatrix = Matrix4.CreateScale(end.Length, 1, 1);
        GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.GuiLineShader.id, "view"), 1, true, in player.Camera.ViewMatrix);
        GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.GuiLineShader.id, "projection"), 1, true, in player.Camera.ProjectionMatrix);
        GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.GuiLineShader.id, "model"), 1, true, in modelMatrix);
        
        GL.Uniform2f(GL.GetUniformLocation(GlobalValues.GuiLineShader.id, "resolution"), GlobalValues.WIDTH, GlobalValues.HEIGHT);
        
        GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
        GL.Enable(EnableCap.CullFace);
        
    }
    
}