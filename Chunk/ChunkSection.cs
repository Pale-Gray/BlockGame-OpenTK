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

    public void SetBlockId(ushort id, int x, int y, int z)
    {
        Data[x + (y * Config.ChunkSize) + (z * Config.ChunkSize * Config.ChunkSize)] = id;
    }
}