using OpenTK.Graphics.OpenGL;
using StbImageSharp;
using System.Collections.Generic;
using System.IO;

namespace Blockgame_OpenTK.Util
{
    internal class Texture
    {

        int Id;
        ImageResult Image;
        public string Path;
        public string FileName;

        public byte[] Data;

        public int Width, Height;

        public Texture(string imageFile)
        {

            Id = GL.GenTexture();

            if (imageFile == null) imageFile = "missing.png";

            FileName = imageFile;
            Path = System.IO.Path.Combine(GlobalValues.TexturePath, imageFile);


            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2d, Id);

            StbImage.stbi_set_flip_vertically_on_load(1);
            if (imageFile == null)
            {

                Path = GlobalValues.MissingTexture;

            }

            using (FileStream stream = File.OpenRead(Path))
            {

                Image = ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);

            }

            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, Image.Width, Image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, Image.Data);

            GL.BindTexture(TextureTarget.Texture2d, 0);

            Width = Image.Width;
            Height = Image.Height;
            Data = Image.Data;

        }

        public Texture(string imageFile, int flip)
        {

            Id = GL.GenTexture();

            Path = GlobalValues.TexturePath + imageFile;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2d, Id);

            StbImage.stbi_set_flip_vertically_on_load(flip);
            if (Path == null)
            {

                Path = GlobalValues.MissingTexture;

            }
            Image = ImageResult.FromStream(File.OpenRead(Path), StbImageSharp.ColorComponents.RedGreenBlueAlpha);

            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, Image.Width, Image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, Image.Data);

            GL.BindTexture(TextureTarget.Texture2d, 0);

            Width = Image.Width;
            Height = Image.Height;
            Data = Image.Data;

        }
        public Texture(byte[] pixeldata, int width, int height)
        {

            Id = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2d, Id);

            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, Image.Width, Image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, Image.Data);

            GL.BindTexture(TextureTarget.Texture2d, 0);

            this.Data = pixeldata;
            this.Width = width;
            this.Height = height;

        }

        public Texture(nint data, int width, int height)
        {

            Id = GL.GenTexture();

            GL.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);
            GL.BindTexture(TextureTarget.Texture2d, Id);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.R8, width, height, 0, PixelFormat.Red, PixelType.UnsignedByte, data);

            GL.BindTexture(TextureTarget.Texture2d, 0);

        }
        // GetPortion takes in six inputs, but the inputs for the dimensions of the texture go from bottom left to top right, not top left to bottom right.
        public static Texture GetPortion(bool flip, Texture t, int xo, int yo, int width, int height)
        {

            byte[] tdata = t.Image.Data;
            int twidth = t.Image.Width;
            int theight = t.Image.Height;

            int c = 0;

            List<byte> datalist = new List<byte>();

            int yoffset;
            for (int y = 0; y < theight; y++)
            {

                yoffset = y * (twidth * 4);

                for (int x = 0; x < twidth * 4; x += 4)
                {

                    // Console.Log("{0}, {1}, {2}, {3}", tdata[yoffset + x], tdata[yoffset + x + 1], tdata[yoffset + x + 2], tdata[yoffset + x + 3]);

                    // Console.Log("{0}, {1}", x, y);

                    if (x / 4 >= xo && x / 4 <= xo + width - 1 && y >= yo && y <= yo + height - 1)
                    {

                        datalist.Add(tdata[yoffset + x]);
                        datalist.Add(tdata[yoffset + x + 1]);
                        datalist.Add(tdata[yoffset + x + 2]);
                        datalist.Add(tdata[yoffset + x + 3]);

                    }

                }
                // Console.Log("newstrip");

            }

            List<byte> datalistflipped = new List<byte>();

            if (flip == true)
            {

                for (int i = 0; i < datalist.Count; i+=4)
                {

                    int flipindex = datalist.Count - 1 - i;

                    datalistflipped.Add(datalist[flipindex-3]);
                    datalistflipped.Add(datalist[flipindex-2]);
                    datalistflipped.Add(datalist[flipindex-1]);
                    datalistflipped.Add(datalist[flipindex]);

                }
                datalist.Clear();
                datalist = datalistflipped;

            }

            return new Texture(datalist.ToArray(), width, height);

        }
        public int GetID()
        {

            return Id; 

        }
        public ImageResult GetImage()
        {
            return Image;
        }

        public void Dispose()
        {

            GL.DeleteTexture(Id);

        }

    }
}
