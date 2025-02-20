using System.Collections.Concurrent;
using System.Collections.Generic;
using Blockgame_OpenTK.BlockUtil;
using OpenTK.Mathematics;

using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Util;
using System;

namespace Blockgame_OpenTK.Core.Worlds;

public class PackedChunkWorld
{
    
    public ConcurrentDictionary<Vector3i, PackedChunk> PackedWorldChunks = new();
    public ConcurrentDictionary<Vector3i, PackedChunkMesh> PackedWorldMeshes = new();
    
    public Dictionary<Vector3i, PackedChunk> GetChunkNeighbors(Vector3i chunkPosition)
    {

        Dictionary<Vector3i, PackedChunk> neighbors = new();
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

    private Vector3i[] _offsets = [ Vector3i.UnitX, -Vector3i.UnitX, Vector3i.UnitY, -Vector3i.UnitY, Vector3i.UnitZ, -Vector3i.UnitZ];
    public void RemoveLight(Vector3i globalBlockPosition)
    {

        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);
        BlockLight light = new BlockLight();
        light.Position = localBlockPosition;
        light.LightColor = ChunkUtils.GetLightColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], light.Position);

        if (light.LightColor == LightColor.Zero)
        {

            ushort currentRedValue = 0;
            Vector3i currentPosition = Vector3i.Zero;
            for (int i = 0; i < _offsets.Length; i++)
            {

                ushort redValue = ChunkUtils.GetLightRedColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + _offsets[i])], ChunkUtils.PositionToBlockLocal(globalBlockPosition + _offsets[i]));
                if (redValue > currentRedValue)
                {
                    currentRedValue = redValue;
                    currentPosition = globalBlockPosition + _offsets[i];
                }

            }
            light.Position = ChunkUtils.PositionToBlockLocal(currentPosition);
            light.LightColor = new LightColor(currentRedValue, 0, 0);
            PackedWorldChunks[ChunkUtils.PositionToChunk(currentPosition)].BlockLightAdditionQueue.Enqueue(light);

            ushort currentGreenValue = 0;
            currentPosition = Vector3i.Zero;
            for (int i = 0; i < _offsets.Length; i++)
            {

                ushort greenValue = ChunkUtils.GetLightGreenColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + _offsets[i])], ChunkUtils.PositionToBlockLocal(globalBlockPosition + _offsets[i]));
                if (greenValue > currentGreenValue)
                {
                    currentGreenValue = greenValue;
                    currentPosition = globalBlockPosition + _offsets[i];
                }

            }
            light.Position = ChunkUtils.PositionToBlockLocal(currentPosition);
            light.LightColor = new LightColor(0, currentGreenValue, 0);
            PackedWorldChunks[ChunkUtils.PositionToChunk(currentPosition)].BlockLightAdditionQueue.Enqueue(light);

            ushort currentBlueValue = 0;
            currentPosition = Vector3i.Zero;
            for (int i = 0; i < _offsets.Length; i++)
            {

                ushort blueValue = ChunkUtils.GetLightBlueColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + _offsets[i])], ChunkUtils.PositionToBlockLocal(globalBlockPosition + _offsets[i]));
                if (blueValue > currentBlueValue)
                {
                    currentBlueValue = blueValue;
                    currentPosition = globalBlockPosition + _offsets[i];
                }

            }
            light.Position = ChunkUtils.PositionToBlockLocal(currentPosition);
            light.LightColor = new LightColor(0, 0, currentBlueValue);
            PackedWorldChunks[ChunkUtils.PositionToChunk(currentPosition)].BlockLightAdditionQueue.Enqueue(light);

        } else 
        {

            PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockLightRemovalQueue.Enqueue(light);

        }
        

    }

    public void AddLight(Vector3i globalBlockPosition, BlockLight light) 
    {

        light.Position = ChunkUtils.PositionToBlockLocal(globalBlockPosition);

        PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockLightAdditionQueue.Enqueue(light);

    }

    public void SetBlock(Vector3i globalBlockPosition, NewBlock block)
    {

        PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition))] = block.Id;
        ChunkUtils.SetSolidBlock(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], ChunkUtils.PositionToBlockLocal(globalBlockPosition), block.IsSolid);
        
    }

    public void QueueChunk(Vector3i globalBlockPosition)
    {

        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);
        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);

        PackedWorldChunks[chunkPosition].HasPriority = true;
        PackedWorldChunks[chunkPosition].QueueType = PackedChunkQueueType.LightPropagation;
        PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition);

        
        for (int x = -1; x <= 1; x++) 
        {

            for (int y = -1; y <= 1; y++) 
            {

                for (int z = -1; z <= 1; z++) 
                {

                    if ((x,y,z) != Vector3i.Zero && PackedWorldChunks.ContainsKey(chunkPosition + (x,y,z))) 
                    {

                        // PackedWorldChunks[chunkPosition + (x,y,z)].HasPriority = true;
                        PackedWorldChunks[chunkPosition + (x,y,z)].QueueType = PackedChunkQueueType.LightPropagation;
                        PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition + (x,y,z));

                    }

                }

            }    

        }
        
        /*
        if (localBlockPosition.Y == 0 && PackedWorldChunks.ContainsKey(chunkPosition - Vector3i.UnitY))
        {
            PackedWorldChunks[chunkPosition - Vector3i.UnitY].HasPriority = true;
            PackedWorldChunks[chunkPosition - Vector3i.UnitY].QueueType = PackedChunkQueueType.LightPropagation;
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition - Vector3i.UnitY);
        }

        if (localBlockPosition.Y == GlobalValues.ChunkSize - 1 && PackedWorldChunks.ContainsKey(chunkPosition + Vector3i.UnitY))
        {
            PackedWorldChunks[chunkPosition + Vector3i.UnitY].HasPriority = true;
            PackedWorldChunks[chunkPosition + Vector3i.UnitY].QueueType = PackedChunkQueueType.LightPropagation;
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition + Vector3i.UnitY);
        }

        if (localBlockPosition.X == 0 && PackedWorldChunks.ContainsKey(chunkPosition - Vector3i.UnitX))
        {
            PackedWorldChunks[chunkPosition - Vector3i.UnitX].HasPriority = true;
            PackedWorldChunks[chunkPosition - Vector3i.UnitX].QueueType = PackedChunkQueueType.LightPropagation;
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition - Vector3i.UnitX);
        }

        if (localBlockPosition.X == GlobalValues.ChunkSize - 1 && PackedWorldChunks.ContainsKey(chunkPosition + Vector3i.UnitX))
        {
            PackedWorldChunks[chunkPosition + Vector3i.UnitX].HasPriority = true;
            PackedWorldChunks[chunkPosition + Vector3i.UnitX].QueueType = PackedChunkQueueType.LightPropagation;
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition + Vector3i.UnitX);
        }
        
        if (localBlockPosition.Z == 0 && PackedWorldChunks.ContainsKey(chunkPosition - Vector3i.UnitZ))
        {
            PackedWorldChunks[chunkPosition - Vector3i.UnitZ].HasPriority = true;
            PackedWorldChunks[chunkPosition - Vector3i.UnitZ].QueueType = PackedChunkQueueType.LightPropagation;
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition - Vector3i.UnitZ);
        }

        if (localBlockPosition.Z == GlobalValues.ChunkSize - 1 && PackedWorldChunks.ContainsKey(chunkPosition + Vector3i.UnitZ))
        {
            PackedWorldChunks[chunkPosition + Vector3i.UnitZ].HasPriority = true;
            PackedWorldChunks[chunkPosition + Vector3i.UnitZ].QueueType = PackedChunkQueueType.LightPropagation;
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunkPosition + Vector3i.UnitZ);
        }
        */

    }

}