using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Platform.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.ChunkUtil
{
    internal class ChunkBuilder
    {

        public static void GenerateThreaded(NewChunk chunk)
        {

            if (chunk.GetGenerationState() == GenerationState.NotGenerated)
            {

                chunk.SetGenerationState(GenerationState.Generating);
                Task.Run(() => GeneratePassOne(chunk));

            }
            if (chunk.GetGenerationState() == GenerationState.PassOne)
            {

                chunk.SetGenerationState(GenerationState.Generating);
                Task.Run(() => GeneratePassTwo(chunk, null));

            }

        }

        public static void GeneratePassOneThreaded(NewChunk chunk)
        {

            chunk.QueueMode = QueueMode.Queued;
            chunk.GenerationState = GenerationState.Generating;
            Task.Run(() => { GeneratePassOne(chunk); });

        }

        public static void GeneratePassOne(NewChunk chunk)
        {

            for (int x = 0; x < Globals.ChunkSize; x++)
            {

                for (int y = 0; y < Globals.ChunkSize; y++)
                {

                    for (int z = 0; z < Globals.ChunkSize; z++)
                    {

                        Vector3i chunkPosition = chunk.GetChunkPosition();
                        int xGlobal = x + (chunkPosition.X * Globals.ChunkSize);
                        int yGlobal = y + (chunkPosition.Y * Globals.ChunkSize);
                        int zGlobal = z + (chunkPosition.Z * Globals.ChunkSize);

                        uint seed = 0;

                        float height = Maths.ValueNoise2Octaves(12345123, xGlobal / 32f, zGlobal / 32f, 3) * 32f;

                        if (yGlobal < height)
                        {

                            chunk.SetBlockDataGlobal((xGlobal, yGlobal, zGlobal), Globals.Register.GetIDFromBlock(Blocks.StoneBlock));

                        }

                    }

                }

            }

            chunk.GenerationState = GenerationState.PassOne;
            chunk.QueueMode = QueueMode.NotQueued;

        }

        public static void GeneratePassTwoThreaded(NewChunk chunk, Dictionary<Vector3i, NewChunk> world)
        {

            // chunk.QueueMode = QueueMode.Queued;
            chunk.GenerationState = GenerationState.Generating;
            Task.Run(() => { GeneratePassTwo(chunk, world); });

        }
        public static void GeneratePassTwo(NewChunk chunk, Dictionary<Vector3i, NewChunk> world)
        {

            // chunk.IsEmpty = chunk.CheckIfEmpty();
            // chunk.IsFull = chunk.CheckIfFull();
            // chunk.IsExposed = chunk.CheckIfExposed(world);

            if (true)// chunk.IsExposed && !chunk.IsEmpty)
            {

                for (int x = 0; x < Globals.ChunkSize; x++)
                {

                    for (int y = 0; y < Globals.ChunkSize; y++)
                    {

                        for (int z = 0; z < Globals.ChunkSize; z++)
                        {

                            Vector3i chunkPosition = chunk.GetChunkPosition();
                            int xGlobal = x + (chunkPosition.X * Globals.ChunkSize);
                            int yGlobal = y + (chunkPosition.Y * Globals.ChunkSize);
                            int zGlobal = z + (chunkPosition.Z * Globals.ChunkSize);

                            if (chunk.GetBlock((x,y,z)) == Blocks.StoneBlock)
                            {

                                chunk.SetBlock((x, y, z), Blocks.DirtBlock);

                            }

                            /*
                            if (chunk.GetBlock((x, y, z)) == Blocks.StoneBlock && ChunkLoader.GetChunk(ChunkUtils.PositionToChunk((xGlobal, yGlobal+1, zGlobal))).GetBlock(ChunkUtils.PositionToBlockLocal((xGlobal, yGlobal+1, zGlobal))) == Blocks.AirBlock)
                            {

                                chunk.SetBlock((x, y, z), Blocks.GrassBlock);

                                for (int i = 1; i <= 4; i++)
                                {

                                    if (ChunkLoader.GetChunk(ChunkUtils.PositionToChunk((xGlobal, yGlobal-i, zGlobal))).GetBlock(ChunkUtils.PositionToBlockLocal((xGlobal, yGlobal-i, zGlobal))) == Blocks.StoneBlock)
                                    {

                                        ChunkLoader.GetChunk(ChunkUtils.PositionToChunk((xGlobal, yGlobal - i, zGlobal))).SetBlock(ChunkUtils.PositionToBlockLocal((xGlobal, yGlobal - i, zGlobal)), Blocks.DirtBlock);

                                    }

                                }

                                if (Maths.FloatRandom2(0, xGlobal*2, zGlobal*2) > 0.99f)
                                {

                                    for (int i = 1; i <= 5; i++)
                                    {

                                        ChunkLoader.GetChunk(ChunkUtils.PositionToChunk((xGlobal, yGlobal + i, zGlobal))).SetBlock(ChunkUtils.PositionToBlockLocal((xGlobal, yGlobal + i, zGlobal)), Blocks.LogBlock);

                                    }

                                }

                            }
                            */


                        }

                    }

                }

            }

            chunk.GenerationState = GenerationState.Generated;
            chunk.QueueMode = QueueMode.NotQueued;
            // Console.WriteLine(chunk.GenerationState);

        }

        public static void MeshThreaded(NewChunk chunk, Dictionary<Vector3i, NewChunk> world)
        {

            chunk.QueueMode = QueueMode.Queued;
            chunk.SetMeshState(MeshState.Meshing);
            Task.Run(() => { Mesh(chunk, world); });

        }

        private static void Mesh(NewChunk chunk, Dictionary<Vector3i, NewChunk> chunkNeighbors)
        {

            // chunk.IsEmpty = chunk.CheckIfEmpty();
            // chunk.IsFull = chunk.CheckIfFull();
            // chunk.IsExposed = chunk.CheckIfExposed(chunkNeighbors);

            Vector3i chunkPosition = chunk.ChunkPosition;

            List<ChunkVertex> mesh = new List<ChunkVertex>();

            if (true)
            {

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

                                Vector3i globalPosition = (x, y, z) * (chunkPosition * Globals.ChunkSize);


                                if (GetBlockWithNeighbors(chunk, chunkNeighbors, up) == Blocks.AirBlock) mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Up));
                                if (GetBlockWithNeighbors(chunk, chunkNeighbors, down) == Blocks.AirBlock) mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Down));
                                if (GetBlockWithNeighbors(chunk, chunkNeighbors, left) == Blocks.AirBlock) mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Left));
                                if (GetBlockWithNeighbors(chunk, chunkNeighbors, right) == Blocks.AirBlock) mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Right));
                                if (GetBlockWithNeighbors(chunk, chunkNeighbors, back) == Blocks.AirBlock) mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Back));
                                if (GetBlockWithNeighbors(chunk, chunkNeighbors, front) == Blocks.AirBlock) mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Front));

                            }

                        }

                    }

                }

            }
            chunk.SetChunkMesh(mesh.ToArray());
            // chunk.ChunkMesh = mesh.ToArray();
            chunk.MeshState = MeshState.Meshed;
            chunk.QueueMode = QueueMode.NotQueued;
            // chunk.SetChunkState(ChunkState.Ready);
            // chunk.MeshState = MeshState.Meshed;

        }

        private static Block GetBlockWithNeighbors(NewChunk chunk, Dictionary<Vector3i, NewChunk> neighbors, Vector3i position)
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
        private static bool AnyNeighborsAirSafe(NewChunk chunk, Vector3i position)
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

        private static BlockModelNewCullDirection[] GetFacesExposedToAirSafe(NewChunk chunk, Vector3i position)
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

        public static void CallOpenGL(NewChunk chunk)
        {

            GL.DeleteVertexArray(chunk.Vao);
            GL.DeleteBuffer(chunk.Vbo);

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
            GL.BufferData(BufferTarget.ArrayBuffer, chunk.GetChunkMesh().Length * Marshal.SizeOf<ChunkVertex>(), chunk.GetChunkMesh(), BufferUsageHint.DynamicDraw);

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
            // chunk.ChunkState = ChunkState.Ready;

        }

        public static void Remesh(NewChunk chunk, Dictionary<Vector3i, NewChunk> neighbors)
        {

            Mesh(chunk, neighbors);
            CallOpenGL(chunk);

        }

    }
}
