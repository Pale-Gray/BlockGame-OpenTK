using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using Blockgame_OpenTK.BlockUtil;
using System.IO;
using Blockgame_OpenTK.Core.World;

namespace Blockgame_OpenTK.ChunkUtil
{

    public struct ChunkVertex
    {

        public int ID = 0;
        public float AmbientValue = 1;
        public int TextureIndex;
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinates;

        public ChunkVertex(int textureIndex, Vector3 position, Vector2 textureCoordinate, Vector3 normal)
        {

            TextureIndex = textureIndex;
            Position = position;
            TextureCoordinates = textureCoordinate;
            Normal = normal;

        }

    }

    public enum GenerationState
    {

        NotGenerated,
        Generating,
        PassOne,
        PassTwo,
        Generated

    }

    public enum ChunkState
    {

        NotReady,
        Processing,
        Ready,
        SaveAndRemove

    }

    public enum MeshState
    {

        NotMeshed,
        Meshing,
        Meshed

    }

    public enum QueueMode
    {

        NotQueued,
        Queued

    }

    public enum QueueType
    {

        None,
        PassOne,
        PassTwo,
        Mesh,
        Final,
        Finish

    }
    internal class Chunk
    {

        // public ushort[,,] BlockData = new ushort[Globals.ChunkSize, Globals.ChunkSize, Globals.ChunkSize];
        public ushort[] BlockData = new ushort[Globals.ChunkSize * Globals.ChunkSize * Globals.ChunkSize];
        public ChunkVertex[] ChunkMesh;
        public GenerationState GenerationState = GenerationState.NotGenerated;
        public MeshState MeshState = MeshState.NotMeshed;
        public ChunkState ChunkState = ChunkState.NotReady;
        public QueueMode QueueMode = QueueMode.NotQueued;
        public QueueType QueueType = QueueType.PassOne;
        public Vector3i ChunkPosition;
        // public int Vao, Vbo;
        public int Vao = 0;
        public int Vbo = 0;
        public bool IsEmpty = true;
        public bool IsFull = false;
        public bool IsExposed = false;
        public bool ShouldRender = false;
        public bool IsQueuedForRemesh = false;
        public float Lifetime = 0;
        public readonly object ChunkLock = new();
        public bool NeedsToRequeue = true;
        public Chunk(Vector3i chunkPosition)
        {

            ChunkPosition = chunkPosition;
            GenerationState = GenerationState.NotGenerated;
            MeshState = MeshState.NotMeshed;
            ChunkState = ChunkState.NotReady;

        }
        public void SaveToFile()
        {

            List<byte> bytes = new List<byte>();

            byte[] rleData = Rle.Compress(BlockData);

            //for (int i = 0; i < BlockData.Length; i++)
            //{

            //    bytes.AddRange(BitConverter.GetBytes(BlockData[i]));

            //}
            using (FileStream fs = new FileStream($"../../../Chunks/{ChunkPosition.X}_{ChunkPosition.Y}_{ChunkPosition.Z}.cdat", FileMode.Create, FileAccess.Write))
            {

                // fs.Write(bytes.ToArray());
                fs.Write(rleData);

            }
            // Console.WriteLine($"wrote to file {ChunkPosition.X}_{ChunkPosition.Y}_{ChunkPosition.Z}.cdat");

        }

        public bool CheckForFile()
        {

            string path = $"../../../Chunks/{ChunkPosition.X}_{ChunkPosition.Y}_{ChunkPosition.Z}.cdat";

            return File.Exists(path);

        }

        public bool TryLoad()
        {

            string path = $"../../../Chunks/{ChunkPosition.X}_{ChunkPosition.Y}_{ChunkPosition.Z}.cdat";

            if (File.Exists(path))
            {

                // byte[] bytes = File.ReadAllBytes(path);
                // byte[] bytes = Rle.Decompress(File.ReadAllBytes(path));
                // System.Buffer.BlockCopy(bytes, 0, BlockData, 0, bytes.Length);
                BlockData = Rle.Decompress(File.ReadAllBytes(path));
                WorldGenerator.ChunkAlterUpdateQueue.Enqueue(ChunkPosition);
                return true;

            }
            return false;

        }

