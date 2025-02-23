using System.Collections.Concurrent;
using System.Collections.Generic;
using Blockgame_OpenTK.BlockUtil;
using OpenTK.Mathematics;

using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Util;
using System;
using System.ComponentModel;
using OpenTK.Graphics.OpenGL;
using System.Reflection.Metadata;

namespace Blockgame_OpenTK.Core.Worlds;

public class PackedChunkWorld
{
    
    public ConcurrentDictionary<Vector3i, PackedChunk> PackedWorldChunks = new();
    public ConcurrentDictionary<Vector3i, PackedChunkMesh> PackedWorldMeshes = new();
    public ConcurrentDictionary<Vector2i, uint[]> MaxColumnBlockHeight = new();
    public ConcurrentDictionary<Vector2i, ChunkColumn> WorldColumns = new();
    
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
    public void RemoveLight(Vector3i globalBlockPosition)
    {

        BlockLight light = new BlockLight();
        light.Position = ChunkUtils.PositionToBlockLocal(globalBlockPosition);
        light.LightColor = ChunkUtils.GetLightColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], ChunkUtils.PositionToBlockLocal(globalBlockPosition));
        Console.WriteLine(light.LightColor);

        if (light.LightColor.R != 0)
        {
            ChunkUtils.SetLightRedColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], light.Position, 0);
            PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockLightRemovalQueue.Enqueue(new BlockLight(ChunkUtils.PositionToBlockLocal(globalBlockPosition), new LightColor(light.LightColor.R, 0, 0)));
        } else
        {
            for (int i = 0; i < _offsets.Length; i++)
            {
                ushort redvalue = ChunkUtils.GetLightRedColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + _offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + _offsets[i]));
                if (redvalue != 0)
                {
                    PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + _offsets[i])].BlockLightAdditionQueue.Enqueue(new BlockLight(ChunkUtils.PositionToBlockLocal(globalBlockPosition + _offsets[i]), new LightColor(redvalue, 0, 0)));
                }
            }
        }
        if (light.LightColor.G != 0)
        {
            ChunkUtils.SetLightGreenColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], light.Position, 0);
            PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockLightRemovalQueue.Enqueue(new BlockLight(ChunkUtils.PositionToBlockLocal(globalBlockPosition), new LightColor(0, light.LightColor.G, 0)));
        } else 
        {
            for (int i = 0; i < _offsets.Length; i++)
            {
                ushort greenValue = ChunkUtils.GetLightGreenColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + _offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + _offsets[i]));
                if (greenValue != 0)
                {
                    PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + _offsets[i])].BlockLightAdditionQueue.Enqueue(new BlockLight(ChunkUtils.PositionToBlockLocal(globalBlockPosition + _offsets[i]), new LightColor(0, greenValue, 0)));
                }
            }
        }
        if (light.LightColor.B != 0)
        {
            ChunkUtils.SetLightBlueColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], light.Position, 0);
            PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockLightRemovalQueue.Enqueue(new BlockLight(ChunkUtils.PositionToBlockLocal(globalBlockPosition), new LightColor(0, 0, light.LightColor.B)));
        } else
        {
            for (int i = 0; i < _offsets.Length; i++)
            {
                ushort blueValue = ChunkUtils.GetLightBlueColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + _offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + _offsets[i]));
                if (blueValue != 0)
                {
                    PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + _offsets[i])].BlockLightAdditionQueue.Enqueue(new BlockLight(ChunkUtils.PositionToBlockLocal(globalBlockPosition + _offsets[i]), new LightColor(0, 0, blueValue)));
                }
            }
        }

    }

    public void AddLight(Vector3i globalBlockPosition, BlockLight light) 
    {

        light.Position = ChunkUtils.PositionToBlockLocal(globalBlockPosition);

        if (light.LightColor.R != 0)
        {
            ChunkUtils.SetLightRedColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], ChunkUtils.PositionToBlockLocal(globalBlockPosition), light.LightColor.R);
            PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockLightAdditionQueue.Enqueue(new BlockLight(light.Position, new LightColor(light.LightColor.R, 0, 0)));
        }

        if (light.LightColor.G != 0)
        {
            ChunkUtils.SetLightGreenColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], ChunkUtils.PositionToBlockLocal(globalBlockPosition), light.LightColor.G);
            PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockLightAdditionQueue.Enqueue(new BlockLight(light.Position, new LightColor(0, light.LightColor.G, 0)));
        }

        if (light.LightColor.B != 0)
        {
            ChunkUtils.SetLightBlueColor(PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], ChunkUtils.PositionToBlockLocal(globalBlockPosition), light.LightColor.B);
            PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockLightAdditionQueue.Enqueue(new BlockLight(light.Position, new LightColor(0, 0, light.LightColor.B)));
        }

    }

    public void SetBlock(Vector3i globalBlockPosition, NewBlock block)
    {

        ColumnUtils.SetBlockId(WorldColumns[ChunkUtils.PositionToChunk(globalBlockPosition).Xz], globalBlockPosition, block.Id);
        ColumnUtils.SetSolidBlock(WorldColumns[ChunkUtils.PositionToChunk(globalBlockPosition).Xz], globalBlockPosition, block.IsSolid);

    }

    public void QueueChunk(Vector3i globalBlockPosition)
    {

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);

        for (int x = -1; x <= 1; x++)
        {

            for (int z = -1; z <= 1; z++)
            {

                WorldColumns[(x,z) + chunkPosition.Xz].QueueType = ColumnQueueType.Mesh;
                WorldColumns[(x,z) + chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates = true;
                PackedWorldGenerator.ColumnWorldGenerationQueue.EnqueueFirst((x,z) + chunkPosition.Xz);

            }

        }

        /*
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
        */
        
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