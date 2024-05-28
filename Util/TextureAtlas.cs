namespace Blockgame_OpenTK.Util
{
    internal class TextureAtlas : Texture
    {

        public float RatioX, RatioY;

        public TextureAtlas(string fileName, int textureResolution) : base(fileName)
        {

            RatioX = 1f / (Width / (float)textureResolution);
            RatioY = 1f / (Height / (float)textureResolution);

        }

        public TextureAtlas(byte[] pixeldata, int width, int height) : base(pixeldata, width, height)
        {
        }

    }
}