        public void Draw(Vector3 sunVec, Camera camera)
        {

            Globals.ChunkShader.Use();

            // Console.WriteLine(ChunkPosition);
            Lifetime += (float) Globals.DeltaTime;

            GL.Uniform1(GL.GetUniformLocation(Globals.ChunkShader.id, "atlas"), 0);
            GL.Uniform1(GL.GetUniformLocation(Globals.ChunkShader.id, "arrays"), 1);
            GL.Uniform3(GL.GetUniformLocation(Globals.ChunkShader.id, "cameraPosition"), camera.Position);
            GL.Uniform3(GL.GetUniformLocation(Globals.ChunkShader.id, "sunDirection"), sunVec);
            GL.Uniform1(GL.GetUniformLocation(Globals.ChunkShader.id, "chunkLifetime"), Lifetime);
            GL.Uniform1(GL.GetUniformLocation(Globals.ChunkShader.id, "radius"), (float) 8);
            GL.Uniform1(GL.GetUniformLocation(Globals.ChunkShader.id, "shouldRenderFog"), Globals.ShouldRenderFog ? 1 : 0);
            GL.Uniform1(GL.GetUniformLocation(Globals.ChunkShader.id, "fogOffset"), Globals.FogOffset);
            // Console.WriteLine(ChunkPosition);
            GL.Uniform3(GL.GetUniformLocation(Globals.ChunkShader.id, "chunkpos"), ChunkPosition.ToVector3());
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Globals.AtlasTexture.getID());
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2DArray, Globals.ArrayTexture.TextureID);
            // GL.UniformMatrix4(GL.GetUniformLocation(Globals.ChunkShader.getID(), "model"), true, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(Globals.ChunkShader.getID(), "view"), true, ref camera.ViewMatrix);
            GL.UniformMatrix4(GL.GetUniformLocation(Globals.ChunkShader.getID(), "projection"), true, ref camera.ProjectionMatrix);
            // GL.Uniform3(GL.GetUniformLocation(shader.getID(), "cpos"), ref ChunkPosition);
            GL.Uniform1(GL.GetUniformLocation(Globals.ChunkShader.getID(), "time"), (float)0);
            GL.BindVertexArray(Vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, ChunkMesh.Length);
            GL.BindVertexArray(0);

