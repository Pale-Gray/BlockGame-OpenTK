using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.World;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.ChunkUtil
{
    internal class ChunkBuilder
    {

        private static readonly object _chunkLock = new();

        public static void GenerateThreaded(Chunk chunk)
        {

            if (chunk.GetGenerationState() == GenerationState.NotGenerated)
            {

                chunk.SetGenerationState(GenerationState.Generating);
                Task.Run(() => GeneratePassOne(chunk));

            }
            if (chunk.GetGenerationState() == GenerationState.PassOne)
            {

                chunk.SetGenerationState(GenerationState.Generating);
                // Task.Run(() => GeneratePassTwo(chunk, null));

            }

        }

        public static void GeneratePassOneThreaded(Chunk chunk)
        {

            chunk.QueueMode = QueueMode.Queued;
            chunk.GenerationState = GenerationState.Generating;
            Task.Run(() => { GeneratePassOne(chunk); });

        }

        public static void GeneratePassOne(Chunk chunk)
        {

            for (int x = 0; x < Globals.ChunkSize; x++)
            {

                for (int y = 0; y < Globals.ChunkSize; y++)
                {

                    for (int z = 0; z < Globals.ChunkSize; z++)
                    {

                        Vector3i chunkPosition = chunk.ChunkPosition;
                        int xGlobal = x + (chunkPosition.X * Globals.ChunkSize);
                        int yGlobal = y + (chunkPosition.Y * Globals.ChunkSize);
                        int zGlobal = z + (chunkPosition.Z * Globals.ChunkSize);

                        uint seed = 0;

                        float height = Maths.ValueNoise2Octaves(12345123, xGlobal / 32f, zGlobal / 32f, 3) * 32f;

                        if (yGlobal < height)
                        {

                            chunk.SetBlock((x, y, z), Blocks.GrassBlock);

                        }

                        if (yGlobal < height - 1)
                        {

                            chunk.SetBlock((x, y, z), Blocks.DirtBlock);

                        }

                        if (yGlobal < height-4)
                        {

                            // chunk.SetBlockDataGlobal((xGlobal, yGlobal, zGlobal), Globals.Register.GetIDFromBlock(Blocks.StoneBlock));
                            chunk.SetBlock((x, y, z), Blocks.StoneBlock);
                            // chunk.IsEmpty = false;

                        }

                    }

                }

            }

            chunk.IsEmpty = chunk.CheckIfEmpty();
            chunk.IsFull = chunk.CheckIfFull();
            chunk.QueueType = QueueType.Mesh;

            WorldGenerator.ChunkUpdateQueue.Enqueue(chunk.ChunkPosition);

        }

        public static void GeneratePassTwoThreaded(Chunk chunk, Dictionary<Vector3i, Chunk> world)
        {

            chunk.GenerationState = GenerationState.Generating;
            Task.Run(() => { GeneratePassTwo(chunk, world); });

        }
        public static void GeneratePassTwo(Chunk chunk, Dictionary<Vector3i, Chunk> world)
        {

            Vector3i chunkPosition = chunk.ChunkPosition;

            for (int x = 0; x < Globals.ChunkSize; x++)
            {

                for (int y = 0; y < Globals.ChunkSize; y++)
                {

                    for (int z = 0; z < Globals.ChunkSize; z++)
                    {

                        Vector3i globalBlockPosition = (x, y, z) + (chunkPosition * Globals.ChunkSize);

                        if (chunk.GetBlock((x, y, z)) == Blocks.StoneBlock)// && chunk.GetBlock((x,y+1,z)) == Blocks.AirBlock)
                        {

                            if (world[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitY)].GetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition + Vector3i.UnitY)) == Blocks.AirBlock)
                            {

                                world[ChunkUtils.PositionToChunk(globalBlockPosition)].SetBlock((x, y, z), Blocks.GrassBlock);

                                for (int i = 1; i <= 4; i++)
                                {

                                    if (world[ChunkUtils.PositionToChunk(globalBlockPosition - (0, i, 0))].GetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition - (0, i, 0))) != Blocks.AirBlock)
                                    {

                                        world[ChunkUtils.PositionToChunk(globalBlockPosition - (0, i, 0))].SetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition - (0, i, 0)), Blocks.DirtBlock);

                                    }

                                }

                            }

                            if (Maths.FloatRandom2(0, globalBlockPosition.X, globalBlockPosition.Z) >= 0.90f)
                            {

                                for (int i = 1; i < 6; i++)
                                {

                                    Vector3i localPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition + (0, i, 0));

                                    // world[ChunkUtils.PositionToChunk(globalBlockPosition + (0, i, 0))].ChunkPregenData[localPosition.X, localPosition.Y, localPosition.Z] = Blocks.LogBlock.ID;
                                   // world[ChunkUtils.PositionToChunk(globalBlockPosition + (0, i, 0))].SetBlockDataGlobal(globalBlockPosition + (0, i, 0), Blocks.LogBlock.ID);

                                }

                            }

                        }

                    }

                }

            }

            chunk.GenerationState = GenerationState.PassTwo;
            chunk.QueueMode = QueueMode.NotQueued;
            chunk.QueueType = QueueType.Final;

        }

        public static void GenerateFinalPassThreaded(Chunk chunk, Dictionary<Vector3i, Chunk> world)
        {

            chunk.GenerationState = GenerationState.Generating;
            Task.Run(() => { GenerateFinalPass(chunk, world); });

        }

        public static void GenerateFinalPass(Chunk chunk, Dictionary<Vector3i, Chunk> world)
        {

            chunk.GenerationState = GenerationState.Generated;
            chunk.QueueMode = QueueMode.NotQueued;
            chunk.QueueType = QueueType.Mesh;

        }

        public static void MeshThreaded(Chunk chunk, Dictionary<Vector3i, Chunk> world, Vector3i cameraPosition)
        {

            chunk.QueueMode = QueueMode.Queued;
            chunk.SetMeshState(MeshState.Meshing);
            Task.Run(() => { Mesh(chunk, world, cameraPosition); });

        }

        private static void Mesh(Chunk chunk, Dictionary<Vector3i, Chunk> world, Vector3i cameraPosition)
        {

            Vector3i chunkPosition = chunk.ChunkPosition;

            List<ChunkVertex> mesh = new List<ChunkVertex>();

            for (int x = 0; x < Globals.ChunkSize; x++)
            {

                for (int y = 0; y < Globals.ChunkSize; y++)
                {

                    for (int z = 0; z < Globals.ChunkSize; z++)
                    {

                        if (chunk.GetBlockID((x, y, z)) != 0)
                        {

                            Block block = Globals.Register.GetBlockFromID(chunk.GetBlockID((x, y, z)));

                            Vector3i up = (x, y + 1, z);
                            Vector3i down = (x, y - 1, z);
                            Vector3i left = (x + 1, y, z);
                            Vector3i right = (x - 1, y, z);
                            Vector3i back = (x, y, z + 1);
                            Vector3i front = (x, y, z - 1);

                            Vector3i globalPosition = (x, y, z) + (chunkPosition * Globals.ChunkSize);

                            if (world[ChunkUtils.PositionToChunk(globalPosition + Vector3i.UnitY)].GetBlockID(ChunkUtils.PositionToBlockLocal((globalPosition + Vector3i.UnitY))) == Blocks.AirBlock.ID)
                            {

                                mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Up));

                            }
                            if (world[ChunkUtils.PositionToChunk(globalPosition - Vector3i.UnitY)].GetBlockID(ChunkUtils.PositionToBlockLocal((globalPosition - Vector3i.UnitY))) == Blocks.AirBlock.ID)
                            {

                                mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Down));

                            }
                            if (world[ChunkUtils.PositionToChunk(globalPosition + Vector3i.UnitX)].GetBlockID(ChunkUtils.PositionToBlockLocal((globalPosition + Vector3i.UnitX))) == Blocks.AirBlock.ID)
                            {

                                mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Left));

                            }
                            if (world[ChunkUtils.PositionToChunk(globalPosition - Vector3i.UnitX)].GetBlockID(ChunkUtils.PositionToBlockLocal((globalPosition - Vector3i.UnitX))) == Blocks.AirBlock.ID)
                            {

                                mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Right));

                            }
                            if (world[ChunkUtils.PositionToChunk(globalPosition + Vector3i.UnitZ)].GetBlockID(ChunkUtils.PositionToBlockLocal((globalPosition + Vector3i.UnitZ))) == Blocks.AirBlock.ID)
                            {

                                mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Back));

                            }
                            if (world[ChunkUtils.PositionToChunk(globalPosition - Vector3i.UnitZ)].GetBlockID(ChunkUtils.PositionToBlockLocal((globalPosition - Vector3i.UnitZ))) == Blocks.AirBlock.ID)
                            {

                                mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Front));

                            }

                        }

                    }

                }

            }

            chunk.ChunkMesh = mesh.ToArray();
            chunk.QueueType = QueueType.Final;
            WorldGenerator.ChunkUpdateQueue.Enqueue(chunk.ChunkPosition);

        }

        private static Block GetBlockWithNeighbors(Chunk chunk, Dictionary<Vector3i, Chunk> neighbors, Vector3i position)
        {

            // Console.WriteLine(position.X < 0);

            if (position.Z >= Globals.ChunkSize) return neighbors[Vector3i.UnitZ].GetBlock((position.X, position.Y, 0));
            if (position.Z < 0) return neighbors[-Vector3i.UnitZ].GetBlock((position.X, position.Y, Globals.ChunkSize - 1));
            if (position.Y >= Globals.ChunkSize) return neighbors[Vector3i.UnitY].GetBlock((position.X, 0, position.Z));
            if (position.Y < 0) return neighbors[-Vector3i.UnitY].GetBlock((position.X, Globals.ChunkSize - 1, position.Z));
            if (position.X >= Globals.ChunkSize) return neighbors[Vector3i.UnitX].GetBlock((0, position.Y, position.Z));
            if (position.X < 0) return neighbors[-Vector3i.UnitX].GetBlock((Globals.ChunkSize - 1, position.Y, position.Z));

            return chunk.GetBlock(position);

        }
        private static bool AnyNeighborsAirSafe(Chunk chunk, Vector3i position)
        {

            Vector3i upVector = position.Y >= Globals.ChunkSize - 1 ? position : position + Vector3i.UnitY;
            Vector3i downVector = position.Y <= 0 ? position : position - Vector3i.UnitY;
            Vector3i leftVector = position.X >= Globals.ChunkSize - 1 ? position : position + Vector3i.UnitX;
            Vector3i rightVector = position.X <= 0 ? position : position - Vector3i.UnitX;
            Vector3i backVector = position.Z >= Globals.ChunkSize - 1 ? position : position + Vector3i.UnitZ;
            Vector3i frontVector = position.Z <= 0 ? position : position - Vector3i.UnitZ;

            if (chunk.GetBlockID(upVector) == 0 || chunk.GetBlockID(downVector) == 0 ||
                chunk.GetBlockID(leftVector) == 0 || chunk.GetBlockID(rightVector) == 0 ||
                chunk.GetBlockID(backVector) == 0 || chunk.GetBlockID(frontVector) == 0)
            {

                return true;

            }

            return false;

        }

        private static BlockModelNewCullDirection[] GetFacesExposedToAirSafe(Chunk chunk, Vector3i position)
        {

            List<BlockModelNewCullDirection> cullDirections = new List<BlockModelNewCullDirection>();
            Vector3i upVector = position.Y >= Globals.ChunkSize - 1 ? position : position + Vector3i.UnitY;
            Vector3i downVector = position.Y <= 0 ? position : position - Vector3i.UnitY;
            Vector3i leftVector = position.X >= Globals.ChunkSize - 1 ? position : position + Vector3i.UnitX;
            Vector3i rightVector = position.X <= 0 ? position : position - Vector3i.UnitX;
            Vector3i backVector = position.Z >= Globals.ChunkSize - 1 ? position : position + Vector3i.UnitZ;
            Vector3i frontVector = position.Z <= 0 ? position : position - Vector3i.UnitZ;

            if (chunk.GetBlockID(upVector) == 0) cullDirections.Add(BlockModelNewCullDirection.Up);
            if (chunk.GetBlockID(downVector) == 0) cullDirections.Add(BlockModelNewCullDirection.Down);
            if (chunk.GetBlockID(leftVector) == 0) cullDirections.Add(BlockModelNewCullDirection.Left);
            if (chunk.GetBlockID(rightVector) == 0) cullDirections.Add(BlockModelNewCullDirection.Right);
            if (chunk.GetBlockID(backVector) == 0) cullDirections.Add(BlockModelNewCullDirection.Back);
            if (chunk.GetBlockID(frontVector) == 0) cullDirections.Add(BlockModelNewCullDirection.Front);

            return cullDirections.ToArray();

        }

        public static void CallOpenGL(Chunk chunk, Dictionary<Vector3i, Chunk> worldChunks)
        {

            GL.DeleteVertexArray(chunk.Vao);
            GL.DeleteBuffer(chunk.Vbo);

            chunk.IsEmpty = chunk.CheckIfEmpty();
            chunk.IsFull = chunk.CheckIfFull();
            chunk.IsExposed = chunk.CheckIfExposed(worldChunks);

            // chunk.SetChunkState(ChunkState.Processing);
            chunk.ChunkState = ChunkState.Processing;
            // chunk.ChunkState = ChunkState.Processing;
            // int vao = GL.GenVertexArray();
            chunk.SetVao(GL.GenVertexArray());
            // chunk.Vao = GL.GenVertexArray();
            GL.BindVertexArray(chunk.GetVao());
            // int vbo = GL.GenBuffer();
            chunk.SetVbo(GL.GenBuffer());
            // chunk.Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, chunk.GetVbo());
            GL.BufferData(BufferTarget.ArrayBuffer, chunk.ChunkMesh == null ? 0 : chunk.ChunkMesh.Length * Marshal.SizeOf<ChunkVertex>(), chunk.GetChunkMesh(), BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 1, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.TextureIndex)));
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.Position)));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.TextureCoordinates)));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.Normal)));
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.AmbientValue)));
            GL.EnableVertexAttribArray(4);
            GL.BindVertexArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // chunk.SetChunkState(ChunkState.Ready);// = ChunkState.Ready;
            chunk.ChunkState = ChunkState.Ready;
            chunk.QueueMode = QueueMode.NotQueued;
            chunk.QueueType = QueueType.Finish;
            // chunk.ChunkState = ChunkState.Ready;

        }

        public static void Remesh(Chunk chunk, Dictionary<Vector3i, Chunk> world)
        {

            Mesh(chunk, world, Vector3i.Zero);
            CallOpenGL(chunk, world);

        }

    }
}
