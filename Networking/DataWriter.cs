using System;
using System.Buffers.Binary;
using System.IO;

namespace VoxelGame.Networking;

public class DataWriter
{
    private MemoryStream _data;
    private byte[] _buffer = new byte[8];

    public byte[] Data => _data.GetBuffer();

    public DataWriter()
    {
        _data = new MemoryStream();
    }

    public void Clear()
    {
        _data.Dispose();
        _data = new MemoryStream();
    }

    public void Write(ushort value)
    {
        Span<byte> buff = _buffer;
        BinaryPrimitives.WriteUInt16LittleEndian(buff, value);
        
        _data.Write(buff.Slice(0, 2));
    }

    public void Write(uint value)
    {
        Span<byte> buff = _buffer;
        BinaryPrimitives.WriteUInt32LittleEndian(buff, value);
        
        _data.Write(buff.Slice(0, 4));
    }

    public void Write(int value)
    {
        Span<byte> buff = _buffer;
        BinaryPrimitives.WriteInt32LittleEndian(buff, value);
        
        _data.Write(buff.Slice(0, 4));
    }

    public void Write(float value)
    {
        Span<byte> buff = _buffer;
        BinaryPrimitives.WriteSingleLittleEndian(buff, value);
        
        _data.Write(buff.Slice(0, 4));
    }

    public void Write(string value)
    {
        Write(value.Length);
        for (int i = 0; i < value.Length; i++) Write(value[i]);
    }

    public void WriteValues(ushort[] values)
    {
        Write(values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            Write(values[i]);
        }
    }

    public void WriteValues(uint[] values)
    {
        Write(values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            Write(values[i]);
        }
    }
}