            Globals.ChunkShader.UnUse();

        }

        public bool CheckIfEmpty()
        {

            for (int i = 0; i < BlockData.Length; i++)
            {

                if (BlockData[i] != Blocks.AirBlock.ID) return false;

            }
            return true;

        }

        public bool CheckIfFull()
        {

            for (int i = 0; i < BlockData.Length; i++)
            {

                if (BlockData[i] == Blocks.AirBlock.ID) return false;

            }
            return true;

        }

        public bool CheckIfExposed(Dictionary<Vector3i, Chunk> worldChunks)
        {

            int amountExposed = 0;

            if (!worldChunks[ChunkPosition + Vector3i.UnitX].CheckIfFull()) amountExposed++;
            if (!worldChunks[ChunkPosition - Vector3i.UnitX].CheckIfFull()) amountExposed++;
            if (!worldChunks[ChunkPosition + Vector3i.UnitY].CheckIfFull()) amountExposed++;
            if (!worldChunks[ChunkPosition - Vector3i.UnitY].CheckIfFull()) amountExposed++;
            if (!worldChunks[ChunkPosition + Vector3i.UnitZ].CheckIfFull()) amountExposed++;
            if (!worldChunks[ChunkPosition - Vector3i.UnitZ].CheckIfFull()) amountExposed++;

            return amountExposed > 0;

        }

        public bool CheckIfShouldRender()
        {

            if (IsExposed == false)
            {

                return false;

            }
            return true;

        }
        public Block GetBlock(Vector3i position)
        {

            return Globals.Register.GetBlockFromID(BlockData[ChunkUtils.VecToIndex(position)]);
            // return Globals.Register.GetBlockFromID(BlockData[position.X, position.Y, position.Z]);

        }

        public void SetBlock(Vector3i position, Block block)
        {

            // BlockData[position.X, position.Y, position.Z] = block.ID;

            BlockData[ChunkUtils.VecToIndex(position)] = block.ID;

        }

        public Block GetBlockSafe(Vector3i position)
        {

            Vector3i clampedPosition = Vector3i.Clamp(position, (0, 0, 0), (Globals.ChunkSize-1, Globals.ChunkSize-1, Globals.ChunkSize-1));

            return Globals.Register.GetBlockFromID(BlockData[ChunkUtils.VecToIndex(position)]);

        }
        public void SetBlockSafe(Vector3i position, Block block)
        {

            Vector3i clampedPosition = Vector3i.Clamp(position, (0, 0, 0), (Globals.ChunkSize, Globals.ChunkSize, Globals.ChunkSize));

            // BlockData[clampedPosition.X, clampedPosition.Y, clampedPosition.Z] = (ushort)Globals.Register.GetIDFromBlock(block);

        }

        public ushort GetBlockID(Vector3i position)
        {

            // return BlockData[position.X, position.Y, position.Z];

            return BlockData[ChunkUtils.VecToIndex(position)];

        }

        public GenerationState GetGenerationState()
        {

            return GenerationState;

        }

        public MeshState GetMeshState()
        {

            return MeshState;

        }

        public ChunkState GetChunkState()
        {

            return ChunkState;

        }

        public void SetGenerationState(GenerationState state)
        {


            GenerationState = state;

        }

        public void SetMeshState(MeshState state)
        {

            MeshState = state;

        }

        public void SetChunkState(ChunkState state)
        {

            ChunkState = state;

        }

        public ushort[] GetBlockData()
        {

            return BlockData;

        }

        public void SetBlockDataGlobal(Vector3i position, ushort data)
        {

            int minX = (ChunkPosition.X * Globals.ChunkSize);
            int minY = (ChunkPosition.Y * Globals.ChunkSize);
            int minZ = (ChunkPosition.Z * Globals.ChunkSize);

            int maxX = ((1 + ChunkPosition.X) * Globals.ChunkSize) - 1;
            int maxY = ((1 + ChunkPosition.Y) * Globals.ChunkSize) - 1;
            int maxZ = ((1 + ChunkPosition.Z) * Globals.ChunkSize) - 1;

            if (position.X >= minX && position.X <= maxX && position.Y >= minY && position.Y <= maxY && position.Z >= minZ && position.Z <= maxZ)
            {

                // int xValue = x % (size-1);
                // int yValue = y % (size-1);
                // /int zValue = z % (size-1);

                int xValue = position.X - (ChunkPosition.X * Globals.ChunkSize);
                int yValue = position.Y - (ChunkPosition.Y * Globals.ChunkSize);
                int zValue = position.Z - (ChunkPosition.Z * Globals.ChunkSize);

                SetBlock((xValue, yValue, zValue), Globals.Register.GetBlockFromID(data));

            }

        }

        public int GetVao()
        {

            return Vao;

        }

        public void SetVao(int vao)
        {

            Vao = vao;

        }

        public int GetVbo()
        {

            return Vbo;

        }

        public void SetVbo(int vbo)
        {

            Vbo = vbo;

        }

        public ChunkVertex[] GetChunkMesh()
        {

            return ChunkMesh;

        }

        public void SetChunkMesh(ChunkVertex[] mesh)
        {

            ChunkMesh = mesh;

        }

        public Vector3i GetChunkPosition()
        {

            return ChunkPosition;

        }

        public void SetChunkPosition(Vector3i position)
        {

            ChunkPosition = position;

        }

    }

}
