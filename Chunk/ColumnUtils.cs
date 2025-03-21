using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Game.Core.Worlds;
using OpenTK.Mathematics;

namespace Game.Core.Chunks;

public class ColumnUtils
{

    public static void SetSolidBlock(ChunkColumn column, Vector3i globalBlockPosition, bool isSolid)
    {

        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);
        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);

        column.Chunks[chunkPosition.Y].SolidMask[ChunkUtils.VecToIndex(localBlockPosition.Xz)] &= ~(1u << localBlockPosition.Y);
        column.Chunks[chunkPosition.Y].SolidMask[ChunkUtils.VecToIndex(localBlockPosition.Xz)] |= (isSolid ? 1u : 0u) << localBlockPosition.Y;

    }

    public static bool GetSolidBlock(ChunkColumn column, Vector3i globalBlockPosition)
    {

        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);
        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);

        if (chunkPosition.Y < 0 || chunkPosition.Y > PackedWorldGenerator.WorldGenerationHeight) return false;

        return (column.Chunks[chunkPosition.Y].SolidMask[ChunkUtils.VecToIndex(localBlockPosition.Xz)] & (1u << localBlockPosition.Y)) != 0u;

    }
    public static void SetBlockId(ChunkColumn column, Vector3i globalBlockPosition, ushort id)
    {

        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);
        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);

        column.Chunks[chunkPosition.Y].BlockData[ChunkUtils.VecToIndex(localBlockPosition)] = id;

    }
    public static List<Vector2i> GetRing(int radius)
    {

        List<Vector2i> vectors = new();

        for (int x = -radius; x <= radius; x++)
        {

            if (x == -radius || x == radius)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    vectors.Add((x,z));
                }
            } else
            {
                vectors.Add((x,-radius));
                vectors.Add((x,radius));
            }

        }

        return vectors;

    }

}