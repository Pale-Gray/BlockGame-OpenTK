using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace Blockgame_OpenTK.Util
{
    internal class ArrayTexture
    {

        string PathToTextures;
        int Width, Height, Depth;
        public int TextureID = 0;
        public List<string> TextureNames = new List<string>();
        private List<byte> _arrayData = new List<byte>();
        private int _currentDepth = 1;
        private int _currentEmptyIndex = 0;

        public ArrayTexture()
        {

            Width = 32;
            Height = 32;
            PathToTextures = GlobalValues.LocalPath;

        }

        public ArrayTexture(int width, int height)
        {

            Width = width;
            Height = height;

        }

        public void Init()
        {

            TextureID = GL.GenTexture();
            GL.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);
            GL.BindTexture(TextureTarget.Texture2dArray, TextureID);
            GL.TexStorage3D(TextureTarget.Texture2dArray, 1, SizedInternalFormat.Srgb8, Width, Height, GL.GetInteger(GetPName.MaxArrayTextureLayers));
            GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.BindTexture(TextureTarget.Texture2dArray, 0);

        }

        public void AddTexture(nint data, int width, int height, out float index)
        {

            GL.BindTexture(TextureTarget.Texture2dArray, TextureID);
            GL.TexSubImage3D(TextureTarget.Texture2dArray, 0, 0, 0, _currentEmptyIndex, width, height, 1, PixelFormat.Red, PixelType.UnsignedByte, data);
            GL.BindTexture(TextureTarget.Texture2dArray, 0);

            index = _currentEmptyIndex;
            _currentEmptyIndex++;

        }

        public void Load()
        {

            Debugger.Log("Loading Textures", Severity.Info);

            StbImage.stbi_set_flip_vertically_on_load(1);
            string[] TextureNamesInDirectory = Directory.GetFiles(PathToTextures);
            
            Depth = TextureNamesInDirectory.Length;

            TextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2dArray, TextureID);
            GL.TexStorage3D(TextureTarget.Texture2dArray, 4, SizedInternalFormat.Srgb8Alpha8, Width, Height, Depth);
            List<byte> ImageBytes = new List<byte>();

            foreach (string FileName in TextureNamesInDirectory)
            {

                Debugger.Log($"Loading File {FileName}", Severity.Info);

                FileStream file = File.OpenRead(FileName);
                ImageBytes.AddRange(ImageResult.FromStream(file).Data);
                TextureNames.Add(file.Name.Split(Path.DirectorySeparatorChar).Last().Split(".")[0]);
                Debugger.Log($"Added {file.Name.Split(Path.DirectorySeparatorChar).Last().Split(".")[0]} to TextureNames", Severity.Info);

            }

            GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
            GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexSubImage3D(TextureTarget.Texture2dArray, 0, 0, 0, 0, Width, Height, Depth, PixelFormat.Rgba, PixelType.UnsignedByte, ImageBytes.ToArray());
            GL.GenerateMipmap(TextureTarget.Texture2dArray);

            Debugger.Log("Finished Loading Textures", Severity.Info);

            GL.BindTexture(TextureTarget.Texture2dArray, 0);

        }

        public int GetTextureIndex(string textureName)
        {

            if (!TextureNames.Contains(textureName))
            {

                return TextureNames.IndexOf("MissingTexture");

            }
            return TextureNames.IndexOf(textureName);

        }

    }
}
