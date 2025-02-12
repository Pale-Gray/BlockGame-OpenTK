using System.Collections.Concurrent;
using System.Collections.Generic;
using Blockgame_OpenTK.BlockUtil;
using OpenTK.Mathematics;

using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Util;

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

    public void SetBlock(Vector3i globalBlockPosition, NewBlock block)
    {

        PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition))] = block.Id;
        if (block.IsSolid)
        {
            PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].LightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition))] = 0;
        }
        else
        {
            PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].LightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition))] = ushort.MaxValue;
        }
        
    }

    public void QueueChunk(Vector3i globalBlockPosition)
    {

        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);
        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);

        PackedWorldChunks[chunkPosition].HasPriority = true;
        PackedWorldChunks[chunkPosition].QueueType = PackedChunkQueueType.Mesh;
        PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition);
        
        if (localBlockPosition.Y == 0 && PackedWorldChunks.ContainsKey(chunkPosition - Vector3i.UnitY))
        {
            PackedWorldChunks[chunkPosition - Vector3i.UnitY].HasPriority = true;
            PackedWorldChunks[chunkPosition - Vector3i.UnitY].QueueType = PackedChunkQueueType.Mesh;
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition - Vector3i.UnitY);
        }

        if (localBlockPosition.Y == GlobalValues.ChunkSize - 1 && PackedWorldChunks.ContainsKey(chunkPosition + Vector3i.UnitY))
        {
            PackedWorldChunks[chunkPosition + Vector3i.UnitY].HasPriority = true;
            PackedWorldChunks[chunkPosition + Vector3i.UnitY].QueueType = PackedChunkQueueType.Mesh;
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition + Vector3i.UnitY);
        }

        if (localBlockPosition.X == 0 && PackedWorldChunks.ContainsKey(chunkPosition - Vector3i.UnitX))
        {
            PackedWorldChunks[chunkPosition - Vector3i.UnitX].HasPriority = true;
            PackedWorldChunks[chunkPosition - Vector3i.UnitX].QueueType = PackedChunkQueueType.Mesh;
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition - Vector3i.UnitX);
        }

        if (localBlockPosition.X == GlobalValues.ChunkSize - 1 && PackedWorldChunks.ContainsKey(chunkPosition + Vector3i.UnitX))
        {
            PackedWorldChunks[chunkPosition + Vector3i.UnitX].HasPriority = true;
            PackedWorldChunks[chunkPosition + Vector3i.UnitX].QueueType = PackedChunkQueueType.Mesh;
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition + Vector3i.UnitX);
        }
        
        if (localBlockPosition.Z == 0 && PackedWorldChunks.ContainsKey(chunkPosition - Vector3i.UnitZ))
        {
            PackedWorldChunks[chunkPosition - Vector3i.UnitZ].HasPriority = true;
            PackedWorldChunks[chunkPosition - Vector3i.UnitZ].QueueType = PackedChunkQueueType.Mesh;
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition - Vector3i.UnitZ);
        }

        if (localBlockPosition.Z == GlobalValues.ChunkSize - 1 && PackedWorldChunks.ContainsKey(chunkPosition + Vector3i.UnitZ))
        {
            PackedWorldChunks[chunkPosition + Vector3i.UnitZ].HasPriority = true;
            PackedWorldChunks[chunkPosition + Vector3i.UnitZ].QueueType = PackedChunkQueueType.Mesh;
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition + Vector3i.UnitZ);
        }

    }

}