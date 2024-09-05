using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Core.Chunks
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

           
            Vector3i chunkPosition = chunk.ChunkPosition;

            if (!chunk.TryLoad())
            {

                for (int x = 0; x < GlobalValues.ChunkSize; x++)
                {

                    for (int z = 0; z < GlobalValues.ChunkSize; z++)
                    {

                        int xGlobal = x + (chunkPosition.X * GlobalValues.ChunkSize);
                        int zGlobal = z + (chunkPosition.Z * GlobalValues.ChunkSize);
                        float height = Maths.ValueNoise2Octaves(12345123, xGlobal / 32f, zGlobal / 32f, 3) * 32f;

                        for (int y = 0; y < GlobalValues.ChunkSize; y++)
                        {

                            int yGlobal = y + (chunkPosition.Y * GlobalValues.ChunkSize);

                            uint seed = 0;

                            if (yGlobal < height)
                            {

                                chunk.SetBlock((x, y, z), Blocks.GrassBlock);

                            }

                            if (yGlobal < height - 1)
                            {

                                chunk.SetBlock((x, y, z), Blocks.DirtBlock);

                            }

                            if (yGlobal < height - 4)
                            {

                                // chunk.SetBlockDataGlobal((xGlobal, yGlobal, zGlobal), Globals.Register.GetIDFromBlock(Blocks.StoneBlock));
                                chunk.SetBlock((x, y, z), Blocks.StoneBlock);
                                // chunk.IsEmpty = false;

                            }

                        }

                    }

                }

            }

            chunk.IsEmpty = chunk.CheckIfEmpty();
            chunk.IsFull = chunk.CheckIfFull();
            // Console.WriteLine(chunk.IsEmpty);
            // Console.WriteLine(chunk.IsFull);
            chunk.QueueType = QueueType.Mesh;
            // WorldGenerator.ChunkUpdateQueue.Enqueue(chunk.ChunkPosition);
            // WorldGenerator.ChunkUpdateQueue.Enqueue(chunk.ChunkPosition);

        }

        public static void GeneratePassTwoThreaded(Chunk chunk, Dictionary<Vector3i, Chunk> world)
        {

            chunk.GenerationState = GenerationState.Generating;
            Task.Run(() => { GeneratePassTwo(chunk, world); });

        }
        public static void GeneratePassTwo(Chunk chunk, Dictionary<Vector3i, Chunk> world)
        {

            Vector3i chunkPosition = chunk.ChunkPosition;

            for (int x = 0; x < GlobalValues.ChunkSize; x++)
            {

                for (int y = 0; y < GlobalValues.ChunkSize; y++)
                {

                    for (int z = 0; z < GlobalValues.ChunkSize; z++)
                    {

                        Vector3i globalBlockPosition = (x, y, z) + (chunkPosition * GlobalValues.ChunkSize);

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

                chunk.GenerationState = GenerationState.PassTwo;
                chunk.QueueMode = QueueMode.NotQueued;
                chunk.QueueType = QueueType.Final;

            }

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

        private static float[] GenerateAmbientPointsForFace(Vector3i position, BlockModelCullDirection direction, Dictionary<Vector3i, Chunk> world)
        {

            Vector3i[] samplePoints = new Vector3i[8];

            switch (direction)
            {

                case BlockModelCullDirection.Up:
                    samplePoints[0] = position + (1, 1, 1);
                    samplePoints[1] = position + (0, 1, 1);
                    samplePoints[2] = position + (-1, 1, 1);
                    samplePoints[3] = position + (1, 1, 0);
                    samplePoints[4] = position + (-1, 1, 0);
                    samplePoints[5] = position + (1, 1, -1);
                    samplePoints[6] = position + (0, 1, -1);
                    samplePoints[7] = position + (-1, 1, -1);
                    break;
                case BlockModelCullDirection.Down:
                    samplePoints[2] = position + (-1, -1, -1);
                    samplePoints[1] = position + (0, -1, -1);
                    samplePoints[0] = position + (1, -1, -1);
                    samplePoints[4] = position + (-1, -1, 0);
                    samplePoints[3] = position + (1, -1, 0);
                    samplePoints[7] = position + (-1, -1, 1);
                    samplePoints[6] = position + (0, -1, 1);
                    samplePoints[5] = position + (1, -1, 1);
                    break;
                case BlockModelCullDirection.Left:
                    samplePoints[0] = position + (1, 1, 1);
                    samplePoints[1] = position + (1, 1, 0);
                    samplePoints[2] = position + (1, 1, -1);
                    samplePoints[3] = position + (1, 0, 1);
                    samplePoints[4] = position + (1, 0, -1);
                    samplePoints[5] = position + (1, -1, 1);
                    samplePoints[6] = position + (1, -1, 0);
                    samplePoints[7] = position + (1, -1, -1);
                    break;
                case BlockModelCullDirection.Right:
                    samplePoints[0] = position + (-1, 1, -1);
                    samplePoints[1] = position + (-1, 1, 0);
                    samplePoints[2] = position + (-1, 1, 1);
                    samplePoints[3] = position + (-1, 0, -1);
                    samplePoints[4] = position + (-1, 0, 1);
                    samplePoints[5] = position + (-1, -1, -1);
                    samplePoints[6] = position + (-1, -1, 0);
                    samplePoints[7] = position + (-1, -1, 1);
                    break;
                case BlockModelCullDirection.Back:
                    samplePoints[0] = position + (-1, 1, 1);
                    samplePoints[1] = position + (0, 1, 1);
                    samplePoints[2] = position + (1, 1, 1);
                    samplePoints[3] = position + (-1, 0, 1);
                    samplePoints[4] = position + (1, 0, 1);
                    samplePoints[5] = position + (-1, -1, 1);
                    samplePoints[6] = position + (0, -1, 1);
                    samplePoints[7] = position + (1, -1, 1);
                    break;
                case BlockModelCullDirection.Front:
                    samplePoints[0] = position + (1, 1, -1);
                    samplePoints[1] = position + (0, 1, -1);
                    samplePoints[2] = position + (-1, 1, -1);
                    samplePoints[3] = position + (1, 0, -1);
                    samplePoints[4] = position + (-1, 0, -1);
                    samplePoints[5] = position + (1, -1, -1);
                    samplePoints[6] = position + (0, -1, -1);
                    samplePoints[7] = position + (-1, -1, -1);
                    break;

            }

            float[] ambientPoints = new float[4] { 1.0f, 1.0f, 1.0f, 1.0f };
            if (world[ChunkUtils.PositionToChunk(samplePoints[0])].GetBlockID(ChunkUtils.PositionToBlockLocal(samplePoints[0])) != 0)
            {

                ambientPoints[0] = 0.0f;

            }
            if (world[ChunkUtils.PositionToChunk(samplePoints[1])].GetBlockID(ChunkUtils.PositionToBlockLocal(samplePoints[1])) != 0)
            {

                ambientPoints[0] = 0.0f;
                ambientPoints[3] = 0.0f;

            }
            if (world[ChunkUtils.PositionToChunk(samplePoints[2])].GetBlockID(ChunkUtils.PositionToBlockLocal(samplePoints[2])) != 0)
            {

                ambientPoints[3] = 0.0f;

            }
            if (world[ChunkUtils.PositionToChunk(samplePoints[3])].GetBlockID(ChunkUtils.PositionToBlockLocal(samplePoints[3])) != 0)
            {

                ambientPoints[0] = 0.0f;
                ambientPoints[1] = 0.0f;

            }
            if (world[ChunkUtils.PositionToChunk(samplePoints[4])].GetBlockID(ChunkUtils.PositionToBlockLocal(samplePoints[4])) != 0)
            {

                ambientPoints[2] = 0.0f;
                ambientPoints[3] = 0.0f;

            }
            if (world[ChunkUtils.PositionToChunk(samplePoints[5])].GetBlockID(ChunkUtils.PositionToBlockLocal(samplePoints[5])) != 0)
            {

                ambientPoints[1] = 0.0f;

            }
            if (world[ChunkUtils.PositionToChunk(samplePoints[6])].GetBlockID(ChunkUtils.PositionToBlockLocal(samplePoints[6])) != 0)
            {

                ambientPoints[1] = 0.0f;
                ambientPoints[2] = 0.0f;

            }
            if (world[ChunkUtils.PositionToChunk(samplePoints[7])].GetBlockID(ChunkUtils.PositionToBlockLocal(samplePoints[7])) != 0)
            {

                ambientPoints[2] = 0.0f;
                // ambientPoints[3] = 0.0f;

            }

            return ambientPoints;

        }

        public static void Mesh(Chunk chunk, Dictionary<Vector3i, Chunk> world, Vector3i cameraPosition)
        {

            Vector3i chunkPosition = chunk.ChunkPosition;

            List<ChunkVertex> mesh = new List<ChunkVertex>();

            for (int x = 0; x < GlobalValues.ChunkSize; x++)
            {

                for (int y = 0; y < GlobalValues.ChunkSize; y++)
                {

                    for (int z = 0; z < GlobalValues.ChunkSize; z++)
                    {

                        if (chunk.GetBlockID((x, y, z)) != 0)
                        {

                            Block block = GlobalValues.Register.GetBlockFromID(chunk.GetBlockID((x, y, z)));

                            Vector3i up = (x, y + 1, z);
                            Vector3i down = (x, y - 1, z);
                            Vector3i left = (x + 1, y, z);
                            Vector3i right = (x - 1, y, z);
                            Vector3i back = (x, y, z + 1);
                            Vector3i front = (x, y, z - 1);

                            Vector3i globalPosition = (x, y, z) + (chunkPosition * GlobalValues.ChunkSize);

                            if (world[ChunkUtils.PositionToChunk(globalPosition + Vector3i.UnitY)].GetBlockID(ChunkUtils.PositionToBlockLocal((globalPosition + Vector3i.UnitY))) == Blocks.AirBlock.ID)
                            {

                                mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelCullDirection.Up, GenerateAmbientPointsForFace(globalPosition, BlockModelCullDirection.Up, world)));

                            }
                            if (world[ChunkUtils.PositionToChunk(globalPosition - Vector3i.UnitY)].GetBlockID(ChunkUtils.PositionToBlockLocal((globalPosition - Vector3i.UnitY))) == Blocks.AirBlock.ID)
                            {

                                mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelCullDirection.Down, GenerateAmbientPointsForFace(globalPosition, BlockModelCullDirection.Down, world)));

                            }
                            if (world[ChunkUtils.PositionToChunk(globalPosition + Vector3i.UnitX)].GetBlockID(ChunkUtils.PositionToBlockLocal((globalPosition + Vector3i.UnitX))) == Blocks.AirBlock.ID)
                            {

                                mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelCullDirection.Left, GenerateAmbientPointsForFace(globalPosition, BlockModelCullDirection.Left, world)));

                            }
                            if (world[ChunkUtils.PositionToChunk(globalPosition - Vector3i.UnitX)].GetBlockID(ChunkUtils.PositionToBlockLocal((globalPosition - Vector3i.UnitX))) == Blocks.AirBlock.ID)
                            {

                                mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelCullDirection.Right, GenerateAmbientPointsForFace(globalPosition, BlockModelCullDirection.Right, world)));

                            }
                            if (world[ChunkUtils.PositionToChunk(globalPosition + Vector3i.UnitZ)].GetBlockID(ChunkUtils.PositionToBlockLocal((globalPosition + Vector3i.UnitZ))) == Blocks.AirBlock.ID)
                            {

                                mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelCullDirection.Back, GenerateAmbientPointsForFace(globalPosition, BlockModelCullDirection.Back, world)));

                            }
                            if (world[ChunkUtils.PositionToChunk(globalPosition - Vector3i.UnitZ)].GetBlockID(ChunkUtils.PositionToBlockLocal((globalPosition - Vector3i.UnitZ))) == Blocks.AirBlock.ID)
                            {

                                mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelCullDirection.Front, GenerateAmbientPointsForFace(globalPosition, BlockModelCullDirection.Front, world)));

                            }

                        }

                    }

                }

            }

            chunk.ChunkMesh = mesh.ToArray();
            chunk.QueueType = QueueType.Final;
            // WorldGenerator.ChunkUpdateQueue.Enqueue(chunk.ChunkPosition);

        }

        private static Block GetBlockWithNeighbors(Chunk chunk, Dictionary<Vector3i, Chunk> neighbors, Vector3i position)
        {

            // Console.WriteLine(position.X < 0);

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
