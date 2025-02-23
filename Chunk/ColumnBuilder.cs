using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Chunks;

public class ColumnBuilder
{

    public static void GeneratePassOne(ChunkColumn column)
    {

        for (int chunkY = PackedWorldGenerator.WorldGenerationHeight - 1; chunkY >= 0; chunkY--)
        {

            for (int x = 0; x < GlobalValues.ChunkSize; x++)
            {

                for (int z = 0; z < GlobalValues.ChunkSize; z++)
                {

                    Vector3i globalBlockPosition = (x,0,z) + (column.Chunks[chunkY].ChunkPosition * GlobalValues.ChunkSize);
                    int height = (int) (64.0 + (32.0 * Maths.Noise2(1234125, globalBlockPosition.X / 128.0f, globalBlockPosition.Z / 256.0f)));
                    for (int y = 0; y < GlobalValues.ChunkSize; y++)
                    {

                        globalBlockPosition.Y = y + (column.Chunks[chunkY].ChunkPosition.Y * GlobalValues.ChunkSize);
                        if (globalBlockPosition.Y <= height - 4)
                        {
                            GlobalValues.NewRegister.GetBlockFromNamespace("Game.StoneBlock").OnBlockSet(PackedWorldGenerator.CurrentWorld, globalBlockPosition);
                        } else if (globalBlockPosition.Y <= height - 1)
                        {
                            GlobalValues.NewRegister.GetBlockFromNamespace("Game.DirtBlock").OnBlockSet(PackedWorldGenerator.CurrentWorld, globalBlockPosition);
                        } else if (globalBlockPosition.Y <= height)
                        {
                            GlobalValues.NewRegister.GetBlockFromNamespace("Game.GrassBlock").OnBlockSet(PackedWorldGenerator.CurrentWorld, globalBlockPosition);
                        }

                    }

                }

            }
            column.Chunks[chunkY].HasUpdates = true;

        }

        column.QueueType = ColumnQueueType.Mesh;
        PackedWorldGenerator.ColumnWorldGenerationQueue.EnqueueLast(column.Position);

    }

