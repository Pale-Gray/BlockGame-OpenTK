using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj
{
    internal class Chunk
    {

        static int size = 32;

        int[,,] blockdata = new int[size, size, size];
        float[] blockvertdata = new float[180 * (size * size)];
        float[] reffront =
        {

            0, -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, // front
            0, 0.5f, -0.5f, 0.5f,  0.0f, 0.0f, -1.0f, 1.0f, 0.0f,
            0, 0.5f,  0.5f, 0.5f, 0.0f, 0.0f, -1.0f, 1.0f, 1.0f,
            0, 0.5f,  0.5f, 0.5f, 0.0f, 0.0f, -1.0f, 1.0f, 1.0f,
            0, -0.5f,  0.5f, 0.5f, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f,
            0, -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f

        };
        float[] refright = {

            0, 0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, // right
            0, 0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            0, 0.5f,  0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f,
            0, 0.5f,  0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f,
            0, 0.5f,  0.5f, 0.5f, 1.0f, 0.0f, 0.0f,  0.0f, 1.0f,
            0, 0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f

        };
        float[] refback =
        {

            0, 0.5f, -0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, // back 
            0, -0.5f, -0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f,
            0, -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
            0, -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
            0, 0.5f, 0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f,
            0, 0.5f, -0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f

        };
        float[] refleft =
        {

            0, -0.5f, -0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, // left
            0, -0.5f, -0.5f, 0.5f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            0, -0.5f,  0.5f, 0.5f, -1.0f, 0.0f, 0.0f, 1.0f, 1.0f,
            0, -0.5f,  0.5f, 0.5f, -1.0f, 0.0f, 0.0f,  1.0f, 1.0f,
            0, -0.5f,  0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f,
            0, -0.5f, -0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f

        };
        float[] reftop =
        {

            0, -0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, // top
            0, 0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f,
            0, 0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f,
            0, 0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f,
            0, -0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f,
            0, -0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,

        };
        float[] refbottom =
        {

            0, 0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, // bottom
            0, -0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f,
            0, -0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 1.0f,
            0, -0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 1.0f,
            0, 0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 1.0f,
            0, 0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f

        };

        public int vbo;
        public int vao;

        public Matrix4 model;
        public Matrix4 projection;

        ArrayList blockvertdataarray = new ArrayList();

        public int cx;
        public int cy;
        public int cz;

        Vector3 cpos;
        // public Matrix4 view; doesnt need its always updated

        public Chunk(int x, int y, int z)
        {

            cx = x;
            cy = y;
            cz = z;

            cpos = new Vector3(x, y, z);

            initialize();

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, blockvertdata.Length * sizeof(float), blockvertdata, BufferUsageHint.DynamicCopy);
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
            GL.BindVertexArray(0);

            model = Matrix4.CreateTranslation(x * size, y * size, z * size);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)Constants.WIDTH / (float)Constants.HEIGHT, 0.1f, 100.0f);

        }
        public void Draw(Shader shader, Matrix4 view, float time)
        {

            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "model"), true, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "view"), true, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.getID(), "projection"), true, ref projection);
            GL.Uniform3(GL.GetUniformLocation(shader.getID(), "cpos"), ref cpos);
            GL.Uniform1(GL.GetUniformLocation(shader.getID(), "time"), (float)time);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, blockvertdata.Length);
            GL.BindVertexArray(0);

        }

        public void allzeros()
        {
            blockdata = new int[size, size, size];
            blockvertdataarray.Clear();

            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        blockdata[x, x, x] = 1;

                    }

                }

            }
            
            meshgen();
            Rewrite();
           
        }

        public void Rewrite()
        {
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, blockvertdata.Length * sizeof(float), blockvertdata, BufferUsageHint.DynamicCopy);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        }

        public void initialize()
        {

            // Console.WriteLine(Blocks.GetIDByBlock(Blocks.Dirt));

            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        // blockdata[x, y, z] = y < size-1 ? Blocks.Dirt.ID : Blocks.Grass.ID;
                        // blockdata[x,y,z] = RandomNumberGenerator.GetInt32(0, 50) > (10+(8*(y-22))) ? Blocks.Stone.ID : blockdata[x,y,z];

                        if (Math.Floor(Maths.Dist3D(x, y, z, 16, 16, 16)) == 15)
                        {

                            blockdata[x, y, z] = Blocks.Grass.ID;

                        }
                        if (Math.Floor(Maths.Dist3D(x, y, z, 16, 16, 16)) == 14)
                        {

                            blockdata[x, y, z] = Blocks.Dirt.ID;

                        }
                        if (Math.Floor(Maths.Dist3D(x, y, z, 16, 16, 16)) == 1)
                        {

                            blockdata[x, y, z] = Blocks.PRIDERAHHHH.ID;

                        }
                        // blockdata[x, y, z] = Math.Floor(Maths.Dist3D(x, y, z, 16, 16, 16)) <= 15 ? Blocks.Grass.ID : Blocks.Air.ID;

                    }

                }

            }

            meshgen();

        }

        public void meshgen()
        {

            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        if (blockdata[x,y,z] != Blocks.Air.GetID())
                        {

                            // operators are flipped on z because z forward is negative (z back is positive

                            if (blockdata[x,y,z-1 < 0 ? z : z-1] == 0) { backface(x, y, z); }
                            if (blockdata[x,y,z] != 0 && z == 0) { backface(x, y, z); } // edge detect

                            if (blockdata[x,y,z+1 > size-1 ? z : z+1] == 0) { frontface(x, y, z); }
                            if (blockdata[x,y,z] != 0 && z == size-1) { frontface(x, y, z); } // edge detect

                            if (blockdata[x-1 < 0 ? x : x-1,y,z] == 0) { leftface(x, y, z); }
                            if (blockdata[x,y,z] != 0 && x == 0) { leftface(x, y, z); } // edge detect

                            if (blockdata[x+1 > size-1 ? x : x+1, y, z] == 0) { rightface(x, y, z); }
                            if (blockdata[x, y, z] != 0 && x == size-1) { rightface(x, y, z); } // edge detect

                            if (blockdata[x,y-1 < 0 ? y : y-1, z] == 0) { bottomface(x, y, z); }
                            if (blockdata[x,y,z] != 0 && y == 0) { bottomface(x, y, z); } // edge detect

                            if (blockdata[x, y+1 > size-1 ? y : y+1, z] == 0) { topface(x, y, z); }
                            if (blockdata[x, y, z] != 0 && y == size-1) { topface(x, y, z); } // edge detect

                        }

                    }

                }

            }

            blockvertdata = (float[]) blockvertdataarray.ToArray(typeof(float));

        }

        // note that z-forward is negative 
        public void frontface(int x, int y, int z)
        {

            for (int i = 0; i < reffront.Length; i+=9)
            {

                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x,y,z]).reffront[i]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reffront[i+1] + (1 * x));
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reffront[i + 2] + (1 * y));
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reffront[i + 3] + (1 * z));
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
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refright[i + 1] + (1 * x));
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refright[i + 2] + (1 * y));
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refright[i + 3] + (1 * z));
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
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refback[i + 1] + (1 * x));
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refback[i + 2] + (1 * y));
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refback[i + 3] + (1 * z));
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
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refleft[i + 1] + (1 * x));
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refleft[i + 2] + (1 * y));
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refleft[i + 3] + (1 * z));
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
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reftop[i + 1] + (1 * x));
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reftop[i + 2] + (1 * y));
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).reftop[i + 3] + (1 * z));
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
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 1] + (1 * x));
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 2] + (1 * y));
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 3] + (1 * z));
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 4]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 5]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 6]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 7]);
                blockvertdataarray.Add(Blocks.GetBlockByID(blockdata[x, y, z]).refbottom[i + 8]);

            }

        }
        public float[] getvertdata()
        {

            return blockvertdata;

        }

        

    }
}
