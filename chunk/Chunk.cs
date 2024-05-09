using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using opentk_proj.block;
using System.IO;
using opentk_proj.util;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Linq;
using System.ComponentModel;
using System.Security.Cryptography;

namespace opentk_proj.chunk
{
    public struct ChunkVertex
    {

        public int ID;
        public float AmbientValue;
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinates;
        public ChunkVertex(ushort id, byte x, byte y, byte z, float u, float v, float nx, float ny, float nz)
        {

            ID = id;
            Position = (x, y, z);
            TextureCoordinates = (u, v);
            Normal = (nx, ny, nz);
            AmbientValue = 1.0f;

        }

    }

    public enum GenerationState
    {

        NotGenerated,
        PassOne,
        PassTwo,
        Generated

    }
    public enum ChunkState
    {

        NotReady,
        Ready

    }
    public enum MeshState
    {

        NotMeshed,
        Meshing,
        Done

    }
    internal class Chunk
    {

        // size of the chunk, keep at 32, but you CAN change it. (dont)
        static int size = Globals.ChunkSize;
        // original block data, in integers, resulting of the blocktype
        // to lookup in a certain coordinate of xyz in the array
        public int[,,] blockdata = new int[size, size, size];
        public int[,,] LightData = new int[size, size, size];
        // public int[,,] Empty = new int[size, size, size];   
        // vertex data of the chunk from the blockdata.
        // This is what is written to after blockvertdataarray gets changed to an array.
        // you technically don't need this, but it's here for now. (change later)
        public ChunkVertex[] MeshData = new ChunkVertex[0];
        // this is the arraylist of the vertex data of the whole chunk, 
        // which gets turned into an array declared as blockvertdata.
        // You technically only need this, but there's the blockvertdata
        // for now. (change later)
        public List<ChunkVertex> MeshDataList = new List<ChunkVertex>();

        // Vertex Buffer Object of the chunk.
        public int vbo;
        // Vertex Array Object of the chunk.
        public int vao;

        // Model matrix of the chunk
        public Matrix4 model;

        public int cx; // chunk x position
        public int cy; // chunk y position
        public int cz; // chunk z position

        public Vector3 ChunkPosition; // vector of cx, cy, cz

        Random r = new Random();

        FastNoiseLite noise = new FastNoiseLite();

        ChunkState ChunkState = ChunkState.NotReady;
        MeshState MeshState = MeshState.NotMeshed;
        GenerationState GenerationState = GenerationState.NotGenerated;

        public bool IsSent = false;

        public Chunk(int x, int y, int z)
        {

            // Console.WriteLine("init");
            // Stopwatch elapsed = Stopwatch.StartNew();
            // TimeSpan elapsedtime;
            cx = x;
            cy = y;
            cz = z;
            ChunkPosition = (cx, cy, cz);
            // ThreadPool.QueueUserWorkItem(new WaitCallback(GenerateThreaded));

            model = Matrix4.CreateTranslation(x * size, y * size, z * size);

        }

        public void UpdateChunk()
        {

            if (ChunkState == ChunkState.Ready && !IsSent)
            {

                ProcessToRender();
                IsSent = true;

            }

        }

        public void GenerateThreaded(object obj)
        {

            ChunkState = ChunkState.NotReady;
            InitializeData();
            GenerateMesh();
            ChunkState = ChunkState.Ready;

        }

