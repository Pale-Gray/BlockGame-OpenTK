using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Util
{
    internal class Image
    {

        public int Width = 0;
        public int Height = 0;
        public int ColorType = 0;
        public int BitDepth = 0;
        public byte[] ImageData;
        public byte[] RawImageData;

        public static Image LoadPng(string filename, bool shouldFlipOnY)
        {

            Image image = new Image();

            byte[] rawBytes = File.ReadAllBytes(filename);

            // check if the file has 'PNG' located in its header
            if (Encoding.ASCII.GetString(new ArraySegment<byte>(rawBytes, 1, 3)) != "PNG")
            {

                // Console.Log("Could not load the image because it is not of the PNG format");
                // Console.Log("Loading invalid texture PNG");
                image = LoadPng("Resources/Textures/invalid.png", true);
                return image;

            }

            // we don't need the file header anymore, so skip it
            rawBytes = rawBytes.Skip(8).ToArray();

            List<byte> compressedImageData = new List<byte>();

            while (Encoding.ASCII.GetString(new ArraySegment<byte>(rawBytes, 4, 4)) != "IEND")
            {

                int length = BitConverter.ToInt32(new ArraySegment<byte>(rawBytes, 0, 4).Reverse().ToArray());
                string type = Encoding.ASCII.GetString(new ArraySegment<byte>(rawBytes, 4, 4));

                // Console.Log(type);

                if (type == "IHDR") // important things like width and height
                {

                    image.Width = BitConverter.ToInt32(new ArraySegment<byte>(rawBytes, 8, 4).Reverse().ToArray());
                    image.Height = BitConverter.ToInt32(new ArraySegment<byte>(rawBytes, 12, 4).Reverse().ToArray());
                    int bitDepth = rawBytes[16];
                    int colorType = rawBytes[17];

                    // Console.Log("Height is " + image.Height);
                    // Console.Log("Color type is " + colorType);
                    // Console.Log("bit depth is " + bitDepth);

                    if (colorType != 6)
                    {

                        // throw new Exception("Cannot load file beacuse it is not of color type 6 (RGBA)");
                        // Console.Log("Could not load the image because is it not of color type 6 (RGBA)");
                        // Console.Log("Loading invalid texture PNG");
                        image = LoadPng("Resources/Textures/invalid.png", true);
                        return image;

                    }
                    image.ColorType = colorType;
                    if (bitDepth != 8)
                    {

                        // throw new Exception("Cannot load file because it is not of bit depth 8 (32BPP)");
                        // Console.Log("Could not load the image beacuse it is not of bit depth 8 (32BPP)");
                        // Console.Log("Loading invalid texture PNG");
                        image = LoadPng("Resources/Textures/invalid.png", true);
                        return image;

                    }
                    image.BitDepth = bitDepth;
                    

                }

                if (type == "IDAT") // this is the image data itself
                {

                    compressedImageData.AddRange(new ArraySegment<byte>(rawBytes, 8, length));

                }

                // skips the chunk since we already read it
                rawBytes = rawBytes.Skip(12 + length).ToArray();

            }

            byte[] decompressedImageData = Decompress(compressedImageData.ToArray());

            image.ImageData = DeFilter(image, decompressedImageData);

            if (shouldFlipOnY) image.FlipY();

            return image;

        }

        private static byte[] Decompress(byte[] compressedData)
        {

            MemoryStream decompressedData = new MemoryStream();
            ZLibStream decompressor = new ZLibStream(new MemoryStream(compressedData), CompressionMode.Decompress);
            decompressor.CopyTo(decompressedData);

            return decompressedData.ToArray();

        }

        private static byte[] DeFilter(Image image, byte[] rawBytes)
        {

            byte[] filterCodes = GetFilterCodes(image, rawBytes);
            byte[] rawImageData = GetRawImageData(image, rawBytes);
            image.RawImageData = rawImageData;

            for (int l = 0; l < filterCodes.Length; l++)
            {

                // Console.Log("Filter code: " + filterCodes[l]);
                if (filterCodes[l] == 1) // undo sub filter
                {

                    rawImageData = UnSubLine(image, rawImageData, l);

                }
                if (filterCodes[l] == 2) // undo up filter
                {

                    rawImageData = UnUpLine(image, rawImageData, l);

                }
                if (filterCodes[l] == 3) // undo mean filter
                {

                    rawImageData = UnMeanLine(image, rawImageData, l);

                }
                if (filterCodes[l] == 4) // undo paeth predictor filter
                {

                    rawImageData = UnPaethLine(image, rawImageData, l);

                }
            }

            return rawImageData;

        }

        public void FlipY()
        {

            // Console.Log("The width flip is " + Width);
            // Console.Log("The height flip is " + Height);


            List<byte> flippedData = new List<byte>();

            for (int l = Height - 1; l >= 0; l--)
            {

                flippedData.AddRange(GetLine(l));

            }

            ImageData = flippedData.ToArray();

        }
        public static byte[] UnPaethLine(Image image, byte[] data, int lineIndex)
        {

            byte[] unfilteredImage = (byte[])data.Clone();
            int startIndex = lineIndex * (image.Width * 4);

            for (int i = startIndex; i < startIndex + (image.Width * 4); i++)
            {

                unfilteredImage[i] = UnPaethByte(image, unfilteredImage, i, startIndex);

            }

            return unfilteredImage;

        }

        public static byte UnPaethByte(Image image, byte[] data, int index, int minValue)
        {

            byte paethPredictByte = PaethPredict(image, data, index, minValue);

            int value = data[index] + paethPredictByte;

            return (byte) (value%256);

        }

        public static byte PaethPredict(Image image, byte[] data, int index, int minValue)
        {

            byte leftValue = 0;
            byte upLeftValue = 0;
            if (!(index - 4 < minValue))
            {

                leftValue = data[index - 4];
                upLeftValue = data[index - 4 - (image.Width * 4)];

            }
            byte upValue = data[index - (image.Width * 4)];

            int basePixel = leftValue + upValue - upLeftValue;
            int diffLeft = Math.Abs(basePixel - leftValue);
            int diffUp = Math.Abs(basePixel - upValue);
            int diffUpLeft = Math.Abs(basePixel - upLeftValue);
            if (diffLeft <= diffUp && diffLeft <= diffUpLeft) return leftValue;
            else if (diffUp <= diffUpLeft) return upValue;
            return upLeftValue;

        }

        public static byte[] UnMeanLine(Image image, byte[] data, int lineIndex)
        {

            byte[] unfilteredImage = (byte[])data.Clone();
            int startIndex = lineIndex * (image.Width * 4);

            for (int i = startIndex; i < startIndex + (image.Width * 4); i++)
            {

                unfilteredImage[i] = UnMeanByte(image, unfilteredImage, i, startIndex);

            }

            return unfilteredImage;

        }

        public static byte UnMeanByte(Image image, byte[] data, int index, int minValue)
        {

            int leftValue = 0;
            if (!(index - 4 < minValue))
            {

                leftValue = data[index - 4];

            }
            int upValue = data[index - (image.Width * 4)];
            int truncatedMean = (int) Math.Floor(((upValue + leftValue) / 2f));

            return (byte)((data[index] + truncatedMean)%256);

        }

        public static byte[] UnUpLine(Image image, byte[] data, int lineIndex)
        {

            byte[] unfilteredImage = (byte[])data.Clone();
            int startIndex = lineIndex * (image.Width * 4);

            for (int i = startIndex; i < startIndex + (image.Width * 4); i++)
            {

                unfilteredImage[i] = UnUpByte(image, unfilteredImage, i);

            }

            return unfilteredImage;

        }

        public static byte UnUpByte(Image image, byte[] data, int index)
        {

            int value = data[index - (image.Width * 4)];

            return (byte)((data[index] + value)%256);

        }

        private static byte[] UnSubLine(Image image, byte[] data, int lineIndex)
        {

            byte[] unfilteredImage = (byte[]) data.Clone();
            int startIndex = lineIndex * (image.Width * 4);

            for (int i = startIndex; i < startIndex + (image.Width*4); i++)
            {

                unfilteredImage[i] = UnSubByte(unfilteredImage, i, startIndex);

            }

            return unfilteredImage;

        }

        private static byte UnSubByte(byte[] data, int index, int minValue)
        {

            int value = 0;
            if (!(index - 4 < minValue))
            {

                value = data[index - 4];

            }

            return (byte)((data[index] + value)%256);

        }

        private static byte[] GetFilterCodes(Image image, byte[] rawBytes)
        {

            byte[] filterCodes = new byte[image.Height];

            for (int l = 0; l < filterCodes.Length; l++)
            {

                filterCodes[l] = rawBytes[l * ((image.Width * 4) + 1)];

            }

            return filterCodes;

        }

        private static byte[] GetLineWithoutFilterCode(Image image, byte[] rawBytes, int lineIndex)
        {

            return new ArraySegment<byte>(rawBytes, lineIndex * ((image.Width * 4) + 1), ((image.Width * 4) + 1)).Skip(1).ToArray();

        }

        public byte[] GetLine(int lineIndex)
        {

            return new ArraySegment<byte>(ImageData, lineIndex * (Width * 4), (Width * 4)).ToArray();

        }

        private static byte[] GetRawImageData(Image image, byte[] rawBytes)
        {

            List<byte> imageData = new List<byte>();

            for (int i = 0; i < image.Height; i++)
            {

                imageData.AddRange(GetLineWithoutFilterCode(image, rawBytes, i));

            }

            return imageData.ToArray();

        }

    }
}
