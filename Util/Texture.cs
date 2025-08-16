using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace VoxelGame;

public class Texture
{
    public int Id;
    private string _path;

    public int Width, Height;
    
    public Texture(string path)
    {
        _path = path;
    }

    public Texture Generate()
    {
        Id = GL.GenTexture();

        GL.BindTexture(TextureTarget.Texture2d, Id);
        using (Stream image = File.OpenRead(_path))
        {
            ImageResult imageResult = ImageResult.FromStream(image);
            Width = imageResult.Width;
            Height = imageResult.Height;
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, imageResult.Width, imageResult.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, imageResult.Data);
        }
        
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.NearestMipmapLinear);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
        
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMaxLevel, 4);
        GL.GenerateMipmap(TextureTarget.Texture2d);
        
        return this;
    }
}