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

namespace opentk_proj.chunk
{
    internal class Chunk
    {

        private static object lockerWrite = new Object();
        private static object lockerRead = new Object();
        // size of the chunk, keep at 32, but you CAN change it. (dont)
        static int size = Constants.ChunkSize;

        object lockObject = new object();

        public volatile float[,,] noiseValues = new float[size,size,size];

        // original block data, in integers, resulting of the blocktype
        // to lookup in a certain coordinate of xyz in the array
        public volatile int[,,] blockdata = new int[size, size, size];
        // public int[,,] Empty = new int[size, size, size];   
        // vertex data of the chunk from the blockdata.
        // This is what is written to after blockvertdataarray gets changed to an array.
        // you technically don't need this, but it's here for now. (change later)
        public volatile float[] blockvertdata;
        // this is the arraylist of the vertex data of the whole chunk, 
        // which gets turned into an array declared as blockvertdata.
        // You technically only need this, but there's the blockvertdata
        // for now. (change later)
        public volatile List<float> blockvertdataarray = new List<float>();

        // I don't even know what this is used for anymore.
        public float[] reffront = new float[9 * 6];

        // Vertex Buffer Object of the chunk.
        public int vbo;
        // Vertex Array Object of the chunk.
        public int vao;

        public volatile bool IsReady = false;

        // Model matrix of the chunk
        public Matrix4 model;
        // Projection matrix of the chunk (do I really need this? [probably not.])
        // Spoiler, you don't.
        // public Matrix4 projection;

        public int cx; // chunk x position
        public int cy; // chunk y position
        public int cz; // chunk z position

        Vector3 cpos; // vector of cx, cy, cz
        // Texture tx;

        public Chunk(int x, int y, int z)
        {
            Stopwatch elapsed = Stopwatch.StartNew();
            cx = x;
            cy = y;
            cz = z;

            // Thread t = new Thread(() => initialize());

            //  GenerateBlockData();
            Thread thread = new Thread(() => {

                IsReady = false;
                GenerateNoiseValues(12345);
                GenerateBlockData();
                GenerateChunkMesh();
                IsReady = true;

            });
            thread.Start();

            vbo = Vbo.Generate(blockvertdata, BufferUsageHint.DynamicDraw);
            vao = Vao.Generate(AttribPointerMode.Chunk);
            Vbo.Unbind();
            Vao.Unbind();

            model = Matrix4.CreateTranslation(x * size, y * size, z * size);
            elapsed.Stop();
            TimeSpan elapsedtime = elapsed.Elapsed;
            Console.WriteLine("Made a chunk in " + elapsedtime.TotalSeconds + " seconds.");
        }

