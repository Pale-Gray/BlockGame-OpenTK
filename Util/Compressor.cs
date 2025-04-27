using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Game.Core.Serialization;

public class Compressor
{

    public static Span<byte> RleCompress<TCount, TValue>(TValue[] data) where TCount : unmanaged, IBinaryInteger<TCount> where TValue : unmanaged, IBinaryInteger<TValue>
    {

        List<byte> bytes = new();

        TCount currentCount = TCount.Zero;
        TValue currentValue = data[0];
        for (int i = 0; i < data.Length; i++)
        {

            if (i == data.Length-1)
            {
                currentCount++;
                bytes.AddRange(GetBytes(currentCount));
                bytes.AddRange(GetBytes(currentValue));
            } else if (data[i] != currentValue)
            {
                bytes.AddRange(GetBytes(currentCount));
                bytes.AddRange(GetBytes(currentValue));
                currentValue = data[i];
                currentCount = TCount.Zero;
            }
            currentCount++;

        }

        return CollectionsMarshal.AsSpan(bytes);

    }

    public static TValue[] RleDecompress<TCount, TValue>(Span<byte> data) where TCount : unmanaged, IBinaryInteger<TCount> where TValue : unmanaged, IBinaryInteger<TValue>
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

        // Console.WriteLine($"decompressed count: {decompressed.Count}");

        return decompressed.ToArray();

    }

    public static Span<byte> GetBytes<T>(T value) where T : IBinaryInteger<T>
    {

        Span<byte> bytes = new byte[value.GetByteCount()];
        value.WriteLittleEndian(bytes);
        return bytes;

    }

    public static T GetValue<T>(Span<byte> data) where T : IBinaryInteger<T>
    {

        T value = default;

        Type type = typeof(T);
        return T.ReadLittleEndian(data, type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong));

    }

}