using OpenTK.Graphics.OpenGL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj
{
    internal class Chunk
    {

        static int size = 32;

        int[,,] blockdata = new int[size,size,size];
        float[] blockvertdata = new float[180 * (size*size)];
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

        ArrayList blockvertdataarray = new ArrayList();

        public void initialize()
        {

            // Console.WriteLine(Blocks.GetIDByBlock(Blocks.Dirt));

            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        blockdata[x, y, z] = y < size - 1 ? Blocks.Dirt.GetID() : Blocks.Grass.GetID();
                        // blockdata[x, y > z ? z : y, z] = 1;

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
