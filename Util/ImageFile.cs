using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using Game.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.Vulkan;
using OpenTK.Mathematics;

namespace Game.Core.Image;

public enum BitDepth : byte
{

    Bits1 = 1,
    Bits2 = 2,
    Bits4 = 4,
    Bits8 = 8,
    Bits16 = 16

}

public enum ImageColorType : byte
{

    Rgb = 2,
    Rgba = 6

}

public enum FilterMode : byte
{

    None = 0,
    Sub = 1,
    Up = 2,
    Average = 3,
    Paeth = 4

}
public struct PngImage
{

    public byte[] Data;
    public int Width;
    public int Height;
    public BitDepth BitDepth;
    public ImageColorType ColorType;

}

public class ImageFile
{

    private static byte[] _signature = [ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A ];

    public static PngImage Load(string file)
    {

        PngImage image = new PngImage();

        using (MemoryStream imageStream = new MemoryStream(File.ReadAllBytes(file)))
        {

            Span<byte> signature = stackalloc byte[8];

            imageStream.ReadExactly(signature);
            if (!signature.SequenceEqual(_signature)) throw new Exception("The file is not a png file.");

            Span<byte> chunkLength = stackalloc byte[4];
            Span<byte> chunkType = stackalloc byte[4];
            byte[] chunkData;
            Span<byte> chunkCrc = stackalloc byte[4];

            List<byte> gZipCompressedImageData = new();
            MemoryStream compressedImageData = new MemoryStream();

            while (!chunkType.SequenceEqual("IEND"u8))
            {

                imageStream.ReadExactly(chunkLength);
                int length = BinaryPrimitives.ReadInt32BigEndian(chunkLength);
                imageStream.ReadExactly(chunkType);
                string type = Encoding.UTF8.GetString(chunkType);
                chunkData = new byte[length];
                imageStream.ReadExactly(chunkData);
                imageStream.ReadExactly(chunkCrc);

                // Console.WriteLine($"{length}, {type}");

                switch (type)
                {
                    case "IHDR":
                        Span<byte> data = chunkData.AsSpan();
                        image.Width = BinaryPrimitives.ReadInt32BigEndian(data.Slice(0, 4));
                        image.Height = BinaryPrimitives.ReadInt32BigEndian(data.Slice(4, 4));
                        image.BitDepth = (BitDepth) data[8];
                        
                        byte CompressionMethod = data[10];
                        byte filterMethod = data[11];
                        byte interlaced = data[12];
                        // Console.WriteLine($"image width: {width}, image height: {height}, colorType: {colorType}");
                        if (data[9] == 6 || data[9] == 2)
                        {
                            image.ColorType = (ImageColorType) data[9];
                        } else
                        {
                            throw new Exception("The color type cannot be loaded.");
                        }
                        break;
                    case "IDAT":
                        gZipCompressedImageData.AddRange(chunkData);
                        compressedImageData.Write(chunkData);
                        break;
                }

            }

            // Console.WriteLine(compressedImageData.Length);
            MemoryStream decompressedImageData = new MemoryStream();

            using (ZLibStream decompress = new ZLibStream(compressedImageData, CompressionMode.Decompress))
            {

                compressedImageData.Position = 0;
                decompress.CopyTo(decompressedImageData);

            }

            // Console.WriteLine($"width: {image.Width}, height: {image.Height}, bit depth: {image.BitDepth}, color type: {image.ColorType}");

            byte[] filteredImageBytes = decompressedImageData.ToArray();

            int numChannels = image.ColorType == ImageColorType.Rgb ? 3 : 4;

            int bpp = (int) image.BitDepth;
            int bytespp = (numChannels * (Math.Max(bpp, 8) / 8));

            int bytesPerPixel = bytespp;

            MemoryStream finalImageStream = new MemoryStream();

            int h = 0;
            while (h < image.Height)
            {

                int filterType = filteredImageBytes[h * ((image.Width * bytesPerPixel) + 1)];
                int current;
                int left;
                int up;
                int upLeft;
                byte result;

                switch (filterType)
                {

                    case 0: // none
                        for (int w = 0; w < (image.Width * bytesPerPixel); w++)
                        {
                            // finalImage.Add(filteredImageBytes[(h * ((image.Width * bytesPerPixel) + 1)) + w + 1]);
                            finalImageStream.WriteByte(filteredImageBytes[(h * ((image.Width * bytesPerPixel) + 1)) + w + 1]);
                        }
                        break;
                    case 1: // sub
                        for (int w = 0; w < (image.Width * bytesPerPixel); w++)
                        {
                            current = filteredImageBytes[(h * ((image.Width * bytesPerPixel) + 1)) + w + 1];
                            left = 0;
                            if (w - bytesPerPixel >= 0)
                            {
                                left = filteredImageBytes[(h * ((image.Width * bytesPerPixel) + 1)) + 1 + w - bytesPerPixel];
                            }
                            result = (byte) (current + left);
                            // finalImage.Add(result);
                            finalImageStream.WriteByte(result);
                            filteredImageBytes[(h * ((image.Width * bytesPerPixel) + 1)) + 1 + w] = result;
                        }
                        break;
                    case 2: // up
                        for (int w = 0; w < (image.Width * bytesPerPixel); w++)
                        {
                            current = filteredImageBytes[(h * ((image.Width * bytesPerPixel) + 1)) + w + 1];
                            up = 0;
                            if (h - 1 >= 0)
                            {
                                up = filteredImageBytes[((h-1) * ((image.Width * bytesPerPixel) + 1)) + w + 1];
                            }
                            result = (byte) (current + up);
                            // finalImage.Add(result);
                            finalImageStream.WriteByte(result);
                            filteredImageBytes[(h * ((image.Width * bytesPerPixel) + 1)) + 1 + w] = result;
                        }
                        break;
                    case 3: // average
                        for (int w = 0; w < (image.Width * bytesPerPixel); w++)
                        {
                            current = filteredImageBytes[(h * ((image.Width * bytesPerPixel) + 1)) + w + 1];
                            up = 0;
                            if (h - 1 >= 0)
                            {
                                up = filteredImageBytes[((h-1) * ((image.Width * bytesPerPixel) + 1)) + w + 1];
                            }
                            left = 0;
                            if (w - bytesPerPixel >= 0)
                            {
                                left = filteredImageBytes[(h * ((image.Width * bytesPerPixel) + 1)) + 1 + w - bytesPerPixel];
                            }
                            result = (byte) (current + Math.Floor((up + left) / 2.0));
                            // finalImage.Add(result);
                            finalImageStream.WriteByte(result);
                            filteredImageBytes[(h * ((image.Width * bytesPerPixel) + 1)) + 1 + w] = result;
                        }
                        break;
                    case 4: // paeth
                        for (int w = 0; w < (image.Width * bytesPerPixel); w++)
                        {
                            current = filteredImageBytes[(h * ((image.Width * bytesPerPixel) + 1)) + w + 1];
                            up = 0;
                            if (h - 1 >= 0)
                            {
                                up = filteredImageBytes[((h-1) * ((image.Width * bytesPerPixel) + 1)) + w + 1];
                            }
                            left = 0;
                            if (w - bytesPerPixel >= 0)
                            {
                                left = filteredImageBytes[(h * ((image.Width * bytesPerPixel) + 1)) + 1 + w - bytesPerPixel];
                            }
                            upLeft = 0;
                            if (h - 1 >= 0 && w - bytesPerPixel >= 0)
                            {
                                upLeft = filteredImageBytes[((h-1) * ((image.Width * bytesPerPixel) + 1)) + 1 + w - bytesPerPixel];
                            }
                            result = (byte) (current + PaethPredictor(left, up, upLeft));
                            // finalImage.Add(result);
                            finalImageStream.WriteByte(result);
                            filteredImageBytes[(h * ((image.Width * bytesPerPixel) + 1)) + 1 + w] = result;
                        }
                        break;

                }
                h++;

            }

            image.Data = finalImageStream.GetBuffer();

        }

        return image;

    }

    private static int PaethPredictor(int left, int up, int upLeft)
    {

        int initialEstimate = left + up - upLeft;
        int distanceLeft = Math.Abs(initialEstimate - left);
        int distanceUp = Math.Abs(initialEstimate - up);
        int distanceUpLeft = Math.Abs(initialEstimate - upLeft);

        if (distanceLeft <= distanceUp && distanceLeft <= distanceUpLeft) return left;
        if (distanceUp <= distanceUpLeft) return up;
        return upLeft;

    }

}