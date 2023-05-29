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

        int[,,] blockdata = new int[size,size,size];
        float[] blockvertdata = new float[125];
        float[] reffront =
        {

            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, // front
             0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f

        };
        float[] refright = {

            0.5f, -0.5f, -0.5f,  0.0f, 0.0f, // right
            0.5f, -0.5f, 0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, 0.5f,  1.0f, 1.0f,
            0.5f,  0.5f, 0.5f,  1.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 0.0f

        };
        float[] refback =
        {

            0.5f, -0.5f, 0.5f, 0.0f, 0.0f, // back 
            -0.5f, -0.5f, 0.5f, 1.0f, 0.0f,
            -0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            -0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 0.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 0.0f, 0.0f

        };
        float[] refleft =
        {

            -0.5f, -0.5f, 0.5f,  0.0f, 0.0f, // left
            -0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, 0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, 0.5f,  0.0f, 0.0f

        };
        float[] reftop =
        {

            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, // top
            0.5f, 0.5f, -0.5f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            -0.5f, 0.5f, 0.5f, 0.0f, 1.0f,
            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f

        };
        float[] refbottom =
        {

            0.5f, -0.5f, -0.5f, 0.0f, 0.0f, // bottom
            -0.5f, -0.5f, -0.5f, 1.0f, 0.0f,
            -0.5f, -0.5f, 0.5f, 1.0f, 1.0f,
            -0.5f, -0.5f, 0.5f, 1.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 0.0f, 1.0f,
            0.5f, -0.5f, -0.5f, 0.0f, 0.0f

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

                        blockdata[x,z,y] = 1;

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

                        if (blockdata[x,z,y] == 1)
                        {

                            frontface(x, y, z);
                            rightface(x, y, z);
                            backface(x, y, z);
                            leftface(x, y, z);
                            topface(x, y, z);
                            bottomface(x, y, z);

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

            blockvertdataarray.Add(reffront[5] + (1 * x));
            blockvertdataarray.Add(reffront[6] + (1 * y));
            blockvertdataarray.Add(reffront[7] + (1 * z));
            blockvertdataarray.Add(reffront[8]);
            blockvertdataarray.Add(reffront[9]);

            blockvertdataarray.Add(reffront[10] + (1 * x));
            blockvertdataarray.Add(reffront[11] + (1 * y));
            blockvertdataarray.Add(reffront[12] + (1 * z));
            blockvertdataarray.Add(reffront[13]);
            blockvertdataarray.Add(reffront[14]);

            blockvertdataarray.Add(reffront[15] + (1 * x));
            blockvertdataarray.Add(reffront[16] + (1 * y));
            blockvertdataarray.Add(reffront[17] + (1 * z));
            blockvertdataarray.Add(reffront[18]);
            blockvertdataarray.Add(reffront[19]);

            blockvertdataarray.Add(reffront[20] + (1 * x));
            blockvertdataarray.Add(reffront[21] + (1 * y));
            blockvertdataarray.Add(reffront[22] + (1 * z));
            blockvertdataarray.Add(reffront[23]);
            blockvertdataarray.Add(reffront[24]);

            blockvertdataarray.Add(reffront[25] + (1 * x));
            blockvertdataarray.Add(reffront[26] + (1 * y));
            blockvertdataarray.Add(reffront[27] + (1 * z));
            blockvertdataarray.Add(reffront[28]);
            blockvertdataarray.Add(reffront[29]);

        }
        public void rightface(int x, int y, int z)
        {

            blockvertdataarray.Add(refright[0] + (1 * x));
            blockvertdataarray.Add(refright[1] + (1 * y));
            blockvertdataarray.Add(refright[2] + (1 * z));
            blockvertdataarray.Add(refright[3]);
            blockvertdataarray.Add(refright[4]);

            blockvertdataarray.Add(refright[5] + (1 * x));
            blockvertdataarray.Add(refright[6] + (1 * y));
            blockvertdataarray.Add(refright[7] + (1 * z));
            blockvertdataarray.Add(refright[8]);
            blockvertdataarray.Add(refright[9]);

            blockvertdataarray.Add(refright[10] + (1 * x));
            blockvertdataarray.Add(refright[11] + (1 * y));
            blockvertdataarray.Add(refright[12] + (1 * z));
            blockvertdataarray.Add(refright[13]);
            blockvertdataarray.Add(refright[14]);

            blockvertdataarray.Add(refright[15] + (1 * x));
            blockvertdataarray.Add(refright[16] + (1 * y));
            blockvertdataarray.Add(refright[17] + (1 * z));
            blockvertdataarray.Add(refright[18]);
            blockvertdataarray.Add(refright[19]);

            blockvertdataarray.Add(refright[20] + (1 * x));
            blockvertdataarray.Add(refright[21] + (1 * y));
            blockvertdataarray.Add(refright[22] + (1 * z));
            blockvertdataarray.Add(refright[23]);
            blockvertdataarray.Add(refright[24]);

            blockvertdataarray.Add(refright[25] + (1 * x));
            blockvertdataarray.Add(refright[26] + (1 * y));
            blockvertdataarray.Add(refright[27] + (1 * z));
            blockvertdataarray.Add(refright[28]);
            blockvertdataarray.Add(refright[29]);

        }
        public void backface(int x, int y, int z)
        {

            blockvertdataarray.Add(refback[0] + (1 * x));
            blockvertdataarray.Add(refback[1] + (1 * y));
            blockvertdataarray.Add(refback[2] + (1 * z));
            blockvertdataarray.Add(refback[3]);
            blockvertdataarray.Add(refback[4]);

            blockvertdataarray.Add(refback[5] + (1 * x));
            blockvertdataarray.Add(refback[6] + (1 * y));
            blockvertdataarray.Add(refback[7] + (1 * z));
            blockvertdataarray.Add(refback[8]);
            blockvertdataarray.Add(refback[9]);

            blockvertdataarray.Add(refback[10] + (1 * x));
            blockvertdataarray.Add(refback[11] + (1 * y));
            blockvertdataarray.Add(refback[12] + (1 * z));
            blockvertdataarray.Add(refback[13]);
            blockvertdataarray.Add(refback[14]);

            blockvertdataarray.Add(refback[15] + (1 * x));
            blockvertdataarray.Add(refback[16] + (1 * y));
            blockvertdataarray.Add(refback[17] + (1 * z));
            blockvertdataarray.Add(refback[18]);
            blockvertdataarray.Add(refback[19]);

            blockvertdataarray.Add(refback[20] + (1 * x));
            blockvertdataarray.Add(refback[21] + (1 * y));
            blockvertdataarray.Add(refback[22] + (1 * z));
            blockvertdataarray.Add(refback[23]);
            blockvertdataarray.Add(refback[24]);

            blockvertdataarray.Add(refback[25] + (1 * x));
            blockvertdataarray.Add(refback[26] + (1 * y));
            blockvertdataarray.Add(refback[27] + (1 * z));
            blockvertdataarray.Add(refback[28]);
            blockvertdataarray.Add(refback[29]);

        }
        public void leftface(int x, int y, int z)
        {

            blockvertdataarray.Add(refleft[0] + (1 * x));
            blockvertdataarray.Add(refleft[1] + (1 * y));
            blockvertdataarray.Add(refleft[2] + (1 * z));
            blockvertdataarray.Add(refleft[3]);
            blockvertdataarray.Add(refleft[4]);

            blockvertdataarray.Add(refleft[5] + (1 * x));
            blockvertdataarray.Add(refleft[6] + (1 * y));
            blockvertdataarray.Add(refleft[7] + (1 * z));
            blockvertdataarray.Add(refleft[8]);
            blockvertdataarray.Add(refleft[9]);

            blockvertdataarray.Add(refleft[10] + (1 * x));
            blockvertdataarray.Add(refleft[11] + (1 * y));
            blockvertdataarray.Add(refleft[12] + (1 * z));
            blockvertdataarray.Add(refleft[13]);
            blockvertdataarray.Add(refleft[14]);

            blockvertdataarray.Add(refleft[15] + (1 * x));
            blockvertdataarray.Add(refleft[16] + (1 * y));
            blockvertdataarray.Add(refleft[17] + (1 * z));
            blockvertdataarray.Add(refleft[18]);
            blockvertdataarray.Add(refleft[19]);

            blockvertdataarray.Add(refleft[20] + (1 * x));
            blockvertdataarray.Add(refleft[21] + (1 * y));
            blockvertdataarray.Add(refleft[22] + (1 * z));
            blockvertdataarray.Add(refleft[23]);
            blockvertdataarray.Add(refleft[24]);

            blockvertdataarray.Add(refleft[25] + (1 * x));
            blockvertdataarray.Add(refleft[26] + (1 * y));
            blockvertdataarray.Add(refleft[27] + (1 * z));
            blockvertdataarray.Add(refleft[28]);
            blockvertdataarray.Add(refleft[29]);

        }
        public void topface(int x, int y, int z)
        {

            blockvertdataarray.Add(reftop[0] + (1 * x));
            blockvertdataarray.Add(reftop[1] + (1 * y));
            blockvertdataarray.Add(reftop[2] + (1 * z));
            blockvertdataarray.Add(reftop[3]);
            blockvertdataarray.Add(reftop[4]);

            blockvertdataarray.Add(reftop[5] + (1 * x));
            blockvertdataarray.Add(reftop[6] + (1 * y));
            blockvertdataarray.Add(reftop[7] + (1 * z));
            blockvertdataarray.Add(reftop[8]);
            blockvertdataarray.Add(reftop[9]);

            blockvertdataarray.Add(reftop[10] + (1 * x));
            blockvertdataarray.Add(reftop[11] + (1 * y));
            blockvertdataarray.Add(reftop[12] + (1 * z));
            blockvertdataarray.Add(reftop[13]);
            blockvertdataarray.Add(reftop[14]);

            blockvertdataarray.Add(reftop[15] + (1 * x));
            blockvertdataarray.Add(reftop[16] + (1 * y));
            blockvertdataarray.Add(reftop[17] + (1 * z));
            blockvertdataarray.Add(reftop[18]);
            blockvertdataarray.Add(reftop[19]);

            blockvertdataarray.Add(reftop[20] + (1 * x));
            blockvertdataarray.Add(reftop[21] + (1 * y));
            blockvertdataarray.Add(reftop[22] + (1 * z));
            blockvertdataarray.Add(reftop[23]);
            blockvertdataarray.Add(reftop[24]);

            blockvertdataarray.Add(reftop[25] + (1 * x));
            blockvertdataarray.Add(reftop[26] + (1 * y));
            blockvertdataarray.Add(reftop[27] + (1 * z));
            blockvertdataarray.Add(reftop[28]);
            blockvertdataarray.Add(reftop[29]);

        }
        public void bottomface(int x, int y, int z)
        {

            blockvertdataarray.Add(refbottom[0] + (1 * x));
            blockvertdataarray.Add(refbottom[1] + (1 * y));
            blockvertdataarray.Add(refbottom[2] + (1 * z));
            blockvertdataarray.Add(refbottom[3]);
            blockvertdataarray.Add(refbottom[4]);

            blockvertdataarray.Add(refbottom[5] + (1 * x));
            blockvertdataarray.Add(refbottom[6] + (1 * y));
            blockvertdataarray.Add(refbottom[7] + (1 * z));
            blockvertdataarray.Add(refbottom[8]);
            blockvertdataarray.Add(refbottom[9]);

            blockvertdataarray.Add(refbottom[10] + (1 * x));
            blockvertdataarray.Add(refbottom[11] + (1 * y));
            blockvertdataarray.Add(refbottom[12] + (1 * z));
            blockvertdataarray.Add(refbottom[13]);
            blockvertdataarray.Add(refbottom[14]);

            blockvertdataarray.Add(refbottom[15] + (1 * x));
            blockvertdataarray.Add(refbottom[16] + (1 * y));
            blockvertdataarray.Add(refbottom[17] + (1 * z));
            blockvertdataarray.Add(refbottom[18]);
            blockvertdataarray.Add(refbottom[19]);

            blockvertdataarray.Add(refbottom[20] + (1 * x));
            blockvertdataarray.Add(refbottom[21] + (1 * y));
            blockvertdataarray.Add(refbottom[22] + (1 * z));
            blockvertdataarray.Add(refbottom[23]);
            blockvertdataarray.Add(refbottom[24]);

            blockvertdataarray.Add(refbottom[25] + (1 * x));
            blockvertdataarray.Add(refbottom[26] + (1 * y));
            blockvertdataarray.Add(refbottom[27] + (1 * z));
            blockvertdataarray.Add(refbottom[28]);
            blockvertdataarray.Add(refbottom[29]);

        }
        public float[] getvertdata()
        {

            return blockvertdata;

        }

    }
}
