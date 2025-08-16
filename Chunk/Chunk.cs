using OpenTK.Mathematics;

namespace VoxelGame;

public enum ChunkStatus
{
    Empty,
    Mesh,
    Upload,
    Done
}

public class Chunk
{
    public ChunkSection[] ChunkSections = new ChunkSection[Config.ColumnSize];
    public ChunkSectionMesh[] ChunkMeshes = new ChunkSectionMesh[Config.ColumnSize];
    public Vector2i Position;
    public ChunkStatus Status = ChunkStatus.Empty;
    
    public Chunk(Vector2i position)
    {
        Position = position;
        for (int i = 0; i < Config.ColumnSize; i++)
        {
            ChunkSections[i] = new ChunkSection();
            ChunkMeshes[i] = new ChunkSectionMesh();
        }
    }

    public static void SetBlock(Chunk column, Vector3i localPosition, Block block)
    {
        int index = localPosition.Y / Config.ChunkSize;
        column.ChunkSections[index].SetBlockId(block.Id, localPosition.X, localPosition.Y % Config.ChunkSize, localPosition.Z);
    }
}