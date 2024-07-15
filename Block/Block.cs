using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK.BlockUtil
{

    public enum BlockFace
    {

        Up,
        Down,
        Left,
        Right,
        Front,
        Back,
        All,
        Sides,
        Tops

    }

    internal class Block
    {
        public string DataName { get; set; }
        public string DisplayName { get; set; }
        [JsonConverter(typeof(JsonBlockModelConverter))]
        [JsonPropertyName("Model")]
        public BlockModel BlockModel { get; set; }
        [JsonPropertyName("Sounds")]
        public string SoundPath { get; set; }
        public int BreakTime { get; set; }


        public Block()
        {

            // BlockModel = BlockModel.Load(BlockModelPath + ".json");
            Console.WriteLine("loaded");

        }
        public static Block LoadFromJson(string fileName)
        {

            Console.WriteLine("loading");
            return JsonSerializer.Deserialize<Block>(File.ReadAllText(Globals.BlockDataPath + fileName));

        }

        public Block Register()
        {

            Blocks.BlockList.Add(this);
            return this;

        }
        public Block SetFaceTexture(int x, int y, params int[] index)
        {

            /* foreach (int i in index)
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
            */

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

        }

    }
}
