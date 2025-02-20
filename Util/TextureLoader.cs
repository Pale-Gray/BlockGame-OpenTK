using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Blockgame_OpenTK.Util
{
    public class TextureLoader
    {

        public static byte[] Flip(byte[] unflippedData)
        {

            List<byte> flippedImageData = new List<byte>();

            for (int i = 31; i >= 0; i--)
            {

                flippedImageData.AddRange(new ArraySegment<byte>(unflippedData, i * (32 * 4), (32 * 4)));

            }

            return flippedImageData.ToArray();

        }
        public static byte[] DecompressPng(byte[] data)
        {

            Stopwatch stopwatch = Stopwatch.StartNew();
            // Console.Log(data.Length);
            int width=0, height=0;
            int bitDepth, colorMode; // color mode will always be 6, bit depth will always be 8
            int filterMethod, interlacedMode;

            byte[] headerEncoded = new ArraySegment<byte>(data, 0, 8).ToArray();
            byte[] fileTypeEncoded = new ArraySegment<byte>(headerEncoded, 1, 3).ToArray();
            string fileType = Encoding.ASCII.GetString(fileTypeEncoded);
            Console.WriteLine($"Reading from file type {fileType}");
            byte[] restEncoded = data.Skip(8).ToArray();

            List<byte> imageDataEncoded = new List<byte>();

            Console.WriteLine(restEncoded.Length);
            Console.WriteLine(GetChunkTypeDecoded(restEncoded));
            while (GetChunkTypeDecoded(restEncoded) != "IEND")
            {

                Console.WriteLine(GetChunkTypeDecoded(restEncoded));
                if (GetChunkTypeDecoded(restEncoded) == "IHDR")
                {

                    Console.WriteLine($"Length: {GetLengthDecoded(restEncoded)}, Type: {GetChunkTypeDecoded(restEncoded)}, Width: {ToInt32(restEncoded, 8, 4)}, Height: {ToInt32(restEncoded, 12, 4)}");
                    width = ToInt32(restEncoded, 8, 4);
                    height = ToInt32(restEncoded, 12, 4);
                    bitDepth = 8;
                    colorMode = 6;
                    filterMethod = 0;
                    interlacedMode = 0;

                }

                if (GetChunkTypeDecoded(restEncoded) == "IDAT")
                {

                    imageDataEncoded.AddRange(GetDataEncoded(restEncoded, GetLengthDecoded(restEncoded)));

                }

                restEncoded = restEncoded.Skip((4 + 4 + GetLengthDecoded(restEncoded) + 4)).ToArray();

            }

            Console.WriteLine($"Encoded data length: {imageDataEncoded.Count()}");
            Console.WriteLine($"First: {imageDataEncoded.First()}, Last: {imageDataEncoded.Last()}");

            MemoryStream dataCompressed = new MemoryStream(imageDataEncoded.ToArray());
            MemoryStream decompressedData = new MemoryStream();
            // DeflateStream decompressor = new DeflateStream(dataCompressed, CompressionMode.Decompress);
            Console.WriteLine($"buffer size {dataCompressed.Length}");
            ZLibStream decompressor = new ZLibStream(dataCompressed, CompressionMode.Decompress);
            decompressor.CopyTo(decompressedData);
            Console.WriteLine(decompressedData.Length);

            /*for (int l = 0; l < 32; l++)
            {

                // Console.Log(decompressedData.ToArray()[l * ((32*4) + 1)]);
                Console.Log(GetLineWithFilterCode(decompressedData.ToArray(), l)[0]);

            }*/
            byte[] lineOne = GetLineWithFilterCode(decompressedData.ToArray(), 0);
            byte filterCode = lineOne[0];
            /* for (int i = 0; i < lineOne.Length; i++)
            {

                Console.Log($"{lineOne[i]}, {i}, {lineOne.Length}");

            } */

            byte[] decompressedDataArray = decompressedData.ToArray();

            byte[] emptyImage = DeFilter(decompressedDataArray);

            /* for (int i = 0; i < 32*4; i++)
            {

                emptyImage[i] = lineOne.Skip(1).ToArray()[i];
                emptyImage[i] = UnSubPixelByte(emptyImage, i);

            } */

            stopwatch.Stop();
            Console.WriteLine($"PNG Decoding took {stopwatch.ElapsedMilliseconds}ms");

            return emptyImage;

        }

        public static byte[] DeFilter(byte[] imageDataDecompressed)
        {

            byte[] filterCodes = GetFilterCodes(imageDataDecompressed);
            byte[] imageData = GetFilteredImageData(imageDataDecompressed);

            for (int i = 0; i < filterCodes.Length; i++)
            {

                if (filterCodes[i] == 1) // this means to undo the sub filter on the png
                {

                    imageData = UnSubLine(imageData, i);

                }
                if (filterCodes[i] == 2) // this means to undo the up filter on the png
                {

                    imageData = UnUpLine(imageData, i);

                }
                if (filterCodes[i] == 3) // this means to undo the mean filter on the png
                {

                    imageData = UnMeanLine(imageData, i);

                }
                if (filterCodes[i] == 4) // this means to unod the paeth filter on the png
                {

                    imageData = UnPaethLine(imageData, i);

                }
                // Console.Log(filterCodes[i]);

            }

            // imageData = UnSubLine(imageData, 0);

            // Console.Log(filterCodes.Length + ", " + imageData.Length);

            return imageData;

        }

        public static byte[] UnSubLine(byte[] imageData, int lineIndex)
        {

            byte[] image = (byte[]) imageData.Clone();
            int startIndex = lineIndex * (32 * 4);

            for (int i = startIndex; i < startIndex + (32*4); i++)
            {

                image[i] = UnSubPixelByte(image, i, startIndex);

            }

            return image;

        }

        public static byte UnSubPixelByte(byte[] data, int pixelIndex, int pixelMin)
        {

            int valueToAdd = 0;
            if (!(pixelIndex - 4 < pixelMin))
            {

                valueToAdd = data[pixelIndex - 4];

            }

            int value = data[pixelIndex] + valueToAdd;

            return (byte)(value%256);

        }

        public static byte[] UnUpLine(byte[] imageData, int lineIndex)
        {

            byte[] image = (byte[])imageData.Clone();
            int start = lineIndex * (32 * 4);
            // Console.Log(lineIndex + ", " + start + ", " + (start + (32*4)));
            for (int i = start; i < start + (32*4); i++)
            {

                image[i] = UnUpPixelByte(image, i);

            }

            return image;

        }
        public static byte UnUpPixelByte(byte[] data, int pixelIndex)
        {

            int upIndex = pixelIndex - (32 * 4);

            int dataUnMod = data[pixelIndex] + data[upIndex];

            return (byte)(dataUnMod % 256);

        }

        public static byte[] UnMeanLine(byte[] imageData, int lineIndex)
        {

            byte[] image = (byte[]) imageData.Clone();
            int start = lineIndex * (32 * 4);
            int min = start;
            for (int i = start; i < start + (32*4); i++)
            {

                image[i] = UnMeanPixelByte(image, i, min);

            }

            return image;

        }

        public static byte UnMeanPixelByte(byte[] data, int index, int minIndex)
        {

            int left = 0;
            if (!(index - 4 < minIndex)) left = data[index - 4];
            int up = data[index - (32 * 4)];

            int mean = (int) Math.Floor(((left + up) / 2f));
            int value = data[index] + mean;

            return (byte) (value%256);

        }

        public static byte[] UnPaethLine(byte[] imageData, int lineIndex)
        {

            byte[] image = (byte[])imageData.Clone();
            int start = lineIndex * (32 * 4);
            int min = start;
            for (int i = start; i < start + (32 * 4); i++)
            {

                image[i] = UnPaethPixelByte(image, i, min);

            }

            return image;

        }

        public static byte UnPaethPixelByte(byte[] data, int pixelIndex, int pixelMin)
        {

            byte paethPredict = PaethPredict(data, pixelIndex, pixelMin);

            int value = data[pixelIndex] + paethPredict;

            return (byte) (value%256);

        }

        public static byte PaethPredict(byte[] data, int pixelIndex, int pixelMin)
        {

            byte[] pixels = (byte[]) data.Clone();

            byte left = 0;
            byte upLeft = 0;
            if (!(pixelIndex - 4 < pixelMin))
            {

                left = data[pixelIndex - 4];
                upLeft = data[pixelIndex - 4 - ((32 * 4))];

            }
            byte up = data[pixelIndex - (32 * 4)];

            int basePixel = left + up - upLeft;
            int diffLeft = Math.Abs(basePixel - left);
            int diffUp = Math.Abs(basePixel - up);
            int diffUpLeft = Math.Abs(basePixel - upLeft);
            if (diffLeft <= diffUp && diffLeft <= diffUpLeft) return left;
            else if (diffUp <= diffUpLeft) return up;
            return upLeft;

        }
        

        public static byte[] GetLine(byte[] data, int lineIndex)
        {

            int globalIndex = lineIndex * (32 * 4);
            return new ArraySegment<byte>(data, globalIndex, globalIndex + ((32 * 4)-1)).ToArray();

        }

        public static byte[] GetFilterCodes(byte[] imageDataDecompressed)
        {

            byte[] filterCodes = new byte[32];

            for (int i = 0; i < filterCodes.Length; i++)
            {

                filterCodes[i] = imageDataDecompressed[i * ((32*4) + 1)];

            }

            return filterCodes;

        }
        public static byte[] GetLineWithFilterCode(byte[] imageData, int lineIndex)
        {

            ArraySegment<byte> line = new ArraySegment<byte>(imageData, (lineIndex * ((32*4) + 1)), (32*4)+1);

            return line.ToArray();

        }

        public static byte[] GetFilteredImageData(byte[] decompressedImage)
        {

            List<byte> filteredImage = new List<byte>();

            for (int i = 0; i < 32; i++)
            {

                filteredImage.AddRange(GetLineWithFilterCode(decompressedImage, i).Skip(1));

            }

            return filteredImage.ToArray();

        }

        public static byte[] GetLineFromLineWithFilterCode(byte[] lineCoded)
        {

            return new ArraySegment<byte>(lineCoded, 1, 32).ToArray();

        }
        public static byte[] UnSubPixelBytes(byte[] lineData, int pixelIndex)
        {

            byte[] lineWithoutCode = lineData.Skip(1).ToArray();

            for (int i = pixelIndex * 4; i < (pixelIndex * 4) + 4; i++)
            {

                // lineWithoutCode[i] = UnSubPixelByte(lineWithoutCode, i);

            }

            return lineWithoutCode;

        }
        public static byte Sub(byte[] data, int pixelIndex)
        {

            int dataInt = data[pixelIndex] - data[pixelIndex - 1];

            byte dataByte = 0;

            while (dataInt < 0)
            {

                dataInt = 255 + dataInt;

            }

            return (byte)dataInt;

        }

        public static byte[] GetDataEncoded(byte[] dataEncoded, int length)
        {

            return new ArraySegment<byte>(dataEncoded, 8, length).ToArray();

        }

        public static int ToInt32(byte[] dataEncoded, int start, int end)
        {

            return BitConverter.ToInt32(new ArraySegment<byte>(dataEncoded, start, end).Reverse().ToArray());

        }
        public static int GetLengthDecoded(byte[] dataEncoded)
        {

            return BitConverter.ToInt32(new ArraySegment<byte>(dataEncoded, 0, 4).Reverse().ToArray());

        }

        public static string GetChunkTypeDecoded(byte[] dataEncoded)
        {

            return Encoding.ASCII.GetString(new ArraySegment<byte>(dataEncoded, 4, 4));

        }

    }
}
