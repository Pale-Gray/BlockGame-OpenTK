using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.Vulkan;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Core.Chunks
{
    internal class ChunkBuilder
    {

        private static readonly object _chunkLock = new();

        public static void GeneratePassOneThreaded(Chunk chunk)
        {

            chunk.IsUpdating = true;
            ThreadPool.QueueUserWorkItem(_ => GeneratePassOne(chunk));

        }

        public static void GeneratePassOne(Chunk chunk)
        {

            Vector3i chunkPosition = chunk.ChunkPosition;

            if (true)
            {

                for (int x = 0; x < GlobalValues.ChunkSize; x++)
                {

                    for (int z = 0; z < GlobalValues.ChunkSize; z++)
                    {

                        Vector3 globalBlockPosition = (x, 0, z) + (chunkPosition * 32);
                        int height = (int)Math.Floor(0.0f * Maths.ValueNoise2Octaves(121312321, globalBlockPosition.X / 64.0f, globalBlockPosition.Z / 38.0f, 3));
                        // chunk.StructurePoints.Add((x, height, chunkY));
                        for (int y = 0; y < GlobalValues.ChunkSize; y++)
                        {

                            globalBlockPosition.Y = y + (chunkPosition.Y * 32);
                            if (globalBlockPosition.Y < height)
                            {

                                Blocks.GrassBlock.OnBlockSet(chunk, (x, y, z));

                            }

                            if (globalBlockPosition.Y == 16 && x > 4 && x < 28 && z > 4 && z < 24)
                            {

                                Blocks.BrickBlock.OnBlockSet(chunk, (x, y, z));

                            }

                            int currentMaxPos = chunk.GlobalBlockMaxHeight[ChunkUtils.VecToIndex((x,z))];
                            if (chunk.GetBlock((x,y,z)).IsSolid ?? true)
                            {

                                chunk.GlobalBlockMaxHeight[ChunkUtils.VecToIndex((x, z))] = (int)Math.Max(currentMaxPos, globalBlockPosition.Y);

                            }

                        }

                    }

                }

            }

            chunk.QueueType = QueueType.Mesh;
            WorldGenerator.ConcurrentChunkUpdateQueue.Enqueue(chunk.ChunkPosition);
            chunk.IsUpdating = false;

        }

        public static void CalculateSunlightColumnThreaded(World world, Vector2i columnPosition, int currentChunkHeight)
        {

            ThreadPool.QueueUserWorkItem(_ => CalculateSunlightColumn(world, columnPosition, currentChunkHeight));

        }

        public static void CalculateSunlightColumn(World world, Vector2i columnPosition, int currentChunkHeight)
        {

            int[] columnMaxBlockHeight = new int[GlobalValues.ChunkSize * GlobalValues.ChunkSize];
            for (int chunkY = WorldGenerator.MaxRadius; chunkY >= -WorldGenerator.MaxRadius; chunkY--)
            {

                for (int x = 0; x < GlobalValues.ChunkSize; x++)
                {

                    for (int z = 0; z < GlobalValues.ChunkSize; z++)
                    {

                        int currentColumnMaxBlockHeight = columnMaxBlockHeight[ChunkUtils.VecToIndex((x, z))];
                        columnMaxBlockHeight[ChunkUtils.VecToIndex((x, z))] = (int)Math.Max(currentColumnMaxBlockHeight, world.WorldChunks[(columnPosition.X, currentChunkHeight + chunkY, columnPosition.Y)].GlobalBlockMaxHeight[ChunkUtils.VecToIndex((x,z))]);

                    }

                }

            }

            // now we do the lighting columns
            for (int chunkY = WorldGenerator.MaxRadius; chunkY >= -WorldGenerator.MaxRadius; chunkY--)
            {

                for (int x = 0; x < GlobalValues.ChunkSize; x++)
                {

                    for (int z = 0; z < GlobalValues.ChunkSize; z++)
                    {

                        for (int y = 0; y < GlobalValues.ChunkSize; y++)
                        {

                            Vector3i globalBlockPosition = (x, y, z) + (new Vector3i(columnPosition.X, currentChunkHeight + chunkY, columnPosition.Y) * GlobalValues.ChunkSize);
                            uint currentLightData = world.WorldChunks[(columnPosition.X, currentChunkHeight + chunkY, columnPosition.Y)].PackedLightData[ChunkUtils.VecToIndex((x, y, z))];

                            if (globalBlockPosition.Y >= columnMaxBlockHeight[ChunkUtils.VecToIndex((x,z))])
                            {

                                world.WorldChunks[(columnPosition.X, currentChunkHeight + chunkY, columnPosition.Y)].PackedLightData[ChunkUtils.VecToIndex((x, y, z))] = currentLightData | 0x0000000F;

                            } else
                            {

                                world.WorldChunks[(columnPosition.X, currentChunkHeight + chunkY, columnPosition.Y)].PackedLightData[ChunkUtils.VecToIndex((x, y, z))] = currentLightData & 0xFFFFFFF0;

                            }

                        }

                    }

                }

                world.WorldChunks[(columnPosition.X, chunkY, columnPosition.Y)].QueueType = QueueType.LightPropagation;
                WorldGenerator.ConcurrentChunkUpdateQueue.Enqueue((columnPosition.X, chunkY, columnPosition.Y));

            }

        }

        public static void MeshThreaded(Chunk chunk, World world, Vector3i cameraPosition)
        {

            chunk.IsUpdating = true;
            ThreadPool.QueueUserWorkItem(_ => Mesh(chunk, world, cameraPosition));

        }

        public static void Mesh(Chunk chunk, World world, Vector3i cameraPosition)
        {

            // chunk.ConcurrentOpaqueMesh.Clear();
            // chunk.ConcurrentMeshIndices.Clear();

            chunk.OpaqueMeshList.Clear();
            chunk.IndicesList.Clear();
            Vector3i chunkPosition = chunk.ChunkPosition;

            Dictionary<Vector3i, bool[]> mask = ChunkUtils.GetChunkNeighborsSolidMaskDictionary(world, chunk.ChunkPosition);

            for (int x = 0; x < GlobalValues.ChunkSize; x++)
            {

                for (int y = 0; y < GlobalValues.ChunkSize; y++)
                {

                    for (int z = 0; z < GlobalValues.ChunkSize; z++)
                    {

                        if (chunk.BlockData[ChunkUtils.VecToIndex((x, y, z))] != 0)
                        {

                            Vector3i globalBlockPosition = (x, y, z) + (32 * chunkPosition);

                            chunk.GetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition)).OnBlockMesh(world, mask, chunk.BlockPropertyNewData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition))], globalBlockPosition);

                        }

                    }

                }

            }

            for (int i = 0; i < chunk.OpaqueMeshList.Count / 4; i++) // amount of faces (quads) existent in the chunk
            {

                int currentIndexCount = (chunk.IndicesList.Count / 6) * 4;
                int[] indices = { 0+currentIndexCount, 1+currentIndexCount, 2+currentIndexCount, 2+ currentIndexCount, 3 + currentIndexCount, 0 + currentIndexCount };
                chunk.IndicesList.AddRange(indices);

            }

            chunk.MeshIndices = chunk.IndicesList.ToArray();
            chunk.SolidMesh = chunk.OpaqueMeshList.ToArray();
            chunk.QueueType = QueueType.Upload;
            // WorldGenerator.ConcurrentChunkUpdateQueue.Enqueue(chunk.ChunkPosition);
            WorldGenerator.ConcurrentChunkUploadQueue.Enqueue(chunk.ChunkPosition);
            chunk.IsUpdating = false;

        }

        public static void RemeshThreaded(Chunk chunk, World world, Vector3i cameraPosition)
        {

            // Task.Run(() => Remesh(chunk, world, cameraPosition));
            ThreadPool.QueueUserWorkItem(_ => Remesh(chunk, world, cameraPosition));

        }
        public static void PropagateBlockLightsThreaded(World world, Chunk chunk)
        {

            // Task.Run(() => PropagateBlockLights(world, chunk));
            ThreadPool.QueueUserWorkItem(_ => PropagateBlockLights(world, chunk));

        }

        private static void PropagateBlockLights(World world, Chunk chunk)
        {

            // chunk.PackedLightData = new uint[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
            // insures sun lighting isnt reset
            for (int i = 0; i < chunk.PackedLightData.Length; i++)
            {

                chunk.PackedLightData[i] = chunk.PackedLightData[i] & 0x0000000F;

            }

            List<Vector3i> sunlightPositions = new List<Vector3i>();

            for (int x = 0; x < GlobalValues.ChunkSize; x++)
            {

                for (int y = 0; y < GlobalValues.ChunkSize; y++)
                {

                    for (int z = 0; z < GlobalValues.ChunkSize; z++)
                    {

                        Vector3i globalBlockPosition = (x, y, z) + (chunk.ChunkPosition * GlobalValues.ChunkSize);

                        if ((chunk.PackedLightData[ChunkUtils.VecToIndex((x,y,z))] & 15) == 15)
                        {

                            if ((world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitX)].PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition + Vector3i.UnitX))] & 15) != 15 ||
                                (world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition - Vector3i.UnitX)].PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition - Vector3i.UnitX))] & 15) != 15 ||
                                (world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitZ)].PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition + Vector3i.UnitZ))] & 15) != 15 ||
                                (world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition - Vector3i.UnitZ)].PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition - Vector3i.UnitZ))] & 15) != 15)
                            {

                                sunlightPositions.Add(globalBlockPosition);
                                // Dda.ComputeSunlightVisibility(world, chunk, globalBlockPosition);

                            }

                        }

                    }

                }

            }

            foreach (Vector3i sunlightPosition in sunlightPositions)
            {

                Dda.ComputeSunlightVisibility(world, chunk, sunlightPosition);

            }

            foreach (KeyValuePair<Vector3i, Vector3i> pair in chunk.GlobalBlockLightPositions)
            {

                Dda.ComputeVisibility(world, chunk, pair.Key, pair.Value);

            }

            chunk.QueueType = QueueType.Mesh;
            WorldGenerator.ConcurrentChunkUpdateQueue.Enqueue(chunk.ChunkPosition);

        }
        public static void Remesh(Chunk chunk, World world, Vector3i cameraPosition)
        {

            // chunk.PackedLightData = new uint[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
            // PropagateBlockLights(world, chunk);
            chunk.OpaqueMeshList.Clear();
            chunk.IndicesList.Clear();

            Dictionary<Vector3i, bool[]> mask = ChunkUtils.GetChunkNeighborsSolidMaskDictionary(world, chunk.ChunkPosition);
            Vector3i chunkPosition = chunk.ChunkPosition;

            for (int x = 0; x < GlobalValues.ChunkSize; x++)
            {

                for (int y = 0; y < GlobalValues.ChunkSize; y++)
                {

                    for (int z = 0; z < GlobalValues.ChunkSize; z++)
                    {

                        if (chunk.BlockData[ChunkUtils.VecToIndex((x, y, z))] != 0)
                        {

                            Vector3i globalBlockPosition = (x, y, z) + (32 * chunkPosition);
                            world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].GetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition)).OnBlockMesh(world, mask, world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockPropertyNewData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition))], globalBlockPosition);

                        }

                    }

                }

            }

            for (int i = 0; i < chunk.OpaqueMeshList.Count / 4; i++) // amount of faces (quads) existent in the chunk
            {

                int currentIndexCount = (chunk.IndicesList.Count / 6) * 4;
                int[] indices = { 0 + currentIndexCount, 1 + currentIndexCount, 2 + currentIndexCount, 2 + currentIndexCount, 3 + currentIndexCount, 0 + currentIndexCount };
                chunk.IndicesList.AddRange(indices);

            }

            // Console.WriteLine($"previous mesh length: {chunk.SolidMesh.Length}");
            chunk.MeshIndices = chunk.IndicesList.ToArray();
            chunk.SolidMesh = chunk.OpaqueMeshList.ToArray();
            // Console.WriteLine($"new mesh length: {chunk.SolidMesh.Length}");
            chunk.QueueType = QueueType.Upload;
            // WorldGenerator.ConcurrentChunkUpdateQueue.Enqueue(chunk.ChunkPosition);
            WorldGenerator.ConcurrentChunkUploadQueue.Enqueue(chunk.ChunkPosition);


        }

        private static Block GetBlockWithNeighbors(Chunk chunk, Dictionary<Vector3i, Chunk> neighbors, Vector3i position)
        {

            // Console.Log(position.X < 0);

            if (position.Z >= GlobalValues.ChunkSize) return neighbors[Vector3i.UnitZ].GetBlock((position.X, position.Y, 0));
            if (position.Z < 0) return neighbors[-Vector3i.UnitZ].GetBlock((position.X, position.Y, GlobalValues.ChunkSize - 1));
            if (position.Y >= GlobalValues.ChunkSize) return neighbors[Vector3i.UnitY].GetBlock((position.X, 0, position.Z));
            if (position.Y < 0) return neighbors[-Vector3i.UnitY].GetBlock((position.X, GlobalValues.ChunkSize - 1, position.Z));
            if (position.X >= GlobalValues.ChunkSize) return neighbors[Vector3i.UnitX].GetBlock((0, position.Y, position.Z));
            if (position.X < 0) return neighbors[-Vector3i.UnitX].GetBlock((GlobalValues.ChunkSize - 1, position.Y, position.Z));

            return chunk.GetBlock(position);

        }
        private static bool AnyNeighborsAirSafe(Chunk chunk, Vector3i position)
        {

            Vector3i upVector = position.Y >= GlobalValues.ChunkSize - 1 ? position : position + Vector3i.UnitY;
            Vector3i downVector = position.Y <= 0 ? position : position - Vector3i.UnitY;
            Vector3i leftVector = position.X >= GlobalValues.ChunkSize - 1 ? position : position + Vector3i.UnitX;
            Vector3i rightVector = position.X <= 0 ? position : position - Vector3i.UnitX;
            Vector3i backVector = position.Z >= GlobalValues.ChunkSize - 1 ? position : position + Vector3i.UnitZ;
            Vector3i frontVector = position.Z <= 0 ? position : position - Vector3i.UnitZ;

            if (chunk.GetBlockID(upVector) == 0 || chunk.GetBlockID(downVector) == 0 ||
                chunk.GetBlockID(leftVector) == 0 || chunk.GetBlockID(rightVector) == 0 ||
                chunk.GetBlockID(backVector) == 0 || chunk.GetBlockID(frontVector) == 0)
            {

                return true;

            }

            return false;

        }

        private static BlockModelCullDirection[] GetFacesExposedToAirSafe(Chunk chunk, Vector3i position)
        {

            List<BlockModelCullDirection> cullDirections = new List<BlockModelCullDirection>();
            Vector3i upVector = position.Y >= GlobalValues.ChunkSize - 1 ? position : position + Vector3i.UnitY;
            Vector3i downVector = position.Y <= 0 ? position : position - Vector3i.UnitY;
            Vector3i leftVector = position.X >= GlobalValues.ChunkSize - 1 ? position : position + Vector3i.UnitX;
            Vector3i rightVector = position.X <= 0 ? position : position - Vector3i.UnitX;
            Vector3i backVector = position.Z >= GlobalValues.ChunkSize - 1 ? position : position + Vector3i.UnitZ;
            Vector3i frontVector = position.Z <= 0 ? position : position - Vector3i.UnitZ;

            if (chunk.GetBlockID(upVector) == 0) cullDirections.Add(BlockModelCullDirection.Up);
            if (chunk.GetBlockID(downVector) == 0) cullDirections.Add(BlockModelCullDirection.Down);
            if (chunk.GetBlockID(leftVector) == 0) cullDirections.Add(BlockModelCullDirection.Left);
            if (chunk.GetBlockID(rightVector) == 0) cullDirections.Add(BlockModelCullDirection.Right);
            if (chunk.GetBlockID(backVector) == 0) cullDirections.Add(BlockModelCullDirection.Back);
            if (chunk.GetBlockID(frontVector) == 0) cullDirections.Add(BlockModelCullDirection.Front);

            return cullDirections.ToArray();

        }

        public static void CallOpenGL(Chunk chunk)
        {

            GL.DeleteVertexArray(chunk.Vao);
            GL.DeleteBuffer(chunk.Ssbo);
            GL.DeleteBuffer(chunk.Vbo);
            GL.DeleteBuffer(chunk.Ibo);

            chunk.Ssbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, chunk.Ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, chunk.SolidMesh.Length * Marshal.SizeOf<ChunkVertex>(), chunk.SolidMesh, BufferUsage.DynamicDraw);
            GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, 3, chunk.Ssbo);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            chunk.Vao = GL.GenVertexArray();
            // chunk.Vao = GL.GenVertexArray();
            GL.BindVertexArray(chunk.Vao);
            // int vbo = GL.GenBuffer();
            chunk.Vbo = GL.GenBuffer();
            // chunk.Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, chunk.Vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, chunk.SolidMesh.Length * Marshal.SizeOf<ChunkVertex>(), chunk.SolidMesh, BufferUsage.DynamicDraw);

            chunk.Ibo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, chunk.Ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, chunk.MeshIndices.Length * sizeof(int), chunk.MeshIndices, BufferUsage.DynamicDraw);

            GL.BindVertexArray(0);

            chunk.QueueType = QueueType.Done;
            chunk.IsRenderable = true;

        }

        public static void Remesh(Chunk chunk, World world)
        {

            Mesh(chunk, world, Vector3i.Zero);
            CallOpenGL(chunk);

        }

    }
}
