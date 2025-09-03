using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using VoxelGame.Util;

namespace VoxelGame;

public class ChunkSection
{
    public ushort[] Data = new ushort[Config.ChunkVolume];
    public uint[] SolidData = new uint[Config.ChunkSize * Config.ChunkSize];
    public uint[] TransparentData = new uint[Config.ChunkSize * Config.ChunkSize];
    public Vector3i Position;

    public bool IsEmpty
    {
        get
        {
            for (int i = 0; i < Data.Length; i++)
                if (Data[i] != 0)
                    return false;
            return true;
        }
    }

    public ushort GetBlockId(int x, int y, int z)
    {
        return Data[x + (y * Config.ChunkSize) + (z * Config.ChunkSize * Config.ChunkSize)];
    }

    public ushort GetBlockId(Vector3i position)
    {
        return Data[position.X + (position.Y * Config.ChunkSize) + (position.Z * Config.ChunkSize * Config.ChunkSize)];
    }

    public void SetBlock(ushort id, int x, int y, int z)
    {
        Data[x + (y * Config.ChunkSize) + (z * Config.ChunkSize * Config.ChunkSize)] = id;
    }

    public void SetSolid(bool solid, int x, int y, int z)
    {
        uint line = SolidData[x + (z * Config.ChunkSize)];
        line &= ~(1u << y);
        if (solid)
        {
            line |= 1u << y;
        }

        SolidData[x + (z * Config.ChunkSize)] = line;
    }

    public bool GetSolid(int x, int y, int z)
    {
        uint line = SolidData[x + (z * Config.ChunkSize)];
        return (line & (1u << y)) != 0;
    }

    public void SetTransparent(bool transparent, int x, int y, int z)
    {
        uint line = TransparentData[x + (z * Config.ChunkSize)];
        line &= ~(1u << y);
        if (transparent)
        {
            line |= 1u << y;
        }

        TransparentData[x + (z * Config.ChunkSize)] = line;
    }

    public bool GetTransparent(int x, int y, int z)
    {
        uint line = TransparentData[x + (z * Config.ChunkSize)];
        return (line & (1u << y)) != 0;
    }
}