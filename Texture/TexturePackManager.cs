using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;
using Tomlet;

namespace Blockgame_OpenTK.Core.TexturePack 
{

    public struct TexturePackInfo
    {

        public string Name = "Missing Name";
        public string Description = "Missing Discription";
        public string Path = System.IO.Path.Combine("TexturePacks", "Default");
        public string VisualPath = System.IO.Path.Combine("TexturePacks", "Default");
        public Texture Icon;

        public TexturePackInfo() {}

        public override string ToString()
        {
            
            return $"""
                    {Name}
                        {Description}
                        Located at {VisualPath}
                        Absolute Path: {Path}
                    """;

        }

    };

    public class TexturePackManager
    {
        private static Dictionary<string, int> _textureArrayIndices = new();

        public static int ArrayTextureName { get; private set; }
        private static Dictionary<string, TexturePackInfo> _availableTexturePacks = new();
        public static Dictionary<string, TexturePackInfo> AvailableTexturePacks => _availableTexturePacks;

        public static int GetTextureIndex(string textureName) 
        {

            if (_textureArrayIndices.TryGetValue(textureName, out int index)) 
            {
                return index;
            }
            return _textureArrayIndices["MissingTexture"];

        }

        public static void LoadTexturePack(TexturePackInfo texturePack) 
        {

            if (texturePack.Path.EndsWith(".zip"))
            {

                using (Zip archive = Zip.Open(texturePack.Path))
                {

                    List<ZipArchiveEntry> blockFiles = archive.GetFilesInDirectory(Path.Combine("Blocks"));
                    StbImage.stbi_set_flip_vertically_on_load(1);
                    ArrayTextureName = GL.CreateTexture(TextureTarget.Texture2dArray);
                    GL.TextureStorage3D(ArrayTextureName, 4, SizedInternalFormat.Srgb8Alpha8, 16, 16, blockFiles.Count);
                    GL.TextureParameteri(ArrayTextureName, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.NearestMipmapLinear);
                    GL.TextureParameteri(ArrayTextureName, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
                    GL.TextureParameteri(ArrayTextureName, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
                    GL.TextureParameteri(ArrayTextureName, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
                    foreach (ZipArchiveEntry entry in blockFiles)
                    {
                        
                        byte[] imageBytes = new byte[entry.Length];
                        using (Stream stream = entry.Open())
                        {

                            stream.ReadExactly(imageBytes);

                            ImageResult image = ImageResult.FromMemory(imageBytes);
                            int currentCount = _textureArrayIndices.Count;
                            GL.TextureSubImage3D(ArrayTextureName, 0, 0, 0, currentCount, 16, 16, 1, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
                            string fileName = entry.FullName.Split(Path.DirectorySeparatorChar).Last().Split('.')[0];
                            _textureArrayIndices.Add(fileName, currentCount);
                            Console.WriteLine($"{fileName}, {_textureArrayIndices[fileName]}");

                        }

                    }
                    GL.GenerateTextureMipmap(ArrayTextureName);

                }

            } else
            {

                string blockTexturePath = Path.Combine(texturePack.Path, "Blocks");
                Console.WriteLine(blockTexturePath);

                GameLogger.Log("loading array texture");
                StbImage.stbi_set_flip_vertically_on_load(1);
                if (Directory.Exists(blockTexturePath))
                {

                    ArrayTextureName = GL.CreateTexture(TextureTarget.Texture2dArray);
                    GL.TextureStorage3D(ArrayTextureName, 4, SizedInternalFormat.Srgb8Alpha8, 16, 16, Directory.GetFiles(blockTexturePath).Length);
                    GL.TextureParameteri(ArrayTextureName, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.NearestMipmapLinear);
                    GL.TextureParameteri(ArrayTextureName, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
                    GL.TextureParameteri(ArrayTextureName, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
                    GL.TextureParameteri(ArrayTextureName, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
                    foreach (string file in Directory.GetFiles(blockTexturePath)) 
                    {

                        using (FileStream imageFile = File.OpenRead(file))
                        {

                            ImageResult image = ImageResult.FromStream(imageFile);
                            int currentCount = _textureArrayIndices.Count;
                            GL.TextureSubImage3D(ArrayTextureName, 0, 0, 0, currentCount, 16, 16, 1, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
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

        }

        public static void IterateAvailableTexturePacks()
        {
            
            foreach (TexturePackInfo texturePack in _availableTexturePacks.Values)
            {

                texturePack.Icon.Dispose();

            }
            _availableTexturePacks.Clear();

            foreach (string dir in Directory.GetFileSystemEntries(GlobalValues.TexturePackPath))
            {

                if (Directory.Exists(dir))
                {

                    TexturePackInfo texturePack = new TexturePackInfo();
                    if (File.Exists(Path.Combine(dir, "info.toml")))
                    {

                        Dictionary<string, string> packProperties = TomletMain.To<Dictionary<string, string>>(File.ReadAllText(Path.Combine(dir, "info.toml")));
                        if (packProperties.TryGetValue("name", out string name)) texturePack.Name = name;
                        if (packProperties.TryGetValue("description", out string description)) texturePack.Description = description;
                        texturePack.Path = dir;
                        texturePack.VisualPath = dir;

                        if (File.Exists(Path.Combine(dir, "icon.png")))
                        {
                            texturePack.Icon = new Texture(Path.Combine(dir, "icon.png"));
                        } else 
                        {
                            texturePack.Icon = GlobalValues.MissingTexturePackIcon;
                        }

                        _availableTexturePacks.Add(dir.Split(Path.DirectorySeparatorChar).Last(), texturePack);

                    }

                } else if (dir.EndsWith(".zip"))
                { 

                    using (ZipArchive archive = ZipFile.OpenRead(dir))
                    {

                        ZipArchiveEntry infoEntry = archive.Entries.Where(entry => entry.Name == "info.toml").First();
                        if (infoEntry != null)
                        {

                            TexturePackInfo texturePackInfo = new TexturePackInfo();
                            using (StreamReader reader = new StreamReader(infoEntry.Open()))
                            {
                                Dictionary<string, string> packProperties = TomletMain.To<Dictionary<string, string>>(reader.ReadToEnd());
                                if (packProperties.TryGetValue("name", out string name)) texturePackInfo.Name = name;
                                if (packProperties.TryGetValue("description", out string description)) texturePackInfo.Description = description;
                            }

                            texturePackInfo.Path = dir;
                            texturePackInfo.VisualPath = dir;

                            _availableTexturePacks.Add(dir.Split(Path.DirectorySeparatorChar).Last().Split('.')[0], texturePackInfo);

                        }

                    }

                }

            }

        }

        public static void Free() {

            GL.DeleteTexture(ArrayTextureName);

        }

    }

}