using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using Game.BlockUtil;
using Game.Core.Generation;
using Game.Core.Worlds;
using Game.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Game.Core.Chunks;

public class ColumnBuilder
{

    public static void GeneratePassOne(ChunkColumn column)
    {

        column.IsUpdating = true;

        if (ColumnSerializer.TryDeserializeColumn(column))
        {

            column.QueueType = QueueType.Mesh;
            column.IsUpdating = false;
            if (column.HasPriority)
            {
                WorldGenerator.HighPriorityWorldGenerationQueue.Enqueue(column.Position);
            } else
            {
                WorldGenerator.LowPriorityWorldGenerationQueue.Enqueue(column.Position);
            }
            return;

        }

        float[] coarseDensity = Noise.CoarseValue3(0
                                                 , (GlobalValues.ChunkSize, GlobalValues.ChunkSize * WorldGenerator.WorldGenerationHeight, GlobalValues.ChunkSize)
                                                 , (8, 8, 8)
                                                 , new Vector3i(column.Position.X, 0, column.Position.Y) * GlobalValues.ChunkSize
                                                 , (24.0f, 32.0f, 24.0f));

        Biome biome = GlobalValues.Register.GetBiomeFromNamespace("Game.RedMushroomBiome");

        for (int x = 0; x < GlobalValues.ChunkSize; x++)
        {

            for (int z = 0; z < GlobalValues.ChunkSize; z++)
            {

                Vector3i globalPosition = (x,0,z) + (new Vector3i(column.Position.X, 0, column.Position.Y) * GlobalValues.ChunkSize);

                float height = Noise.OctaveValue2(0, globalPosition.Xz / new Vector2(128.0f, 64.0f), 2);

                float e = Noise.Value2(0, globalPosition.Xz / new Vector2(64.0f, 128.0f)) + (0.5f * Noise.Value2(0, (globalPosition.Xz + (123, 50913)) / new Vector2(256.0f, 256.0f)));

                for (int y = (WorldGenerator.WorldGenerationHeight * GlobalValues.ChunkSize) - 1; y >= 0; y--)
                {

                    globalPosition = (x,y,z) + (new Vector3i(column.Position.X, 0, column.Position.Y) * GlobalValues.ChunkSize);

                    float d = Noise.InterpolatedValue3(coarseDensity
                                                     , (GlobalValues.ChunkSize, GlobalValues.ChunkSize * WorldGenerator.WorldGenerationHeight, GlobalValues.ChunkSize)
                                                     , (8, 8, 8)
                                                     , new Vector3(x,y,z));

                    // float d = Noise.UpsampledValue3(0, (Vector3) globalPosition / new Vector3(24.0f, 32.0f, 24.0f), (8, 8, 8));

                    float val = y / (float) (WorldGenerator.WorldGenerationHeight * GlobalValues.ChunkSize);

                    if (d + float.Lerp(-2.0f, 0.6f, (1.0f - val) * float.Lerp(1.0f, 2.0f, e)) + height > 0.5f)
                    {

                        GlobalValues.Register.GetBlockFromNamespace("Game.StoneBlock").OnBlockPlace(GameState.World, globalPosition, false, false);

                    }

                }

            }

        }

        for (int x = 0; x < GlobalValues.ChunkSize; x++)
        {

            for (int z = 0; z < GlobalValues.ChunkSize; z++)
            {

                for (int y = (WorldGenerator.WorldGenerationHeight * GlobalValues.ChunkSize) - 1; y >= 0; y--)
                {

                    Vector3i globalPosition = (x,y,z) + (new Vector3i(column.Position.X, 0, column.Position.Y) * GlobalValues.ChunkSize);

                    biome.OnTerrainPass(GameState.World, globalPosition);

                }

            }

        }

        for (int chunkY = WorldGenerator.WorldGenerationHeight - 1; chunkY >= 0; chunkY--)
        {

            column.Chunks[chunkY].HasUpdates = true;

        }

        for (int x = 0; x < GlobalValues.ChunkSize; x++)
        {

            for (int z = 0; z < GlobalValues.ChunkSize; z++)
            {

                for (int y = column.SolidHeightmap[ChunkUtils.VecToIndex((x,z))] + 1; y < GlobalValues.ChunkSize * WorldGenerator.WorldGenerationHeight; y++)
                {

                    Vector3i chunkPosition = ChunkUtils.PositionToChunk((x,y,z));
                    Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal((x,y,z));
                    column.Chunks[chunkPosition.Y].LightData[ChunkUtils.VecToIndex(localBlockPosition)] = 15;

                }

            }

        }

        /*
        for (int x = 0; x < GlobalValues.ChunkSize; x++)
        {

            for (int z = 0; z < GlobalValues.ChunkSize; z++)
            {

                Vector3i globalBlockPosition = (x,0,z) + (column.Chunks[WorldGenerator.WorldGenerationHeight - 1].ChunkPosition * GlobalValues.ChunkSize);

                globalBlockPosition = (x, 0, z) + new Vector3i(column.Position.X, (GlobalValues.ChunkSize * WorldGenerator.WorldGenerationHeight) - 1, column.Position.Y) * (GlobalValues.ChunkSize, 1, GlobalValues.ChunkSize);
                if (!ColumnUtils.GetSolidBlock(column, globalBlockPosition))
                {

                    ColumnUtils.SetSunlightValue(column, (x,globalBlockPosition.Y,z), 15);
                    column.SunlightAdditionQueue.Enqueue((globalBlockPosition, 15));

                }

            }

        }
        */

        column.QueueType = QueueType.SunlightCalculation;
        column.IsUpdating = false;
        if (column.HasPriority)
        {
            WorldGenerator.HighPriorityWorldGenerationQueue.Enqueue(column.Position);
        } else
        {
            WorldGenerator.LowPriorityWorldGenerationQueue.Enqueue(column.Position);
        }

    }
    private static Vector3i[] _horizontalOffsets = { Vector3i.UnitX, -Vector3i.UnitX, Vector3i.UnitZ, -Vector3i.UnitZ };
    public static void PrecalculateSunlight(ConcurrentDictionary<Vector2i, ChunkColumn> columns, Vector2i columnPosition) 
    {

        columns[columnPosition].IsUpdating = true;

        for (int x = 0; x < GlobalValues.ChunkSize; x++)
        {

            for (int z = 0; z < GlobalValues.ChunkSize; z++)
            {

                int currentHeight = ColumnUtils.GetHeightmap(columns[columnPosition], (x,z));
                for (int i = 0; i < _horizontalOffsets.Length; i++)
                {

                    Vector3i globalPosition = (x,currentHeight,z) + (new Vector3i(columnPosition.X, 0, columnPosition.Y) * GlobalValues.ChunkSize);

                    Vector2i neighborColumn = ColumnUtils.PositionToChunk(globalPosition + _horizontalOffsets[i]);
                    int neighborHeight = ColumnUtils.GetHeightmap(columns[neighborColumn], ColumnUtils.GlobalToLocal(globalPosition + _horizontalOffsets[i]).Xz);

                    if (currentHeight + 1 < neighborHeight)
                    {

                        for (int y = neighborHeight - 1; y > currentHeight; y--)
                        {

                            bool isBlockSolid = ColumnUtils.GetSolidBlock(columns[neighborColumn], ColumnUtils.GlobalToLocal((x,y,z) + _horizontalOffsets[i]));
                            bool isSunlightZero = ColumnUtils.GetSunlightValue(columns[neighborColumn], ColumnUtils.GlobalToLocal((x,y,z) + _horizontalOffsets[i])) == 0;

                            if (!isBlockSolid && isSunlightZero)
                            {

                                columns[columnPosition].SunlightAdditionQueue.Enqueue((globalPosition.X, y, globalPosition.Z));
                                break;

                            }

                        }

                    }

                }

            }

        }

        columns[columnPosition].QueueType = QueueType.LightPropagation;
        columns[columnPosition].IsUpdating = false;
        if (columns[columnPosition].HasPriority)
        {
            WorldGenerator.HighPriorityWorldGenerationQueue.Enqueue(columnPosition);
        } else
        {
            WorldGenerator.LowPriorityWorldGenerationQueue.Enqueue(columnPosition);
        }

    }

