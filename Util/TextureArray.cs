using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Blockgame_OpenTK.Util
{
    internal class TextureArray
    {

        string PathToTextures;
        int Width, Height, Depth;
        public int TextureID = 0;

        public TextureArray()
        {
            Width = 32;
            Height = 32;
            PathToTextures = Globals.LocalPath;

        }

        public void Load()
        {

            DebugMessage.WriteLine("Loading Textures", DebugMessageType.Info);

            StbImage.stbi_set_flip_vertically_on_load(1);
            string[] TextureNamesInDirectory = Directory.GetFiles(PathToTextures);
            Depth = TextureNamesInDirectory.Length;

            TextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, TextureID);
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba16, Width, Height, Depth);
            ImageResult temp = ImageResult.FromStream(File.OpenRead(TextureNamesInDirectory[0]), ColorComponents.RedGreenBlueAlpha);
            // List<ImageResult> Images = new List<ImageResult>();
            List<byte> ImageBytes = new List<byte>();

            foreach (string FileName in TextureNamesInDirectory)
            {

                DebugMessage.WriteLine($"Loading File {FileName}", DebugMessageType.Info);

                FileStream file = File.OpenRead(FileName);
                ImageBytes.AddRange(ImageResult.FromStream(file).Data);
                // Images.Add(ImageResult.FromStream(file, ColorComponents.RedGreenBlueAlpha));
                // GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, Array.IndexOf(TextureNamesInDirectory, FileName), Width, Height, Depth, PixelFormat.Rgba, PixelType.UnsignedByte, t.Data);
            }

            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, 0, Width, Height, Depth, PixelFormat.Rgba, PixelType.UnsignedByte, ImageBytes.ToArray());

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            DebugMessage.WriteLine("Finished Loading Textures", DebugMessageType.Info);

        }

    }
}
