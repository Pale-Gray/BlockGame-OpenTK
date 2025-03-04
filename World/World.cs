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
using System.Net;
using Blockgame_OpenTK.PlayerUtil;

namespace Blockgame_OpenTK.Core.Worlds;

public class World
{
    
    public ConcurrentDictionary<Vector3i, PackedChunk> PackedWorldChunks = new();
    public ConcurrentDictionary<Vector3i, PackedChunkMesh> PackedWorldMeshes = new();
    public ConcurrentDictionary<Vector2i, uint[]> MaxColumnBlockHeight = new();
    public ConcurrentDictionary<Vector2i, ChunkColumn> WorldColumns = new();


    public void Draw(Player player)
    {

        foreach (ChunkColumn column in WorldColumns.Values)
        {

            for (int i = 0; i < PackedWorldGenerator.WorldGenerationHeight; i++)
            {

                column.ChunkMeshes[i].Draw(player);

            }

        }

    }
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

        WorldColumns[chunkPosition.Xz].QueueType = ColumnQueueType.Mesh;
        WorldColumns[chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates = true;
        WorldColumns[chunkPosition.Xz].HasPriority = true;
        PackedWorldGenerator.ColumnWorldGenerationQueue.EnqueueFirst(chunkPosition.Xz);

        for (int x = -1; x <= 1; x++)
        {

            for (int z = -1; z <= 1; z++)
            {

                if ((x,z) != Vector2i.Zero)
                {
                    WorldColumns[(x,z) + chunkPosition.Xz].QueueType = ColumnQueueType.Mesh;
                    WorldColumns[(x,z) + chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates = true;
                    WorldColumns[(x,z) + chunkPosition.Xz].HasPriority = true;
                    PackedWorldGenerator.ColumnWorldGenerationQueue.EnqueueFirst((x,z) + chunkPosition.Xz);
                }

            }

        }

    }

}