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
                    if (PackedWorldGenerator.CurrentWorld.PackedWorldChunks.TryGetValue((x,y,z) + chunkPosition, out PackedChunk chunk)) neighbors.TryAdd((x, y, z), chunk);
                }
            }
        }
        return neighbors;

    }

    private Vector3i[] _offsets = [ Vector3i.UnitX, -Vector3i.UnitX, Vector3i.UnitY, -Vector3i.UnitY, Vector3i.UnitZ, -Vector3i.UnitZ];
    public void RemoveLight(Vector3i globalBlockPosition, LightColor lightColor)
    {

        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);
        BlockLight light = new BlockLight();
        light.Position = localBlockPosition;
        LightColor initialLightColor = ChunkUtils.GetLightColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], light.Position);
        light.LightColor = lightColor;
        // ChunkUtils.SetLightColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], light.Position, LightColor.Zero);
        if (light.LightColor.R != 0) ChunkUtils.SetLightRedColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], light.Position, 0);
        if (light.LightColor.G != 0) ChunkUtils.SetLightGreenColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], light.Position, 0);
        if (light.LightColor.B != 0) ChunkUtils.SetLightBlueColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], light.Position, 0);

        if (initialLightColor != LightColor.Zero)
        {
            light.LightColor = initialLightColor;
            PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockLightRemovalQueue.Enqueue(light);
        } else
        {
            /*
            ushort redValue = light.LightColor.R;
            Vector3i redPosition = Vector3i.Zero;
            for (int i = 0; i < _offsets.Length; i++)
            {

                ushort sample = ChunkUtils.GetLightRedColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + _offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + _offsets[i]));
                if (sample > redValue)
                {
                    redPosition = globalBlockPosition + _offsets[i];
                    redValue = sample;
                }

            }
            if (redValue != light.LightColor.R)
            {
                PackedWorldChunks[ChunkUtils.PositionToChunk(redPosition)].BlockLightAdditionQueue.Enqueue(new BlockLight(ChunkUtils.PositionToBlockLocal(redPosition), new LightColor(redValue, 0, 0)));
            }
            
            /*
            ushort greenValue = light.LightColor.G;
            Vector3i greenPosition = Vector3i.Zero;
            for (int i = 0; i < _offsets.Length; i++)
            {

                ushort sample = ChunkUtils.GetLightGreenColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + _offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + _offsets[i]));
                if (sample > greenValue)
                {
                    greenPosition = globalBlockPosition + _offsets[i];
                    greenValue = sample;
                }

            }
            if (greenValue != light.LightColor.G)
            {
                PackedWorldChunks[ChunkUtils.PositionToChunk(greenPosition)].BlockLightAdditionQueue.Enqueue(new BlockLight(ChunkUtils.PositionToBlockLocal(greenPosition), new LightColor(0, greenValue, 0)));
            }

            ushort blueValue = light.LightColor.B;
            Vector3i bluePosition = Vector3i.Zero;
            for (int i = 0; i < _offsets.Length; i++)
            {

                ushort sample = ChunkUtils.GetLightBlueColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + _offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + _offsets[i]));
                if (sample > blueValue)
                {
                    bluePosition = globalBlockPosition + _offsets[i];
                    blueValue = sample;
                }

            }
            if (blueValue != light.LightColor.B)
            {
                PackedWorldChunks[ChunkUtils.PositionToChunk(bluePosition)].BlockLightAdditionQueue.Enqueue(new BlockLight(ChunkUtils.PositionToBlockLocal(bluePosition), new LightColor(0, 0, blueValue)));
            }
            */

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