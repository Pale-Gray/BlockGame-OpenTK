using System.Collections.Concurrent;
using System.Collections.Generic;
using Game.BlockUtil;
using OpenTK.Mathematics;

using Game.Core.Chunks;
using Game.Util;
using System;
using System.ComponentModel;
using OpenTK.Graphics.OpenGL;
using System.Reflection.Metadata;
using System.Net;
using Game.PlayerUtil;
using Game.Core.PlayerUtil;
using System.IO;
using Game.Core.Chrono;

namespace Game.Core.Worlds;

public class World
{
    
    public ConcurrentDictionary<Vector3i, Chunk> PackedWorldChunks = new();
    public ConcurrentDictionary<Vector2i, ChunkColumn> WorldColumns = new();
    public long TickTime;
    public Skybox Skybox;
    public string WorldPath { get; private set; }

    public World(string worldPath)
    {

        WorldPath = worldPath;
        Skybox = new Skybox();
        Directory.CreateDirectory(Path.Combine("Worlds", worldPath, "Chunks"));
        // if (!Directory.Exists(Path.Combine("Worlds", Path.Combine(worldPath.Split('/'))))) Directory.CreateDirectory(Path.Combine("Worlds", Path.Combine(worldPath.Split('/'))));

    }

    public void TickUpdate()
    {

        TickTime++;

    }
    public void Draw(Player player)
    {

        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.Disable(EnableCap.CullFace);
        Skybox.Draw(player);
        GL.Enable(EnableCap.CullFace);
        GL.Clear(ClearBufferMask.DepthBufferBit);

        foreach (ChunkColumn column in WorldColumns.Values)
        {

            if (Maths.ChebyshevDistance2D(column.Position, player.Loader.PlayerPosition) <= WorldGenerator.WorldGenerationRadius - 3)
            {

                for (int i = 0; i < WorldGenerator.WorldGenerationHeight; i++) column.ChunkMeshes[i].Draw(player);

            }

        }

    }

    public void AddModel(BlockModel model, Vector3i globalBlockPosition)
    {

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);
        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);

        ChunkMesh mesh = WorldColumns[chunkPosition.Xz].ChunkMeshes[chunkPosition.Y];

        

    }

    public void AddLight(Vector3i globalBlockPosition, ushort red, ushort green, ushort blue) 
    {

        Vector2i columnPosition = ColumnUtils.PositionToChunk(globalBlockPosition);
        Vector3i columnLocalPosition = ColumnUtils.GlobalToLocal(globalBlockPosition);

        if (red != 0)
        {
            ColumnUtils.SetRedBlocklightValue(WorldColumns[columnPosition], columnLocalPosition, red);
            WorldColumns[columnPosition].RedBlocklightAdditionQueue.Enqueue(globalBlockPosition);
        }

        if (green != 0)
        {
            ColumnUtils.SetGreenBlocklightValue(WorldColumns[columnPosition], columnLocalPosition, green);
            WorldColumns[columnPosition].GreenBlocklightAdditionQueue.Enqueue(globalBlockPosition);
        }

        if (blue != 0)
        {
            ColumnUtils.SetBlueBlocklightValue(WorldColumns[columnPosition], columnLocalPosition, blue);
            WorldColumns[columnPosition].BlueBlocklightAdditionQueue.Enqueue(globalBlockPosition);
        }

    }

    public void SetBlock(Vector3i globalBlockPosition, Block block, bool isPlayerPlaced = true)
    {

        ColumnUtils.SetBlockId(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition)], globalBlockPosition, block.Id);
        ColumnUtils.SetSolidBlock(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition)], globalBlockPosition, block.IsSolid);
        ColumnUtils.SetHeightmap(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition)], globalBlockPosition);

        if (isPlayerPlaced)
        {

            Vector2i chunkPosition = ColumnUtils.PositionToChunk(globalBlockPosition);

            ushort sunlightValue = ColumnUtils.GetSunlightValue(WorldColumns[chunkPosition], ColumnUtils.GlobalToLocal(globalBlockPosition));
            ushort redLightValue = ColumnUtils.GetRedBlocklightValue(WorldColumns[chunkPosition], ColumnUtils.GlobalToLocal(globalBlockPosition));
            ushort greenLightValue = ColumnUtils.GetGreenBlocklightValue(WorldColumns[chunkPosition], ColumnUtils.GlobalToLocal(globalBlockPosition));
            ushort blueLightValue = ColumnUtils.GetBlueBlocklightValue(WorldColumns[chunkPosition], ColumnUtils.GlobalToLocal(globalBlockPosition));
            ColumnUtils.SetSunlightValue(WorldColumns[chunkPosition], ColumnUtils.GlobalToLocal(globalBlockPosition), 0);
            WorldColumns[chunkPosition].SunlightRemovalQueue.Enqueue((globalBlockPosition, sunlightValue));
            ColumnUtils.SetRedBlocklightValue(WorldColumns[chunkPosition], ColumnUtils.GlobalToLocal(globalBlockPosition), 0);
            WorldColumns[chunkPosition].RedBlocklightRemovalQueue.Enqueue((globalBlockPosition, redLightValue));
            ColumnUtils.SetGreenBlocklightValue(WorldColumns[chunkPosition], ColumnUtils.GlobalToLocal(globalBlockPosition), 0);
            WorldColumns[chunkPosition].GreenBlocklightRemovalQueue.Enqueue((globalBlockPosition, greenLightValue));
            ColumnUtils.SetBlueBlocklightValue(WorldColumns[chunkPosition], ColumnUtils.GlobalToLocal(globalBlockPosition), 0);
            WorldColumns[chunkPosition].BlueBlocklightRemovalQueue.Enqueue((globalBlockPosition, blueLightValue));

            Commit(globalBlockPosition);

        }

    }

    private void Commit(Vector3i globalBlockPosition)
    {

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);

        WorldColumns[chunkPosition.Xz].QueueType = QueueType.LightPropagation;
        WorldColumns[chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates = true;
        WorldColumns[chunkPosition.Xz].HasPriority = true;
        WorldGenerator.HighPriorityWorldGenerationQueue.Enqueue(chunkPosition.Xz);

        for (int x = -1; x <= 1; x++)
        {

            for (int z = -1; z <= 1; z++)
            {

                if ((x,z) != Vector2i.Zero)
                {

                    WorldColumns[(x,z) + chunkPosition.Xz].QueueType = QueueType.LightPropagation;
                    if ((x,z) != Vector2i.Zero) WorldColumns[(x,z) + chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates = true;
                    if (chunkPosition.Y + 1 < WorldGenerator.WorldGenerationHeight) WorldColumns[(x,z) + chunkPosition.Xz].Chunks[chunkPosition.Y + 1].HasUpdates = true;
                    if (chunkPosition.Y - 1 > 0) WorldColumns[(x,z) + chunkPosition.Xz].Chunks[chunkPosition.Y - 1].HasUpdates = true;
                    WorldColumns[(x,z) + chunkPosition.Xz].HasPriority = false;
                    WorldGenerator.LowPriorityWorldGenerationQueue.Enqueue((x,z) + chunkPosition.Xz);

                }

            }

        }

    }

}