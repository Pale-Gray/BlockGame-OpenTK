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

    public uint ReadUInt32()
    {
        _data.ReadExactly(_buffer, 0, 4);
        return BinaryPrimitives.ReadUInt32LittleEndian(_buffer);
    }

    public float ReadFloat()
    {
        _data.ReadExactly(_buffer, 0, 4);
        return BinaryPrimitives.ReadSingleLittleEndian(_buffer);
    }

    public string ReadString()
    {
        string value = string.Empty;
        int length = ReadInt32();
        for (int i = 0; i < length; i++) value += (char) ReadUInt16();
        return value;
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
    
    public uint[] ReadUInt32Values()
    {
        int count = ReadInt32();

        uint[] values = new uint[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = ReadUInt32();
        }

        return values;
    }
}