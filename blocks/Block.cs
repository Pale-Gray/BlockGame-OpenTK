using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj
{
    internal class Block
    {
        public int ID = 0;

        public float[] reffront = {

            0, -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, // front
            0, 0.5f, -0.5f, 0.5f,  0.0f, 0.0f, 1.0f, 1.0f, 0.0f,
            0, 0.5f,  0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
            0, 0.5f,  0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
            0, -0.5f,  0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f,
            0, -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f

        };
        public float[] refright = {

            0, 0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, // right
            0, 0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            0, 0.5f,  0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f,
            0, 0.5f,  0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f,
            0, 0.5f,  0.5f, 0.5f, 1.0f, 0.0f, 0.0f,  0.0f, 1.0f,
            0, 0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f

        };
        public float[] refback =
        {

            0, 0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, // back 
            0, -0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f,
            0, -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 1.0f, 1.0f,
            0, -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 1.0f, 1.0f,
            0, 0.5f, 0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f,
            0, 0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f

        };
        public float[] refleft =
        {

            0, -0.5f, -0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, // left
            0, -0.5f, -0.5f, 0.5f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            0, -0.5f,  0.5f, 0.5f, -1.0f, 0.0f, 0.0f, 1.0f, 1.0f,
            0, -0.5f,  0.5f, 0.5f, -1.0f, 0.0f, 0.0f,  1.0f, 1.0f,
            0, -0.5f,  0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f,
            0, -0.5f, -0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f

        };
        public float[] reftop =
        {

            0, -0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, // top
            0, 0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f,
            0, 0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f,
            0, 0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f,
            0, -0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f,
            0, -0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,

        };
        public float[] refbottom =
        {

            0, 0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, // bottom
            0, -0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f,
            0, -0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 1.0f,
            0, -0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 1.0f,
            0, 0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 1.0f,
            0, 0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f

        };

        float txi = (32f / 256f);

        public Block SetID(int bt)
        {
            ID = bt;
            for (int i = 0; i < reffront.Length; i += 9)
            {

                reffront[i] = bt;
                refright[i] = bt;
                refback[i] = bt;
                refleft[i] = bt;
                reftop[i] = bt;
                refbottom[i] = bt;

            }
            return this;

        }

        public Block SetFront(float xindex, float yindex)
        {

            float[] tx =
            {

                0+(xindex*txi), (1-txi) + (yindex*txi),
                txi+(xindex*txi), (1-txi)+ (yindex*txi),
                txi+(xindex*txi), 1+ (yindex*txi),
                txi+(xindex*txi), 1+ (yindex*txi),
                0+(xindex*txi), 1+ (yindex*txi),
                0+(xindex*txi), (1-txi)+ (yindex*txi)

            };
            SetTexCoordinates(reffront, tx);
            return this;

        }
        public Block SetLeft(float xindex, float yindex)
        {

            float[] tx =
            {

                0+(xindex*txi), (1-txi) + (yindex*txi),
                txi+(xindex*txi), (1-txi)+ (yindex*txi),
                txi+(xindex*txi), 1+ (yindex*txi),
                txi+(xindex*txi), 1+ (yindex*txi),
                0+(xindex*txi), 1+ (yindex*txi),
                0+(xindex*txi), (1-txi)+ (yindex*txi)

            };
            SetTexCoordinates(refleft, tx);
            return this;

        }
        public Block SetBack(float xindex, float yindex)
        {

            float[] tx =
            {

                0+(xindex*txi), (1-txi) + (yindex*txi),
                txi+(xindex*txi), (1-txi)+ (yindex*txi),
                txi+(xindex*txi), 1+ (yindex*txi),
                txi+(xindex*txi), 1+ (yindex*txi),
                0+(xindex*txi), 1+ (yindex*txi),
                0+(xindex*txi), (1-txi)+ (yindex*txi)

            };
            SetTexCoordinates(refback, tx);
            return this;

        }
        public Block SetRight(float xindex, float yindex)
        {

            float[] tx =
            {

                0+(xindex*txi), (1-txi) + (yindex*txi),
                txi+(xindex*txi), (1-txi)+ (yindex*txi),
                txi+(xindex*txi), 1+ (yindex*txi),
                txi+(xindex*txi), 1+ (yindex*txi),
                0+(xindex*txi), 1+ (yindex*txi),
                0+(xindex*txi), (1-txi)+ (yindex*txi)

            };
            SetTexCoordinates(refright, tx);
            return this;

        }
        public Block SetTop(float xindex, float yindex)
        {

            float[] tx =
            {

                0+(xindex*txi), (1-txi) + (yindex*txi),
                txi+(xindex*txi), (1-txi)+ (yindex*txi),
                txi+(xindex*txi), 1+ (yindex*txi),
                txi+(xindex*txi), 1+ (yindex*txi),
                0+(xindex*txi), 1+ (yindex*txi),
                0+(xindex*txi), (1-txi)+ (yindex*txi)

            };
            SetTexCoordinates(reftop, tx);
            return this;

        }
        public Block SetBottom(float xindex, float yindex)
        {

            float[] tx =
            {

                0+(xindex*txi), (1-txi) + (yindex*txi),
                txi+(xindex*txi), (1-txi)+ (yindex*txi),
                txi+(xindex*txi), 1+ (yindex*txi),
                txi+(xindex*txi), 1+ (yindex*txi),
                0+(xindex*txi), 1+ (yindex*txi),
                0+(xindex*txi), (1-txi)+ (yindex*txi)

            };
            SetTexCoordinates(refbottom, tx);
            return this;

        }

        public Block SetTexCoordinates(float[] vertexdata, float[] texcoords)
        {

            vertexdata[7] = texcoords[0];
            vertexdata[8] = texcoords[1];

            vertexdata[16] = texcoords[2];
            vertexdata[17] = texcoords[3];

            vertexdata[25] = texcoords[4];
            vertexdata[26] = texcoords[5];

            vertexdata[34] = texcoords[6];
            vertexdata[35] = texcoords[7];

            vertexdata[43] = texcoords[8];
            vertexdata[44] = texcoords[9];

            vertexdata[52] = texcoords[10];
            vertexdata[53] = texcoords[11];
            return this;
        }

        public int GetID()
        {

            return ID;

        }

    }
}
