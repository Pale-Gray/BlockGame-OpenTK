using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.BlockProperty;

public class DataReader : IDisposable
{

    private Stream _stream;

    public static DataReader Open(string path)
    {

        GameLogger.Log($"Opened a file for reading to at {path}");
        DataReader reader = new DataReader();
        reader._stream = File.OpenRead(path);
        return reader;

    }

    public Vector3i GetVector3i()
    {

        return (GetInt(), GetInt(), GetInt());

    }   

    public Vector3 GetVector3()
    {

        return (GetFloat(), GetFloat(), GetFloat());

    }

    public List<string> GetStringList()
    {

        List<string> value = new();

        Span<byte> bytes = stackalloc byte[sizeof(int)];
        _stream.ReadExactly(bytes);
        int count = BinaryPrimitives.ReadInt32LittleEndian(bytes);
        for (int i = 0; i < count; i++)
        {

            value.Add(GetString());

        }

        return value;

    }

    public List<int> GetIntList()
    {

        List<int> value = new();

        Span<byte> bytes = stackalloc byte[sizeof(int)];
        _stream.ReadExactly(bytes);
        int count = BinaryPrimitives.ReadInt32LittleEndian(bytes);
        for (int i = 0; i < count; i++)
        {

            value.Add(GetInt());

        }

        return value;

    }

    public string GetString()
    {

        string value = string.Empty;
        Span<byte> countBytes = stackalloc byte[sizeof(int)];
        Span<byte> charBytes = stackalloc byte[sizeof(char)];
        _stream.ReadExactly(countBytes);
        int count = BinaryPrimitives.ReadInt32LittleEndian(countBytes);
        for (int i = 0; i < count; i++)
        {
            _stream.ReadExactly(charBytes);
            value += BinaryPrimitives.ReadUInt16LittleEndian(charBytes);
        }
        return value;

    }

    public double GetDouble()
    {

        Span<byte> bytes = stackalloc byte[sizeof(double)];
        _stream.ReadExactly(bytes);
        return BinaryPrimitives.ReadDoubleLittleEndian(bytes);

    }
    
    public float GetFloat()
    {

        Span<byte> bytes = stackalloc byte[sizeof(float)];
        _stream.ReadExactly(bytes);
        return BinaryPrimitives.ReadSingleLittleEndian(bytes);

    }

    public long GetLong()
    {

        Span<byte> bytes = stackalloc byte[sizeof(long)];
        _stream.ReadExactly(bytes);
        return BinaryPrimitives.ReadInt64LittleEndian(bytes);

    }

    public int GetInt()
    {

        Span<byte> bytes = stackalloc byte[sizeof(int)];
        _stream.ReadExactly(bytes);
        return BinaryPrimitives.ReadInt32LittleEndian(bytes);

    }

    public bool GetBool() 
    {

        Span<byte> bytes = stackalloc byte[sizeof(bool)];
        _stream.ReadExactly(bytes);
        return bytes[0] == 1;

    }

    public void Dispose()
    {

        GameLogger.Log("Finished reading from file.");
        _stream.Dispose();

    }

}