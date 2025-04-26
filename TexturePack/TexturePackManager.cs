using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Game.Util;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;
using Tomlet;

namespace Game.Core.TexturePack 
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

        public static int ArrayTextureHandle { get; private set; }
        private static Dictionary<string, TexturePackInfo> _availableTexturePacks = new();
        public static Dictionary<string, TexturePackInfo> AvailableTexturePacks => _availableTexturePacks;
        private static List<byte[]> _loadedTextures = new();

        public static uint GetTextureIndex(string textureName) 
        {

            if (_textureArrayIndices.TryGetValue(textureName, out int index)) 
            {
                return (uint) index;
            }
            return (uint) _textureArrayIndices["MissingTexture"];

        }

        public static void LoadTexturePack(TexturePackInfo texturePack) 
        {

            if (texturePack.Path.EndsWith(".zip"))
            {

                using (Zip archive = Zip.Open(texturePack.Path))
                {

                    List<ZipArchiveEntry> blockFiles = archive.GetFilesInDirectory(Path.Combine("Blocks"));
                    StbImage.stbi_set_flip_vertically_on_load(1);
                    ArrayTextureHandle = GL.CreateTexture(TextureTarget.Texture2dArray);
                    GL.TextureStorage3D(ArrayTextureHandle, 4, SizedInternalFormat.Srgb8Alpha8, 16, 16, blockFiles.Count);
                    GL.TextureParameteri(ArrayTextureHandle, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.NearestMipmapLinear);
                    GL.TextureParameteri(ArrayTextureHandle, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
                    GL.TextureParameteri(ArrayTextureHandle, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
                    GL.TextureParameteri(ArrayTextureHandle, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
                    foreach (ZipArchiveEntry entry in blockFiles)
                    {
                        
                        byte[] imageBytes = new byte[entry.Length];
                        using (Stream stream = entry.Open())
                        {

                            stream.ReadExactly(imageBytes);

                            ImageResult image = ImageResult.FromMemory(imageBytes);
                            int currentCount = _textureArrayIndices.Count;
                            GL.TextureSubImage3D(ArrayTextureHandle, 0, 0, 0, currentCount, 16, 16, 1, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
                            string fileName = entry.FullName.Split('/').Last().Split('.')[0];
                            _textureArrayIndices.Add(fileName, currentCount);
                            Console.WriteLine($"{fileName}, {_textureArrayIndices[fileName]}");

                        }

                    }
                    GL.GenerateTextureMipmap(ArrayTextureHandle);

                }

            } else
            {

                string blockTexturePath = Path.Combine(texturePack.Path, "Blocks");
                Console.WriteLine(blockTexturePath);

                GameLogger.Log("loading array texture");
                StbImage.stbi_set_flip_vertically_on_load(1);
                if (Directory.Exists(blockTexturePath))
                {

                    ArrayTextureHandle = GL.CreateTexture(TextureTarget.Texture2dArray);
                    // GL.TextureStorage3D(ArrayTextureHandle, 4, SizedInternalFormat.Srgb8Alpha8, 16, 16, Directory.GetFiles(blockTexturePath).Length);
                    // GL.TextureParameteri(ArrayTextureHandle, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.NearestMipmapLinear);
                    // GL.TextureParameteri(ArrayTextureHandle, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
                    // GL.TextureParameteri(ArrayTextureHandle, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
                    // GL.TextureParameteri(ArrayTextureHandle, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
                    foreach (string file in Directory.GetFiles(blockTexturePath).Where(file => file.EndsWith(".toml")))
                    {

                        // Console.WriteLine(file);
                        AnimatedTextureProperties abstractProperties = TomletMain.To<AnimatedTextureProperties>(File.ReadAllText(file));
                        abstractProperties.Size = (16, 16);
                        abstractProperties.Frames = new();
                        AnimatedTextureManager.AnimatedTextureIndices.Add(abstractProperties.TextureHandle, AnimatedTextureManager.AnimatexTextures.Count);
                        AnimatedTextureManager.AnimatexTextures.Add(abstractProperties);

                    }

                    foreach (string file in Directory.GetFiles(blockTexturePath).Where(file => file.EndsWith(".png"))) 
                    {

                        string fileName = file.Split(Path.DirectorySeparatorChar).Last().Split('.')[0];
                        string fileNamePrefix = fileName.Split('_')[0];

                        Console.WriteLine($"file name: {fileName}, prefix: {fileNamePrefix}");

                        using (FileStream imageStream = File.OpenRead(file))
                        {

                            ImageResult image = ImageResult.FromStream(imageStream);

                            if (AnimatedTextureManager.AnimatedTextureIndices.ContainsKey(fileNamePrefix))
                            {

                                if (fileName.Split('_').Last() == "1")
                                {

                                    Console.WriteLine($"filename: {fileName}");

                                    _textureArrayIndices.Add(fileNamePrefix, _textureArrayIndices.Count);
                                    AnimatedTextureProperties properties = AnimatedTextureManager.AnimatexTextures[AnimatedTextureManager.AnimatedTextureIndices[fileNamePrefix]];
                                    properties.TextureSlotIndex = _loadedTextures.Count;
                                    AnimatedTextureManager.AnimatexTextures[AnimatedTextureManager.AnimatedTextureIndices[fileNamePrefix]] = properties;
                                    _loadedTextures.Add(image.Data);

                                } 
                                AnimatedTextureManager.AnimatexTextures[AnimatedTextureManager.AnimatedTextureIndices[fileNamePrefix]].Frames.Add(int.Parse(fileName.Split('_').Last()) - 1, image.Data);

                            } else
                            {

                                _textureArrayIndices.TryAdd(fileNamePrefix, _textureArrayIndices.Count);
                                _loadedTextures.Add(image.Data);

                            }

                        }

                        /*
                        if (AnimatedTextureManager.AnimatedTextures.ContainsKey(fileNamePrefix))
                        {

                            // image is part of an animated texture.
                            if (fileName.EndsWith('1'))
                            {



                            }

                        }

                        Console.WriteLine(file.Split('/').Last().Split('.')[0]);
                        

                        using (FileStream imageFile = File.OpenRead(file))
                        {

                            ImageResult image = ImageResult.FromStream(imageFile);
                            int currentCount = _textureArrayIndices.Count;
                            GL.TextureSubImage3D(ArrayTextureHandle, 0, 0, 0, currentCount, 16, 16, 1, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
                            // string fileName = file.Split('/').Last().Split('.')[0];
                            _textureArrayIndices.Add(fileName, currentCount);
                            // Console.WriteLine($"{fileName}, {_textureArrayIndices[fileName]}");

                        }
                        */

                    }

                    GL.TextureStorage3D(ArrayTextureHandle, 4, SizedInternalFormat.Srgb8Alpha8, 16, 16, _loadedTextures.Count());
                    GL.TextureParameteri(ArrayTextureHandle, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.NearestMipmapLinear);
                    GL.TextureParameteri(ArrayTextureHandle, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
                    GL.TextureParameteri(ArrayTextureHandle, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
                    GL.TextureParameteri(ArrayTextureHandle, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
                    for (int i = 0; i < _loadedTextures.Count(); i++)
                    {
                        GL.TextureSubImage3D(ArrayTextureHandle, 0, 0, 0, i, 16, 16, 1, PixelFormat.Rgba, PixelType.UnsignedByte, _loadedTextures[i]);
                    }
                    GL.GenerateTextureMipmap(ArrayTextureHandle);

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

                            _availableTexturePacks.Add(dir.Split('/').Last().Split('.')[0], texturePackInfo);

                        }

                    }

                }

            }

        }

        public static void Free() {

            GL.DeleteTexture(ArrayTextureHandle);

        }

    }

}