        public void Generate()
        {

            // Console.WriteLine("For Chunk at {3} | ChunkState is {0}, GenerationState is {1}, MeshState is {2}", ChunkState, GenerationState, MeshState, ChunkPosition);

            ChunkState = ChunkState.NotReady;
            if (GenerationState == GenerationState.Generated)
            {

                if (IsAllAir() || IsFilled())
                {



                } else
                {

                    GenerateMesh();
                    ProcessToRender();

                }
                ChunkState = ChunkState.Ready;

            }
            if (GenerationState == GenerationState.PassTwo)
            {
                
                // GeneratePassTwo();
                // GenerationState = GenerationState.Generated;

            }
            if (GenerationState == GenerationState.NotGenerated)
            {

                // GeneratePassOne();
                // GenerationState = GenerationState.PassTwo;

            }
            if (GenerationState == GenerationState.NotGenerated)
            {

                InitializeData();
                // PropegateLightValues();
                GenerationState = GenerationState.Generated;

            }

        }
        private float GetNoise2D(int octaves, int x, int y)
        {

            // int octaves = 3;
            float value = 0;

            float xValue = (float)x + (cx * size);
            float yValue = (float)y + (cz * size);

            // for (float i = 1; i <= octaves; i++)
            for (float i = 1; i <= octaves; i++)
            {

                float noiseValue = (noise.GetNoise(xValue / (4 / (i * 4)) + noise.GetNoise(xValue / 8, yValue / 8), yValue / (4 / (i * 4))) - noise.GetNoise(xValue / 3, yValue / 7)) * (((noise.GetNoise(xValue / 64, yValue / 64))) * 2);

                value += (noiseValue / 2) + 0.5f;

            }

            return value;

        }
        private float GetNoise3D(int x, int y, int z)
        {

            int octaves = 1;
            float value = 1;

            float xValue = (float)x + (cx * size);
            float yValue = (float)y + (cy * size);
            float zValue = (float)z + (cz * size);

            for (float i = 0; i <= octaves; i++)
            {

                value *= noise.GetNoise(xValue * (2 * (1 + i)), yValue * (2 * (1 + i)), zValue * (2 * (1 + i))) / (1 + i);

            }
            value /= (octaves);

            return (value / 2) + 0.5f;

        }
        private void InitializeData()
        {

            GenerationState = GenerationState.PassOne;
            // for (int x = 0; x < size; x++)
                Parallel.For(0, size, x =>
                {

                    for (int y = 0; y < size; y++)
                    {

                        for (int z = 0; z < size; z++)
                        {

                            int globalX = x + (cx * size);
                            int globalZ = z + (cz * size);
                            int globalY = y + (cy * size);

                            float value = Maths.MapValueToMinMax(GetNoise2D(3, x, z), 27, 105);

                            // SetBlock(Blocks.Stone, x,y,z);

                            if (globalY <= value - r.Next(4, 10))
                            {

                                SetBlockGlobal(Blocks.Stone, globalX, globalY, globalZ);

                            }
                            else if (globalY < value - 1)
                            {

                                SetBlockGlobal(Blocks.Dirt, globalX, globalY, globalZ);

                            }
                            else if (globalY <= value)
                            {

                                SetBlockGlobal(Blocks.Grass, globalX, globalY, globalZ);

                            }

                            // SetBlockGlobal(Blocks.Grass, globalX, globalX, globalZ);

                            // SetBlockGlobal(Blocks.Stone, globalX, (int) value, globalZ);
                            // SetBlockGlobal(Blocks.Dirt, globalX, 0, 0);
                            // SetBlockGlobal(Blocks.Dirt, 0, 0, globalZ);

                            // SetBlockGlobal(Blocks.Dirt, 0, 15, 0);

                            // SetBlock(Blocks.Dirt, 0, 0, 0);

                            /* if (globalY < offset)
                            {

                                SetBlock(Blocks.Stone, x, y, z);

                            } else
                            {

                                if (GetNoise3D(x,y,z) > (globalY) / (maxHeight))
                                {

                                    // SetBlock(Blocks.Stone, x, y, z);
                                    // SetBlock(Blocks.Stone, x, y, z);

                                    // SetBlock(Blocks.Stone, x, y, z);
                                    // SetBlockGlobal(Blocks.Stone, x+(cx*size), 36, z+(cz*size));
                                    SetBlock(Blocks.Stone, x, y, z);

                                }
                                else
                                {

                                    // SetBlock(Blocks.Air, x, y, z);

                                }

                            } */
                            // SetBlockGlobal(Blocks.Grass, 0, 0, 0);
                            // SetBlockGlobal(Blocks.Grass, 0, (int)offset, 0);
                            // SetBlockGlobal(Blocks.Grass, 0, (int)(offset+maxHeight), 0);


                        }

                    }

                });
            GenerationState = GenerationState.Generated;

        }
        private void GenerateMesh()
        {

            MeshState = MeshState.Meshing;
            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        // Console.WriteLine("{0}, {1}, {2}", x, y, z);

                        if (blockdata[x, y, z] != Blocks.GetIDFromBlock(Blocks.Air))
                        {

                            InsertBlock(Blocks.BlockList[blockdata[x, y, z]], x, y, z);

                        }
                        // InsertBlock(Blocks.BlockList[blockdata[x,y,z]], x,y,z);
                        // MeshDataList.AddRange(Block.GetFaceShifted(Blocks.Dirt.FrontFace, x, y, z));
                        // MeshDataList.AddRange(Block.GetFaceShifted(Blocks.Dirt.RightFace, x, y, z));
                        // MeshDataList.AddRange(Block.GetFaceShifted(Blocks.Dirt.BackFace, x, y, z));
                        // MeshDataList.AddRange(Block.GetFaceShifted(Blocks.Dirt.LeftFace, x, y, z));
                        // MeshDataList.AddRange(Block.GetFaceShifted(Blocks.Dirt.TopFace, x, y, z));
                        // MeshDataList.AddRange(Block.GetFaceShifted(Blocks.Dirt.BottomFace, x, y, z));

                    }

                }

            }
            MeshState = MeshState.Done;

        }

        private void PropegateLightValues()
        {

            Vector3 lightPosition = (16, 30, 16);

            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        // LightData[x, y, z] = x;
                        Vector3 positionNormalized = Vector3.Divide((x,y,z), size);
                        Vector3 lightPositionNormalized = Vector3.Divide(lightPosition, size);
                        LightData[x, y, z] = (int)(size * (1 - Vector3.Distance(lightPositionNormalized, positionNormalized)));

                    }

                }

            }

        }
        private void ProcessToRender()
        {

            MeshData = MeshDataList.ToArray();
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, MeshData.Length * Marshal.SizeOf<ChunkVertex>(), MeshData, BufferUsageHint.DynamicDraw);

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.VertexAttribIPointer(0, 1, VertexAttribIntegerType.Int, Marshal.SizeOf<ChunkVertex>(), (IntPtr)0);
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

        public ChunkState GetChunkState()
        {

            return ChunkState;

        }

        public MeshState GetMeshState()
        {

            return MeshState;

        }

        public GenerationState GetGenerationState()
        {

            return GenerationState;

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

        public int GetLightData(int x, int y, int z)
        {

            if (x < 0 || y < 0 || z < 0 || x >= size || y >= size || z >= size)
            {

                return 0;

            }
            return LightData[x, y, z];

        }
        public Block GetBlock(int x, int y, int z)
        {

            return Blocks.GetBlockFromID(blockdata[x, y, z]);

        }
        public void SetBlock(Block block, int x, int y, int z)
        {

            blockdata[x, y, z] = Blocks.GetIDFromBlock(block);

        }
        public void InsertBlock(Block block, int x, int y, int z)
        {

            if (CheckAir(x,y,z+1))
            {

                MeshDataList.AddRange(Block.GetFaceShifted(block.BackFace, x, y, z, GetLightData(x,y,z+1)));

            }
            if (CheckAir(x,y, z-1))
            {

                MeshDataList.AddRange(Block.GetFaceShifted(block.FrontFace, x, y, z,GetLightData(x,y,z-1)));

            }

            if (CheckAir(x+1, y, z))
            {

                MeshDataList.AddRange(Block.GetFaceShifted(block.LeftFace, x, y, z,GetLightData(x+1,y,z)));

            }
            if (CheckAir(x-1, y, z))
            {

                MeshDataList.AddRange(Block.GetFaceShifted(block.RightFace, x, y, z,GetLightData(x-1,y,z)));

            }

            if (CheckAir(x, y+1, z))
            {

                MeshDataList.AddRange(Block.GetFaceShifted(block.TopFace, x, y, z,GetLightData(x,y+1,z)));

            }
            if (CheckAir(x, y-1, z))
            {

                MeshDataList.AddRange(Block.GetFaceShifted(block.BottomFace, x, y, z,GetLightData(x,y-1,z)));

            }


        }
        public bool CheckAir(int x, int y, int z)
        {

            if (x > size - 1 || x < 0 || y > size - 1 || y < 0 || z > size - 1 || z < 0)
            {

                return true;

            }
            if (blockdata[x > size-1 ? size-1 : x < 0 ? 0 : x,y > size-1 ? size-1 : y < 0 ? 0 : y,z > size - 1 ? size-1 : z < 0 ? 0 : z] == Blocks.GetIDFromBlock(Blocks.Air))
            {

                return true;

            }

            return false;

        }
        public bool IsAllAir()
        {

            foreach (int id in blockdata)
            {

                if (id != Blocks.GetIDFromBlock(Blocks.Air))
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

                if (id == Blocks.GetIDFromBlock(Blocks.Air))
                {

                    return false;

                }

            }

            return true;

        }
        public void Draw(Shader shader, Camera camera, float time)
        {

            //shader.Use();
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "model"), true, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "view"), true, ref camera.view);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "projection"), true, ref camera.projection);
            // GL.Uniform3(GL.GetUniformLocation(shader.getID(), "cpos"), ref ChunkPosition);
            GL.Uniform1(GL.GetUniformLocation(shader.getID(), "time"), (float)time);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, MeshData.Length);
            GL.BindVertexArray(0);

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
    }
}
