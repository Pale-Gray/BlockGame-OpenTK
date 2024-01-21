using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using StbImageWriteSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj
{
    internal class Texture
    {

        public int id;
        public ImageResult img;
        public string path;

        public byte[] data;

        public int width, height;

        public Texture(string pathtoimage)
        {

            id = GL.GenTexture();

            path = pathtoimage;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, id);

            StbImage.stbi_set_flip_vertically_on_load(1);
            if (pathtoimage == null)
            {

                pathtoimage = "../../../res/textures/missing.png";

            }
            img = ImageResult.FromStream(File.OpenRead(pathtoimage), StbImageSharp.ColorComponents.RedGreenBlueAlpha);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.Data);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            width = img.Width;
            height = img.Height;

        }

        public Texture(string pathtoimage, int flip)
        {

            id = GL.GenTexture();

            path = pathtoimage;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, id);

            StbImage.stbi_set_flip_vertically_on_load(flip);
            if (pathtoimage == null)
            {

                pathtoimage = "../../../res/textures/missing.png";

            }
            img = ImageResult.FromStream(File.OpenRead(pathtoimage), StbImageSharp.ColorComponents.RedGreenBlueAlpha);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.Data);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            width = img.Width;
            height = img.Height;

        }
        public Texture(byte[] pixeldata, int width, int height)
        {

            id = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, id);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixeldata);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            this.data = pixeldata;
            this.width = width;
            this.height = height;

        }
        // GetPortion takes in six inputs, but the inputs for the dimensions of the texture go from bottom left to top right, not top left to bottom right.
        public static Texture GetPortion(bool flip, Texture t, int xo, int yo, int width, int height)
        {

            byte[] tdata = t.img.Data;
            int twidth = t.img.Width;
            int theight = t.img.Height;

            int c = 0;

            List<byte> datalist = new List<byte>();

            int yoffset;
            for (int y = 0; y < theight; y++)
            {

                yoffset = y * (twidth * 4);

                for (int x = 0; x < twidth * 4; x += 4)
                {

                    // Console.WriteLine("{0}, {1}, {2}, {3}", tdata[yoffset + x], tdata[yoffset + x + 1], tdata[yoffset + x + 2], tdata[yoffset + x + 3]);

                    // Console.WriteLine("{0}, {1}", x, y);

                    if (x / 4 >= xo && x / 4 <= xo + width - 1 && y >= yo && y <= yo + height - 1)
                    {

                        datalist.Add(tdata[yoffset + x]);
                        datalist.Add(tdata[yoffset + x + 1]);
                        datalist.Add(tdata[yoffset + x + 2]);
                        datalist.Add(tdata[yoffset + x + 3]);

                    }

                }
                // Console.WriteLine("newstrip");

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
        public int getID()
        {

            return id; 

        }
        public ImageResult getImage()
        {
            return img;
        }



    }
}
