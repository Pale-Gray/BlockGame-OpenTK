using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Chunks;

public class PackedChunkBuilder
{

    public static void GeneratePassOne(PackedChunk chunk)
    {

        Array.Fill<ushort>(chunk.LightData, 0);
        
        for (int x = 0; x < GlobalValues.ChunkSize; x++)
        {
            for (int z = 0; z < GlobalValues.ChunkSize; z++)
            {
                int height = 128 + (int) (16.0 * Maths.Noise2(132423430, (x + (chunk.ChunkPosition.X * 32)) / 128.0f, (z + (chunk.ChunkPosition.Z * 32)) / 128.0f));
                for (int y = 0; y < GlobalValues.ChunkSize; y++)
                {
                    if (y + (chunk.ChunkPosition.Y * 32) <= height)
                    {
                        GlobalValues.NewRegister.GetBlockFromId(1).OnBlockSet(PackedWorldGenerator.CurrentWorld, (x,y,z) + (chunk.ChunkPosition * GlobalValues.ChunkSize));
                    }
                }
            }
        }

        chunk.QueueType = PackedChunkQueueType.LightPropagation;
        if (chunk.HasPriority)
        {
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueFirst(chunk.ChunkPosition);
        }
        else
        {
            PackedWorldGenerator.PackedChunkWorldGenerationQueue.EnqueueLast(chunk.ChunkPosition);
        }
        
    }

    private static Vector3i[] offsets = [ Vector3i.UnitX, Vector3i.UnitZ, Vector3i.UnitY, -Vector3i.UnitX, -Vector3i.UnitZ, -Vector3i.UnitY ];

