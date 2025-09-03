using System;
using System.Collections.Generic;

namespace VoxelGame.Util;

public class Palette<T>
{
    private List<T> _elements = new();
    private BitArray _data;
    public int ByteSize => _data.Data.Length * 4;
    public Palette(int capacity, int bitSize = 1)
    {
        _data = new BitArray(capacity, bitSize);
    }

    public void Remove(int index)
    {
        _data.Set(index, 0);
    }

    public void Insert(int index, T value)
    {
        int numericValue = 0;
        bool found = false;

        for (int i = 0; i < _elements.Count; i++)
        {
            if (value.Equals(_elements[i]))
            {
                found = true;
                numericValue = i + 1;
                break;
            }
        }

        if (found)
        {
            _data.Set(index, (uint) numericValue);
        }
        else
        {
            _elements.Add(value);
            numericValue = _elements.Count;

            int bitSize = BitSize(numericValue);

            if (bitSize > _data.BitSize)
            {
                // grow array
                Console.WriteLine($"the palette grew in size");
                _data = new BitArray(_data, bitSize);
            } else if (bitSize < _data.BitSize)
            {
                // shrink array
                Console.WriteLine("the palette shrunk in size!");
                _data = new BitArray(_data, bitSize);
            }
            
            _data.Set(index, (uint) numericValue);
            // else
            // {
            //     // just set
            //     _data.Set(index, (uint) numericValue);
            //     Console.WriteLine(_data);
            // }
        }
    }

    public T? Get(int index)
    {
        uint elm = _data.Get(index);
        if (elm == 0) return default;
        return _elements[(int)elm - 1];
    }

    private int BitSize(int value)
    {
        return (int)(Math.Log(value, 2) + 1);
    }
}