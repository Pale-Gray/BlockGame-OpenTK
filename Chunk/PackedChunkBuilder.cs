using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.TexturePack;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Chunks;

public class PackedChunkBuilder
{

    public static void GeneratePassOne(PackedChunk chunk)
    {

        Array.Fill(chunk.LightData, (ushort) 0b1111111111111111);
        
        for (int x = 0; x < GlobalValues.ChunkSize; x++)
        {
            for (int z = 0; z < GlobalValues.ChunkSize; z++)
            {
                int height = (int) (64.0 * Maths.Noise2(132423430, (x + (chunk.ChunkPosition.X * 32)) / 128.0f, (z + (chunk.ChunkPosition.Z * 32)) / 128.0f));
                for (int y = 0; y < GlobalValues.ChunkSize; y++)
                {
                    if (y + (chunk.ChunkPosition.Y * 32) <= height)
                    {
                        // chunk.BlockData[ChunkUtils.VecToIndex((x, y, z))] = 1;
                        // chunk.BlockData[ChunkUtils.VecToIndex((x, y, z))] = 1;
                        GlobalValues.NewRegister.GetBlockFromId(1).OnBlockSet(PackedWorldGenerator.CurrentWorld, (x,y,z) + (chunk.ChunkPosition * GlobalValues.ChunkSize));
                    }
                }
            }
        }
        
        // if (chunk.ChunkPosition.Y <= 0) Array.Fill(chunk.BlockData, (ushort)1);

        chunk.QueueType = PackedChunkQueueType.Mesh;
        if (chunk.HasPriority)
        {
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunk.ChunkPosition);
        }
        else
        {
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueLast(chunk.ChunkPosition);
        }
        
    }

    public static void Mesh(ConcurrentDictionary<Vector3i, PackedChunk> chunks, PackedChunk chunk)
    {
        
        List<PackedChunkVertex> vertices = new();
        List<int> indicesList = new();
        
        for (int x = 0; x < GlobalValues.ChunkSize; x++)
        {
            for (int y = 0; y < GlobalValues.ChunkSize; y++)
            {
                for (int z = 0; z < GlobalValues.ChunkSize; z++)
                {
                    if (chunk.BlockData[ChunkUtils.VecToIndex((x, y, z))] != 0)
                    {
                        NewBlock block = GlobalValues.NewRegister.GetBlockFromId(chunk.BlockData[ChunkUtils.VecToIndex((x, y, z))]);
                        if (chunks[ChunkUtils.PositionToChunk((x,y+1,z))].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal((x,y+1,z)))] == 0)
                        {
                            vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Top, (x,y,z), chunks));
                        }

                        if (chunks[ChunkUtils.PositionToChunk((x, y - 1, z))].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal((x, y - 1, z)))] == 0)
                        {
                            vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Bottom, (x,y,z), chunks));
                        }
                        
                        if (chunks[ChunkUtils.PositionToChunk((x+1,y,z))].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal((x+1,y,z)))] == 0)
                        {
                            vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Left, (x,y,z), chunks));
                        }

                        if (chunks[ChunkUtils.PositionToChunk((x-1, y, z))].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal((x-1, y, z)))] == 0)
                        {
                            vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Right, (x,y,z), chunks));
                        }
                        
                        if (chunks[ChunkUtils.PositionToChunk((x,y,z+1))].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal((x,y,z+1)))] == 0)
                        {
                            vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Back, (x,y,z), chunks));
                        }

                        if (chunks[ChunkUtils.PositionToChunk((x, y, z-1))].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal((x, y, z-1)))] == 0)
                        {
                            vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Front, (x,y,z), chunks));
                        }
                    }
                }
            }
        }
        
        for (int i = 0; i < vertices.Count / 4; i++)
        {

            int[] indices =
            {
                0 + (i * 4),
                1 + (i * 4),
                2 + (i * 4),
                2 + (i * 4),
                3 + (i * 4),
                0 + (i * 4)
            };
            indicesList.AddRange(indices);
        }
        
        PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunk.ChunkPosition].PackedChunkMeshIndices = indicesList.ToArray();
        PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunk.ChunkPosition].PackedChunkVertices = vertices.ToArray();
        
        chunk.QueueType = PackedChunkQueueType.Renderable;
        if (chunk.HasPriority)
        {
            PackedWorldGenerator.PackedChunkWorldUploadQueue.EnqueueFirst(chunk.ChunkPosition);
        }
        else
        {
            PackedWorldGenerator.PackedChunkWorldUploadQueue.EnqueueLast(chunk.ChunkPosition);
        }

    }

    public static void Upload(Vector3i chunkPosition)
    {

        GL.DeleteBuffer(PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].Vbo);
        GL.DeleteVertexArray(PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].Vao);
        GL.DeleteBuffer(PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].Ibo);
        
        PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].Vbo = GL.GenBuffer();
        PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].Ibo = GL.GenBuffer();
        PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].Vao = GL.GenVertexArray();

        GL.BindVertexArray(PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].Vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].Vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].PackedChunkVertices.Length * Marshal.SizeOf<PackedChunkVertex>(), PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].PackedChunkVertices, BufferUsage.DynamicDraw);

        GL.VertexAttribIPointer(0, 1, VertexAttribIType.UnsignedInt, Marshal.SizeOf<PackedChunkVertex>(), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribIPointer(1, 1, VertexAttribIType.UnsignedInt, Marshal.SizeOf<PackedChunkVertex>(), Marshal.OffsetOf<PackedChunkVertex>(nameof(PackedChunkVertex.PackedExtraInfo)));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<PackedChunkVertex>(), Marshal.OffsetOf<PackedChunkVertex>(nameof(PackedChunkVertex.LightColor)));
        GL.EnableVertexAttribArray(2);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].Ibo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].PackedChunkMeshIndices.Length * sizeof(int), PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].PackedChunkMeshIndices, BufferUsage.DynamicDraw);
        
        PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].IsRenderable = true;
        PackedWorldGenerator.CurrentWorld.PackedWorldChunks[chunkPosition].HasPriority = false;

    }
    
}