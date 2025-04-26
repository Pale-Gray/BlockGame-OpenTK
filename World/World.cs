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
using Game.Core.BlockStorage;
using System.Reflection;
using OpenTK.Graphics.Wgl;
using System.Diagnostics;

namespace Game.Core.Worlds;

public class World
{
    
    public ConcurrentDictionary<Vector3i, Chunk> PackedWorldChunks = new();
    public ConcurrentDictionary<Vector2i, ChunkColumn> WorldColumns = new();
    public long TickTime;
    public Skybox Skybox;
    public string WorldPath { get; private set; }
    public double DrawTime = 0;
    public int ChunksDrawn = 0;

    public World(string worldPath)
    {

        WorldPath = worldPath;
        Skybox = new Skybox();
        Directory.CreateDirectory(Path.Combine("Worlds", worldPath, "Chunks"));
        
    }

    public void TickUpdate()
    {

        TickTime++;

        foreach (ChunkColumn column in WorldColumns.Values)
        {

            foreach (KeyValuePair<Vector3i, (uint time, uint rate)> blockTicker in column.BlockTickers)
            {

                Vector3i globalPosition = blockTicker.Key + (new Vector3i(column.Position.X, 0, column.Position.Y) * GlobalValues.ChunkSize);

                if (blockTicker.Value.time >= blockTicker.Value.rate)
                {

                    (uint time, uint rate) iter = blockTicker.Value;
                    iter.time = 0;
                    column.BlockTickers[blockTicker.Key] = iter;
                    GlobalValues.Register.GetBlockFromId(ColumnUtils.GetBlockId(column, globalPosition)).OnFixedTick(GameState.World, globalPosition, true, false);

                } else
                {

                    (uint time, uint rate) iter = blockTicker.Value;
                    iter.time++;
                    column.BlockTickers[blockTicker.Key] = iter;

                }

            }

            foreach (Player player in NetworkingValues.Server.ConnectedPlayers.Values)
            {

                if (Maths.ChebyshevDistance2D(player.Loader.PlayerPosition, column.Position) <= 1 && column.QueueType >= QueueType.Done)
                {

                    for (int chunkY = 0; chunkY < WorldGenerator.WorldGenerationHeight; chunkY++)
                    {

                        for (int i = 0; i < 8; i++)
                        {

                            int x = GlobalValues.RandomGenerator.Next(0, GlobalValues.ChunkSize);
                            int y = GlobalValues.RandomGenerator.Next(0, GlobalValues.ChunkSize);
                            int z = GlobalValues.RandomGenerator.Next(0, GlobalValues.ChunkSize);

                            Vector3i globalPosition = (x + (column.Position.X * GlobalValues.ChunkSize), y + (chunkY * GlobalValues.ChunkSize), z + (column.Position.Y * GlobalValues.ChunkSize));

                            GlobalValues.Register.GetBlockFromId(ChunkUtils.GetBlockId(column.Chunks[chunkY], ChunkUtils.PositionToBlockLocal(globalPosition))).OnRandomTick(GameState.World, globalPosition, true, false);

                        }

                    }

                }

            }

        }

    }

