using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

namespace VoxelGame;

public struct ChunkVertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TextureCoordinate;

    public ChunkVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
    {
        Position = position;
        Normal = normal;
        TextureCoordinate = textureCoordinate;
    }
}
public class ChunkSectionMesh
{
    public List<ChunkVertex> Vertices = new();
    public List<int> Indices = new();
    public int VerticesLength = 0;
    public int IndicesLength = 0;
    public int Vbo, Vao, Ibo;
    public bool ShouldUpdate = true;

    public void Draw(Camera camera)
    {
        GL.BindVertexArray(Vao);
        GL.DrawElements(PrimitiveType.Triangles, IndicesLength, DrawElementsType.UnsignedInt, 0);
    }

    public void Update()
    {
        GL.DeleteBuffer(Vbo);
        GL.DeleteBuffer(Ibo);
        GL.DeleteVertexArray(Vao);

        Vao = GL.GenVertexArray();
        GL.BindVertexArray(Vao);

        Vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
        GL.BufferData<ChunkVertex>(BufferTarget.ArrayBuffer, Marshal.SizeOf<ChunkVertex>() * Vertices.Count, CollectionsMarshal.AsSpan(Vertices), BufferUsage.DynamicDraw);

        Ibo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ibo);
        GL.BufferData<int>(BufferTarget.ElementArrayBuffer, sizeof(int) * Indices.Count, CollectionsMarshal.AsSpan(Indices), BufferUsage.DynamicDraw);
        
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.Position)));
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.Normal)));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.TextureCoordinate)));
        GL.EnableVertexAttribArray(2);

        VerticesLength = Vertices.Count;
        IndicesLength = Indices.Count;
        
        Vertices.Clear();
        Indices.Clear();
        ShouldUpdate = false;
    }
}