    public static void Mesh(ConcurrentDictionary<Vector2i, ChunkColumn> columns)
    {

        List<PackedChunkVertex> vertices = new();
        List<int> indices = new();

        for (int chunkY = PackedWorldGenerator.WorldGenerationHeight - 1; chunkY >= 0; chunkY--)
        {

            if (columns[Vector2i.Zero].Chunks[chunkY].HasUpdates)
            {

                vertices.Clear();
                indices.Clear();
                for (int x = 0; x < GlobalValues.ChunkSize; x++)
                {
                    for (int y = 0; y < GlobalValues.ChunkSize; y++)
                    {
                        for (int z = 0; z < GlobalValues.ChunkSize; z++)
                        {
                            if (columns[Vector2i.Zero].Chunks[chunkY].BlockData[ChunkUtils.VecToIndex((x,y,z))] != 0)
                            {

                                Vector3i globalBlockPosition = (x,y,z) + (columns[Vector2i.Zero].Chunks[chunkY].ChunkPosition * GlobalValues.ChunkSize);
                                NewBlock block = GlobalValues.NewRegister.GetBlockFromId(columns[Vector2i.Zero].Chunks[chunkY].BlockData[ChunkUtils.VecToIndex((x,y,z))]);

                                if (y + 1 < GlobalValues.ChunkSize)
                                {

                                    if (columns[Vector2i.Zero].Chunks[chunkY].BlockData[ChunkUtils.VecToIndex((x,y+1,z))] == 0)
                                    {
                                        vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Top, (x,y,z), LightColor.Zero));
                                    }

                                } else
                                {

                                    if (chunkY + 1 < PackedWorldGenerator.WorldGenerationHeight)
                                    {   
                                        if (columns[Vector2i.Zero].Chunks[chunkY + 1].BlockData[ChunkUtils.VecToIndex((x,0,z))] == 0)
                                        {
                                            vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Top, (x,y,z), LightColor.Zero));
                                        }
                                    } else
                                    {
                                        vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Top, (x,y,z), LightColor.Zero));
                                    }

                                }

                                if (y - 1 >= 0)
                                {

                                    if (columns[Vector2i.Zero].Chunks[chunkY].BlockData[ChunkUtils.VecToIndex((x,y-1,z))] == 0)
                                    {
                                        vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Bottom, (x,y,z), LightColor.Zero));
                                    }

                                } else
                                {

                                    if (chunkY - 1 >= 0)
                                    {
                                        if (columns[Vector2i.Zero].Chunks[chunkY - 1].BlockData[ChunkUtils.VecToIndex((x,GlobalValues.ChunkSize - 1,z))] == 0)
                                        {
                                            vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Bottom, (x,y,z), LightColor.Zero));
                                        }
                                    } else
                                    {
                                        vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Bottom, (x,y,z), LightColor.Zero));
                                    }

                                }

                                if (x + 1 < GlobalValues.ChunkSize)
                                {
                                    if (columns[Vector2i.Zero].Chunks[chunkY].BlockData[ChunkUtils.VecToIndex((x+1,y,z))] == 0)
                                    {
                                        vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Left, (x,y,z), LightColor.Zero));
                                    }
                                } else
                                {
                                    if (columns.TryGetValue((1,0), out ChunkColumn column))
                                    {
                                        if (column.Chunks[chunkY].BlockData[ChunkUtils.VecToIndex((0,y,z))] == 0)
                                        {
                                            vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Left, (x,y,z), LightColor.Zero));
                                        }
                                    } else
                                    {
                                        vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Left, (x,y,z), LightColor.Zero));
                                    }
                                }

                                if (x - 1 >= 0)
                                {
                                    if (columns[Vector2i.Zero].Chunks[chunkY].BlockData[ChunkUtils.VecToIndex((x-1,y,z))] == 0)
                                    {
                                        vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Right, (x,y,z), LightColor.Zero));
                                    }
                                } else
                                {
                                    if (columns.TryGetValue((-1, 0), out ChunkColumn column))
                                    {
                                        if (column.Chunks[chunkY].BlockData[ChunkUtils.VecToIndex((GlobalValues.ChunkSize-1,y,z))] == 0)
                                        {
                                            vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Right, (x,y,z), LightColor.Zero));
                                        }
                                    } else
                                    {
                                        vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Right, (x,y,z), LightColor.Zero));
                                    }
                                }

                                if (z + 1 < GlobalValues.ChunkSize)
                                {
                                    if (columns[Vector2i.Zero].Chunks[chunkY].BlockData[ChunkUtils.VecToIndex((x,y,z+1))] == 0)
                                    {
                                        vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Back, (x,y,z), LightColor.Zero));
                                    }
                                } else
                                {
                                    if (columns.TryGetValue((0,1), out ChunkColumn column))
                                    {
                                        if (column.Chunks[chunkY].BlockData[ChunkUtils.VecToIndex((x,y,0))] == 0)
                                        {
                                            vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Back, (x,y,z), LightColor.Zero));
                                        }
                                    } else
                                    {
                                        vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Back, (x,y,z), LightColor.Zero));
                                    }
                                }

                                if (z - 1 >= 0)
                                {
                                    if (columns[Vector2i.Zero].Chunks[chunkY].BlockData[ChunkUtils.VecToIndex((x,y,z-1))] == 0)
                                    {
                                        vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Front, (x,y,z), LightColor.Zero));
                                    }
                                } else
                                {
                                    if (columns.TryGetValue((0, -1), out ChunkColumn column))
                                    {
                                        if (column.Chunks[chunkY].BlockData[ChunkUtils.VecToIndex((x,y,GlobalValues.ChunkSize-1))] == 0)
                                        {
                                            vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Front, (x,y,z), LightColor.Zero));
                                        }
                                    } else
                                    {
                                        vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Front, (x,y,z), LightColor.Zero));
                                    }
                                }

                            }
                        }
                    }
                }

                for (int i = 0; i < vertices.Count / 4; i++)
                {

                    indices.AddRange(0 + (i * 4), 1 + (i * 4), 2 + (i * 4), 2 + (i * 4), 3 + (i * 4), 0 + (i * 4));

                }

                // TODO: Change these properties to spans instead of arrays. ToArray sucks ass!
                columns[Vector2i.Zero].ChunkMeshes[chunkY].PackedChunkVertices = vertices.ToArray();
                columns[Vector2i.Zero].ChunkMeshes[chunkY].PackedChunkMeshIndices = indices.ToArray();

            }

        }

        columns[Vector2i.Zero].QueueType = ColumnQueueType.Upload;
        PackedWorldGenerator.ColumnWorldUploadQueue.EnqueueLast(columns[Vector2i.Zero].Position);

    }

    public static void Upload(ChunkColumn column)
    {

        for (int chunkY = PackedWorldGenerator.WorldGenerationHeight - 1; chunkY >= 0; chunkY--)
        {

            if (column.Chunks[chunkY].HasUpdates)
            {

                column.Chunks[chunkY].HasUpdates = false;
                column.Chunks[chunkY].HasPriority = false;
                GL.DeleteBuffer(column.ChunkMeshes[chunkY].Vbo);
                GL.DeleteVertexArray(column.ChunkMeshes[chunkY].Vao);
                GL.DeleteBuffer(column.ChunkMeshes[chunkY].Ibo);
                
                column.ChunkMeshes[chunkY].Vbo = GL.GenBuffer();
                column.ChunkMeshes[chunkY].Ibo = GL.GenBuffer();
                column.ChunkMeshes[chunkY].Vao = GL.GenVertexArray();

                GL.BindVertexArray(column.ChunkMeshes[chunkY].Vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, column.ChunkMeshes[chunkY].Vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, column.ChunkMeshes[chunkY].PackedChunkVertices.Length * Marshal.SizeOf<PackedChunkVertex>(), column.ChunkMeshes[chunkY].PackedChunkVertices, BufferUsage.DynamicDraw);

                GL.VertexAttribIPointer(0, 1, VertexAttribIType.UnsignedInt, Marshal.SizeOf<PackedChunkVertex>(), 0);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribIPointer(1, 1, VertexAttribIType.UnsignedInt, Marshal.SizeOf<PackedChunkVertex>(), Marshal.OffsetOf<PackedChunkVertex>(nameof(PackedChunkVertex.PackedExtraInfo)));
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<PackedChunkVertex>(), Marshal.OffsetOf<PackedChunkVertex>(nameof(PackedChunkVertex.LightColor)));
                GL.EnableVertexAttribArray(2);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, column.ChunkMeshes[chunkY].Ibo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, column.ChunkMeshes[chunkY].PackedChunkMeshIndices.Length * sizeof(int), column.ChunkMeshes[chunkY].PackedChunkMeshIndices, BufferUsage.DynamicDraw);
                
                column.ChunkMeshes[chunkY].IsRenderable = true;

            }

        }

    }

}