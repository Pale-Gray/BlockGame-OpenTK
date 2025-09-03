using System;

namespace VoxelGame.Util;

public class BitArray
{
    private uint[] _data;
    public uint[] Data => _data;
    private int _bitSize;
    public int BitSize => _bitSize;
    private int _capacity;
    public int Capacity => _capacity;
    
    public BitArray(int capacity, int bitSize)
    {
        int size = (int)float.Ceiling((bitSize / 32.0f) * capacity);
        _bitSize = bitSize;
        _capacity = capacity;
        _data = new uint[size];
    }

    public BitArray(BitArray previous, int bitSize) : this(previous._capacity, bitSize)
    {
        for (int i = 0; i < previous._capacity; i++)
        {
            uint previousValue = previous.Get(i);
            Set(i, previousValue);
        }
    }

    public void Set(int index, uint value)
    {
        int dataIndex = (int) float.Floor((index * _bitSize) / 32.0f);

        uint left = _data[dataIndex];
        uint right = 0;
        if (dataIndex + 1 < _data.Length) right = _data[dataIndex + 1];

        ulong valueLong = (ulong)value << (64 - _bitSize); // ex, 0b0001011 turns to 0b1011000
        ulong strip = ((ulong)left << 32) | right; // combines two indices together into one for ease
        ulong mask = (ulong)_masks[_bitSize] << (64 - _bitSize); // ex, 0b0001111 turns to 0b1111000

        int bitShift = (index * _bitSize) % 32;
        strip &= ~(mask >> bitShift);
        strip |= valueLong >> bitShift;

        _data[dataIndex] = (uint) (strip >> 32);
        if (dataIndex + 1 < _data.Length) _data[dataIndex + 1] = (uint) strip;
    }

    public uint Get(int index)
    {
        int dataIndex = (int)float.Floor((index * _bitSize) / 32.0f);

        uint left = _data[dataIndex];
        uint right = 0;
        if (dataIndex + 1 < _data.Length) right = _data[dataIndex + 1];
        
        ulong strip = ((ulong)left << 32) | right; // combines two indices together into one for ease
        ulong mask = (ulong)_masks[_bitSize] << (64 - _bitSize); // ex, 0b0001111 turns to 0b1111000

        int bitShift = (index * _bitSize) % 32;
        strip &= mask >> bitShift;
        strip <<= bitShift;

        return (uint)(strip >> (64 - _bitSize));
    }

    public override string ToString()
    {
        string str = string.Empty;
        for (int i = 0; i < _data.Length; i++)
        {
            str += Convert.ToString(_data[i], 2).PadLeft(32, '0');
        }
        return str;
    }
    
    private static uint[] _masks = 
    [
        0b0,
        0b1,
        0b11,
        0b111,
        0b1111,
        0b11111,
        0b111111,
        0b1111111,
        0b11111111,
        0b111111111,
        0b1111111111,
        0b11111111111,
        0b111111111111,
        0b1111111111111,
        0b11111111111111,
        0b111111111111111,
        0b1111111111111111,
        0b11111111111111111,
        0b111111111111111111,
        0b1111111111111111111,
        0b11111111111111111111,
        0b111111111111111111111,
        0b1111111111111111111111,
        0b11111111111111111111111,
        0b111111111111111111111111,
        0b1111111111111111111111111,
        0b11111111111111111111111111,
        0b111111111111111111111111111,
        0b1111111111111111111111111111,
        0b11111111111111111111111111111,
        0b111111111111111111111111111111,
        0b1111111111111111111111111111111,
        0b11111111111111111111111111111111
    ];
}