using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Blockgame_OpenTK.Util;
using Blockgame_OpenTK.BlockUtil;
using System.Text.Json.Serialization;
using Blockgame_OpenTK.Registry;
using OpenTK.Audio.OpenAL;
using System.Runtime.CompilerServices;

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
        Ready

    }
    public enum MeshState
    {

        NotMeshed,
        Meshing,
        Meshed

    }
    internal class Chunk
    {

        static int size = Globals.ChunkSize;
        public int[,,] blockdata = new int[size,size,size];
        public byte[,,] DirectionData = new byte[size,size,size];
        
        public ChunkVertex[] MeshData = new ChunkVertex[0];

        // Vertex Buffer Object of the chunk.
        public int vbo;
        // Vertex Array Object of the chunk.
        public int vao;

        public int PreviousNeighbors = 0;

        // Model matrix of the chunk
        public Matrix4 model;

        public int cx; // chunk x position
        public int cy; // chunk y position
        public int cz; // chunk z position

        public Vector3 ChunkPosition; // vector of cx, cy, cz

        bool hasMesh = true;

        Random r = new Random();

        ChunkState chunkState = ChunkState.NotReady;
        MeshState meshState = MeshState.NotMeshed;
        GenerationState generationState = GenerationState.NotGenerated;

        public readonly object ChunkLock = new();

        public Chunk(int x, int y, int z)
        {

            cx = x;
            cy = y;
            cz = z;
            ChunkPosition = (cx, cy, cz);
            Globals.noise.SetSeed(Globals.seed);
            model = Matrix4.CreateTranslation(x * size, y * size, z * size);

        }

        public Chunk(Vector3 chunkPosition)
        {

            // Console.WriteLine("init");
            // Stopwatch elapsed = Stopwatch.StartNew();
            // TimeSpan elapsedtime;
            cx = (int) chunkPosition.X;
            cy = (int) chunkPosition.Y;
            cz = (int) chunkPosition.Z;
            ChunkPosition = chunkPosition;
            // ThreadPool.QueueUserWorkItem(new WaitCallback(Generate));

            model = Matrix4.CreateTranslation(cx * size, cy * size, cz * size);

        }

        public void UpdateChunk()
        {

            //if (generationState == GenerationState.Generated && meshState == MeshState.Done && chunkState == ChunkState.Ready && IsSent == false)
            //{

                // Console.WriteLine("yes");
             //   ProcessToRender();

            //}

        }

        /* 
        public void GenerateTerrainThreaded()
        {

            generationState = GenerationState.Generating;
            Task.Run(() => InitializeData());

        }

        public void GenerateMeshThreaded()
        {

            chunkState = ChunkState.NotReady;
            meshState = MeshState.Meshing;
            Task.Run(() => GenerateMesh());

        }

        public bool ContainsMesh()
        {

            if (hasMesh)
            {

                return true;

            }

            return false;

        }
        public void Generate()
        {

            if (chunkState == ChunkState.NotReady && meshState == MeshState.Meshed)
            {
                ProcessToRender();
                chunkState = ChunkState.Ready;

            }

            // Console.WriteLine("Position: {0}, GenerationState: {1}, MeshState: {2}, ChunkState: {3}", ChunkPosition, GenerationState, MeshState, ChunkState);

            if (generationState == GenerationState.Generated && meshState == MeshState.NotMeshed)
            {

                // NeighborsGenerated();
                // Console.WriteLine("Yes");
                // Console.WriteLine(NeighborsGenerated().Count());
                Task.Run(() => GenerateMesh());
                // Task.Run(() => GenerateMesh(ChunkLock));

            }

            if (generationState == GenerationState.NotGenerated)
            {

                Task.Run(() => InitializeData());

            }

        }

        /*
        public int GetAmountNeighborsGenerated()
        {

            int num = 0;
            if (ChunkLoader.ChunkDictionary.ContainsKey(ChunkPosition + (1,0,0)))
            {

                if (ChunkLoader.GetChunk(ChunkPosition + (1,0,0)).GetGenerationState() == GenerationState.Generated)
                {

                    num++;

                }

            }
            if (ChunkLoader.ChunkDictionary.ContainsKey(ChunkPosition + (0, 1, 0)))
            {

                if (ChunkLoader.GetChunk(ChunkPosition + (0, 1, 0)).GetGenerationState() == GenerationState.Generated)
                {

                    num++;

                }

            }
            if (ChunkLoader.ChunkDictionary.ContainsKey(ChunkPosition + (0, 0, 1)))
            {

                if (ChunkLoader.GetChunk(ChunkPosition + (0, 0, 1)).GetGenerationState() == GenerationState.Generated)
                {

                    num++;

                }

            }
            if (ChunkLoader.ChunkDictionary.ContainsKey(ChunkPosition + (-1, 0, 0)))
            {

                if (ChunkLoader.GetChunk(ChunkPosition + (-1, 0, 0)).GetGenerationState() == GenerationState.Generated)
                {

                    num++;

                }

            }
            if (ChunkLoader.ChunkDictionary.ContainsKey(ChunkPosition + (0, -1, 0)))
            {

                if (ChunkLoader.GetChunk(ChunkPosition + (0, -1, 0)).GetGenerationState() == GenerationState.Generated)
                {

                    num++;

                }

            }
            if (ChunkLoader.ChunkDictionary.ContainsKey(ChunkPosition + (0, 0, -1)))
            {

                if (ChunkLoader.GetChunk(ChunkPosition + (0, 0, -1)).GetGenerationState() == GenerationState.Generated)
                {

                    num++;

                }

            }

            return num;

        }

        */
        /* 
        public Block GetBlockOverrided(Vector3i direction)
        {

            int positionOverrideX = (int) Math.Floor(direction.X/(float)size);
            int positionOverrideY = (int)Math.Floor(direction.Y / (float)size);
            int positionOverrideZ = (int)Math.Floor(direction.Z / (float)size);
            Vector3 overridePosition = (positionOverrideX, positionOverrideY, positionOverrideZ);
            // int localZ = z;
            // Console.WriteLine(localZ);

            if (positionOverrideX > 0)
            {

                if (ChunkLoader.ContainsChunk(ChunkPosition + overridePosition))
                {

                    if (ChunkLoader.GetChunk(ChunkPosition + overridePosition).GetGenerationState() == GenerationState.Generated)
                    {

                        return ChunkLoader.GetChunk(ChunkPosition + overridePosition).GetBlock(0, direction.Y, direction.Z);

                    }

                }

            }

            if (positionOverrideX < 0)
            {

                if (ChunkLoader.ContainsChunk(ChunkPosition + overridePosition))
                {

                    if (ChunkLoader.GetChunk(ChunkPosition + overridePosition).GetGenerationState() == GenerationState.Generated)
                    {

                        return ChunkLoader.GetChunk(ChunkPosition + overridePosition).GetBlock(size-1, direction.Y, direction.Z);

                    }

                }

            }

            if (positionOverrideY > 0)
            {

                if (ChunkLoader.ContainsChunk(ChunkPosition + overridePosition))
                {

                    if (ChunkLoader.GetChunk(ChunkPosition + overridePosition).GetGenerationState() == GenerationState.Generated)
                    {

                        return ChunkLoader.GetChunk(ChunkPosition + overridePosition).GetBlock(direction.X, 0, direction.Z);

                    }

                }

            }

            if (positionOverrideY < 0)
            {

                if (ChunkLoader.ContainsChunk(ChunkPosition + overridePosition))
                {

                    if (ChunkLoader.GetChunk(ChunkPosition + overridePosition).GetGenerationState() == GenerationState.Generated)
                    {

                        return ChunkLoader.GetChunk(ChunkPosition + overridePosition).GetBlock(direction.X, size-1, direction.Z);

                    }

                }

            }

            if (positionOverrideZ > 0)
            {

                if (ChunkLoader.ContainsChunk(ChunkPosition + overridePosition))
                {

                    if (ChunkLoader.GetChunk(ChunkPosition + overridePosition).GetGenerationState() == GenerationState.Generated)
                    {

                        return ChunkLoader.GetChunk(ChunkPosition + overridePosition).GetBlock(direction.X, direction.Y, 0);

                    }

                }

            }

            if (positionOverrideZ < 0)
            {

                if (ChunkLoader.ContainsChunk(ChunkPosition + overridePosition))
                {

                    if (ChunkLoader.GetChunk(ChunkPosition + overridePosition).GetGenerationState() == GenerationState.Generated)
                    {

                        return ChunkLoader.GetChunk(ChunkPosition + overridePosition).GetBlock(direction.X, direction.Y, size-1);

                    }

                }

            }

            return GetBlock(direction.X, direction.Y, direction.Z);

        }
        private float GetNoise2D(int octaves, int x, int y)
        {

            // int octaves = 3;
            float value = 0;

            

            float xValue = (float)x + (cx * size);
            float yValue = (float)y + (cz * size);

            // for (float i = 1; i <= octaves; i++)
            float noisevalue = Globals.noise.GetNoise(xValue/4, yValue/4);
            noisevalue += Globals.noise.GetNoise(xValue*2, yValue*2)/4;
            noisevalue += Globals.noise.GetNoise(xValue / 8, yValue / 8) * 4;
            value = (noisevalue / 2) + 0.5f;

            return value;

        }

        private float GetNoiseOctaves3D(int x, int y, int z, int octaves)
        {

            float value = 1;
            float X = (float) x;
            float Y = (float) y;
            float Z = (float) z;

            float XValue = X + (cx * size);
            float YValue = Y + (cy * size);
            float ZValue = Z + (cz * size);
            float Octaves = (float) octaves;

            for (float i = 1; i <= Octaves; i++)
            {

                value *= Globals.noise.GetNoise(XValue*i, YValue*i, ZValue*i)/i;

            }
            // value /= octaves;

            return (value / 2) + 0.5f;

        }

        private float GetNoise3D(float x, float y, float z)
        {

            float value = 0;

            float xValue = (float)x + (cx * size);
            float yValue = (float)y + (cy * size);
            float zValue = (float)z + (cz * size);

            value = Globals.noise.GetNoise(xValue, yValue, zValue);

            return (value / 2) + 0.5f;

        }
        private float GetNoise3D(int x, int y, int z)
        {

            float value = 0;

            float xValue = (float)x + (cx * size);
            float yValue = (float)y + (cy * size);
            float zValue = (float)z + (cz * size);

            value = Globals.noise.GetNoise(xValue, yValue, zValue);

            return (value / 2) + 0.5f;

        }
        public void InitializeData()
        {

            // generationState = GenerationState.Generating;
            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        int globalX = x + (cx * size);
                        int globalZ = z + (cz * size);
                        int globalY = y + (cy * size);

                        float value = Maths.MapValueToMinMax(GetNoise2D(3, x, z), 27, 50);

                        float val = GetNoise3D(x,y,z);
                        // Console.WriteLine(val);
                        // float val = GetNoiseOctaves3D(x, y, z, 2);
                        // Console.WriteLine(val);

                        float YOffset = 0;
                        float MaxHeight = 64;

                        SetBlockGlobal(Blocks.AirBlock, globalX, globalY, globalZ);

                        if (val >= globalY / 64f + (value/20))
                        {

                            SetBlockGlobal(Blocks.GrassBlock, globalX, globalY, globalZ);

                        }

                    }

                }

            }
            generationState = GenerationState.Generated;

        }
        public void GenerateMesh()
        {

            List<ChunkVertex> MeshDataList = new List<ChunkVertex>();

            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        // Console.WriteLine($"Position: {(x,y,z)}, Name: {GetBlock((x,y,z)).DataName}, ID: {GetBlockID((x,y,z))}");

                        if (GetBlockID((x,y,z)) != Globals.Register.GetIDFromBlock(Blocks.AirBlock))
                        {
                            
                            MeshBlock(Globals.Register.GetBlockFromID(GetBlockID((x, y, z))), (x,y,z), MeshDataList);
                            
                        }

                    }

                }

            }
            lock (ChunkLock)
            {
                
                MeshData = MeshDataList.ToArray();

            }
            meshState = MeshState.Meshed;

        }

        public void MeshBlock(Block block, Vector3i position, List<ChunkVertex> chunkMesh)
        {

            float[] ambientPoints = new float[4];

            if (SampleBlock(position + Vector3i.UnitY))
            {

                // chunkMesh.AddRange(block.BlockModel.OffsetFace(block.BlockModel.GetConvertedFace(BlockFaceType.Up), position));

            }
            if (SampleBlock(position - Vector3i.UnitY))
            {

                // chunkMesh.AddRange(block.BlockModel.OffsetFace(block.BlockModel.GetConvertedFace(BlockFaceType.Down), position));

            }
            if (SampleBlock(position + Vector3i.UnitX))
            {

                // chunkMesh.AddRange(block.BlockModel.OffsetFace(block.BlockModel.GetConvertedFace(BlockFaceType.Left), position));

            }
            if (SampleBlock(position - Vector3i.UnitX))
            {

                // chunkMesh.AddRange(block.BlockModel.OffsetFace(block.BlockModel.GetConvertedFace(BlockFaceType.Right), position));

            }
            if (SampleBlock(position + Vector3i.UnitZ))
            {

                // chunkMesh.AddRange(block.BlockModel.OffsetFace(block.BlockModel.GetConvertedFace(BlockFaceType.Back), position));

            }
            if (SampleBlock(position - Vector3i.UnitZ))
            {

                // chunkMesh.AddRange(block.BlockModel.OffsetFace(block.BlockModel.GetConvertedFace(BlockFaceType.Front), position));

            }

            // return blockMesh.ToArray();

        }

        public bool SampleBlock(Vector3i direction)
        {

            // Console.WriteLine(GetBlock(direction).DataName);

            if (GetBlockOverload(direction) == Blocks.AirBlock)
            {

                return true;

            }

            return false;

        }

        public int GetBlockID(Vector3i position)
        {

            return blockdata[position.X, position.Y, position.Z];

        }

        public void ProcessToRender()
        {

            if (MeshData.Length > 0)
            {

                hasMesh = true;

            } else
            {

                hasMesh = false;

            }

            if (hasMesh)
            {

                vao = GL.GenVertexArray();
                GL.BindVertexArray(vao);
                vbo = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, MeshData.Length * Marshal.SizeOf<ChunkVertex>(), MeshData, BufferUsageHint.DynamicDraw);

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

            }

            chunkState = ChunkState.Ready;

        }

        public void SetBlockRewrite(Block block, Vector3i coordinates)
        {

            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);

            int x = coordinates.X;
            int y = coordinates.Y;
            int z = coordinates.Z;

            // blockdata[x, y, z] = Blocks.GetIDFromBlock(block);
            SetBlock(block,x,y,z);
            Remesh();

        }

        public void Remesh()
        {

            GenerateMesh();
            ProcessToRender();

        }
        public ChunkState GetChunkState()
        {
            return chunkState;

        }

        public void OverrideGenerationState(GenerationState generationState)
        {

            generationState = generationState;

        }
        public void OverrideChunkState(ChunkState chunkState)
        {

            chunkState = chunkState;

        }
        public void OverrideMeshState(MeshState meshState)
        {

            meshState = meshState;

        }
        public MeshState GetMeshState()
        {

            return meshState;

        }

        public GenerationState GetGenerationState()
        {

            return generationState;

        }
        public int GlobalXToLocalX(int x)
        {

            return x - (cx * size);

        }
        public int GlobalYToLocalY(int y)
        {

            return y - (cy * size);

        }
        public int GlobalZToLocalZ(int z)
        {

            return z - (cz * size);

        }
        public bool IsGlobalPositionAtLocalChunk(int x, int y, int z)
        {

            int minX = (cx * size);
            int minY = (cy * size);
            int minZ = (cz * size);

            int maxX = ((1 + cx) * size) - 1;
            int maxY = ((1 + cy) * size) - 1;
            int maxZ = ((1 + cz) * size) - 1;

            if (x >= minX && x <= maxX && y >= minY && y <= maxY && z >= minZ && z <= maxZ)
            {

                return true;

            }

            return false;

        }
        public void SetBlockGlobal(Block block, int x, int y, int z)
        {

            // x + (cx * size);

            int minX = (cx * size);
            int minY = (cy * size);
            int minZ = (cz * size);

            int maxX = ((1+cx) * size)-1;
            int maxY = ((1+cy) * size)-1;
            int maxZ = ((1+cz) * size)-1;

            if (x >= minX && x <= maxX && y >= minY && y <= maxY && z >= minZ && z <= maxZ)
            {

                // int xValue = x % (size-1);
                // int yValue = y % (size-1);
                // /int zValue = z % (size-1);

                int xValue = x - (cx*size);
                int yValue = y - (cy * size);
                int zValue = z - (cz * size);

                SetBlock(block, xValue, yValue, zValue);

            }

        }
        public Block GetBlockGlobal(int x, int y, int z)
        {

            int xValue = x - (cx * size);
            int yValue = y - (cy * size);
            int zValue = z - (cz * size);

            return Blocks.GetBlockFromID(blockdata[xValue, yValue, zValue]);

        }

        public Block GetBlock(Vector3 position)
        {

            int x = (int)position.X;
            int y = (int)position.Y;
            int z = (int)position.Z;

            return Globals.Register.GetBlockFromID(blockdata[x > size - 1 ? size - 1 : x < 0 ? 0 : x, y > size - 1 ? size - 1 : y < 0 ? 0 : y, z > size - 1 ? size - 1 : z < 0 ? 0 : z]);

        }

        public Block GetBlockOverload(Vector3 position)
        {

            if (position.X >= size)
            {

                return ChunkLoader.GetChunk(ChunkPosition + (1,0,0)).GetBlock((0, position.Y, position.Z));

            }
            if (position.X < 0)
            {

                return ChunkLoader.GetChunk(ChunkPosition - (1, 0, 0)).GetBlock((size-1, position.Y, position.Z));

            }
            if (position.Y >= size)
            {

                return ChunkLoader.GetChunk(ChunkPosition + (0, 1, 0)).GetBlock((position.X, 0, position.Z));

            }
            if (position.Y < 0)
            {

                return ChunkLoader.GetChunk(ChunkPosition - (0, 1, 0)).GetBlock((position.X, size-1, position.Z));

            }
            if (position.Z >= size)
            {

                return ChunkLoader.GetChunk(ChunkPosition + (0, 0, 1)).GetBlock((position.X, position.Y, 0));

            }
            if (position.Z < 0)
            {

                return ChunkLoader.GetChunk(ChunkPosition - (0, 0, 1)).GetBlock((position.X, position.Y, size-1));

            }

            return GetBlock(position);

        }

        public Block GetBlock(int x, int y, int z)
        {

            return Blocks.GetBlockFromID(blockdata[x > size - 1 ? size - 1 : x < 0 ? 0 : x, y > size - 1 ? size - 1 : y < 0 ? 0 : y, z > size - 1 ? size - 1 : z < 0 ? 0 : z]);

        }
        public void SetBlock(Block block, int x, int y, int z)
        {

            lock (ChunkLock)
            {

                blockdata[x, y, z] = Globals.Register.GetIDFromBlock(block);

            }

        }
        public void InsertBlock(List<ChunkVertex> MeshDataList, Block block, int x, int y, int z)
        {

            /* if (GetBlockOverrided(x,y,z+1) == Blocks.Air)// CheckAir(x, y, z + 1))// CheckAir(x, y, z + 1))
            {

                MeshDataList.AddRange(Block.GetFaceShifted(block.BackFace, x, y, z, 1));

            }
            if (GetBlockOverrided(x, y, z - 1) == Blocks.Air)
            {

                MeshDataList.AddRange(Block.GetFaceShifted(block.FrontFace, x, y, z, 1));

            }

            if (GetBlockOverrided(x + 1, y, z) == Blocks.Air)
            {

                MeshDataList.AddRange(Block.GetFaceShifted(block.LeftFace, x, y, z, 1));

            }
            if (GetBlockOverrided(x - 1, y, z) == Blocks.Air)
            {

                MeshDataList.AddRange(Block.GetFaceShifted(block.RightFace, x, y, z, 1));

            }

            if (GetBlockOverrided(x, y + 1, z) == Blocks.Air)
            {

                MeshDataList.AddRange(Block.GetFaceShifted(block.TopFace, x, y, z, 1));

            }
            if (GetBlockOverrided(x, y - 1, z) == Blocks.Air)
            {

                MeshDataList.AddRange(Block.GetFaceShifted(block.BottomFace, x, y, z, 1));

            }

        }
        public bool CheckAir(int x, int y, int z)
        {

            /* if (x > size - 1 || x < 0 || y > size - 1 || y < 0 || z > size - 1 || z < 0)
            {

               return true;

            }
            if (blockdata[x > size - 1 ? size - 1 : x < 0 ? 0 : x, y > size - 1 ? size - 1 : y < 0 ? 0 : y, z > size - 1 ? size - 1 : z < 0 ? 0 : z] == Blocks.GetIDFromBlock(Blocks.Air))
            {

                return true;

            }

            return false;

        }
        public bool IsAllAir()
        {

            foreach (int id in blockdata)
            {

                /* if (id != Blocks.GetIDFromBlock(Blocks.Air))
                {

                    return false;

                }

            }

            return true;

        }

        public bool IsFilled()
        {

            foreach (int id in blockdata)
            {

                /* if (id == Blocks.GetIDFromBlock(Blocks.Air))
                {

                    return false;

                }

            }

            return true;

        }
        public void Draw(Shader shader, Camera camera, float time)
        {

            // Console.WriteLine(vao);

            shader.Use();
            GL.Uniform1(GL.GetUniformLocation(shader.id, "atlas"), 0);
            GL.Uniform1(GL.GetUniformLocation(shader.id, "arrays"), 1);
            GL.Uniform3(GL.GetUniformLocation(shader.id, "cameraPosition"), camera.Position);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Globals.AtlasTexture.getID());
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2DArray, Globals.ArrayTexture.TextureID);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "model"), true, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "view"), true, ref camera.ViewMatrix);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "projection"), true, ref camera.ProjectionMatrix);
            // GL.Uniform3(GL.GetUniformLocation(shader.getID(), "cpos"), ref ChunkPosition);
            GL.Uniform1(GL.GetUniformLocation(shader.getID(), "time"), (float)time);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, MeshData.Length);
            GL.BindVertexArray(0);
            shader.UnUse();

        }
        public void Save(string pathToWrite)
        {
            Console.WriteLine("saving...");
            Stopwatch elapsed = Stopwatch.StartNew();
            using (BinaryWriter bwr = new BinaryWriter(File.OpenWrite(pathToWrite)))
            {

                for (int x = 0; x < size; x++)
                {

                    for (int y = 0; y < size; y++)
                    {

                        for (int z = 0; z < size; z++)
                        {

                            byte[] bytes = new byte[4];
                            bytes[0] = (byte)blockdata[x, y, z];
                            byte b = (byte)(0b11100000 | BitConverter.GetBytes(x)[0] & 0b00011111);
                            bytes[1] =
                            bytes[2] = 0b00000000;
                            bytes[3] = 0b00000000;
                            // bwr.Write(blockdata[x, y, z]);
                            // File.write
                            // newlist.Add(blockdata[x, y, z].ToString());
                            bwr.Write((UInt16)blockdata[x, y, z]);
                        }

                    }

                }

            }
            elapsed.Stop();
            TimeSpan elapsedtime = elapsed.Elapsed;
            Console.WriteLine("Saved in " + elapsedtime.TotalSeconds + " seconds.");

        }
        public void Load(string pathToRead)
        {

            Console.WriteLine("loading data...");
            Stopwatch elapsed = Stopwatch.StartNew();
            using (BinaryReader br = new BinaryReader(File.OpenRead(pathToRead)))
            {

                for (int x = 0; x < size; x++)
                {

                    for (int y = 0; y < size; y++)
                    {

                        for (int z = 0; z < size; z++)
                        {

                            blockdata[x, y, z] = br.ReadUInt16();



                        }

                    }

                }

            }
            elapsed.Stop();
            TimeSpan elapsedtime = elapsed.Elapsed;
            Console.WriteLine("Loaded in " + elapsedtime.TotalSeconds + " seconds.");
            Console.WriteLine("meshing...");

            // GenerateChunkMesh();

        }
        public void Rewrite()
        {
            // ushort e = 10;

            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            // vbo = Vbo.Generate(blockvertdata, BufferUsageHint.DynamicDraw);
            // vao = Vao.Generate(AttribPointerMode.Chunk);
            Vbo.Unbind();
            Vao.Unbind();

        }

        */

    }
}
