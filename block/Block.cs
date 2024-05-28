using System;

using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK.BlockUtil
{
    internal class Block
    {

        public string Name = "Placeholder";
        public ChunkVertex[] FrontFace = new ChunkVertex[6];
        public ChunkVertex[] RightFace = new ChunkVertex[6];
        public ChunkVertex[] BackFace = new ChunkVertex[6];
        public ChunkVertex[] LeftFace = new ChunkVertex[6];
        public ChunkVertex[] TopFace = new ChunkVertex[6];
        public ChunkVertex[] BottomFace = new ChunkVertex[6];
        public Block(string name)
        {

            Name = name;

            Array.Copy(Faces.FrontFace, FrontFace, Faces.FrontFace.Length);
            Array.Copy(Faces.RightFace, RightFace, Faces.RightFace.Length);
            Array.Copy(Faces.BackFace, BackFace, Faces.BackFace.Length);
            Array.Copy(Faces.LeftFace, LeftFace, Faces.LeftFace.Length);
            Array.Copy(Faces.TopFace, TopFace, Faces.TopFace.Length);
            Array.Copy(Faces.BottomFace, BottomFace, Faces.BottomFace.Length);

            // FrontFace = Faces.FrontFace;
            // RightFace = Faces.RightFace;
            // BackFace = Faces.BackFace;
            // LeftFace = Faces.LeftFace;
            // TopFace = Faces.TopFace;
            // BottomFace = Faces.BottomFace;

        }

        public Block Register()
        {

            Blocks.BlockList.Add(this);
            return this;

        }
        public Block SetFaceTexture(int x, int y, params int[] index)
        {

            
            foreach (int i in index)
            {

                switch (i)
                {

                    case 0:
                        SetTextureCoordinates(FrontFace, x, y);
                        break;
                    case 1:
                        SetTextureCoordinates(RightFace, x, y);
                        break;
                    case 2:
                        SetTextureCoordinates(BackFace, x, y);
                        break;
                    case 3:
                        SetTextureCoordinates(LeftFace, x, y);
                        break;
                    case 4:
                        SetTextureCoordinates(TopFace, x, y);
                        break;
                    case 5:
                        SetTextureCoordinates(BottomFace, x, y);
                        break;

                }

            }

            return this;

        }
        public static ChunkVertex[] GetFaceShifted(ChunkVertex[] face, int x, int y, int z, float ambientValue)
        {

            ChunkVertex[] tmp = new ChunkVertex[6];
            face.CopyTo(tmp, 0);
            tmp[0].Position += (x, y, z);
            tmp[1].Position += (x, y, z);
            tmp[2].Position += (x, y, z);
            tmp[3].Position += (x, y, z);
            tmp[4].Position += (x, y, z);
            tmp[5].Position += (x, y, z);

            tmp[0].AmbientValue = ambientValue;
            tmp[1].AmbientValue = ambientValue;
            tmp[2].AmbientValue = ambientValue;
            tmp[3].AmbientValue = ambientValue;
            tmp[4].AmbientValue = ambientValue;
            tmp[5].AmbientValue = ambientValue;
            return tmp;

        }
        private void SetTextureCoordinates(ChunkVertex[] face, int x, int y)
        {

            face[0].TextureCoordinates = (Globals.AtlasTexture.RatioX * x, 1 - Globals.AtlasTexture.RatioY * y);
            face[1].TextureCoordinates = (Globals.AtlasTexture.RatioX * x, 1 - Globals.AtlasTexture.RatioY - (Globals.AtlasTexture.RatioY * y));
            face[2].TextureCoordinates = (Globals.AtlasTexture.RatioX + Globals.AtlasTexture.RatioY * x, 1 - Globals.AtlasTexture.RatioY - (Globals.AtlasTexture.RatioY * y));

            face[3].TextureCoordinates = (Globals.AtlasTexture.RatioX + Globals.AtlasTexture.RatioY * x, 1 - Globals.AtlasTexture.RatioY - (Globals.AtlasTexture.RatioY * y));
            face[4].TextureCoordinates = (Globals.AtlasTexture.RatioX + Globals.AtlasTexture.RatioY * x, 1 - Globals.AtlasTexture.RatioY * y);
            face[5].TextureCoordinates = (Globals.AtlasTexture.RatioX * x, 1 - Globals.AtlasTexture.RatioY * y);

            // face[0].U = Globals.Ratio * x; face[0].V = 1 - Globals.Ratio * y;
            // face[1].U = Globals.Ratio * x; face[1].V = 1 - Globals.Ratio - (Globals.Ratio * y);
            // face[2].U = Globals.Ratio + Globals.Ratio * x; face[2].V = 1 - Globals.Ratio - (Globals.Ratio * y);

        }

    }
}
