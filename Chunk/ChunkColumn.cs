using System.Collections.Concurrent;
using Game.Core.Worlds;
using OpenTK.Mathematics;

namespace Game.Core.Chunks;

public enum ColumnQueueType : byte
{

    PassOne,
    LightPropagation,
    Mesh,
    Upload,
    Done

}
public class ChunkColumn
{

    public Vector2i Position;
    public Chunk[] Chunks = new Chunk[PackedWorldGenerator.WorldGenerationHeight];
    public ChunkMesh[] ChunkMeshes = new ChunkMesh[PackedWorldGenerator.WorldGenerationHeight]; 
    public ColumnQueueType QueueType = ColumnQueueType.PassOne;
    public bool HasPriority = false;

    public ChunkColumn(Vector2i position)
    {
        Position = position;
        for (int i = 0; i < PackedWorldGenerator.WorldGenerationHeight; i++)
        {
            Chunks[i] = new Chunk((position.X, i, position.Y));
            ChunkMeshes[i] = new ChunkMesh((position.X, i, position.Y));
        }
    }
    
}