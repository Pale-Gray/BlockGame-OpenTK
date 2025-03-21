using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace Blockgame_OpenTK.Core.Image;

public enum ColorType : byte
{

    Grayscale = 0,
    Rgb = 2,
    Indexed = 3,
    GrayscaleWithAlpha = 4,
    Rgba = 6

}

public enum ColorFormat
{
    Rgb,
    Rgba

}
public enum BitDepthType : byte
{

    OneBpp = 1,
    TwoBpp = 2,
    FourBpp = 4,
    EightBpp = 8,
    SixteenBpp = 16

}
public struct ImageProperties
{

    public int Width;
    public int Height;
    public ColorType ColorType;
    public BitDepthType BitDepth;
    public PixelFormat PixelFormat;
    public byte CompressionMethod;
    public byte FilterMethod;
    public bool IsInterlaced;

    public override string ToString()
    {
        return 
        $"""
        Width: {Width}
        Height: {Height}
        Color Type: {ColorType}
        Bit Depth: {BitDepth}
        Is Interlaced: {IsInterlaced}
        """;
    }

}

public struct Pimage
{

    public ImageProperties Properties;
    public int TextureID;
    public byte[] Data;

}

public class ImageLoader
{

    private static List<byte> _compressedImageData = new();

    // notes
    // png data is big endian
    public static Pimage LoadPng(string pathToFile, ColorFormat format = ColorFormat.Rgba)
    {

        Pimage img = new Pimage();

        Stopwatch sw = Stopwatch.StartNew();
        ImageProperties properties = new ImageProperties();

        List<byte> compressedData = new();
        List<byte> convertedData = new();

        using (Stream stream = File.OpenRead(pathToFile))
        {

            Span<byte> signature = stackalloc byte[8];
            stream.ReadExactly(signature);

            // signature verification
            if (!signature.SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) throw new Exception("File is not a valid png file");
            Console.WriteLine("Valid png");

            Span<byte> length = stackalloc byte[4];
            Span<byte> chunkType = stackalloc byte[4];
            Span<byte> checksum = stackalloc byte[4];

            Span<byte> buffer = stackalloc byte[4];

            string chunkTypeString = "";
            int lengthValue;

            while (chunkTypeString != "IEND")
            {

                stream.ReadExactly(length);
                stream.ReadExactly(chunkType);
                lengthValue = BinaryPrimitives.ReadInt32BigEndian(length);
                chunkTypeString = Encoding.ASCII.GetString(chunkType);
                Span<byte> data = new byte[lengthValue];
                stream.ReadExactly(data);
                stream.ReadExactly(checksum);

                Console.WriteLine($"{lengthValue}, {chunkTypeString}");

                switch (chunkTypeString)
                {

                    case "IHDR": // Image properties
                        properties.Width = BinaryPrimitives.ReadInt32BigEndian(data.Slice(0, 4));
                        properties.Height = BinaryPrimitives.ReadInt32BigEndian(data.Slice(4, 4));
                        properties.BitDepth = (BitDepthType)data[8];
                        properties.ColorType = (ColorType)data[9];
                        properties.CompressionMethod = data[10];
                        properties.FilterMethod = data[11];
                        properties.IsInterlaced = data[12] == 1 ? true : false;
                        Console.WriteLine(properties);
                        img.Properties = properties;
                        break;
                    case "IDAT": // image data
                        compressedData.AddRange(data);
                        break;

                }
                
            }

        }

        MemoryStream decompressedData = new MemoryStream();
        ZLibStream compressedStream = new ZLibStream(new MemoryStream(compressedData.ToArray()), CompressionMode.Decompress);
        compressedStream.CopyTo(decompressedData);

        List<byte> decompressedImage = new();
        byte[] decompressedDataArray = decompressedData.ToArray();

        Console.WriteLine($"{decompressedData.Length}, {img.Properties.Width * img.Properties.Height}");

        while (decompressedImage.Count != img.Properties.Width * img.Properties.Height * (int)img.Properties.BitDepth) decompressedImage.Add(0);

        img.Data = decompressedImage.ToArray();

        sw.Stop();
        Console.WriteLine($"Image loading took {sw.Elapsed.TotalMilliseconds}ms");

        return img;

    }

    public static void AddPixel(byte[] data, int ax, int ay, int bx, int by, int size, int width, int height)
    {

        

    }

}