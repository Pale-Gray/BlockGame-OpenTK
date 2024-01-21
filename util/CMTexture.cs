using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace opentk_proj.util
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

            Texture tex = new Texture(t.path, 1);

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
            
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, PixelInternalFormat.Rgba, front.width, front.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, front.data);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat.Rgba, right.width, right.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, right.data);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, PixelInternalFormat.Rgba, back.width, back.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, back.data);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, PixelInternalFormat.Rgba, left.width, left.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, left.data);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, PixelInternalFormat.Rgba, top.width, top.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, top.data);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, PixelInternalFormat.Rgba, bottom.width, bottom.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, bottom.data);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

    }
}
