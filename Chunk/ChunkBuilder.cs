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

        static object ChunkLock = new();

        public static void GenerateThreaded(NewChunk chunk)
        {

            if (chunk.GetGenerationState() == GenerationState.NotGenerated)
            {

                chunk.SetGenerationState(GenerationState.Generating);
                Task.Run(() => { GeneratePassOne(chunk); });

            }
            if (chunk.GetGenerationState() == GenerationState.PassOne)
            {

                chunk.SetGenerationState(GenerationState.Generating);
                Task.Run(() => { GeneratePassTwo(chunk); });

            }

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
                        ushort data = (ushort)Globals.Register.GetIDFromBlock(Blocks.GrassBlock);

                        Vector3i chunkPosition = chunk.GetChunkPosition();

                        int height = (int)((Globals.noise.GetNoise(x + (chunkPosition.X * Globals.ChunkSize), z + (chunkPosition.Z + Globals.ChunkSize)) / 2f + 0.5f) * 10);

                        // chunk.SetBlockData((x, height, z), data);
                        // chunk.BlockData[x, y, z] = data;
                        chunk.SetBlockData((x, y, z), data);

                    }

                }

            }
            // chunk.SetGenerationState(GenerationState.Generated);
            // chunk.GenerationState = GenerationState.Generated;
            chunk.SetGenerationState(GenerationState.Generated);
            // chunk.GenerationState = GenerationState.Generated;

        }

        public static void GeneratePassTwo(NewChunk chunk)
        {

            for (int x = 0; x < Globals.ChunkSize; x++)
            {

                for (int y = 0; y < Globals.ChunkSize; y++)
                {

                    for (int z = 0; z < Globals.ChunkSize; z++)
                    {

                        // chunk.BlockData[x, y, z] = (ushort)Globals.Register.GetIDFromBlock(Blocks.GrassBlock);

                        // ushort data = (ushort)1;
                        // chunk.SetBlockData((x, y, z), data);

                    }

                }

            }
            chunk.SetGenerationState(GenerationState.Generated);
            // Console.WriteLine(chunk.GenerationState);

        }

        public static void MeshThreaded(NewChunk chunk)
        {

            if (chunk.GetMeshState() == MeshState.NotMeshed)
            {

                // chunk.SetMeshState(MeshState.Meshing);
                // chunk.MeshState = MeshState.Meshing;
                chunk.SetMeshState(MeshState.Meshing);
                Task.Run(() => { Mesh(chunk); });

            }

        }

        private static void Mesh(NewChunk chunk)
        {

            List<ChunkVertex> mesh = new List<ChunkVertex>();
            for (int x = 0; x < Globals.ChunkSize; x++)
            {

                for (int y = 0; y < Globals.ChunkSize; y++)
                {

                    for (int z = 0; z < Globals.ChunkSize; z++)
                    {

                        mesh.AddRange(Blocks.GrassBlock.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Up));
                        mesh.AddRange(Blocks.GrassBlock.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Down));
                        mesh.AddRange(Blocks.GrassBlock.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Left));
                        mesh.AddRange(Blocks.GrassBlock.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Right));
                        mesh.AddRange(Blocks.GrassBlock.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Front));
                        mesh.AddRange(Blocks.GrassBlock.BlockModel.ConvertToChunkReadableFaceOffset((x, y, z), BlockModelNewCullDirection.Back));

                    }

                }

            }
            chunk.SetChunkMesh(mesh.ToArray());
            // chunk.ChunkMesh = mesh.ToArray();
            chunk.SetMeshState(MeshState.Meshed);
            // chunk.MeshState = MeshState.Meshed;

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

    }
}
