using System.Collections.Concurrent;
using Blockgame_OpenTK.Core.Worlds;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Chunks;

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
    public PackedChunk[] Chunks = new PackedChunk[PackedWorldGenerator.WorldGenerationHeight];
    public PackedChunkMesh[] ChunkMeshes = new PackedChunkMesh[PackedWorldGenerator.WorldGenerationHeight]; 
    public ColumnQueueType QueueType = ColumnQueueType.PassOne;

    public ChunkColumn(Vector2i position)
    {
        Position = position;
        for (int i = 0; i < PackedWorldGenerator.WorldGenerationHeight; i++)
        {
            Chunks[i] = new PackedChunk((position.X, i, position.Y));
            ChunkMeshes[i] = new PackedChunkMesh((position.X, i, position.Y));
        }
    }
    
}