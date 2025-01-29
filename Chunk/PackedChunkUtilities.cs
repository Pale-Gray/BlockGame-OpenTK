using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Chunks;

public class PackedChunkUtilities
{

    public static bool GetSolidBlockAt(PackedChunk chunk, Vector3i blockPosition)
    {

        return chunk.SolidMask[Vector2ToIndex(blockPosition.Xz)] >> blockPosition.Y == 1;

    }

    public static void SetSolidBlockAt(PackedChunk chunk, Vector3i blockPosition, bool isSolid)
    {
        
        chunk.SolidMask[Vector2ToIndex(blockPosition.Xz)] = chunk.SolidMask[Vector2ToIndex(blockPosition.Xz)] & ~(1u << blockPosition.Y) | ((isSolid ? 1u : 0u) << blockPosition.Y);
        
    }

    public static ushort GetBlockIdAt(PackedChunk chunk, Vector3i blockPosition)
    {
        
        return chunk.BlockData[Vector3ToIndex(blockPosition)];
        
    }

    public static void SetBlockIdAt(PackedChunk chunk, Vector3i blockPosition, ushort blockId)
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