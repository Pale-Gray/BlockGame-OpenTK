using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using Blockgame_OpenTK.BlockUtil;
using System.IO;
using System.Threading.Tasks;
using Blockgame_OpenTK.Core.Worlds;
using OpenTK.Graphics.Vulkan;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Blockgame_OpenTK.Core.Chunks
{
    public struct ChunkVertex
    {

        public Vector3 Position = (0, 0, 0);
        public int ID = 0;
        public float AmbientValue = 1.0f;
        public int TextureIndex = 0;
        public uint LightData = 0;
        public Vector2 TextureCoordinates = (0, 0);
        public Vector3 Normal = (0, 0, 0);

        public ChunkVertex(int textureIndex, Vector3 position, Vector2 textureCoordinate, Vector3 normal, float ambientValue)
        {

            TextureIndex = textureIndex;
            Position = position;
            TextureCoordinates = textureCoordinate;
            Normal = normal;
            AmbientValue = ambientValue;

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
        SunlightGeneration,
        LightPropagation,
        Mesh,
        Upload,
        Done,
        Remesh,
        Remove

    }

    public struct LightProperties
    {

        public uint PackedLightData;
        public Vector3i GlobalBlockPosition;

        public LightProperties(Vector3i globalBlockPosition, uint packedLightData)
        {

            PackedLightData = packedLightData;
            GlobalBlockPosition = globalBlockPosition;

        }

    }

    public struct LightData
    {

        uint[] LightValues = new uint[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
        Vector3i[] BlockLightPositions;

        public LightData() { }

    }

    internal class Chunk
    {

        // public ushort[,,] BlockData = new ushort[Globals.ChunkSize, Globals.ChunkSize, Globals.ChunkSize];
        public ushort[] BlockData = new ushort[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
        public BlockProperties[] BlockPropertyData = new BlockProperties[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
        // public BlockProperties[] BlockPropertyData = Enumerable.Repeat(new BlockProperties(), GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize).ToArray();
        public bool[] SolidMask = new bool[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
        public uint[] PackedLightData = new uint[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
        public int[] MeshIndices;
        public Dictionary<Vector3i, Vector3i> GlobalBlockLightPositions = new Dictionary<Vector3i, Vector3i>();
        public ChunkVertex[] SolidMesh; // for things that are backface culled ie. grass blocks, dirt blocks, any solid blocks or specified in the mesher
        public ChunkVertex[] NonSolidMesh; // for things that are totally transparent/cutaway blocks ie tree leaves, grass foliage, flowers, etc
        public List<ChunkVertex> OpaqueMeshList = new List<ChunkVertex>();
        public List<int> IndicesList = new List<int>();
        public List<Vector3i> StructurePoints = new List<Vector3i>();
        public GenerationState GenerationState = GenerationState.NotGenerated;
        public MeshState MeshState = MeshState.NotMeshed;
        public ChunkState ChunkState = ChunkState.NotReady;
        public QueueMode QueueMode = QueueMode.NotQueued;
        public QueueType QueueType = QueueType.PassOne;
        public Vector3i ChunkPosition;
        // public int Vao, Vbo;
        public int Vao = 0;
        public int Vbo = 0;
        public int Ibo = 0;
        public int Ssbo = 0;
        public int BlockDataSsbo = 0;
        public int LightSsbo = 0;
        public bool IsEmpty = true;
        public bool IsFull = false;
        public bool IsExposed = false;
        public bool ShouldRender = false;
        public bool IsQueuedForRemesh = false;
        public float Lifetime = 0;
        public readonly object ChunkLock = new();
        public bool NeedsToRequeue = true;
        public bool CallForRemesh = false;
        public bool ForceAborted = false;
        public bool ShouldRemesh = false;
        public bool IsMeshEditable = false;
        public bool IsRenderable = false;
        public string FileName { get { return $"{ChunkPosition.X}_{ChunkPosition.Y}_{ChunkPosition.Z}.cdat"; } }
        public Chunk(Vector3i chunkPosition)
        {

            ChunkPosition = chunkPosition;
            GenerationState = GenerationState.NotGenerated;
            MeshState = MeshState.NotMeshed;
            ChunkState = ChunkState.NotReady;

        }

        public void SaveToFileThreaded()
        {

            Task.Run(() => 
            {

                SaveToFile();
            
            });

        }
        public void SaveToFile()
        {

            List<byte> bytes = new List<byte>();

            byte[] rleData = Rle.Compress(BlockData);

            // byte[] destination = new byte[BrotliEncoder.GetMaxCompressedLength(BlockData.Length * 2)];
            // BrotliEncoder.TryCompress(MemoryMarshal.AsBytes<ushort>(BlockData), destination, out int amtWritten);
            // Array.Resize(ref destination, amtWritten);

            using (FileStream fs = new FileStream($"Chunks/{ChunkPosition.X}_{ChunkPosition.Y}_{ChunkPosition.Z}.cdat", FileMode.Create, FileAccess.Write))
            {

                fs.Write(rleData);

            }
            
        }

        public bool CheckForFile()
        {

            string path = $"Chunks/{ChunkPosition.X}_{ChunkPosition.Y}_{ChunkPosition.Z}.cdat";

            return File.Exists(path);

        }

        public bool TryLoad()
        {

            string path = Path.Combine("Chunks", $"{ChunkPosition.X}_{ChunkPosition.Y}_{ChunkPosition.Z}.cdat");

            if (File.Exists(path))
            {

                BlockData = Rle.Decompress(File.ReadAllBytes(path));
                return true;

            }
            return false;

        }

        public void Draw(Vector3 sunVec, Camera camera)
        {

            GlobalValues.ChunkShader.Use();

            // Console.Log(ChunkPosition);
            Lifetime += (float) GlobalValues.DeltaTime;

            Vector3 s = (Vector3) new Vector3d(Math.Cos(Maths.ToRadians((float)GlobalValues.Time * 45.0f)), Math.Sin(Maths.ToRadians((float)GlobalValues.Time * 45.0f)), 0);

            // GL.Uniform1f(GL.GetUniformLocation(GlobalValues.ChunkShader.id, "arrays"), 0);
            GL.Uniform3f(GL.GetUniformLocation(GlobalValues.ChunkShader.id, "cameraPosition"), 1, camera.Position);
            GL.Uniform3f(GL.GetUniformLocation(GlobalValues.ChunkShader.id, "sunDirection"), 1, sunVec);
            GL.Uniform1f(GL.GetUniformLocation(GlobalValues.ChunkShader.id, "chunkLifetime"), Lifetime);
            GL.Uniform1f(GL.GetUniformLocation(GlobalValues.ChunkShader.id, "radius"), (float) 8);
            GL.Uniform1f(GL.GetUniformLocation(GlobalValues.ChunkShader.id, "shouldRenderFog"), GlobalValues.ShouldRenderFog ? 1 : 0);
            GL.Uniform1f(GL.GetUniformLocation(GlobalValues.ChunkShader.id, "shouldRenderAmbientOcclusion"), GlobalValues.RenderAmbientOcclusion ? 1 : 0);
            GL.Uniform1f(GL.GetUniformLocation(GlobalValues.ChunkShader.id, "fogOffset"), GlobalValues.FogOffset);
            // Console.Log(ChunkPosition);
            GL.Uniform3f(GL.GetUniformLocation(GlobalValues.ChunkShader.id, "chunkpos"), 1, ChunkPosition.ToVector3());
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2dArray, GlobalValues.ArrayTexture.TextureID);
            // GL.UniformMatrix4(GL.GetUniformLocation(Globals.ChunkShader.getID(), "model"), true, ref model);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.ChunkShader.getID(), "view"), 1, true, camera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.ChunkShader.getID(), "projection"), 1, true, camera.ProjectionMatrix);
            // GL.Uniform3(GL.GetUniformLocation(shader.getID(), "cpos"), ref ChunkPosition);
            GL.Uniform1f(GL.GetUniformLocation(GlobalValues.ChunkShader.getID(), "time"), (float) GlobalValues.Time);
            // GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, 3, Ssbo);
            // GL.DrawArrays(PrimitiveType.Triangles, 0, SolidMesh.Length);
            GL.BindVertexArray(Vao);
            // GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Ssbo);
            GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, 3, Ssbo);
            GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, 2, BlockDataSsbo);
            // GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ibo);
            GL.DrawElements(PrimitiveType.Triangles, MeshIndices.Length, DrawElementsType.UnsignedInt, 0);
            // GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            GL.BindVertexArray(0);
            // GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GlobalValues.ChunkShader.UnUse();

        }

        public static void Throw()
        {

            throw new System.Exception("Forced a crash");

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

            if (worldChunks.ContainsKey(ChunkPosition + Vector3i.UnitX) && !worldChunks[ChunkPosition + Vector3i.UnitX].CheckIfFull()) amountExposed++;
            if (worldChunks.ContainsKey(ChunkPosition - Vector3i.UnitX) && !worldChunks[ChunkPosition - Vector3i.UnitX].CheckIfFull()) amountExposed++;
            if (worldChunks.ContainsKey(ChunkPosition + Vector3i.UnitY) && !worldChunks[ChunkPosition + Vector3i.UnitY].CheckIfFull()) amountExposed++;
            if (worldChunks.ContainsKey(ChunkPosition - Vector3i.UnitY) && !worldChunks[ChunkPosition - Vector3i.UnitY].CheckIfFull()) amountExposed++;
            if (worldChunks.ContainsKey(ChunkPosition + Vector3i.UnitZ) && !worldChunks[ChunkPosition + Vector3i.UnitZ].CheckIfFull()) amountExposed++;
            if (worldChunks.ContainsKey(ChunkPosition - Vector3i.UnitZ) && !worldChunks[ChunkPosition - Vector3i.UnitZ].CheckIfFull()) amountExposed++;

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

            return GlobalValues.Register.GetBlockFromID(BlockData[ChunkUtils.VecToIndex(position)]);
            // return Globals.Register.GetBlockFromID(BlockData[position.X, position.Y, position.Z]);

        }

        public void SetBlockLight(Vector3i localLightPosition, Vector3i lightColor)
        {

            Vector3i globalLightPosition = localLightPosition + (ChunkPosition * GlobalValues.ChunkSize);

            GlobalBlockLightPositions.Add(globalLightPosition, lightColor);

        } 

        public void SetBlock(Vector3i position, Block block)
        {
    
            BlockData[ChunkUtils.VecToIndex(position)] = block.ID;
            SolidMask[ChunkUtils.VecToIndex(position)] = block.IsSolid ?? true;

        }

        public Block GetBlockSafe(Vector3i position)
        {

            Vector3i clampedPosition = Vector3i.Clamp(position, (0, 0, 0), (GlobalValues.ChunkSize-1, GlobalValues.ChunkSize-1, GlobalValues.ChunkSize-1));

            return GlobalValues.Register.GetBlockFromID(BlockData[ChunkUtils.VecToIndex(position)]);

        }
        public void SetBlockSafe(Vector3i position, Block block)
        {

            Vector3i min = ChunkPosition * GlobalValues.ChunkSize;
            Vector3i max = min + (GlobalValues.ChunkSize, GlobalValues.ChunkSize, GlobalValues.ChunkSize);

            if (position.X >= min.X && position.X < max.X && position.Y >= min.Y && position.Y < max.Y && position.Z >= min.Z && position.Z < max.Z) 
            {

                BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(position))] = block.ID;
                SolidMask[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(position))] = block.IsSolid ?? true;

            }

            // Vector3i clampedPosition = Vector3i.Clamp(position, (0, 0, 0), (GlobalValues.ChunkSize, GlobalValues.ChunkSize, GlobalValues.ChunkSize));

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

            int minX = (ChunkPosition.X * GlobalValues.ChunkSize);
            int minY = (ChunkPosition.Y * GlobalValues.ChunkSize);
            int minZ = (ChunkPosition.Z * GlobalValues.ChunkSize);

            int maxX = ((1 + ChunkPosition.X) * GlobalValues.ChunkSize) - 1;
            int maxY = ((1 + ChunkPosition.Y) * GlobalValues.ChunkSize) - 1;
            int maxZ = ((1 + ChunkPosition.Z) * GlobalValues.ChunkSize) - 1;

            if (position.X >= minX && position.X <= maxX && position.Y >= minY && position.Y <= maxY && position.Z >= minZ && position.Z <= maxZ)
            {

                // int xValue = x % (size-1);
                // int yValue = y % (size-1);
                // /int zValue = z % (size-1);

                int xValue = position.X - (ChunkPosition.X * GlobalValues.ChunkSize);
                int yValue = position.Y - (ChunkPosition.Y * GlobalValues.ChunkSize);
                int zValue = position.Z - (ChunkPosition.Z * GlobalValues.ChunkSize);

                SetBlock((xValue, yValue, zValue), GlobalValues.Register.GetBlockFromID(data));

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

            return SolidMesh;

        }

        public void SetChunkMesh(ChunkVertex[] mesh)
        {

            SolidMesh = mesh;

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