    public ushort GetBlockId(Vector3i globalBlockPosition)
    {

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);
        return WorldColumns[chunkPosition.Xz].Chunks[chunkPosition.Y].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition))];

    }
    public void Draw(Player player)
    {

        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.Disable(EnableCap.CullFace);
        Skybox.Draw(player);
        GL.Enable(EnableCap.CullFace);
        GL.Clear(ClearBufferMask.DepthBufferBit);

        Stopwatch stopwatch = Stopwatch.StartNew();
        ChunksDrawn = 0;
        foreach (ChunkColumn column in WorldColumns.Values)
        {

            if (Maths.ChebyshevDistance2D(column.Position, player.Loader.PlayerPosition) <= WorldGenerator.WorldGenerationRadius - 3)
            {

                for (int i = 0; i < WorldGenerator.WorldGenerationHeight; i++) column.ChunkMeshes[i].Draw(player);

            }

        }
        stopwatch.Stop();
        DrawTime = stopwatch.Elapsed.TotalMilliseconds;

    }

    public void AddBlockProperty(IBlockProperties properties, Vector3i globalBlockPosition)
    {

        Vector2i columnPosition = ColumnUtils.PositionToChunk(globalBlockPosition);
        WorldColumns[columnPosition].BlockProperties.Add(ColumnUtils.GlobalToLocal(globalBlockPosition), properties);

    }

    public bool TryAddBlockProperty(IBlockProperties properties, Vector3i globalBlockPosition)
    {

        Vector2i columnPosition = ColumnUtils.PositionToChunk(globalBlockPosition);
        return WorldColumns[columnPosition].BlockProperties.TryAdd(ColumnUtils.GlobalToLocal(globalBlockPosition), properties);

    }

    public void RemoveBlockProperty(Vector3i globalBlockPosition)
    {

        Vector2i columnPosition = ColumnUtils.PositionToChunk(globalBlockPosition);
        WorldColumns[columnPosition].BlockProperties.Remove(ColumnUtils.GlobalToLocal(globalBlockPosition));

    }

    public T GetBlockProperty<T>(Vector3i globalBlockPosition) where T : IBlockProperties
    {

        Vector2i columnPosition = ColumnUtils.PositionToChunk(globalBlockPosition);
        return (T) WorldColumns[columnPosition].BlockProperties[ColumnUtils.GlobalToLocal(globalBlockPosition)];

    }

    public bool TryGetBlockProperty<T>(Vector3i globalBlockPosition, out T blockProperty) where T : IBlockProperties
    {

        Vector2i columnPosition = ColumnUtils.PositionToChunk(globalBlockPosition);
        if (WorldColumns[columnPosition].BlockProperties.TryGetValue(ColumnUtils.GlobalToLocal(globalBlockPosition), out IBlockProperties properties) && properties is T)
        {

            blockProperty = (T) properties;
            return true;

        }
        blockProperty = default;
        return false;

    }

    public void AddBlockTicker(Vector3i globalBlockPosition, uint rate)
    {

        Vector2i columnPosition = ColumnUtils.PositionToChunk(globalBlockPosition);
        WorldColumns[columnPosition].BlockTickers.Add(ColumnUtils.GlobalToLocal(globalBlockPosition), (0, rate));

    }

    public void RemoveBlockTicker(Vector3i globalBlockPosition)
    {

        Vector2i columnPosition = ColumnUtils.PositionToChunk(globalBlockPosition);
        WorldColumns[columnPosition].BlockTickers.Remove(ColumnUtils.GlobalToLocal(globalBlockPosition));

    }

    private Vector3i[] _offsets = { Vector3i.UnitX, -Vector3i.UnitX, Vector3i.UnitY, -Vector3i.UnitY, Vector3i.UnitZ, -Vector3i.UnitZ};

    public void AddModel(BlockModel model, Vector3i globalBlockPosition, List<Rectangle> solids, List<Rectangle> cutouts)
    {

        Vector2i columnPosition = ColumnUtils.PositionToChunk(globalBlockPosition);
        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);
        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);

        model.AddFreeformFace(solids, BlockUtil.Direction.None, localBlockPosition, ColumnUtils.GetNormalizedLightValues(WorldColumns[columnPosition], ColumnUtils.GlobalToLocal(globalBlockPosition)));
        model.AddCutoutFace(cutouts, BlockUtil.Direction.None, localBlockPosition, ColumnUtils.GetNormalizedLightValues(WorldColumns[columnPosition], ColumnUtils.GlobalToLocal(globalBlockPosition)));

        for (int i = 0; i < _offsets.Length; i++)
        {

            Vector2i sampleColumnPosition = ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i]);
            Vector3i sampleLocalPosition = ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i]);

            if (!ColumnUtils.GetSolidBlock(WorldColumns[sampleColumnPosition], sampleLocalPosition))
            {

                switch (_offsets[i])
                {

                    case (0, 1, 0):
                        {

                            Vector4 centerLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[sampleColumnPosition], sampleLocalPosition);

                            if (WorldGenerator.IsSmoothLightingEnabled)
                            {

                                Vector4 topLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (1, 0, 1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (1, 0, 1)));
                                Vector4 centerLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (1, 0, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (1, 0, 0)));
                                Vector4 bottomLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (1, 0, -1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (1, 0, -1)));
                                Vector4 centerTopLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 0, 1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 0, 1)));
                                Vector4 centerBottomLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 0, -1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 0, -1)));
                                Vector4 topRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (-1, 0, 1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (-1, 0, 1)));
                                Vector4 centerRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (-1, 0, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (-1, 0, 0)));
                                Vector4 bototmRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (-1, 0, -1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (-1, 0, -1)));
                                model.AddAmbientOcclusionFace(solids, BlockUtil.Direction.Top, localBlockPosition, (
                                    (topLeftLight + centerLeftLight + centerTopLight + centerLight) / 4.0f,
                                    (centerLeftLight + bottomLeftLight + centerLight + centerBottomLight) / 4.0f,
                                    (centerLight + centerBottomLight + centerRightLight + bototmRightLight) / 4.0f, 
                                    (centerTopLight + centerLight + topRightLight + centerRightLight) / 4.0f
                                    ));

                            } else
                            {

                                model.AddAmbientOcclusionFace(solids, BlockUtil.Direction.Top, localBlockPosition, (
                                    centerLight,
                                    centerLight,
                                    centerLight, 
                                    centerLight
                                    ));

                            }

                            model.AddFreeformFace(solids, BlockUtil.Direction.Top, localBlockPosition, centerLight);
                            model.AddCutoutFace(cutouts, BlockUtil.Direction.Top, localBlockPosition, centerLight);

                        }
                        break;
                    case (0, -1, 0):
                        {

                            Vector4 centerLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[sampleColumnPosition], sampleLocalPosition);

                            if (WorldGenerator.IsSmoothLightingEnabled)
                            {

                                Vector4 topLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (1, 0, -1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (1, 0, -1)));
                                Vector4 centerLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (1, 0, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (1, 0, 0)));
                                Vector4 bottomLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (1, 0, 1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (1, 0, 1)));
                                Vector4 centerTopLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 0, -1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 0, -1)));
                                Vector4 centerBottomLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 0, 1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 0, 1)));
                                Vector4 topRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (-1, 0, -1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (-1, 0, -1)));
                                Vector4 centerRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (-1, 0, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (-1, 0, 0)));
                                Vector4 bototmRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (-1, 0, 1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (-1, 0, 1)));
                                model.AddAmbientOcclusionFace(solids, BlockUtil.Direction.Bottom, localBlockPosition, (
                                    (topLeftLight + centerLeftLight + centerTopLight + centerLight) / 4.0f,
                                    (centerLeftLight + bottomLeftLight + centerLight + centerBottomLight) / 4.0f,
                                    (centerLight + centerBottomLight + centerRightLight + bototmRightLight) / 4.0f, 
                                    (centerTopLight + centerLight + topRightLight + centerRightLight) / 4.0f
                                    ));

                            } else
                            {

                                model.AddAmbientOcclusionFace(solids, BlockUtil.Direction.Bottom, localBlockPosition, (
                                    centerLight,
                                    centerLight,
                                    centerLight, 
                                    centerLight
                                    ));

                            }

                            model.AddFreeformFace(solids, BlockUtil.Direction.Bottom, localBlockPosition, centerLight);
                            model.AddCutoutFace(cutouts, BlockUtil.Direction.Bottom, localBlockPosition, centerLight);

                        }
                        break;
                    case (1, 0, 0):
                        {

                            Vector4 centerLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[sampleColumnPosition], sampleLocalPosition);

                            if (WorldGenerator.IsSmoothLightingEnabled)
                            {

                                Vector4 topLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 1, 1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 1, 1)));
                                Vector4 centerLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 0, 1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 0, 1)));
                                Vector4 bottomLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, -1, 1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, -1, 1)));
                                Vector4 centerTopLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 1, 0)));
                                Vector4 centerBottomLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, -1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, -1, 0)));
                                Vector4 topRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 1, -1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 1, -1)));
                                Vector4 centerRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 0, -1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 0, -1)));
                                Vector4 bototmRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, -1, -1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, -1, -1)));
                                model.AddAmbientOcclusionFace(solids, BlockUtil.Direction.Left, localBlockPosition, (
                                    (topLeftLight + centerLeftLight + centerTopLight + centerLight) / 4.0f,
                                    (centerLeftLight + bottomLeftLight + centerLight + centerBottomLight) / 4.0f,
                                    (centerLight + centerBottomLight + centerRightLight + bototmRightLight) / 4.0f, 
                                    (centerTopLight + centerLight + topRightLight + centerRightLight) / 4.0f
                                    ));

                            } else
                            {

                                model.AddAmbientOcclusionFace(solids, BlockUtil.Direction.Left, localBlockPosition, (
                                    centerLight,
                                    centerLight,
                                    centerLight, 
                                    centerLight
                                    ));

                            }

                            model.AddFreeformFace(solids, BlockUtil.Direction.Left, localBlockPosition, centerLight);
                            model.AddCutoutFace(cutouts, BlockUtil.Direction.Left, localBlockPosition, centerLight);

                        }
                        break;
                    case (-1, 0, 0):
                        {

                            Vector4 centerLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[sampleColumnPosition], sampleLocalPosition);

                            if (WorldGenerator.IsSmoothLightingEnabled)
                            {

                                Vector4 topLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 1, -1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 1, -1)));
                                Vector4 centerLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 0, -1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 0, -1)));
                                Vector4 bottomLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, -1, -1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, -1, -1)));
                                Vector4 centerTopLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 1, 0)));
                                Vector4 centerBottomLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, -1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, -1, 0)));
                                Vector4 topRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 1, 1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 1, 1)));
                                Vector4 centerRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 0, 1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 0, 1)));
                                Vector4 bototmRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, -1, 1))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, -1, 1)));
                                model.AddAmbientOcclusionFace(solids, BlockUtil.Direction.Right, localBlockPosition, (
                                    (topLeftLight + centerLeftLight + centerTopLight + centerLight) / 4.0f,
                                    (centerLeftLight + bottomLeftLight + centerLight + centerBottomLight) / 4.0f,
                                    (centerLight + centerBottomLight + centerRightLight + bototmRightLight) / 4.0f, 
                                    (centerTopLight + centerLight + topRightLight + centerRightLight) / 4.0f
                                    ));

                            } else
                            {

                                model.AddAmbientOcclusionFace(solids, BlockUtil.Direction.Right, localBlockPosition, (
                                    centerLight,
                                    centerLight,
                                    centerLight, 
                                    centerLight
                                    ));

                            }

                            model.AddFreeformFace(solids, BlockUtil.Direction.Right, localBlockPosition, centerLight);
                            model.AddCutoutFace(cutouts, BlockUtil.Direction.Right, localBlockPosition, centerLight);

                        }
                        break;
                    case (0, 0, 1):
                        {
                            
                            Vector4 centerLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[sampleColumnPosition], sampleLocalPosition);

                            if (WorldGenerator.IsSmoothLightingEnabled)
                            {

                                Vector4 topLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (-1, 1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (-1, 1, 0)));
                                Vector4 centerLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (-1, 0, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (-1, 0, 0)));
                                Vector4 bottomLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (-1, -1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (-1, -1, 0)));
                                Vector4 centerTopLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 1, 0)));
                                Vector4 centerBottomLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, -1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, -1, 0)));
                                Vector4 topRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (1, 1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (1, 1, 0)));
                                Vector4 centerRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (1, 0, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (1, 0, 0)));
                                Vector4 bototmRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (1, -1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (1, -1, 0)));
                                model.AddAmbientOcclusionFace(solids, BlockUtil.Direction.Back, localBlockPosition, (
                                    (topLeftLight + centerLeftLight + centerTopLight + centerLight) / 4.0f,
                                    (centerLeftLight + bottomLeftLight + centerLight + centerBottomLight) / 4.0f,
                                    (centerLight + centerBottomLight + centerRightLight + bototmRightLight) / 4.0f, 
                                    (centerTopLight + centerLight + topRightLight + centerRightLight) / 4.0f
                                    ));

                            } else
                            {

                                model.AddAmbientOcclusionFace(solids, BlockUtil.Direction.Back, localBlockPosition, (
                                    centerLight,
                                    centerLight,
                                    centerLight, 
                                    centerLight
                                    ));

                            }

                            model.AddFreeformFace(solids, BlockUtil.Direction.Back, localBlockPosition, centerLight);
                            model.AddCutoutFace(cutouts, BlockUtil.Direction.Back, localBlockPosition, centerLight);

                        }
                        break;
                    case (0, 0, -1):
                        {
                            
                            Vector4 centerLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[sampleColumnPosition], sampleLocalPosition);

                            if (WorldGenerator.IsSmoothLightingEnabled)
                            {

                                Vector4 topLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (1, 1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (1, 1, 0)));
                                Vector4 centerLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (1, 0, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (1, 0, 0)));
                                Vector4 bottomLeftLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (1, -1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (1, -1, 0)));
                                Vector4 centerTopLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, 1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, 1, 0)));
                                Vector4 centerBottomLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (0, -1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (0, -1, 0)));
                                Vector4 topRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (-1, 1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (-1, 1, 0)));
                                Vector4 centerRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (-1, 0, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (-1, 0, 0)));
                                Vector4 bototmRightLight = ColumnUtils.GetNormalizedLightValues(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition + _offsets[i] + (-1, -1, 0))], ColumnUtils.GlobalToLocal(globalBlockPosition + _offsets[i] + (-1, -1, 0)));
                                model.AddAmbientOcclusionFace(solids, BlockUtil.Direction.Front, localBlockPosition, (
                                    (topLeftLight + centerLeftLight + centerTopLight + centerLight) / 4.0f,
                                    (centerLeftLight + bottomLeftLight + centerLight + centerBottomLight) / 4.0f,
                                    (centerLight + centerBottomLight + centerRightLight + bototmRightLight) / 4.0f, 
                                    (centerTopLight + centerLight + topRightLight + centerRightLight) / 4.0f
                                    ));

                            } else
                            {

                                model.AddAmbientOcclusionFace(solids, BlockUtil.Direction.Front, localBlockPosition, (
                                    centerLight,
                                    centerLight,
                                    centerLight, 
                                    centerLight
                                    ));

                            }

                            model.AddFreeformFace(solids, BlockUtil.Direction.Front, localBlockPosition, centerLight);
                            model.AddCutoutFace(cutouts, BlockUtil.Direction.Front, localBlockPosition, centerLight);

                        }
                        break;

                }

            }

        }

    }

    public bool GetSolidBlock(Vector3i globalBlockPosition)
    {

        Vector2i columnPosition = ColumnUtils.PositionToChunk(globalBlockPosition);
        return ColumnUtils.GetSolidBlock(WorldColumns[columnPosition], ColumnUtils.GlobalToLocal(globalBlockPosition));

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

    public void SetBlock(Vector3i globalBlockPosition, Block block, bool shouldUpdateMesh, bool hasPriority)
    {

        ColumnUtils.SetBlockId(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition)], globalBlockPosition, block.Id);
        ColumnUtils.SetSolidBlock(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition)], globalBlockPosition, block.IsSolid);
        if (block.IsSolid) ColumnUtils.SetHeightmap(WorldColumns[ColumnUtils.PositionToChunk(globalBlockPosition)], globalBlockPosition);

        if (shouldUpdateMesh)
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

            Commit(globalBlockPosition, hasPriority);

        }

    }

    private void Commit(Vector3i globalBlockPosition, bool hasPriority)
    {

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);

        WorldColumns[chunkPosition.Xz].QueueType = QueueType.LightPropagation;
        WorldColumns[chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates = true;
        WorldColumns[chunkPosition.Xz].HasPriority = hasPriority;
        if (hasPriority)
        {
            WorldGenerator.HighPriorityWorldGenerationQueue.Enqueue(chunkPosition.Xz);
        } else
        {
            WorldGenerator.LowPriorityWorldGenerationQueue.Enqueue(chunkPosition.Xz);
        }

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