using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Chunks;

public class PackedChunkBuilder
{

    public static void GeneratePassOne(PackedChunk chunk)
    {
        
        /*
        for (int x = 0; x < GlobalValues.ChunkSize; x++)
        {
            for (int y = 0; y < GlobalValues.ChunkSize; y++)
            {
                for (int z = 0; z < GlobalValues.ChunkSize; z++)
                {

                    Vector3i globalBlockPosition = (x, y, z) + (chunk.ChunkPosition * 32);
                    
                    if (y <= x)
                    {
                        chunk.BlockData[ChunkUtils.VecToIndex((x, y, z))] = 1;
                    }
                    
                }
            }
        }
        */
        if (chunk.ChunkPosition.Y <= 0) Array.Fill(chunk.BlockData, (ushort)1);

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
                        if (chunks[ChunkUtils.PositionToChunk((x,y+1,z))].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal((x,y+1,z)))] == 0)
                        {
                            PackedChunkVertex[] topVertices =
                            {
                                new PackedChunkVertex((x + 1,y + 1,z + 1), Direction.Up),
                                new PackedChunkVertex((x + 1,y + 1,z), Direction.Up),
                                new PackedChunkVertex((x,y + 1,z), Direction.Up),
                                new PackedChunkVertex((x,y + 1,z + 1), Direction.Up),
                            };
                            vertices.AddRange(topVertices);
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
        GL.BufferData(BufferTarget.ArrayBuffer, PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].PackedChunkVertices.Length * sizeof(uint), PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].PackedChunkVertices, BufferUsage.DynamicDraw);

        GL.VertexAttribIPointer(0, 1, VertexAttribIType.UnsignedInt, sizeof(uint), 0);
        GL.EnableVertexAttribArray(0);
        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].Ibo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].PackedChunkMeshIndices.Length * sizeof(int), PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].PackedChunkMeshIndices, BufferUsage.DynamicDraw);
        
        PackedWorldGenerator.CurrentWorld.PackedWorldMeshes[chunkPosition].IsRenderable = true;

    }
    
}