using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Game.Util;
using OpenTK.Mathematics;

namespace Game.BlockProperty;
public class DataWriter : IDisposable
{

    private Stream _stream;

    public byte[] GetUnderlyingBytes() => ((MemoryStream)_stream).ToArray();

    public DataWriter()
    {

        _stream = new MemoryStream(); 

    }

    public static DataWriter OpenFile(string path)
    {

        GameLogger.Log($"Opened a file for writing to at {path}");
        DataWriter writer = new DataWriter();
        writer._stream = File.Open(path, FileMode.Create);
        return writer;

    }
    public void WriteVector3i(Vector3i value)
    {

        WriteInt(value.X);
        WriteInt(value.Y);
        WriteInt(value.Z);

    }

    public void WriteVector3(Vector3 value)
    {

        WriteFloat(value.X);
        WriteFloat(value.Y);
        WriteFloat(value.Z);

    }

    public void WriteByteSpan(Span<byte> bytes)
    {

        WriteInt(bytes.Length);
        _stream.Write(bytes);

    }

    public void WriteString(string value)
    {

        Span<byte> intBytes = stackalloc byte[sizeof(int)];
        Span<byte> charBytes = stackalloc byte[sizeof(char)];
        BinaryPrimitives.WriteInt32LittleEndian(intBytes, value.Length);
        _stream.Write(intBytes);
        for (int i = 0; i < value.Length; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(charBytes, value[i]);
            _stream.Write(charBytes);
        }
        
    }

    public void WriteBinaryInteger<T>(T value) where T : System.Numerics.IBinaryInteger<T>
    {

        Span<byte> bytes = stackalloc byte[value.GetByteCount()];
        value.WriteLittleEndian(bytes);
        _stream.Write(bytes);

    }

    public void WriteLong(long value) 
    {

        Span<byte> longBytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(longBytes, value);
        _stream.Write(longBytes);

    }

    public void WriteDouble(double value)
    {

        Span<byte> doubleBytes = stackalloc byte[sizeof(double)];
        BinaryPrimitives.WriteDoubleLittleEndian(doubleBytes, value);
        _stream.Write(doubleBytes);

    }

    public void WriteFloat(float value)
    {

        Span<byte> floatBytes = stackalloc byte[sizeof(float)];
        BinaryPrimitives.WriteSingleLittleEndian(floatBytes, value);
        _stream.Write(floatBytes);

    }

    public void WriteUInt(uint value)
    {

        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        _stream.Write(bytes);

    }

    public void WriteInt(int value) 
    {

        Span<byte> intBytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(intBytes, value);
        _stream.Write(intBytes);

    }

    public void WriteUshort(ushort value)
    {

        Span<byte> ushortBytes = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(ushortBytes, value);
        _stream.Write(ushortBytes);

    }

    public void WriteBool(bool value) => _stream.WriteByte(value ? (byte)1 : (byte)0);

    public void WriteList(List<string> list)
    {

        Span<byte> listCountBytes = stackalloc byte[sizeof(int)];
        Span<byte> countBytes = stackalloc byte[sizeof(int)];
        Span<byte> characterBytes = stackalloc byte[sizeof(char)];
        BinaryPrimitives.WriteInt32LittleEndian(listCountBytes, list.Count);
        _stream.Write(listCountBytes);
        foreach (string item in list) 
        {

            WriteString(item);

        }

    }

    public void WriteList(List<int> list)
    {

        Span<byte> listCountBytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(listCountBytes, list.Count);
        _stream.Write(listCountBytes);
        foreach (int item in list) 
        {

            WriteInt(item);

        }

    }

    public void Dispose()
    {

        GameLogger.Log("Finished writing to file.");
        _stream.Dispose();

    }

}