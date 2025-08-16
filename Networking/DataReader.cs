using System;
using System.Buffers.Binary;
using System.IO;
using OpenTK.Graphics.Vulkan;

namespace VoxelGame.Networking;

public class DataReader
{
    private MemoryStream _data;
    private byte[] _buffer = new byte[8];

    public byte[] Data => _data.GetBuffer()[..(int)_data.Length];

    public DataReader(byte[] data)
    {
        _data = new MemoryStream(data);
    }

    public void Clear()
    {
        _data.SetLength(0);
        _data.Position = 0;
    }

    public int ReadInt32()
    {
        _data.ReadExactly(_buffer, 0, 4);
        return BinaryPrimitives.ReadInt32LittleEndian(_buffer);
    }

    public ushort ReadUInt16()
    {
        _data.ReadExactly(_buffer, 0, 2);
        return BinaryPrimitives.ReadUInt16LittleEndian(_buffer);
    }

    public ushort[] ReadUInt16Values()
    {
        int count = ReadInt32();

        ushort[] values = new ushort[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = ReadUInt16();
        }

        return values;
    }
}