using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace Blockgame_OpenTK.Core.TexturePack 
{

    public class TexturePackManager
    {

        private static Dictionary<string, int> _textureArrayIndices = new();

        public static int ArrayTextureName { get; private set; }

        public static int GetTextureIndex(string textureName) 
        {

            if (_textureArrayIndices.TryGetValue(textureName, out int index)) 
            {
                return index;
            }
            return _textureArrayIndices["MissingTexture"];

        }

        public static void LoadTexturePack(string directory) 
        {

            GameLogger.Log("loading array texture");
            StbImage.stbi_set_flip_vertically_on_load(1);
            if (Directory.Exists(directory)) 
            {

                ArrayTextureName = GL.CreateTexture(TextureTarget.Texture2dArray);
                GL.TextureStorage3D(ArrayTextureName, 4, SizedInternalFormat.Srgb8Alpha8, 32, 32, Directory.GetFiles(directory).Length);
                GL.TextureParameteri(ArrayTextureName, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.NearestMipmapLinear);
                GL.TextureParameteri(ArrayTextureName, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
                GL.TextureParameteri(ArrayTextureName, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
                GL.TextureParameteri(ArrayTextureName, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
                foreach (string file in Directory.GetFiles(directory)) 
                {

                    using (FileStream imageFile = File.OpenRead(file))
                    {

                        ImageResult image = ImageResult.FromStream(imageFile);
                        int currentCount = _textureArrayIndices.Count;
                        GL.TextureSubImage3D(ArrayTextureName, 0, 0, 0, currentCount, 32, 32, 1, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
                        string fileName = file.Split(Path.DirectorySeparatorChar).Last().Split('.')[0];
                        _textureArrayIndices.Add(fileName, currentCount);
                        Console.WriteLine($"{fileName}, {_textureArrayIndices[fileName]}");

                    }

                }
                GL.GenerateTextureMipmap(ArrayTextureName);

            } else 
            {
                GameLogger.Log("Directory doesnt exist.", Severity.Error);
            }

        }

        public static void Free() {

            GL.DeleteTexture(ArrayTextureName);

        }

    }

}