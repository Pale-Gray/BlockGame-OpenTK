using Game.Util;
using OpenTK.Mathematics;

namespace Game.Core.Chunks;

public class ChunkUtilities
{

    public static bool GetSolidBlockAt(Chunk chunk, Vector3i blockPosition)
    {

        return chunk.SolidMask[Vector2ToIndex(blockPosition.Xz)] >> blockPosition.Y == 1;

    }

    public static void SetSolidBlockAt(Chunk chunk, Vector3i blockPosition, bool isSolid)
    {
        
        chunk.SolidMask[Vector2ToIndex(blockPosition.Xz)] = chunk.SolidMask[Vector2ToIndex(blockPosition.Xz)] & ~(1u << blockPosition.Y) | ((isSolid ? 1u : 0u) << blockPosition.Y);
        
    }

    public static ushort GetBlockIdAt(Chunk chunk, Vector3i blockPosition)
    {
        
        return chunk.BlockData[Vector3ToIndex(blockPosition)];
        
    }

    public static void SetBlockIdAt(Chunk chunk, Vector3i blockPosition, ushort blockId)
    {
        
        chunk.BlockData[Vector3ToIndex(blockPosition)] = blockId;
        
    }

    public static int Vector2ToIndex(Vector2i vector)
    {

        return vector.X + (vector.Y * GlobalValues.ChunkSize);

    }

    public static int Vector3ToIndex(Vector3i vector)
    {
        
        return vector.X + (vector.Y * GlobalValues.ChunkSize) + (vector.Z * GlobalValues.ChunkSize * GlobalValues.ChunkSize);
        
    }
    
}