using OpenTK.Graphics.OpenGL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj
{
    internal class Chunk
    {

        static int size = 32;

        int[,,] blockdata = new int[size+1,size+1,size+1];
        float[] blockvertdata = new float[180 * (size*size)];
        float[] reffront =
        {

            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 1, // front
             0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 1,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 1,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 1,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 1,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 1

        };
        float[] refright = {

            0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 0, // right
            0.5f, -0.5f, 0.5f,  1.0f, 0.0f, 0,
            0.5f,  0.5f, 0.5f,  1.0f, 1.0f, 0,
            0.5f,  0.5f, 0.5f,  1.0f, 1.0f, 0,
            0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 0,
            0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 0

        };
        float[] refback =
        {

            0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 0, // back 
            -0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0,
            -0.5f, 0.5f, 0.5f, 1.0f, 1.0f, 0,
            -0.5f, 0.5f, 0.5f, 1.0f, 1.0f, 0,
            0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0,
            0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 0

        };
        float[] refleft =
        {

            -0.5f, -0.5f, 0.5f,  0.0f, 0.0f, 0,// left
            -0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 0,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0,
            -0.5f,  0.5f, 0.5f,  0.0f, 1.0f, 0,
            -0.5f, -0.5f, 0.5f,  0.0f, 0.0f, 0

        };
        float[] reftop =
        {

            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, 0, // top
            0.5f, 0.5f, -0.5f, 1.0f, 0.0f, 0,
            0.5f, 0.5f, 0.5f, 1.0f, 1.0f, 0,
            0.5f, 0.5f, 0.5f, 1.0f, 1.0f, 0,
            -0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0,
            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, 0,

        };
        float[] refbottom =
        {

            0.5f, -0.5f, -0.5f, 0.0f, 0.0f, 0, // bottom
            -0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0,
            -0.5f, -0.5f, 0.5f, 1.0f, 1.0f, 0,
            -0.5f, -0.5f, 0.5f, 1.0f, 1.0f, 0,
            0.5f, -0.5f, 0.5f, 0.0f, 1.0f, 0,
            0.5f, -0.5f, -0.5f, 0.0f, 0.0f, 0

        };

        ArrayList blockvertdataarray = new ArrayList();

        public void initialize()
        {

            for (int x = 0; x < size; x++)
            {

                for (int y = 0; y < size; y++)
                {

                    for (int z = 0; z < size; z++)
                    {

                        blockdata[x, x, z] = 1;

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

                        if (blockdata[x,y,z] == 1)
                        {

                            if (blockdata[x,y,z] == 1 && x == 0) { leftface(x, y, z); }
                            if (blockdata[x, y, z] == 1 && x == size - 1) { rightface(x, y, z); }
                            if (blockdata[x,y,z] == 1 && y == 0) { bottomface(x, y, z); }
                            if (blockdata[x, y, z] == 1 && y == size-1) { topface(x, y, z); }
                            if (blockdata[x,y,z] == 1 && z == 0) { frontface(x, y, z); }
                            if (blockdata[x,y,z] == 1 && z == size-1) { backface(x, y, z); }


                            if (blockdata[x,y+1 > size-1 ? y : y+1,z] == 0) { topface(x, y, z); }
                            if (blockdata[x,y-1 < 0 ? y : y-1,z] == 0) { bottomface(x, y, z); }
                            if (blockdata[x+1 > size-1 ? x : x+1,y,z] == 0) { rightface(x, y, z); }
                            if (blockdata[x-1 < 0 ? x : x-1,y,z] == 0) { leftface(x, y, z); }
                            if (blockdata[x,y,z+1 > size-1 ? z : z+1] == 0) { backface(x, y, z); }
                            if (blockdata[x,y,z-1 < 0 ? z : z-1] == 0) { frontface(x, y, z); }

                        }

                    }

                }

            }

            blockvertdata = (float[]) blockvertdataarray.ToArray(typeof(float));

        }

        public void frontface(int x, int y, int z)
        {

            blockvertdataarray.Add(reffront[0] + (1 * x));
            blockvertdataarray.Add(reffront[1] + (1 * y));
            blockvertdataarray.Add(reffront[2] + (1 * z));
            blockvertdataarray.Add(reffront[3]);
            blockvertdataarray.Add(reffront[4]);
            blockvertdataarray.Add(reffront[5]);

            blockvertdataarray.Add(reffront[6] + (1 * x));
            blockvertdataarray.Add(reffront[7] + (1 * y));
            blockvertdataarray.Add(reffront[8] + (1 * z));
            blockvertdataarray.Add(reffront[9]);
            blockvertdataarray.Add(reffront[10]);
            blockvertdataarray.Add(reffront[11]);

            blockvertdataarray.Add(reffront[12] + (1 * x));
            blockvertdataarray.Add(reffront[13] + (1 * y));
            blockvertdataarray.Add(reffront[14] + (1 * z));
            blockvertdataarray.Add(reffront[15]);
            blockvertdataarray.Add(reffront[16]);
            blockvertdataarray.Add(reffront[17]);

            blockvertdataarray.Add(reffront[18] + (1 * x));
            blockvertdataarray.Add(reffront[19] + (1 * y));
            blockvertdataarray.Add(reffront[20] + (1 * z));
            blockvertdataarray.Add(reffront[21]);
            blockvertdataarray.Add(reffront[22]);
            blockvertdataarray.Add(reffront[23]);

            blockvertdataarray.Add(reffront[24] + (1 * x));
            blockvertdataarray.Add(reffront[25] + (1 * y));
            blockvertdataarray.Add(reffront[26] + (1 * z));
            blockvertdataarray.Add(reffront[27]);
            blockvertdataarray.Add(reffront[28]);
            blockvertdataarray.Add(reffront[29]);

            blockvertdataarray.Add(reffront[30] + (1 * x));
            blockvertdataarray.Add(reffront[31] + (1 * y));
            blockvertdataarray.Add(reffront[32] + (1 * z));
            blockvertdataarray.Add(reffront[33]);
            blockvertdataarray.Add(reffront[34]);
            blockvertdataarray.Add(reffront[35]);

        }
        public void rightface(int x, int y, int z)
        {

            blockvertdataarray.Add(refright[0] + (1 * x));
            blockvertdataarray.Add(refright[1] + (1 * y));
            blockvertdataarray.Add(refright[2] + (1 * z));
            blockvertdataarray.Add(refright[3]);
            blockvertdataarray.Add(refright[4]);
            blockvertdataarray.Add(refright[5]);

            blockvertdataarray.Add(refright[6] + (1 * x));
            blockvertdataarray.Add(refright[7] + (1 * y));
            blockvertdataarray.Add(refright[8] + (1 * z));
            blockvertdataarray.Add(refright[9]);
            blockvertdataarray.Add(refright[10]);
            blockvertdataarray.Add(refright[11]);

            blockvertdataarray.Add(refright[12] + (1 * x));
            blockvertdataarray.Add(refright[13] + (1 * y));
            blockvertdataarray.Add(refright[14] + (1 * z));
            blockvertdataarray.Add(refright[15]);
            blockvertdataarray.Add(refright[16]);
            blockvertdataarray.Add(refright[17]);

            blockvertdataarray.Add(refright[18] + (1 * x));
            blockvertdataarray.Add(refright[19] + (1 * y));
            blockvertdataarray.Add(refright[20] + (1 * z));
            blockvertdataarray.Add(refright[21]);
            blockvertdataarray.Add(refright[22]);
            blockvertdataarray.Add(refright[23]);

            blockvertdataarray.Add(refright[24] + (1 * x));
            blockvertdataarray.Add(refright[25] + (1 * y));
            blockvertdataarray.Add(refright[26] + (1 * z));
            blockvertdataarray.Add(refright[27]);
            blockvertdataarray.Add(refright[28]);
            blockvertdataarray.Add(refright[29]);

            blockvertdataarray.Add(refright[30] + (1 * x));
            blockvertdataarray.Add(refright[31] + (1 * y));
            blockvertdataarray.Add(refright[32] + (1 * z));
            blockvertdataarray.Add(refright[33]);
            blockvertdataarray.Add(refright[34]);
            blockvertdataarray.Add(refright[35]);

        }
        public void backface(int x, int y, int z)
        {

            blockvertdataarray.Add(refback[0] + (1 * x));
            blockvertdataarray.Add(refback[1] + (1 * y));
            blockvertdataarray.Add(refback[2] + (1 * z));
            blockvertdataarray.Add(refback[3]);
            blockvertdataarray.Add(refback[4]);
            blockvertdataarray.Add(refback[5]);

            blockvertdataarray.Add(refback[6] + (1 * x));
            blockvertdataarray.Add(refback[7] + (1 * y));
            blockvertdataarray.Add(refback[8] + (1 * z));
            blockvertdataarray.Add(refback[9]);
            blockvertdataarray.Add(refback[10]);
            blockvertdataarray.Add(refback[11]);

            blockvertdataarray.Add(refback[12] + (1 * x));
            blockvertdataarray.Add(refback[13] + (1 * y));
            blockvertdataarray.Add(refback[14] + (1 * z));
            blockvertdataarray.Add(refback[15]);
            blockvertdataarray.Add(refback[16]);
            blockvertdataarray.Add(refback[17]);

            blockvertdataarray.Add(refback[18] + (1 * x));
            blockvertdataarray.Add(refback[19] + (1 * y));
            blockvertdataarray.Add(refback[20] + (1 * z));
            blockvertdataarray.Add(refback[21]);
            blockvertdataarray.Add(refback[22]);
            blockvertdataarray.Add(refback[23]);

            blockvertdataarray.Add(refback[24] + (1 * x));
            blockvertdataarray.Add(refback[25] + (1 * y));
            blockvertdataarray.Add(refback[26] + (1 * z));
            blockvertdataarray.Add(refback[27]);
            blockvertdataarray.Add(refback[28]);
            blockvertdataarray.Add(refback[29]);

            blockvertdataarray.Add(refback[30] + (1 * x));
            blockvertdataarray.Add(refback[31] + (1 * y));
            blockvertdataarray.Add(refback[32] + (1 * z));
            blockvertdataarray.Add(refback[33]);
            blockvertdataarray.Add(refback[34]);
            blockvertdataarray.Add(refback[35]);

        }
        public void leftface(int x, int y, int z)
        {

            blockvertdataarray.Add(refleft[0] + (1 * x));
            blockvertdataarray.Add(refleft[1] + (1 * y));
            blockvertdataarray.Add(refleft[2] + (1 * z));
            blockvertdataarray.Add(refleft[3]);
            blockvertdataarray.Add(refleft[4]);
            blockvertdataarray.Add(refleft[5]);

            blockvertdataarray.Add(refleft[6] + (1 * x));
            blockvertdataarray.Add(refleft[7] + (1 * y));
            blockvertdataarray.Add(refleft[8] + (1 * z));
            blockvertdataarray.Add(refleft[9]);
            blockvertdataarray.Add(refleft[10]);
            blockvertdataarray.Add(refleft[11]);

            blockvertdataarray.Add(refleft[12] + (1 * x));
            blockvertdataarray.Add(refleft[13] + (1 * y));
            blockvertdataarray.Add(refleft[14] + (1 * z));
            blockvertdataarray.Add(refleft[15]);
            blockvertdataarray.Add(refleft[16]);
            blockvertdataarray.Add(refleft[17]);

            blockvertdataarray.Add(refleft[18] + (1 * x));
            blockvertdataarray.Add(refleft[19] + (1 * y));
            blockvertdataarray.Add(refleft[20] + (1 * z));
            blockvertdataarray.Add(refleft[21]);
            blockvertdataarray.Add(refleft[22]);
            blockvertdataarray.Add(refleft[23]);

            blockvertdataarray.Add(refleft[24] + (1 * x));
            blockvertdataarray.Add(refleft[25] + (1 * y));
            blockvertdataarray.Add(refleft[26] + (1 * z));
            blockvertdataarray.Add(refleft[27]);
            blockvertdataarray.Add(refleft[28]);
            blockvertdataarray.Add(refleft[29]);

            blockvertdataarray.Add(refleft[30] + (1 * x));
            blockvertdataarray.Add(refleft[31] + (1 * y));
            blockvertdataarray.Add(refleft[32] + (1 * z));
            blockvertdataarray.Add(refleft[33]);
            blockvertdataarray.Add(refleft[34]);
            blockvertdataarray.Add(refleft[35]);

        }
        public void topface(int x, int y, int z)
        {

            blockvertdataarray.Add(reftop[0] + (1 * x));
            blockvertdataarray.Add(reftop[1] + (1 * y));
            blockvertdataarray.Add(reftop[2] + (1 * z));
            blockvertdataarray.Add(reftop[3]);
            blockvertdataarray.Add(reftop[4]);
            blockvertdataarray.Add(reftop[5]);

            blockvertdataarray.Add(reftop[6] + (1 * x));
            blockvertdataarray.Add(reftop[7] + (1 * y));
            blockvertdataarray.Add(reftop[8] + (1 * z));
            blockvertdataarray.Add(reftop[9]);
            blockvertdataarray.Add(reftop[10]);
            blockvertdataarray.Add(reftop[11]);

            blockvertdataarray.Add(reftop[12] + (1 * x));
            blockvertdataarray.Add(reftop[13] + (1 * y));
            blockvertdataarray.Add(reftop[14] + (1 * z));
            blockvertdataarray.Add(reftop[15]);
            blockvertdataarray.Add(reftop[16]);
            blockvertdataarray.Add(reftop[17]);

            blockvertdataarray.Add(reftop[18] + (1 * x));
            blockvertdataarray.Add(reftop[19] + (1 * y));
            blockvertdataarray.Add(reftop[20] + (1 * z));
            blockvertdataarray.Add(reftop[21]);
            blockvertdataarray.Add(reftop[22]);
            blockvertdataarray.Add(reftop[23]);

            blockvertdataarray.Add(reftop[24] + (1 * x));
            blockvertdataarray.Add(reftop[25] + (1 * y));
            blockvertdataarray.Add(reftop[26] + (1 * z));
            blockvertdataarray.Add(reftop[27]);
            blockvertdataarray.Add(reftop[28]);
            blockvertdataarray.Add(reftop[29]);

            blockvertdataarray.Add(reftop[30] + (1 * x));
            blockvertdataarray.Add(reftop[31] + (1 * y));
            blockvertdataarray.Add(reftop[32] + (1 * z));
            blockvertdataarray.Add(reftop[33]);
            blockvertdataarray.Add(reftop[34]);
            blockvertdataarray.Add(reftop[35]);

        }
        public void bottomface(int x, int y, int z)
        {

            blockvertdataarray.Add(refbottom[0] + (1 * x));
            blockvertdataarray.Add(refbottom[1] + (1 * y));
            blockvertdataarray.Add(refbottom[2] + (1 * z));
            blockvertdataarray.Add(refbottom[3]);
            blockvertdataarray.Add(refbottom[4]);
            blockvertdataarray.Add(refbottom[5]);

            blockvertdataarray.Add(refbottom[6] + (1 * x));
            blockvertdataarray.Add(refbottom[7] + (1 * y));
            blockvertdataarray.Add(refbottom[8] + (1 * z));
            blockvertdataarray.Add(refbottom[9]);
            blockvertdataarray.Add(refbottom[10]);
            blockvertdataarray.Add(refbottom[11]);

            blockvertdataarray.Add(refbottom[12] + (1 * x));
            blockvertdataarray.Add(refbottom[13] + (1 * y));
            blockvertdataarray.Add(refbottom[14] + (1 * z));
            blockvertdataarray.Add(refbottom[15]);
            blockvertdataarray.Add(refbottom[16]);
            blockvertdataarray.Add(refbottom[17]);

            blockvertdataarray.Add(refbottom[18] + (1 * x));
            blockvertdataarray.Add(refbottom[19] + (1 * y));
            blockvertdataarray.Add(refbottom[20] + (1 * z));
            blockvertdataarray.Add(refbottom[21]);
            blockvertdataarray.Add(refbottom[22]);
            blockvertdataarray.Add(refbottom[23]);

            blockvertdataarray.Add(refbottom[24] + (1 * x));
            blockvertdataarray.Add(refbottom[25] + (1 * y));
            blockvertdataarray.Add(refbottom[26] + (1 * z));
            blockvertdataarray.Add(refbottom[27]);
            blockvertdataarray.Add(refbottom[28]);
            blockvertdataarray.Add(refbottom[29]);

            blockvertdataarray.Add(refbottom[30] + (1 * x));
            blockvertdataarray.Add(refbottom[31] + (1 * y));
            blockvertdataarray.Add(refbottom[32] + (1 * z));
            blockvertdataarray.Add(refbottom[33]);
            blockvertdataarray.Add(refbottom[34]);
            blockvertdataarray.Add(refbottom[35]);

        }
        public float[] getvertdata()
        {

            return blockvertdata;

        }

        

    }
}
