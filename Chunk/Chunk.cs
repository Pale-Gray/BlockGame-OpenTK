using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Game.Util;
using OpenTK.Mathematics;

namespace Game.Core.Chunks;

public enum PackedChunkQueueType : int
{
    PassOne = 0,
    SunlightCalculation = 1,
    LightPropagation = 2,
    Mesh = 3,
    Renderable = 4
}

public struct Vector3b
{

    public bool IsTrue => X && Y && Z;
    public bool X;
    public bool Y;
    public bool Z;

    public Vector3b(bool value)
    {
        X = value;
        Y = value;
        Z = value;
    }

    public Vector3b(bool x, bool y, bool z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static implicit operator Vector3b((bool, bool, bool) values) => new Vector3b(values.Item1, values.Item2, values.Item3);

}

public class Chunk
{

    public Vector3i ChunkPosition;
    public PackedChunkQueueType QueueType = PackedChunkQueueType.PassOne;
    public bool HasPriority = false;
    public bool HasUpdates = false;
    
    public ushort[] BlockData = new ushort[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
    public ushort[] LightData = new ushort[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
    public uint[] SolidMask = new uint[GlobalValues.ChunkSize * GlobalValues.ChunkSize];
    public uint[] IgnoreCullMask = new uint[GlobalValues.ChunkSize * GlobalValues.ChunkSize];

    public Chunk(Vector3i chunkPosition)
    {
        
        ChunkPosition = chunkPosition;
        
    }

    public Vector3 GetSunlightValue(Vector3i localBlockPosition)
    {
        uint val = (ushort) (LightData[ChunkUtils.VecToIndex(localBlockPosition)] & 15);
        return (val, val, val);
    }

    public void SetSunlightValue(Vector3i localBlockPosition, ushort value)
    {

        LightData[ChunkUtils.VecToIndex(localBlockPosition)] = (ushort) ((LightData[ChunkUtils.VecToIndex(localBlockPosition)] & ~15) | (ushort) Math.Clamp(value, 0.0, 15.0));

    }

    public bool IsEmpty()
    {

        for (int x = 0; x < GlobalValues.ChunkSize; x++)
        {
            for (int y = 0; y < GlobalValues.ChunkSize; y++)
            {
                for (int z = 0; z < GlobalValues.ChunkSize; z++)
                {
                    if (BlockData[ChunkUtils.VecToIndex((x,y,z))] != 0) return false;
                }
            }
        }
        return true;

    }

}