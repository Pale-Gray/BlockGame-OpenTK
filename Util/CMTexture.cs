﻿using OpenTK.Graphics.OpenGL;

namespace Game.Util
{
    public class CMTexture
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

            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
            
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, InternalFormat.Rgba, front.Width, front.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, front.Data);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, InternalFormat.Rgba, right.Width, right.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, right.Data);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, InternalFormat.Rgba, back.Width, back.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, back.Data);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, InternalFormat.Rgba, left.Width, left.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, left.Data);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, InternalFormat.Rgba, top.Width, top.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, top.Data);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, InternalFormat.Rgba, bottom.Width, bottom.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, bottom.Data);

            GL.BindTexture(TextureTarget.Texture2d, 0);
        }

    }
}
