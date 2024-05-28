using OpenTK.Graphics.OpenGL4;

namespace Blockgame_OpenTK.Util
{
    internal class CMTexture
    {

        public int id;
        Texture front;
        Texture right;
        Texture back;
        Texture left;
        Texture top;
        Texture bottom;

        public CMTexture(Texture t, int tS) // tS is tileSize
        {

            id = GL.GenTexture();

            Texture tex = new Texture(t.FileName, 1);

            front = Texture.GetPortion(true, tex, 0, tS, tS, tS);
            right = Texture.GetPortion(true, tex, tS, tS, tS, tS);
            back = Texture.GetPortion(true, tex, tS+tS, tS, tS, tS);
            left = Texture.GetPortion(true, tex, tS+tS+tS, tS, tS, tS);
            top = Texture.GetPortion(true, tex, 0, 0, tS, tS);
            bottom = Texture.GetPortion(true, tex, tS, 0, tS, tS);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.TextureCubeMap, id);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
            
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, PixelInternalFormat.Rgba, front.Width, front.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, front.Data);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat.Rgba, right.Width, right.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, right.Data);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, PixelInternalFormat.Rgba, back.Width, back.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, back.Data);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, PixelInternalFormat.Rgba, left.Width, left.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, left.Data);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, PixelInternalFormat.Rgba, top.Width, top.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, top.Data);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, PixelInternalFormat.Rgba, bottom.Width, bottom.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, bottom.Data);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

    }
}
