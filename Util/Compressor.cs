using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.Glx;
using OpenTK.Platform;
using Tomlet.Models;

namespace Blockgame_OpenTK.Core.Serialization;

public class Compressor
{

    public static Span<byte> RleCompress<TCount, TValue>(TValue[] data) where TCount : struct, INumber<TCount> where TValue : struct, INumber<TValue>
    {

        List<byte> bytes = new();

        TCount currentCount = TCount.Zero;
        TValue currentValue = data[0];
        for (int i = 0; i < data.Length; i++)
        {

            if (i == data.Length-1)
            {
                currentCount++;
                // Console.WriteLine("data changed");
                // Console.WriteLine($"value: {currentValue}");
                // Console.WriteLine($"count: {currentCount}");
                bytes.AddRange(GetBytes(currentCount));
                bytes.AddRange(GetBytes(currentValue));
            } else if (data[i] != currentValue)
            {
                // Console.WriteLine("data changed");
                // Console.WriteLine($"value: {currentValue}");
                // Console.WriteLine($"count: {currentCount}");
                bytes.AddRange(GetBytes(currentCount));
                bytes.AddRange(GetBytes(currentValue));
                currentValue = data[i];
                currentCount = TCount.Zero;
            }
            currentCount++;

        }

        return CollectionsMarshal.AsSpan(bytes);

    }

    public static TValue[] RleDecompress<TCount, TValue>(Span<byte> data) where TCount : struct, INumber<TCount> where TValue : struct, INumber<TValue>
    {

        List<TValue> decompressed = new();
        int currentIndex = 0;

        while (currentIndex < data.Length)
        {

            TCount count = GetValue<TCount>(data.Slice(currentIndex, Unsafe.SizeOf<TCount>()));
            currentIndex += Unsafe.SizeOf<TCount>();
            TValue value = GetValue<TValue>(data.Slice(currentIndex, Unsafe.SizeOf<TValue>()));
            currentIndex += Unsafe.SizeOf<TValue>();

            for (TCount i = TCount.Zero; i < count; i++)
            {

                decompressed.Add(value);

            }

        }

        Console.WriteLine($"decompressed count: {decompressed.Count}");

        return decompressed.ToArray();

    }

    private static T GetValue<T>(Span<byte> data) where T: struct, INumber<T> => MemoryMarshal.Read<T>(data);

    private static Span<byte> GetBytes<T>(T value) where T : notnull, INumber<T>
    {

        byte[] b = new byte[Unsafe.SizeOf<T>()];
        GCHandle valueHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
        Marshal.Copy(valueHandle.AddrOfPinnedObject(), b, 0, Unsafe.SizeOf<T>());
        valueHandle.Free();

        if (!BitConverter.IsLittleEndian)
        {
            b.Reverse();
        }

        return b;

    }

}