    public static void ComputeBlockLights(Dictionary<Vector3i, PackedChunk> chunks, PackedChunk chunk) 
    {

        HashSet<Vector3i> alreadySampledRedPositions = new();
        HashSet<Vector3i> alreadySampledBluePositions = new();
        HashSet<Vector3i> alreadySampledGreenPositions = new();

        Stopwatch lightRemovalStopwatch = Stopwatch.StartNew();
        
        /*
        while (chunk.BlockLightRemovalQueue.TryDequeue(out BlockLight light))
        {

            if (light.LightColor.G != 0)
            {

                if (!alreadySampledGreenPositions.Contains(light.Position))
                {

                    alreadySampledGreenPositions.Add(light.Position);
                    ChunkUtils.SetLightGreenColor(chunks[ChunkUtils.PositionToChunk(light.Position)], ChunkUtils.PositionToBlockLocal(light.Position), 0);
                    for (int i = 0; i < offsets.Length; i++)
                    {

                        ushort greenSample = ChunkUtils.GetLightGreenColor(chunks[ChunkUtils.PositionToChunk(light.Position + offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + offsets[i]));
                        if (greenSample != 0 && greenSample < light.LightColor.G)
                        {

                            if (light.LightColor.G > 0 && light.LightColor.G - 1 != 0) chunk.BlockLightRemovalQueue.Enqueue(new BlockLight(light.Position + offsets[i], new LightColor(0, (ushort) (light.LightColor.G - 1), 0)));

                        } else if (greenSample >= light.LightColor.G)
                        {

                            chunk.BlockLightAdditionQueue.Enqueue(new BlockLight(light.Position + offsets[i], new LightColor(0, greenSample, 0)));

                        }

                    }

                }  

            }

            if (light.LightColor.R != 0)
            {

                if (!alreadySampledRedPositions.Contains(light.Position))
                {

                    alreadySampledRedPositions.Add(light.Position);
                    ChunkUtils.SetLightRedColor(chunks[ChunkUtils.PositionToChunk(light.Position)], ChunkUtils.PositionToBlockLocal(light.Position), 0);
                    for (int i = 0; i < offsets.Length; i++)
                    {

                        ushort redSample = ChunkUtils.GetLightRedColor(chunks[ChunkUtils.PositionToChunk(light.Position + offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + offsets[i]));
                        if (redSample != 0 && redSample < light.LightColor.R)
                        {

                            if (light.LightColor.R > 0 && light.LightColor.R - 1 != 0) chunk.BlockLightRemovalQueue.Enqueue(new BlockLight(light.Position + offsets[i], new LightColor((ushort) (light.LightColor.R - 1), 0, 0)));

                        } else if (redSample >= light.LightColor.R)
                        {

                            chunk.BlockLightAdditionQueue.Enqueue(new BlockLight(light.Position + offsets[i], new LightColor(redSample, 0, 0)));

                        }

                    }

                }

            }

            if (light.LightColor.B != 0)
            {

                if (!alreadySampledBluePositions.Contains(light.Position))
                {

                    alreadySampledBluePositions.Add(light.Position);
                    ChunkUtils.SetLightBlueColor(chunks[ChunkUtils.PositionToChunk(light.Position)], ChunkUtils.PositionToBlockLocal(light.Position), 0);
                    for (int i = 0; i < offsets.Length; i++)
                    {

                        ushort blueSample = ChunkUtils.GetLightBlueColor(chunks[ChunkUtils.PositionToChunk(light.Position + offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + offsets[i]));
                        if (blueSample != 0 && blueSample < light.LightColor.B)
                        {

                            if (light.LightColor.B > 0 && light.LightColor.B - 1 != 0) chunk.BlockLightRemovalQueue.Enqueue(new BlockLight(light.Position + offsets[i], new LightColor(0, 0, (ushort) (light.LightColor.B - 1))));

                        } else if (blueSample >= light.LightColor.B)
                        {

                            chunk.BlockLightAdditionQueue.Enqueue(new BlockLight(light.Position + offsets[i], new LightColor(0, 0, blueSample)));

                        }

                    }

                }

            }

        }
        */
        lightRemovalStopwatch.Stop();
        if (chunk.HasPriority) if (lightRemovalStopwatch.Elapsed.TotalMilliseconds > 0.0) GameLogger.Log($"Light removal took {Math.Round(lightRemovalStopwatch.Elapsed.TotalMilliseconds, 4)}ms");

        alreadySampledRedPositions.Clear();
        alreadySampledGreenPositions.Clear();
        alreadySampledBluePositions.Clear();

        Stopwatch lightAdditionStopwatch = Stopwatch.StartNew();
        while (chunk.BlockLightAdditionQueue.TryDequeue(out BlockLight light))
        {

            if (light.LightColor.R != 0)
            {

                for (int i = 0; i < offsets.Length; i++)
                {

                    ushort redSample = ChunkUtils.GetLightRedColor(chunks[ChunkUtils.PositionToChunk(light.Position + offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + offsets[i]));
                    if (!ChunkUtils.GetSolidBlock(chunks[ChunkUtils.PositionToChunk(light.Position + offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + offsets[i])))
                    {

                        if (redSample + 2 <= light.LightColor.R)
                        {
                            if (light.LightColor.R - 1 > 0) 
                            {
                                ChunkUtils.SetLightRedColor(chunks[ChunkUtils.PositionToChunk(light.Position + offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + offsets[i]), (ushort) (light.LightColor.R - 1));
                                chunk.BlockLightAdditionQueue.Enqueue(new BlockLight(light.Position + offsets[i], new LightColor((ushort) (light.LightColor.R - 1), 0, 0)));
                            }
                        }

                    }

                }

            }

            if (light.LightColor.G != 0)
            {

                for (int i = 0; i < offsets.Length; i++)
                {

                    ushort greenSample = ChunkUtils.GetLightGreenColor(chunks[ChunkUtils.PositionToChunk(light.Position + offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + offsets[i]));
                    if (!ChunkUtils.GetSolidBlock(chunks[ChunkUtils.PositionToChunk(light.Position + offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + offsets[i])))
                    {

                        if (greenSample + 2 <= light.LightColor.G)
                        {
                            if (light.LightColor.G - 1 > 0)
                            {
                                ChunkUtils.SetLightGreenColor(chunks[ChunkUtils.PositionToChunk(light.Position + offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + offsets[i]), (ushort) (light.LightColor.G - 1));
                                chunk.BlockLightAdditionQueue.Enqueue(new BlockLight(light.Position + offsets[i], new LightColor(0, (ushort) (light.LightColor.G - 1), 0)));
                            }
                        }

                    }

                }

            }

            if (light.LightColor.B != 0)
            {

                for (int i = 0; i < offsets.Length; i++)
                {

                    ushort blueSample = ChunkUtils.GetLightBlueColor(chunks[ChunkUtils.PositionToChunk(light.Position + offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + offsets[i]));
                    if (!ChunkUtils.GetSolidBlock(chunks[ChunkUtils.PositionToChunk(light.Position + offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + offsets[i])))
                    {

                        if (blueSample + 2 <= light.LightColor.B)
                        {
                            if (light.LightColor.B - 1 > 0)
                            {
                                ChunkUtils.SetLightBlueColor(chunks[ChunkUtils.PositionToChunk(light.Position + offsets[i])], ChunkUtils.PositionToBlockLocal(light.Position + offsets[i]), (ushort) (light.LightColor.B - 1));
                                chunk.BlockLightAdditionQueue.Enqueue(new BlockLight(light.Position + offsets[i], new LightColor(0, 0, (ushort) (light.LightColor.B - 1))));
                            }
                        }

                    }

                }

            }

        }  
        
        lightAdditionStopwatch.Stop();
        if (chunk.HasPriority) if (lightAdditionStopwatch.Elapsed.TotalMilliseconds > 0.0) GameLogger.Log($"Light addition took {Math.Round(lightAdditionStopwatch.Elapsed.TotalMilliseconds, 4)}ms");

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

    public static void Mesh(Dictionary<Vector3i, PackedChunk> chunks, PackedChunk chunk)
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
                
                        if (chunks.ContainsKey(ChunkUtils.PositionToChunk((x,y+1,z))) && chunks[ChunkUtils.PositionToChunk((x,y+1,z))].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal((x,y+1,z)))] == 0)
                        {
                            vertices.AddRange(block.BlockModel.QueryPackedFace(BlockUtil.Direction.Top, (x,y,z), chunks));
                        }

                        if (chunks.ContainsKey(ChunkUtils.PositionToChunk((x,y-1,z))) && chunks[ChunkUtils.PositionToChunk((x, y - 1, z))].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal((x, y - 1, z)))] == 0)
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