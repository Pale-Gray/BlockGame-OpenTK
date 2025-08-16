using System.Collections.Generic;
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
    public List<ChunkVertex> Data = new();
    public int Length = 0;
    public int Vbo, Vao;
    public bool NeedsUpdates = true;

    public void Draw(Camera camera)
    {
        GL.BindVertexArray(Vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, Length);
    }
}