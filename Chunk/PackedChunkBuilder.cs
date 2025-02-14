using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
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

        // Array.Fill(chunk.LightData, (ushort) 0b1111111111111111);
        
        for (int x = 0; x < GlobalValues.ChunkSize; x++)
        {
            for (int z = 0; z < GlobalValues.ChunkSize; z++)
            {
                int height = (int) (0.0 * Maths.Noise2(132423430, (x + (chunk.ChunkPosition.X * 32)) / 128.0f, (z + (chunk.ChunkPosition.Z * 32)) / 128.0f));
                for (int y = 0; y < GlobalValues.ChunkSize; y++)
                {
                    if (y + (chunk.ChunkPosition.Y * 32) <= height)
                    {
                        GlobalValues.NewRegister.GetBlockFromId(1).OnBlockSet(PackedWorldGenerator.CurrentWorld, (x,y,z) + (chunk.ChunkPosition * GlobalValues.ChunkSize));
                    }
                }
            }
        }
        
        // if (chunk.ChunkPosition.Y <= 0) Array.Fill(chunk.BlockData, (ushort)1);

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

    private static Vector3i[] offsets = [ Vector3i.UnitY, -Vector3i.UnitY, Vector3i.UnitX, -Vector3i.UnitX, Vector3i.UnitZ, -Vector3i.UnitZ ];

    public static void ComputeBlockLights(Dictionary<Vector3i, PackedChunk> chunks, PackedChunk chunk) 
    {

        Stopwatch sw = Stopwatch.StartNew();

        while (chunk.BlockLightRemovalQueue.TryDequeue(out BlockLight light)) 
        {
        
            List<Vector3i> currentLightSamples = [ light.Position ];
            List<Vector3i> traversedSamples = [ light.Position ];
            List<Vector3i> newLightSamples = new();

            while (light.LightColor.R > 0)
            {
                foreach (Vector3i lightSample in currentLightSamples)
                {

                    ChunkUtils.SetLightRedColor(chunks[ChunkUtils.PositionToChunk(lightSample)], ChunkUtils.PositionToBlockLocal(lightSample), 0);
                    for (int i = 0; i < offsets.Length; i++)
                    {

                        if (ChunkUtils.GetLightRedColor(chunks[ChunkUtils.PositionToChunk(lightSample + offsets[i])], ChunkUtils.PositionToBlockLocal(lightSample + offsets[i])) != 0 && !traversedSamples.Contains(lightSample + offsets[i]) && !newLightSamples.Contains(lightSample + offsets[i]))
                        {

                            ushort redValue = ChunkUtils.GetLightRedColor(chunks[ChunkUtils.PositionToChunk(lightSample + offsets[i])], ChunkUtils.PositionToBlockLocal(lightSample + offsets[i]));
                            if (redValue != 0)
                            {
                                if (redValue < light.LightColor.R)
                                {
                                    newLightSamples.Add(lightSample + offsets[i]);
                                } 
                                else 
                                {
                                    LightColor v = ChunkUtils.GetLightColor(chunks[ChunkUtils.PositionToChunk(lightSample + offsets[i])], ChunkUtils.PositionToBlockLocal(lightSample + offsets[i]));
                                    if (redValue == light.LightColor.R)
                                    {
                                        v.R = (ushort) Math.Max(v.R - 1, 0);
                                        v.G = (ushort) Math.Max(v.G - 1, 0);
                                        v.B = (ushort) Math.Max(v.B - 1, 0);
                                        chunk.BlockLightAdditionQueue.Enqueue(new BlockLight(new LightColor(v.R, 0, 0)) { Position = lightSample });
                                    } else
                                    {
                                        chunk.BlockLightAdditionQueue.Enqueue(new BlockLight(new LightColor(light.LightColor.R, 0, 0)) { Position = lightSample });
                                    }
                                }
                            }   

                        }
                    }
                }

                traversedSamples.AddRange(currentLightSamples);
                currentLightSamples.Clear();
                currentLightSamples.AddRange(newLightSamples);
                newLightSamples.Clear();
                light.LightColor.R = (ushort) Math.Max((int)light.LightColor.R - 1, 0);
            }

            currentLightSamples = [ light.Position ];
            traversedSamples = [ light.Position ];
            newLightSamples.Clear();

            while (light.LightColor.G > 0)
            {
                foreach (Vector3i lightSample in currentLightSamples)
                {

                    ChunkUtils.SetLightGreenColor(chunks[ChunkUtils.PositionToChunk(lightSample)], ChunkUtils.PositionToBlockLocal(lightSample), 0);
                    for (int i = 0; i < offsets.Length; i++)
                    {
                        if (ChunkUtils.GetLightGreenColor(chunks[ChunkUtils.PositionToChunk(lightSample + offsets[i])], ChunkUtils.PositionToBlockLocal(lightSample + offsets[i])) != 0 && !traversedSamples.Contains(lightSample + offsets[i]) && !newLightSamples.Contains(lightSample + offsets[i]))
                        {

                            ushort greenValue = ChunkUtils.GetLightGreenColor(chunks[ChunkUtils.PositionToChunk(lightSample + offsets[i])], ChunkUtils.PositionToBlockLocal(lightSample + offsets[i]));
                            if (greenValue != 0)
                            {
                                if (greenValue < light.LightColor.G)
                                {
                                    newLightSamples.Add(lightSample + offsets[i]);
                                } 
                                else 
                                {
                                    if (greenValue == light.LightColor.G)
                                    {
                                        chunk.BlockLightAdditionQueue.Enqueue(new BlockLight(new LightColor(0, (ushort)Math.Max(light.LightColor.G - 1, 0), 0)) { Position = lightSample });
                                    } else
                                    {
                                        chunk.BlockLightAdditionQueue.Enqueue(new BlockLight(new LightColor(0, light.LightColor.G, 0)) { Position = lightSample });
                                    }
                                }
                            }   

                        }
                    }
                }

                traversedSamples.AddRange(currentLightSamples);
                currentLightSamples.Clear();
                currentLightSamples.AddRange(newLightSamples);
                newLightSamples.Clear();
                light.LightColor.G = (ushort) Math.Max(light.LightColor.G - 1, 0);

            }

            currentLightSamples = [ light.Position ];
            traversedSamples = [ light.Position ];
            newLightSamples.Clear();

            while (light.LightColor.B > 0)
            {
                foreach (Vector3i lightSample in currentLightSamples)
                {

                    ChunkUtils.SetLightBlueColor(chunks[ChunkUtils.PositionToChunk(lightSample)], ChunkUtils.PositionToBlockLocal(lightSample), 0);
                    for (int i = 0; i < offsets.Length; i++)
                    {
                        if (ChunkUtils.GetLightBlueColor(chunks[ChunkUtils.PositionToChunk(lightSample + offsets[i])], ChunkUtils.PositionToBlockLocal(lightSample + offsets[i])) != 0 && !traversedSamples.Contains(lightSample + offsets[i]) && !newLightSamples.Contains(lightSample + offsets[i]))
                        {

                            ushort blueValue = ChunkUtils.GetLightBlueColor(chunks[ChunkUtils.PositionToChunk(lightSample + offsets[i])], ChunkUtils.PositionToBlockLocal(lightSample + offsets[i]));
                            if (blueValue != 0)
                            {
                                if (blueValue < light.LightColor.B)
                                {
                                    newLightSamples.Add(lightSample + offsets[i]);
                                } 
                                else 
                                {
                                    if (blueValue == light.LightColor.B)
                                    {
                                        chunk.BlockLightAdditionQueue.Enqueue(new BlockLight(new LightColor(0, 0, (ushort)Math.Max(light.LightColor.B - 1, 0))) { Position = lightSample });
                                    } else
                                    {
                                        chunk.BlockLightAdditionQueue.Enqueue(new BlockLight(new LightColor(0, 0, light.LightColor.B)) { Position = lightSample });
                                    }
                                }
                            }   

                        }
                    }
                }

                traversedSamples.AddRange(currentLightSamples);
                currentLightSamples.Clear();
                currentLightSamples.AddRange(newLightSamples);
                newLightSamples.Clear();
                light.LightColor.B = (ushort) Math.Max((int)light.LightColor.B - 1, 0);

            }

            currentLightSamples = [ light.Position ];
            traversedSamples = [ light.Position ];
            newLightSamples.Clear();

        }

        while (chunk.BlockLightAdditionQueue.TryDequeue(out BlockLight light)) 
        {

            int lightSpreadValue = light.LightColor.MaxValue;
            List<Vector3i> currentLightSamples = [ light.Position ];
            List<Vector3i> traversedSamples = [ light.Position ];
            List<Vector3i> newLightSamples = new();

            while (light.LightColor.R > 0)
            {
                foreach (Vector3i lightSample in currentLightSamples)
                {
                    if (ChunkUtils.GetLightRedColor(chunks[ChunkUtils.PositionToChunk(lightSample)], ChunkUtils.PositionToBlockLocal(lightSample)) < light.LightColor.R)
                    {
                        ChunkUtils.SetLightRedColor(chunks[ChunkUtils.PositionToChunk(lightSample)], ChunkUtils.PositionToBlockLocal(lightSample), light.LightColor.R);
                        for (int i = 0; i < offsets.Length; i++) 
                        {
                            if (!ChunkUtils.GetSolidBlock(chunks[ChunkUtils.PositionToChunk(lightSample + offsets[i])], ChunkUtils.PositionToBlockLocal(lightSample + Vector3i.UnitY)) && !traversedSamples.Contains(lightSample + offsets[i]) && !newLightSamples.Contains(lightSample + offsets[i])) newLightSamples.Add(lightSample + offsets[i]);
                        }
                    }
                }

                traversedSamples.AddRange(currentLightSamples);
                currentLightSamples.Clear();
                currentLightSamples.AddRange(newLightSamples);
                newLightSamples.Clear();
                light.LightColor.R = (ushort) Math.Max((int)light.LightColor.R - 1, 0);
            }

            currentLightSamples = [ light.Position ];
            traversedSamples = [ light.Position ];
            newLightSamples.Clear();

            while (light.LightColor.G > 0)
            {
                foreach (Vector3i lightSample in currentLightSamples)
                {
                    if (ChunkUtils.GetLightGreenColor(chunks[ChunkUtils.PositionToChunk(lightSample)], ChunkUtils.PositionToBlockLocal(lightSample)) < light.LightColor.G)
                    {
                        ChunkUtils.SetLightGreenColor(chunks[ChunkUtils.PositionToChunk(lightSample)], ChunkUtils.PositionToBlockLocal(lightSample), light.LightColor.G);
                        for (int i = 0; i < offsets.Length; i++) 
                        {
                            if (!ChunkUtils.GetSolidBlock(chunks[ChunkUtils.PositionToChunk(lightSample + offsets[i])], ChunkUtils.PositionToBlockLocal(lightSample + Vector3i.UnitY)) && !traversedSamples.Contains(lightSample + offsets[i]) && !newLightSamples.Contains(lightSample + offsets[i])) newLightSamples.Add(lightSample + offsets[i]);
                        }
                    }
                }

                traversedSamples.AddRange(currentLightSamples);
                currentLightSamples.Clear();
                currentLightSamples.AddRange(newLightSamples);
                newLightSamples.Clear();
                light.LightColor.G = (ushort) Math.Max((int)light.LightColor.G - 1, 0);
            }

            currentLightSamples = [ light.Position ];
            traversedSamples = [ light.Position ];
            newLightSamples.Clear();

            while (light.LightColor.B > 0)
            {
                foreach (Vector3i lightSample in currentLightSamples)
                {
                    if (ChunkUtils.GetLightBlueColor(chunks[ChunkUtils.PositionToChunk(lightSample)], ChunkUtils.PositionToBlockLocal(lightSample)) < light.LightColor.B)
                    {
                        ChunkUtils.SetLightBlueColor(chunks[ChunkUtils.PositionToChunk(lightSample)], ChunkUtils.PositionToBlockLocal(lightSample), light.LightColor.B);
                        for (int i = 0; i < offsets.Length; i++) 
                        {
                            if (!ChunkUtils.GetSolidBlock(chunks[ChunkUtils.PositionToChunk(lightSample + offsets[i])], ChunkUtils.PositionToBlockLocal(lightSample + Vector3i.UnitY)) && !traversedSamples.Contains(lightSample + offsets[i]) && !newLightSamples.Contains(lightSample + offsets[i])) newLightSamples.Add(lightSample + offsets[i]);
                        }
                    }
                }

                traversedSamples.AddRange(currentLightSamples);
                currentLightSamples.Clear();
                currentLightSamples.AddRange(newLightSamples);
                newLightSamples.Clear();
                light.LightColor.B = (ushort) Math.Max((int)light.LightColor.B - 1, 0);
            }
        
        }
        sw.Stop();
        if (sw.Elapsed.TotalMilliseconds > 1.0) GameLogger.Log($"Block light calcs took {sw.Elapsed.TotalMilliseconds}ms");

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