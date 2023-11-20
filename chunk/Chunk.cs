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

        // size of the chunk, keep at 32, but you CAN change it. (dont)
        static int size = 32;

        // original block data, in integers, resulting of the blocktype
        // to lookup in a certain coordinate of xyz in the array
        public int[,,] blockdata = new int[size, size, size];
        // vertex data of the chunk from the blockdata.
        // This is what is written to after blockvertdataarray gets changed to an array.
        // you technically don't need this, but it's here for now. (change later)
        public float[] blockvertdata;
        // this is the arraylist of the vertex data of the whole chunk, 
        // which gets turned into an array declared as blockvertdata.
        // You technically only need this, but there's the blockvertdata
        // for now. (change later)
        public ArrayList blockvertdataarray = new ArrayList();

        // I don't even know what this is used for anymore.
        public float[] reffront = new float[9 * 6];

        // Vertex Buffer Object of the chunk.
        public int vbo;
        // Vertex Array Object of the chunk.
        public int vao;

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
            bwr.Dispose();
            Console.WriteLine("saved.");

        }
        public void Load(string pathToRead)
        {
            
            Console.WriteLine("loading chunk data from {0}", pathToRead.Split("/")[5]);
            Stream readFrom = File.Open(pathToRead, FileMode.Open);
            BinaryReader br = new BinaryReader(readFrom);
            Console.WriteLine("filesize of chunk data in bytes: {0}", readFrom.Length);
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
            br.Dispose();
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

            // This is really bad. Fix it.
            // This for loop is to generate the blockdata numbers for mesh generation. 
            // Pretty important, right?
            // yes.
            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        blockdata[x, y, z] = Blocks.Sand.ID;

                    }

                }

            }

            meshgen();

        }
        public void meshgen()
        {

            // makes the mesh of the chunk. VERY LONG NEED TO OPTIMIZE.

            // clear arraylist just in case.
            blockvertdataarray.Clear();


            // MASSIVE for loop. makes all the mesh data :)
            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        if (blockdata[x, y, z] != Blocks.Air.GetID())
                        {

                            // operators are flipped on z because z forward is negative (z back is positive

                            // if (blockdata[x, y, z - 1 <= 0 ? z : z - 1] == 0) { backface(x, y, z); }
                            // if (blockdata[x, y, z] != 0 && z == 0) { blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refback); } // edge detect
                            blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refback);

                            //if (blockdata[x, y, z + 1 > size - 1 ? z : z + 1] == 0) { frontface(x, y, z); }
                            //if (blockdata[x, y, z] != 0 && z == size - 1) { frontface(x, y, z); } // edge detect

                            //if (blockdata[x - 1 < 0 ? x : x - 1, y, z] == 0) { leftface(x, y, z); }
                            //if (blockdata[x, y, z] != 0 && x == 0) { leftface(x, y, z); } // edge detect

                            //if (blockdata[x + 1 > size - 1 ? x : x + 1, y, z] == 0) { rightface(x, y, z); }
                            //if (blockdata[x, y, z] != 0 && x == size - 1) { rightface(x, y, z); } // edge detect

                            //if (blockdata[x, y - 1 < 0 ? y : y - 1, z] == 0) { bottomface(x, y, z); }
                            //if (blockdata[x, y, z] != 0 && y == 0) { bottomface(x, y, z); } // edge detect

                            //if (blockdata[x, y + 1 > size - 1 ? y : y + 1, z] == 0) { topface(x, y, z); }
                            //if (blockdata[x, y, z] != 0 && y == size - 1) { topface(x, y, z); } // edge detect

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
