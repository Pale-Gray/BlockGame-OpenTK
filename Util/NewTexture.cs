using System.IO;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace Game.Core.Graphics;

public class NewTexture
{

    public int Handle { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public NewTexture(string pathToFile, TextureMinFilter minFilter, TextureMagFilter magFilter)
    {

        using (Stream image = File.OpenRead(pathToFile))
        {

            ImageResult imageResult = ImageResult.FromStream(image);

            Handle = GL.CreateTexture(TextureTarget.Texture2d);
            GL.TextureParameteri(Handle, TextureParameterName.TextureMinFilter, (int) minFilter);
            GL.TextureParameteri(Handle, TextureParameterName.TextureMagFilter, (int) magFilter);
            GL.TextureParameteri(Handle, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
            GL.TextureParameteri(Handle, TextureParameterName.TextureWrapT, (int) TextureWrapMode.MirroredRepeat);
            GL.TextureStorage2D(Handle, 1, SizedInternalFormat.Srgb8Alpha8, imageResult.Width, imageResult.Height);
            GL.TextureSubImage2D(Handle, 0, 0, 0, imageResult.Width, imageResult.Height, PixelFormat.Rgba, PixelType.UnsignedByte, imageResult.Data);

        }

    }

    public void Bind(uint textureUnit)
    {

        GL.BindTextureUnit(textureUnit, Handle);

    }

}