using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using FreeTypeSharp;

namespace Blockgame_OpenTK.Core.Image;

public enum ColorType : byte
{

    Grayscale = 0,
    Rgb = 2,
    Indexed = 3,
    GrayscaleWithAlpha = 4,
    Rgba = 6

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
public class ImageLoader
{

    private static List<byte> _compressedImageData = new();

    // notes
    // png data is big endian
    public static void LoadPng(string pathToFile)
    {

        Stopwatch sw = Stopwatch.StartNew();
        ImageProperties properties = new ImageProperties();

        using (Stream stream = File.OpenRead(pathToFile))
        {

            Span<byte> signature = stackalloc byte[8];
            stream.ReadExactly(signature);

            // signature verification
            if (!signature.SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) throw new Exception("File is not a valid png file");
            Console.WriteLine("Valid png");

            Span<byte> length = stackalloc byte[4];
            Span<byte> chunkType = stackalloc byte[4];
            byte[] data;
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
                data = new byte[lengthValue];
                stream.ReadExactly(data);
                MemoryStream dat = new MemoryStream(data);
                stream.ReadExactly(checksum);

                Console.WriteLine($"{lengthValue}, {chunkTypeString}");

                switch (chunkTypeString)
                {

                    case "IHDR": // Image properties
                        dat.ReadExactly(buffer);
                        properties.Width = BinaryPrimitives.ReadInt32BigEndian(buffer);
                        dat.ReadExactly(buffer);
                        properties.Height = BinaryPrimitives.ReadInt32BigEndian(buffer);
                        properties.BitDepth = (BitDepthType)dat.ReadByte();
                        properties.ColorType = (ColorType)dat.ReadByte();
                        properties.CompressionMethod = (byte) dat.ReadByte();
                        properties.FilterMethod = (byte) dat.ReadByte();
                        properties.IsInterlaced = dat.ReadByte() == 1 ? true : false;
                        Console.WriteLine(properties);
                        break;

                }
                
            }

        }
        sw.Stop();
        Console.WriteLine($"Image loading took {sw.Elapsed.TotalMilliseconds}ms");

    }

}