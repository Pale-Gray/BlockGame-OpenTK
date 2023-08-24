using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using opentk_proj.block;
using StbImageSharp;
using System.IO;
using OpenTK.Graphics;
using StbImageWriteSharp;
using opentk_proj.util;

namespace opentk_proj.chunk
{
    internal class Chunk
    {

        static int size = 32;

        public int[,,] blockdata = new int[size, size, size];
        public float[] blockvertdata;
        public ArrayList blockvertdataarray = new ArrayList();
        public float[] reffront = new float[9 * 6];

        public int vbo;
        public int vao;

        public Matrix4 model;
        public Matrix4 projection;

        public int amt = 0;

        public int cx;
        public int cy;
        public int cz;

        public int i;
        public byte[] pixels = new byte[(640 * 480) * 4];

        Vector3 cpos;
        Texture tx;


        public Chunk(int x, int y, int z)
        {

            // Texture tex = new Texture("../../../res/textures/testatlas.png");

            // Texture t = new Texture("../../../res/textures/portiontest.png");
            // tx = Texture.GetPortion(t, 1, 1, 24, 24);

            cx = x;
            cy = y;
            cz = z;

            cpos = new Vector3(x, y, z);

            initialize();

            /* vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, blockvertdata.Length * sizeof(float), blockvertdata, BufferUsageHint.DynamicDraw);
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.VertexAttribPointer(0, 1, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0 * sizeof(float)); // this is the blocktype data
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 1 * sizeof(float)); // this is the vertices
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 4 * sizeof(float)); // this is the normals
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 7 * sizeof(float)); // UVs 
            GL.EnableVertexAttribArray(3);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0); */
            vbo = Vbo.Generate(blockvertdata, BufferUsageHint.DynamicDraw);
            vao = Vao.Generate(AttribPointerMode.Chunk);
            Vbo.Unbind();
            Vao.Unbind();

            model = Matrix4.CreateTranslation(x * size, y * size, z * size);
            // projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)Constants.WIDTH / (float)Constants.HEIGHT, 0.1f, 100.0f);

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

            Console.WriteLine("saving chunk data to {0}", pathToWrite.Split("/")[5]);
            Stream writeTo = File.Open(pathToWrite, FileMode.Create);
            BinaryWriter bwr = new BinaryWriter(writeTo);
            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        bwr.Write((UInt16)blockdata[x, y, z]);

                    }

                }

            }
            Console.WriteLine("saved.");

        }
        public void Load(string pathToRead)
        {
            
            Console.WriteLine("loading chunk data from {0}", pathToRead.Split("/")[5]);
            Stream readFrom = File.Open(pathToRead, FileMode.Open);
            BinaryReader br = new BinaryReader(readFrom);
            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        blockdata[x,y,z] = br.ReadUInt16();

                    }

                }

            }
            meshgen();
            Rewrite();

            // (zindex * size * size + (yindex * size + xindex))

            Console.WriteLine("loaded data.");

        }
        public void Rewrite()
        {

            GL.DeleteBuffer(vbo);
            /*vbo = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, blockvertdata.Length * sizeof(float), blockvertdata, BufferUsageHint.DynamicDraw);

            GL.BindVertexArray(vao);
            GL.VertexAttribPointer(0, 1, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0 * sizeof(float)); // this is the blocktype data
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 1 * sizeof(float)); // this is the vertices
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 4 * sizeof(float)); // this is the normals
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 7 * sizeof(float)); // UVs 
            GL.EnableVertexAttribArray(3);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0); */
            vbo = Vbo.Generate(blockvertdata, BufferUsageHint.DynamicDraw);
            vao = Vao.Generate(AttribPointerMode.Chunk);
            Vbo.Unbind();
            Vao.Unbind();

        }
        public void initialize()
        {

            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        int ccx = (int)Math.Round(Maths.Lerp(0, 31, (float)new Random().NextDouble()));
                        int ccz = (int)Math.Round(Maths.Lerp(0, 31, (float)new Random().NextDouble()));

                        blockdata[x, y > 2 ? 2 : y, z] = Blocks.Dirt.ID;
                        blockdata[x, 3, z] = Blocks.Grass.ID;

                        if ((int) Maths.Dist3D(x,3,z,15,3,15) <= 8)
                        {

                            blockdata[x, 3, z] = Blocks.Temp_Water.ID;

                        }

                    }

                }

            }

            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        if (blockdata[x,3,z] == Blocks.Temp_Water.ID)
                        {

                            if (blockdata[x-1,3,z] != Blocks.Temp_Water.ID)
                            {

                                blockdata[x - 1, 3, z] = Blocks.Sand.ID;
                                blockdata[x - 2, 3, z] = Blocks.Sand.ID;

                            }
                            if (blockdata[x + 1, 3, z] != Blocks.Temp_Water.ID)
                            {

                                blockdata[x + 1, 3, z] = Blocks.Sand.ID;
                                blockdata[x + 2, 3, z] = Blocks.Sand.ID;

                            }
                            if (blockdata[x, 3, z-1] != Blocks.Temp_Water.ID)
                            {

                                blockdata[x, 3, z-1] = Blocks.Sand.ID;
                                blockdata[x, 3, z-2] = Blocks.Sand.ID;

                            }
                            if (blockdata[x, 3, z+1] != Blocks.Temp_Water.ID)
                            {

                                blockdata[x, 3, z+1] = Blocks.Sand.ID;
                                blockdata[x, 3, z+2] = Blocks.Sand.ID;

                            }

                        }

                    }

                }

            }

            meshgen();

        }
        public void meshgen()
        {

            blockvertdataarray.Clear();

            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        if (blockdata[x, y, z] != Blocks.Air.GetID())
                        {

                            // operators are flipped on z because z forward is negative (z back is positive

                            if (blockdata[x, y, z - 1 < 0 ? z : z - 1] == 0) { backface(x, y, z); }
                            if (blockdata[x, y, z] != 0 && z == 0) { backface(x, y, z); } // edge detect

                            if (blockdata[x, y, z + 1 > size - 1 ? z : z + 1] == 0) { frontface(x, y, z); }
                            if (blockdata[x, y, z] != 0 && z == size - 1) { frontface(x, y, z); } // edge detect

                            if (blockdata[x - 1 < 0 ? x : x - 1, y, z] == 0) { leftface(x, y, z); }
                            if (blockdata[x, y, z] != 0 && x == 0) { leftface(x, y, z); } // edge detect

                            if (blockdata[x + 1 > size - 1 ? x : x + 1, y, z] == 0) { rightface(x, y, z); }
                            if (blockdata[x, y, z] != 0 && x == size - 1) { rightface(x, y, z); } // edge detect

                            if (blockdata[x, y - 1 < 0 ? y : y - 1, z] == 0) { bottomface(x, y, z); }
                            if (blockdata[x, y, z] != 0 && y == 0) { bottomface(x, y, z); } // edge detect

                            if (blockdata[x, y + 1 > size - 1 ? y : y + 1, z] == 0) { topface(x, y, z); }
                            if (blockdata[x, y, z] != 0 && y == size - 1) { topface(x, y, z); } // edge detect

                        }

                    }

                }

            }

            Console.WriteLine("Chunk Mesh Vertex Count: {0}", blockvertdataarray.Count / 9);

            blockvertdata = (float[])blockvertdataarray.ToArray(typeof(float));
            
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
        public static int[,,] LoadFromFile()
        {

            
            return new int[5,5,5];

        }
        public void SetBlockId(int x, int y, int z, int ID)
        {

            blockdata[x, y, z] = ID;
            meshgen();
            Rewrite();

        }
        // note that z-forward is negative 
        public void frontface(int x, int y, int z)
        {

            for (int i = 0; i < reffront.Length; i += 9)
            {

                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reffront[i]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reffront[i + 1] + 1 * x);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reffront[i + 2] + 1 * y);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reffront[i + 3] + 1 * z);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reffront[i + 4]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reffront[i + 5]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reffront[i + 6]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reffront[i + 7]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reffront[i + 8]);

            }

        }
        public void rightface(int x, int y, int z)
        {

            for (int i = 0; i < reffront.Length; i += 9)
            {

                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refright[i]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refright[i + 1] + 1 * x);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refright[i + 2] + 1 * y);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refright[i + 3] + 1 * z);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refright[i + 4]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refright[i + 5]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refright[i + 6]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refright[i + 7]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refright[i + 8]);

            }

        }
        public void backface(int x, int y, int z)
        {

            for (int i = 0; i < reffront.Length; i += 9)
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

            }

        }
        public void leftface(int x, int y, int z)
        {

            for (int i = 0; i < reffront.Length; i += 9)
            {

                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refleft[i]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refleft[i + 1] + 1 * x);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refleft[i + 2] + 1 * y);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refleft[i + 3] + 1 * z);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refleft[i + 4]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refleft[i + 5]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refleft[i + 6]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refleft[i + 7]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refleft[i + 8]);

            }

        }
        public void topface(int x, int y, int z)
        {

            for (int i = 0; i < reffront.Length; i += 9)
            {

                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reftop[i]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reftop[i + 1] + 1 * x);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reftop[i + 2] + 1 * y);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reftop[i + 3] + 1 * z);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reftop[i + 4]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reftop[i + 5]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reftop[i + 6]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reftop[i + 7]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reftop[i + 8]);

            }

        }
        public void bottomface(int x, int y, int z)
        {

            for (int i = 0; i < reffront.Length; i += 9)
            {

                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 1] + 1 * x);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 2] + 1 * y);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 3] + 1 * z);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 4]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 5]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 6]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 7]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 8]);

            }

        }

    }
}
