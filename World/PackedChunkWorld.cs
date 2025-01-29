using System.Collections.Concurrent;
using System.Collections.Generic;
using OpenTK.Mathematics;

using Blockgame_OpenTK.Core.Chunks;

namespace Blockgame_OpenTK.Core.Worlds;

public class PackedChunkWorld
{
    
    public ConcurrentDictionary<Vector3i, PackedChunk> PackedWorldChunks = new();
    public ConcurrentDictionary<Vector3i, PackedChunkMesh> PackedWorldMeshes = new();
    
    public ConcurrentDictionary<Vector3i, PackedChunk> GetChunkNeighbors(Vector3i chunkPosition)
    {

        ConcurrentDictionary<Vector3i, PackedChunk> neighbors = new();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    neighbors.TryAdd((x, y, z), PackedWorldGenerator.CurrentWorld.PackedWorldChunks[(x, y, z) + chunkPosition]);
                }
            }
        }
        return neighbors;

    } 

}