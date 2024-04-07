using System;
using System.Collections.Generic;
using System.Linq;
// using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using System.Text;
using System.Threading.Tasks;
using opentk_proj.registry;

namespace opentk_proj.block
{
    internal class Block
    {
        public int ID = 0;
        public string Name = "Placeholder";

        public const int FF = 0;
        public const int RF = 1;
        public const int BF = 2;
        public const int LF = 3;

        public const int TOF = 4;
        public const int BOF = 5;

        public const int SIDES = 6;
        public const int TOPANDBOTTOM = 7;
        public const int ALL = 8;

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

        public BoundingBox boundingBox = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(0.5f, 0.5f, 0.5f));
        public NakedModel boundingModel;

        Dictionary<int, float[]> FaceDictionary = new Dictionary<int, float[]>();

        float txi = 32f / 256f;

        public Block()
        {

            SetID(0);
            FaceDictionary[FF] = reffront;
            FaceDictionary[RF] = refright;
            FaceDictionary[BF] = refback;
            FaceDictionary[LF] = refleft;
            FaceDictionary[TOF] = reftop;
            FaceDictionary[BOF] = refbottom;

            boundingModel = new NakedModel(boundingBox.triangles);

        }

        public Block SetLookup(Dictionary<int, Block> d)
        {

            d[ID] = this;

            return this;

        }

        public Block SetName(string name)
        {

            Name = name;

            return this;

        }
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
            SetLookup(BlockLookups.DefaultLookup);
            return this;

        }

        public Block SetFaces(int xindex, int yindex, params int[] faces)
        {

            float[] tx =
            {

                0+xindex*txi, 1-txi - yindex*txi,
                txi+xindex*txi, 1-txi- yindex*txi,
                txi+xindex*txi, 1- yindex*txi,
                txi+xindex*txi, 1- yindex*txi,
                0+xindex*txi, 1- yindex*txi,
                0+xindex*txi, 1 - txi -yindex * txi

            };

            if (faces[0] == SIDES && faces.Length == 1)
            {

                SetTexCoordinates(reffront, tx);
                SetTexCoordinates(refright, tx);
                SetTexCoordinates(refback, tx);
                SetTexCoordinates(refleft, tx);

            }
            else if (faces[0] == TOPANDBOTTOM && faces.Length == 1)
            {

                SetTexCoordinates(reftop, tx);
                SetTexCoordinates(refbottom, tx);

            }
            else if (faces[0] == ALL && faces.Length == 1)
            {

                SetTexCoordinates(reffront, tx);
                SetTexCoordinates(refright, tx);
                SetTexCoordinates(refback, tx);
                SetTexCoordinates(refleft, tx);
                SetTexCoordinates(reftop, tx);
                SetTexCoordinates(refbottom, tx);

            } else
            {

                foreach (int i in faces)
                {

                    SetTexCoordinates(FaceDictionary[i], tx);

                }

            }
            return this;

        }
        public Block SetFront(float xindex, float yindex)
        {

            float[] tx =
            {

                0+xindex*txi, 1-txi - yindex*txi,
                txi+xindex*txi, 1-txi- yindex*txi,
                txi+xindex*txi, 1- yindex*txi,
                txi+xindex*txi, 1- yindex*txi,
                0+xindex*txi, 1- yindex*txi,
                0+xindex*txi, 1 - txi -yindex * txi

            };
            SetTexCoordinates(reffront, tx);
            return this;

        }
        public Block SetLeft(float xindex, float yindex)
        {

            float[] tx =
            {

                0+xindex*txi, 1-txi - yindex*txi,
                txi+xindex*txi, 1-txi- yindex*txi,
                txi+xindex*txi, 1- yindex*txi,
                txi+xindex*txi, 1- yindex*txi,
                0+xindex*txi, 1- yindex*txi,
                0+xindex*txi, 1 - txi -yindex * txi

            };
            SetTexCoordinates(refleft, tx);
            return this;

        }
        public Block SetBack(float xindex, float yindex)
        {

            float[] tx =
            {

                0+xindex*txi, 1-txi - yindex*txi,
                txi+xindex*txi, 1-txi- yindex*txi,
                txi+xindex*txi, 1- yindex*txi,
                txi+xindex*txi, 1- yindex*txi,
                0+xindex*txi, 1- yindex*txi,
                0+xindex*txi, 1 - txi -yindex * txi

            };
            SetTexCoordinates(refback, tx);
            return this;

        }
        public Block SetRight(float xindex, float yindex)
        {

            float[] tx =
            {

                0+xindex*txi, 1-txi - yindex*txi,
                txi+xindex*txi, 1-txi- yindex*txi,
                txi+xindex*txi, 1- yindex*txi,
                txi+xindex*txi, 1- yindex*txi,
                0+xindex*txi, 1- yindex*txi,
                0+xindex*txi, 1 - txi -yindex * txi

            };
            SetTexCoordinates(refright, tx);
            return this;

        }
        public Block SetTop(float xindex, float yindex)
        {

            float[] tx =
            {

                0+xindex*txi, 1-txi - yindex*txi,
                txi+xindex*txi, 1-txi- yindex*txi,
                txi+xindex*txi, 1- yindex*txi,
                txi+xindex*txi, 1- yindex*txi,
                0+xindex*txi, 1- yindex*txi,
                0+xindex*txi, 1 - txi -yindex * txi

            };
            SetTexCoordinates(reftop, tx);
            return this;

        }
        public Block SetBottom(float xindex, float yindex)
        {

            float[] tx =
            {

                0+xindex*txi, 1-txi - yindex*txi,
                txi+xindex*txi, 1-txi- yindex*txi,
                txi+xindex*txi, 1- yindex*txi,
                txi+xindex*txi, 1- yindex*txi,
                0+xindex*txi, 1- yindex*txi,
                0+xindex*txi, 1 - txi -yindex * txi

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