    private static Vector3i[] _offsets = [ Vector3i.UnitX, -Vector3i.UnitX, Vector3i.UnitY, -Vector3i.UnitY, Vector3i.UnitZ, -Vector3i.UnitZ ];
    public static void PropagateLights(ConcurrentDictionary<Vector2i, ChunkColumn> columns, Vector2i columnPosition)
    {

        columns[columnPosition].IsUpdating = true;

        while (columns[columnPosition].SunlightRemovalQueue.TryDequeue(out (Vector3i position, ushort value) light))
        {

            for (int i = 0; i < _offsets.Length; i++)
            {

                Vector3i chunkPosition = ChunkUtils.PositionToChunk(light.position + _offsets[i]);
                Vector3i columnLocalPosition = ColumnUtils.GlobalToLocal(light.position + _offsets[i]);

                if (light.position.Y + _offsets[i].Y >= WorldGenerator.WorldGenerationHeight * GlobalValues.ChunkSize || light.position.Y + _offsets[i].Y < 0) continue;

                columns[chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates = true;

                ushort neighborValue = ColumnUtils.GetSunlightValue(columns[chunkPosition.Xz], columnLocalPosition);

                if (neighborValue != 0 && light.value == 15 && _offsets[i] == -Vector3i.UnitY )
                {

                    ColumnUtils.SetSunlightValue(columns[chunkPosition.Xz], columnLocalPosition, 0);
                    columns[columnPosition].SunlightRemovalQueue.Enqueue((light.position + _offsets[i], light.value));

                } else if (neighborValue != 0 && neighborValue < light.value)
                {

                    ColumnUtils.SetSunlightValue(columns[chunkPosition.Xz], columnLocalPosition, 0);
                    columns[columnPosition].SunlightRemovalQueue.Enqueue((light.position + _offsets[i], neighborValue));

                } else if (neighborValue >= light.value)
                {

                    columns[columnPosition].SunlightAdditionQueue.Enqueue(light.position + _offsets[i]);

                }

            }

        }

        while (columns[columnPosition].SunlightAdditionQueue.TryDequeue(out Vector3i position))
        {
            
            for (int i = 0; i < _offsets.Length; i++)
            {

                Vector3i chunkPosition = ChunkUtils.PositionToChunk(position + _offsets[i]);
                Vector3i columnLocalPosition = ColumnUtils.GlobalToLocal(position + _offsets[i]);

                if (position.Y + _offsets[i].Y >= WorldGenerator.WorldGenerationHeight * GlobalValues.ChunkSize || position.Y + _offsets[i].Y < 0) continue;

                bool blockIsSolid = ColumnUtils.GetSolidBlock(columns[chunkPosition.Xz], columnLocalPosition);
                ushort sunlightValue = ColumnUtils.GetSunlightValue(columns[chunkPosition.Xz], columnLocalPosition);

                ushort currentValue = ColumnUtils.GetSunlightValue(columns[ColumnUtils.PositionToChunk(position)], ColumnUtils.GlobalToLocal(position));

                columns[chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates = true;

                if (!blockIsSolid && currentValue == 15 && _offsets[i] == -Vector3i.UnitY)
                {

                    ColumnUtils.SetSunlightValue(columns[chunkPosition.Xz], columnLocalPosition, currentValue);
                    columns[columnPosition].SunlightAdditionQueue.Enqueue(position + _offsets[i]);

                } else if (!blockIsSolid && sunlightValue + 1 < currentValue)
                {

                    ColumnUtils.SetSunlightValue(columns[chunkPosition.Xz], columnLocalPosition, (ushort) (currentValue - 1));
                    columns[columnPosition].SunlightAdditionQueue.Enqueue(position + _offsets[i]);

                }

            }

        }

        while (columns[columnPosition].RedBlocklightRemovalQueue.TryDequeue(out (Vector3i position, ushort value) light))
        {

            for (int i = 0; i < _offsets.Length; i++)
            {

                Vector3i chunkPosition = ChunkUtils.PositionToChunk(light.position + _offsets[i]);
                Vector3i columnLocalPosition = ColumnUtils.GlobalToLocal(light.position + _offsets[i]);

                if (light.position.Y + _offsets[i].Y >= WorldGenerator.WorldGenerationHeight * GlobalValues.ChunkSize || light.position.Y + _offsets[i].Y < 0) continue;

                columns[chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates = true;

                ushort neighborValue = ColumnUtils.GetRedBlocklightValue(columns[chunkPosition.Xz], columnLocalPosition);

                if (neighborValue != 0 && neighborValue < light.value)
                {

                    ColumnUtils.SetRedBlocklightValue(columns[chunkPosition.Xz], columnLocalPosition, 0);
                    columns[columnPosition].RedBlocklightRemovalQueue.Enqueue((light.position + _offsets[i], neighborValue));

                } else if (neighborValue >= light.value)
                {

                    columns[columnPosition].RedBlocklightAdditionQueue.Enqueue(light.position + _offsets[i]);

                }

            }

        }

        while (columns[columnPosition].RedBlocklightAdditionQueue.TryDequeue(out Vector3i position))
        {

            for (int i = 0; i < _offsets.Length; i++)
            {

                Vector3i chunkPosition = ChunkUtils.PositionToChunk(position + _offsets[i]);
                Vector3i columnLocalPosition = ColumnUtils.GlobalToLocal(position + _offsets[i]);

                if (position.Y + _offsets[i].Y >= WorldGenerator.WorldGenerationHeight * GlobalValues.ChunkSize || position.Y + _offsets[i].Y < 0) continue;

                bool blockIsSolid = ColumnUtils.GetSolidBlock(columns[chunkPosition.Xz], columnLocalPosition);
                ushort neighborValue = ColumnUtils.GetRedBlocklightValue(columns[chunkPosition.Xz], columnLocalPosition);

                ushort currentValue = ColumnUtils.GetRedBlocklightValue(columns[ColumnUtils.PositionToChunk(position)], ColumnUtils.GlobalToLocal(position));

                columns[chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates = true;

                if (!blockIsSolid && neighborValue + 1 < currentValue)
                {

                    ColumnUtils.SetRedBlocklightValue(columns[chunkPosition.Xz], columnLocalPosition, (ushort) (currentValue - 1));
                    columns[columnPosition].RedBlocklightAdditionQueue.Enqueue(position + _offsets[i]);

                }

            }

        }

        while (columns[columnPosition].GreenBlocklightRemovalQueue.TryDequeue(out (Vector3i position, ushort value) light))
        {

            for (int i = 0; i < _offsets.Length; i++)
            {

                Vector3i chunkPosition = ChunkUtils.PositionToChunk(light.position + _offsets[i]);
                Vector3i columnLocalPosition = ColumnUtils.GlobalToLocal(light.position + _offsets[i]);

                if (light.position.Y + _offsets[i].Y >= WorldGenerator.WorldGenerationHeight * GlobalValues.ChunkSize || light.position.Y + _offsets[i].Y < 0) continue;

                columns[chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates = true;

                ushort neighborValue = ColumnUtils.GetGreenBlocklightValue(columns[chunkPosition.Xz], columnLocalPosition);

                if (neighborValue != 0 && neighborValue < light.value)
                {

                    ColumnUtils.SetGreenBlocklightValue(columns[chunkPosition.Xz], columnLocalPosition, 0);
                    columns[columnPosition].GreenBlocklightRemovalQueue.Enqueue((light.position + _offsets[i], neighborValue));

                } else if (neighborValue >= light.value)
                {

                    columns[columnPosition].GreenBlocklightAdditionQueue.Enqueue(light.position + _offsets[i]);

                }

            }

        }

        while (columns[columnPosition].GreenBlocklightAdditionQueue.TryDequeue(out Vector3i position))
        {

            for (int i = 0; i < _offsets.Length; i++)
            {

                Vector3i chunkPosition = ChunkUtils.PositionToChunk(position + _offsets[i]);
                Vector3i columnLocalPosition = ColumnUtils.GlobalToLocal(position + _offsets[i]);

                if (position.Y + _offsets[i].Y >= WorldGenerator.WorldGenerationHeight * GlobalValues.ChunkSize || position.Y + _offsets[i].Y < 0) continue;

                bool blockIsSolid = ColumnUtils.GetSolidBlock(columns[chunkPosition.Xz], columnLocalPosition);
                ushort neighborValue = ColumnUtils.GetGreenBlocklightValue(columns[chunkPosition.Xz], columnLocalPosition);

                ushort currentValue = ColumnUtils.GetGreenBlocklightValue(columns[ColumnUtils.PositionToChunk(position)], ColumnUtils.GlobalToLocal(position));

                columns[chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates = true;

                if (!blockIsSolid && neighborValue + 1 < currentValue)
                {

                    ColumnUtils.SetGreenBlocklightValue(columns[chunkPosition.Xz], columnLocalPosition, (ushort) (currentValue - 1));
                    columns[columnPosition].GreenBlocklightAdditionQueue.Enqueue(position + _offsets[i]);

                }

            }

        }

        while (columns[columnPosition].BlueBlocklightRemovalQueue.TryDequeue(out (Vector3i position, ushort value) light))
        {

            for (int i = 0; i < _offsets.Length; i++)
            {

                Vector3i chunkPosition = ChunkUtils.PositionToChunk(light.position + _offsets[i]);
                Vector3i columnLocalPosition = ColumnUtils.GlobalToLocal(light.position + _offsets[i]);

                if (light.position.Y + _offsets[i].Y >= WorldGenerator.WorldGenerationHeight * GlobalValues.ChunkSize || light.position.Y + _offsets[i].Y < 0) continue;

                columns[chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates = true;

                ushort neighborValue = ColumnUtils.GetBlueBlocklightValue(columns[chunkPosition.Xz], columnLocalPosition);

                if (neighborValue != 0 && neighborValue < light.value)
                {

                    ColumnUtils.SetBlueBlocklightValue(columns[chunkPosition.Xz], columnLocalPosition, 0);
                    columns[columnPosition].BlueBlocklightRemovalQueue.Enqueue((light.position + _offsets[i], neighborValue));

                } else if (neighborValue >= light.value)
                {

                    columns[columnPosition].BlueBlocklightAdditionQueue.Enqueue(light.position + _offsets[i]);

                }

            }

        }

        while (columns[columnPosition].BlueBlocklightAdditionQueue.TryDequeue(out Vector3i position))
        {

            for (int i = 0; i < _offsets.Length; i++)
            {

                Vector3i chunkPosition = ChunkUtils.PositionToChunk(position + _offsets[i]);
                Vector3i columnLocalPosition = ColumnUtils.GlobalToLocal(position + _offsets[i]);

                if (position.Y + _offsets[i].Y >= WorldGenerator.WorldGenerationHeight * GlobalValues.ChunkSize || position.Y + _offsets[i].Y < 0) continue;

                bool blockIsSolid = ColumnUtils.GetSolidBlock(columns[chunkPosition.Xz], columnLocalPosition);
                ushort neighborValue = ColumnUtils.GetBlueBlocklightValue(columns[chunkPosition.Xz], columnLocalPosition);

                ushort currentValue = ColumnUtils.GetBlueBlocklightValue(columns[ColumnUtils.PositionToChunk(position)], ColumnUtils.GlobalToLocal(position));

                columns[chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates = true;

                if (!blockIsSolid && neighborValue + 1 < currentValue)
                {

                    ColumnUtils.SetBlueBlocklightValue(columns[chunkPosition.Xz], columnLocalPosition, (ushort) (currentValue - 1));
                    columns[columnPosition].BlueBlocklightAdditionQueue.Enqueue(position + _offsets[i]);

                }

            }

        }

        columns[columnPosition].QueueType = QueueType.Mesh;
        columns[columnPosition].IsUpdating = false;
        if (columns[columnPosition].HasPriority)
        {
            WorldGenerator.HighPriorityWorldGenerationQueue.Enqueue(columnPosition);
        } else
        {
            WorldGenerator.LowPriorityWorldGenerationQueue.Enqueue(columnPosition);
        }

    }

    public static void Mesh(ConcurrentDictionary<Vector2i, ChunkColumn> columns, Vector2i columnPosition)
    {

        columns[columnPosition].IsUpdating = true;

        List<int> solidIndices = new();
        List<int> cutoutIndices = new();

        List<Rectangle> solidRectangles = new();
        List<Rectangle> cutoutRectangles = new();

        for (int chunkY = WorldGenerator.WorldGenerationHeight - 1; chunkY >= 0; chunkY--)
        {

            if (columns[columnPosition].Chunks[chunkY].HasUpdates)  
            {
                
                solidIndices.Clear();
                cutoutIndices.Clear();
                solidRectangles.Clear();
                cutoutRectangles.Clear();

                for (int x = 0; x < GlobalValues.ChunkSize; x++)
                {

                    for (int y = 0; y < GlobalValues.ChunkSize; y++)
                    {

                        for (int z = 0; z < GlobalValues.ChunkSize; z++)
                        {

                            Vector3i globalBlockPosition = (x,y,z) + (new Vector3i(columnPosition.X,chunkY,columnPosition.Y) * GlobalValues.ChunkSize);

                            ushort id = ChunkUtils.GetBlockId(columns[columnPosition].Chunks[chunkY], (x,y,z));
                            if (id != 0)
                            {

                                Block block = GlobalValues.Register.GetBlockFromId(id);
                                block.OnBlockMesh(GameState.World, globalBlockPosition, solidRectangles, cutoutRectangles);
                                
                            }

                        }

                    }

                }

                for (int i = 0; i < solidRectangles.Count; i++)
                {

                    solidIndices.AddRange(0 + (i * 4), 1 + (i * 4), 2 + (i * 4), 2 + (i * 4), 3 + (i * 4), 0 + (i * 4));

                }

                for (int i = 0; i < cutoutRectangles.Count; i++)
                {

                    cutoutIndices.AddRange(0 + (i * 4), 1 + (i * 4), 2 + (i * 4), 2 + (i * 4), 3 + (i * 4), 0 + (i * 4));

                }

                columns[columnPosition].ChunkMeshes[chunkY].Solids = new List<Rectangle>(solidRectangles);
                columns[columnPosition].ChunkMeshes[chunkY].SolidIndices = new List<int>(solidIndices);
                columns[columnPosition].ChunkMeshes[chunkY].Cutouts = new List<Rectangle>(cutoutRectangles);
                columns[columnPosition].ChunkMeshes[chunkY].CutoutIndices = new List<int>(cutoutIndices);

                // columns[Vector2i.Zero].ChunkMeshes[chunkY].ChunkVertices = new List<PackedChunkVertex>(vertices);
                // columns[columnPosition].ChunkMeshes[chunkY].SolidIndices = new List<int>(indices);
                // columns[columnPosition].ChunkMeshes[chunkY].Solids = new List<Rectangle>(rectangles);

            }

        }

        columns[columnPosition].QueueType = QueueType.Upload;
        columns[columnPosition].IsUpdating = false;
        WorldGenerator.UploadQueue.Enqueue(columnPosition);

    }

    public static void Upload(ChunkColumn column)
    {

        column.IsUpdating = true;
        column.HasPriority = false;
        for (int chunkY = WorldGenerator.WorldGenerationHeight - 1; chunkY >= 0; chunkY--)
        {

            if (column.Chunks[chunkY].HasUpdates)
            {

                column.Chunks[chunkY].HasUpdates = false;
                GL.DeleteVertexArray(column.ChunkMeshes[chunkY].SolidsVao);
                GL.DeleteBuffer(column.ChunkMeshes[chunkY].SolidsHandle);
                GL.DeleteBuffer(column.ChunkMeshes[chunkY].SolidsIbo);

                GL.DeleteVertexArray(column.ChunkMeshes[chunkY].CutoutVao);
                GL.DeleteBuffer(column.ChunkMeshes[chunkY].CutoutHandle);
                GL.DeleteBuffer(column.ChunkMeshes[chunkY].CutoutIbo);
                
                column.ChunkMeshes[chunkY].SolidsIbo = GL.GenBuffer();
                column.ChunkMeshes[chunkY].SolidsHandle = GL.GenBuffer();
                column.ChunkMeshes[chunkY].SolidsVao = GL.GenVertexArray();

                column.ChunkMeshes[chunkY].CutoutIbo = GL.GenBuffer();
                column.ChunkMeshes[chunkY].CutoutHandle = GL.GenBuffer();
                column.ChunkMeshes[chunkY].CutoutVao = GL.GenVertexArray();

                GL.BindVertexArray(column.ChunkMeshes[chunkY].SolidsVao);
                
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, column.ChunkMeshes[chunkY].SolidsHandle);
                GL.BufferData<Rectangle>(BufferTarget.ShaderStorageBuffer, column.ChunkMeshes[chunkY].Solids.Count * Marshal.SizeOf<Rectangle>(), CollectionsMarshal.AsSpan(column.ChunkMeshes[chunkY].Solids), BufferUsage.DynamicDraw);
                GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, 0, column.ChunkMeshes[chunkY].SolidsHandle);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, column.ChunkMeshes[chunkY].SolidsIbo);
                GL.BufferData<int>(BufferTarget.ElementArrayBuffer, column.ChunkMeshes[chunkY].SolidIndices.Count * sizeof(int), CollectionsMarshal.AsSpan(column.ChunkMeshes[chunkY].SolidIndices), BufferUsage.DynamicDraw);
                
                column.ChunkMeshes[chunkY].SolidIndicesCount = column.ChunkMeshes[chunkY].SolidIndices.Count;

                GL.BindVertexArray(column.ChunkMeshes[chunkY].CutoutVao);
                
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, column.ChunkMeshes[chunkY].CutoutHandle);
                GL.BufferData<Rectangle>(BufferTarget.ShaderStorageBuffer, column.ChunkMeshes[chunkY].Cutouts.Count * Marshal.SizeOf<Rectangle>(), CollectionsMarshal.AsSpan(column.ChunkMeshes[chunkY].Cutouts), BufferUsage.DynamicDraw);
                GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, 0, column.ChunkMeshes[chunkY].CutoutHandle);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, column.ChunkMeshes[chunkY].CutoutIbo);
                GL.BufferData<int>(BufferTarget.ElementArrayBuffer, column.ChunkMeshes[chunkY].CutoutIndices.Count * sizeof(int), CollectionsMarshal.AsSpan(column.ChunkMeshes[chunkY].CutoutIndices), BufferUsage.DynamicDraw);
                
                column.ChunkMeshes[chunkY].CutoutIndicesCount = column.ChunkMeshes[chunkY].CutoutIndices.Count;

                column.ChunkMeshes[chunkY].IsRenderable = true;

            }

        }

        column.QueueType = QueueType.Done;
        column.IsUpdating = false;

    }

}