using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;

namespace VoxelGame.Util;

public class RunLengthEncoder
{
    public static ReadOnlySpan<ushort> Encode(ushort[] decompressedData)
    {
        List<ushort> data = new();
        
        ushort currentCount = 0;
        ushort currentValue = decompressedData[0];

        for (int i = 0; i < decompressedData.Length; i++)
        {
            if (i == decompressedData.Length - 1)
            {
                currentCount++;
                data.Add(currentCount);
                data.Add(currentValue);
            }
            else if (decompressedData[i] != currentValue)
            {
                data.Add(currentCount);
                data.Add(currentValue);
                currentValue = decompressedData[i];
                currentCount = 0;
            }

            currentCount++;
        }
        
        return CollectionsMarshal.AsSpan(data);
    }

    public static ReadOnlySpan<ushort> Decode(ushort[] compressedData)
    {
        List<ushort> data = new();

        for (int i = 0; i < compressedData.Length; i += 2)
        {
            ushort count = compressedData[i];
            ushort value = compressedData[i + 1];

            for (int c = 0; c < count; c++)
            {
                data.Add(value);
            }
        }
        
        return CollectionsMarshal.AsSpan(data);
    }
}