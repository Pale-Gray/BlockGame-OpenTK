using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

                        // chunk.BlockData[x, y, z] = (ushort)Globals.Register.GetIDFromBlock(Blocks.GrassBlock);
                        ushort data = (ushort)Globals.Register.GetIDFromBlock(Blocks.StoneBlock);

                        Vector3i chunkPosition = chunk.GetChunkPosition();

                        int xGlobal = x + (chunkPosition.X * Globals.ChunkSize);
                        int yGlobal = y + (chunkPosition.Y * Globals.ChunkSize);
                        int zGlobal = z + (chunkPosition.Z * Globals.ChunkSize);

                        int height = (int)( (((Globals.noise.GetNoise(xGlobal, zGlobal)*2 + Globals.noise.GetNoise(xGlobal * 2, zGlobal * 2)) / 2f) + 0.5f) * 10);
                        // int height = 0;
                        // int height = 0; //xGlobal; // (int) (15f * Blockgame_OpenTK.Util.Noise.Noise2(0, xGlobal/64f, zGlobal/64f));
                        // int height = (int) (12f * Util.Noise.Noise2(0, xGlobal, yGlobal));

                        // int height = (int)((Globals.noise.GetNoise(x + (chunkPosition.X * Globals.ChunkSize), z + (chunkPosition.Z + Globals.ChunkSize)) / 2f + 0.5f) * 10);

                        // chunk.SetBlockData((x, height, z), data);
                        // chunk.BlockData[x, y, z] = data;
                        if (yGlobal <= height)
                        {

                            chunk.SetBlockDataGlobal((xGlobal, yGlobal, zGlobal), data);

                        }

                    }

                }

            }

            chunk.GenerationState = GenerationState.PassOne;

        }

        public static void GeneratePassTwoThreaded(NewChunk chunk, Dictionary<Vector3i, NewChunk> chunkNeighbors)
        {

            chunk.GenerationState = GenerationState.Generating;
            Task.Run(() => { GeneratePassTwo(chunk, chunkNeighbors); });

        }
        public static void GeneratePassTwo(NewChunk chunk, Dictionary<Vector3i, NewChunk> chunkNeighbors)
        {

            chunk.GenerationState = GenerationState.Generated;
            // Console.WriteLine(chunk.GenerationState);

        }

        public static void MeshThreaded(NewChunk chunk, Dictionary<Vector3i, NewChunk> chunkNeighbors)
        {

            if (chunk.GetMeshState() == MeshState.NotMeshed)
            {

                // chunk.SetMeshState(MeshState.Meshing);
                // chunk.MeshState = MeshState.Meshing;
                chunk.SetMeshState(MeshState.Meshing);
                Task.Run(() => { Mesh(chunk, chunkNeighbors); });

            }

        }

        private static void Mesh(NewChunk chunk, Dictionary<Vector3i, NewChunk> chunkNeighbors)
        {

            List<ChunkVertex> mesh = new List<ChunkVertex>();
            for (int x = 0; x < Globals.ChunkSize; x++)
            {

                for (int y = 0; y < Globals.ChunkSize; y++)
                {

                    for (int z = 0; z < Globals.ChunkSize; z++)
                    {

                        if (chunk.GetBlockID((x,y,z)) != 0)
                        {

                            Block block = Globals.Register.GetBlockFromID(chunk.GetBlockID((x, y, z)));

                            Vector3i up = (x, y + 1, z);
                            Vector3i down = (x, y - 1, z);
                            Vector3i left = (x+1, y, z);
                            Vector3i right = (x - 1, y, z);
                            Vector3i back = (x, y, z + 1);
                            Vector3i front = (x, y, z - 1);

                            if (GetBlockWithNeighbors(chunk, chunkNeighbors, up) == Blocks.AirBlock) mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Up));
                            if (GetBlockWithNeighbors(chunk, chunkNeighbors, down) == Blocks.AirBlock) mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Down));
                            if (GetBlockWithNeighbors(chunk, chunkNeighbors, left) == Blocks.AirBlock) mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Left));
                            if (GetBlockWithNeighbors(chunk, chunkNeighbors, right) == Blocks.AirBlock) mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Right));
                            if (GetBlockWithNeighbors(chunk, chunkNeighbors, back) == Blocks.AirBlock) mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Back));
                            if (GetBlockWithNeighbors(chunk, chunkNeighbors, front) == Blocks.AirBlock) mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Front));

                            // mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Up));
                            //mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Down));
                            //mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Left));
                            //mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Right));
                            //mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Front));
                            //mesh.AddRange(block.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Back));

                        }

                    }

                }

            }
            chunk.SetChunkMesh(mesh.ToArray());
            // chunk.ChunkMesh = mesh.ToArray();
            chunk.SetMeshState(MeshState.Meshed);
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

            chunk.SetChunkState(ChunkState.Processing);
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
            chunk.SetChunkState(ChunkState.Ready);// = ChunkState.Ready;
            // chunk.ChunkState = ChunkState.Ready;

        }

        public static void Remesh(NewChunk chunk, Dictionary<Vector3i, NewChunk> neighbors)
        {

            Mesh(chunk, neighbors);
            CallOpenGL(chunk);

        }

    }
}