        public Chunk(string pathtosave)
        {

            int[] cposfromfile = ChunkLoader.GetChunkPositionFromFile(pathtosave);

            cx = cposfromfile[0];
            cy = cposfromfile[1];
            cz = cposfromfile[2];
            cpos = new Vector3(cx, cy, cz);

            if (!File.Exists(pathtosave))
            {

                GenerateBlockData();
                

            } else
            {

                Load(pathtosave);

            }
            // meshgen();
            vbo = Vbo.Generate(blockvertdata, BufferUsageHint.DynamicDraw);
            //  = Vbo.Generate(blockvertdata, BufferUsageHint.DynamicDraw);
            vao = Vao.Generate(AttribPointerMode.Chunk);
            Vbo.Unbind();
            Vao.Unbind();

            model = Matrix4.CreateTranslation(cx * size, cy * size, cz * size);

        }
        public void GenerateNoiseValues(int seed)
        {
            // FastNoise simplex = new FastNoise("Simplex");
            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        noiseValues[x, y, z] = OpenSimplex2.Noise3_Fallback(seed, (x + (cx * size)) / 32f, (y + (cy * size)) / 32f, (z + (cz * size)) / 32f);
                        // noiseValues[x, y, z] = 4;

                    }

                }

            }

        }
        public void Draw(Shader shader, Camera camera, float time)
        {

            // GL.ActiveTexture(TextureUnit.Texture0);
            // GL.BindTexture(TextureTarget.Texture2D, tx.id);
            // shader.Use();
            
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "model"), true, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "view"), true, ref camera.view);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "projection"), true, ref camera.projection);
            GL.Uniform3(GL.GetUniformLocation(shader.getID(), "cpos"), ref cpos);
            GL.Uniform1(GL.GetUniformLocation(shader.getID(), "time"), (float)time);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, blockvertdata.Length);
            GL.BindVertexArray(0);
            
            // shader.UnUse();
            // GL.BindTexture(TextureTarget.Texture2D, 0);

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

                            // bwr.Write(blockdata[x, y, z]);
                            // File.write
                            // newlist.Add(blockdata[x, y, z].ToString());
                            bwr.Write((UInt16)blockdata[x,y,z]);
                        }

                    }

                }

            }
            elapsed.Stop();
            TimeSpan elapsedtime = elapsed.Elapsed;
            Console.WriteLine("Saved in " + elapsedtime.TotalSeconds + " seconds.");

        }
        public async void Load(string pathToRead)
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

            GenerateChunkMesh();
          
        }
        public void Rewrite()
        {

            GL.DeleteBuffer(vbo);
            vbo = Vbo.Generate(blockvertdata, BufferUsageHint.DynamicDraw);
            vao = Vao.Generate(AttribPointerMode.Chunk);
            Vbo.Unbind();
            Vao.Unbind();

        }
        public void GenerateBlockData()
        {

            // This is really bad. Fix it.
            // This for loop is to generate the blockdata numbers for mesh generation. 
            // Pretty important, right?
            // yes.
            // Console.WriteLine("initializing...");
            // Stopwatch elapsed = Stopwatch.StartNew();
            blockvertdataarray.Clear();

            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        float xpos = (float)(x + cx * size);
                        float ypos = (float)(y + cy * size);
                        float zpos = (float)(z + cz * size);

                        blockdata[x, y, z] = 1;// noiseValues[x, y, z] < 0.3f ? Blocks.Air.ID : Blocks.Grass.ID;
                        blockdata[0, 0, 0] = 0;
                        blockdata[size-1,0,0] = 0;
                        blockdata[0, size - 1, 0] = 0;
                        blockdata[0,0, size - 1] = 0;
                        blockdata[size - 1, size - 1, 0] = 0;
                        blockdata[size - 1, 0, size - 1] = 0;
                        blockdata[0, size - 1, size - 1] = 0;
                        blockdata[size - 1, size - 1, size - 1] = 0;
                    }

                }

            }
            // elapsed.Stop();
            // TimeSpan elapsedtime = elapsed.Elapsed;
            // Console.WriteLine("Finished intializing in " + elapsedtime.TotalSeconds + " seconds.");

        }
        public void GenerateChunkMesh()
        {

            // makes the mesh of the chunk. VERY LONG NEED TO OPTIMIZE.
            // generating = true;
            // clear arraylist just in case.
            // Console.WriteLine("meshing...");
            // Stopwatch elapsed = Stopwatch.StartNew();

            blockvertdataarray.Clear();

            // MASSIVE for loop. makes all the mesh data :)
            // There's got to be a better way to do this.

            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        if (blockdata[x, y, z] != Blocks.Air.GetID())
                        {

                            // operators are flipped on z because z forward is negative (z back is positive


                            // These conditionals look very ugly, but I'll explain.
                            // The left side of the conditional (before the OR operator) checks regularly if the block around it is air.
                            // The right side of the conditional (after the OR operator) checks the edges of the chunk for air.
                            if (blockdata[x, y, z - 1 < 0 ? z : z - 1] == 0 || blockdata[x, y, z] != 0 && z == 0) { backface(x, y, z); }
                            if (blockdata[x, y, z + 1 > size - 1 ? z : z + 1] == 0 || blockdata[x, y, z] != 0 && z == size - 1) { frontface(x, y, z); }
                            if (blockdata[x - 1 < 0 ? x : x - 1, y, z] == 0 || blockdata[x, y, z] != 0 && x == 0) { leftface(x, y, z); }
                            if (blockdata[x + 1 > size - 1 ? x : x + 1, y, z] == 0 || blockdata[x, y, z] != 0 && x == size - 1) { rightface(x, y, z); }
                            if (blockdata[x, y - 1 < 0 ? y : y - 1, z] == 0 || blockdata[x, y, z] != 0 && y == 0) { bottomface(x, y, z); }
                            if (blockdata[x, y + 1 > size - 1 ? y : y + 1, z] == 0 || blockdata[x, y, z] != 0 && y == size - 1) { topface(x, y, z); }



                        }

                    }

                }

            }

            // Console.WriteLine("Chunk Mesh Vertex Count: {0}", blockvertdataarray.Count / 9);
            // Console.WriteLine("Chunks Mesh float count: {0}", blockvertdataarray.Count);
            // elapsed.Stop();
            // TimeSpan elapsedtime = elapsed.Elapsed;
            // Console.WriteLine("Finished meshing in " + elapsedtime.TotalSeconds + " seconds.");

            // blockvertdata = (float[])blockvertdataarray.ToArray(typeof(float));
            // blockvertdata = blockvertdataarray.ToArray();
            // blockvertdata = MemoryMarshal.AsBytes();
            // blockvertdata = blockvertdataarray;
            blockvertdata = blockvertdataarray.ToArray();// CollectionsMarshal.AsSpan(blockvertdataarray);
            // MemoryMarshal.Cast<List<float>, float[]>(CollectionsMarshal.AsSpan(blockvertdataarray));
            // blockvertdata = MemoryMarshal.
            // MemoryMarshal.TryGetArray(blockvertdataarray, blockvertdata);
            
        }
        public Vector3 getPlayerPositionRelativeToChunk(Vector3 position)
        {

            float x = Math.Max(0, (float)Math.Floor(position.X+0.5f));
            float y = Math.Max(0, (float)Math.Floor(position.Y+0.5f));
            float z = Math.Max(0, (float)Math.Floor(position.Z+0.5f));

            x = Math.Min(size - 1, x);
            y = Math.Min(size - 1, y);
            z = Math.Min(size - 1, z);

            return new Vector3(x, y, z);

        }
        public void SetBlockId(int x, int y, int z, int ID)
        {

            blockdata[x, y, z] = ID;
            GenerateChunkMesh();
            Rewrite();

        }
        // note that z-forward is negative 
        public void frontface(int x, int y, int z)
        {
            
            blockvertdataarray.AddRange(ArrayUtils.BlockFaceShift(Blocks.GetBlockByID(blockdata[x, y, z]).reffront, x, y, z));

        }
        public void rightface(int x, int y, int z)
        {

            blockvertdataarray.AddRange(ArrayUtils.BlockFaceShift(Blocks.GetBlockByID(blockdata[x, y, z]).refright, x, y, z));

        }
        public void backface(int x, int y, int z)
        {
            blockvertdataarray.AddRange(ArrayUtils.BlockFaceShift(Blocks.GetBlockByID(blockdata[x, y, z]).refback, x, y, z));
            /* for (int i = 0; i < reffront.Length; i += 9)
            {

                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refback[i]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refback[i + 1] + 1 * x);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refback[i + 2] + 1 * y);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refback[i + 3] + 1 * z);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refback[i + 4]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refback[i + 5]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refback[i + 6]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refback[i + 7]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refback[i + 8]);

            } */

        }
        public void leftface(int x, int y, int z)
        {

            blockvertdataarray.AddRange(ArrayUtils.BlockFaceShift(Blocks.GetBlockByID(blockdata[x, y, z]).refleft, x, y, z));

        }
        public void topface(int x, int y, int z)
        {

            blockvertdataarray.AddRange(ArrayUtils.BlockFaceShift(Blocks.GetBlockByID(blockdata[x, y, z]).reftop, x, y, z));

        }
        public void bottomface(int x, int y, int z)
        {

            blockvertdataarray.AddRange(ArrayUtils.BlockFaceShift(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom, x, y, z));

        }

    }
}
