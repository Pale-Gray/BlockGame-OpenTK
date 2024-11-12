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
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Core.Chunks
{
    internal class ChunkBuilder
    {

        private static readonly object _chunkLock = new();

        public static void GeneratePassOneThreaded(Chunk chunk)
        {

            // Task.Run(() => { GeneratePassOne(chunk); });
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
                        // chunk.StructurePoints.Add((x, height, y));
                        for (int y = 0; y < GlobalValues.ChunkSize; y++)
                        {

                            globalBlockPosition.Y = y + (chunkPosition.Y * 32);
                            if (globalBlockPosition.Y < height)
                            {

                                Blocks.GrassBlock.OnBlockSet(chunk, (x, y, z));

                            }
                        }

                    }

                }

            }

            chunk.QueueType = QueueType.LightPropagation;
            WorldGenerator.ConcurrentChunkUpdateQueue.Enqueue(chunk.ChunkPosition);
            // WorldGenerator.ConcurrentChunkUpdateQueue.Enqueue(chunk.ChunkPosition);

        }

        public static void MeshThreaded(Chunk chunk, World world, Vector3i cameraPosition)
        {

            // chunk.QueueMode = QueueMode.Queued;
            // chunk.SetMeshState(MeshState.Meshing);
            // .Run(() => { Mesh(chunk, world, cameraPosition); });
            ThreadPool.QueueUserWorkItem(_ => Mesh(chunk, world, cameraPosition));

        }

        public static void Mesh(Chunk chunk, World world, Vector3i cameraPosition)
        {

            chunk.OpaqueMeshList.Clear();
            chunk.IndicesList.Clear();
            Vector3i chunkPosition = chunk.ChunkPosition;

            // List<ChunkVertex> mesh = new List<ChunkVertex>();

            for (int x = 0; x < GlobalValues.ChunkSize; x++)
            {

                for (int y = 0; y < GlobalValues.ChunkSize; y++)
                {

                    for (int z = 0; z < GlobalValues.ChunkSize; z++)
                    {

                        if (chunk.BlockData[ChunkUtils.VecToIndex((x, y, z))] != 0)
                        {

                            Vector3i globalBlockPosition = (x, y, z) + (32 * chunkPosition);

                            chunk.GetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition)).OnBlockMesh(world, chunk.BlockPropertyData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition))] ?? new BlockProperties(), globalBlockPosition);

                        }

                    }

                }

            }

            chunk.MeshIndices = chunk.IndicesList.ToArray();
            chunk.SolidMesh = chunk.OpaqueMeshList.ToArray();
            chunk.QueueType = QueueType.Upload;
            // WorldGenerator.ConcurrentChunkUpdateQueue.Enqueue(chunk.ChunkPosition);
            WorldGenerator.ConcurrentChunkUploadQueue.Enqueue(chunk.ChunkPosition);

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

            chunk.PackedLightData = new uint[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];

            foreach (Vector3i lightPosition in chunk.GlobalBlockLightPositions.Keys)
            {

                Dda.ComputeVisibility(world, chunk, lightPosition, chunk.GlobalBlockLightPositions[lightPosition]);

            }

            chunk.QueueType = QueueType.Mesh;
            WorldGenerator.ConcurrentChunkUpdateQueue.Enqueue(chunk.ChunkPosition);

        }
        public static void Remesh(Chunk chunk, World world, Vector3i cameraPosition)
        {

            chunk.PackedLightData = new uint[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
            // PropagateBlockLights(world, chunk);
            chunk.IndicesList.Clear();
            chunk.OpaqueMeshList.Clear();

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
                            world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].GetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition)).OnBlockMesh(world, world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockPropertyData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition))] ?? new BlockProperties(), globalBlockPosition);

                        }

                    }

                }

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
