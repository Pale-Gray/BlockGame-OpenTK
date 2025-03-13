using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Chunks;

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

public struct LightColor
{
    public static LightColor Zero => new LightColor(0, 0, 0);
    public ushort LightData { get; private set; }
    public ushort R 
    {
        get => (ushort) ((LightData >> 12) & 15);
        set => LightData = (ushort) ((LightData & 0x0FFF) | ((ushort)Math.Clamp(value, 0u, 15u) << 12)); 
    }

    public ushort G 
    {
        get => (ushort) ((LightData >> 8) & 15);
        set => LightData = (ushort) ((LightData & 0xF0FF) | ((ushort)Math.Clamp(value, 0u, 15u) << 8)); 
    }

    public ushort B 
    {
        get => (ushort) ((LightData >> 4) & 15);
        set => LightData = (ushort) ((LightData & 0xFF0F) | ((ushort)Math.Clamp(value, 0u, 15u) << 4)); 
    }

    public int MaxValue => Math.Max(R, Math.Max(G, B));
    public Vector3 Normalized => (R / 15.0f, G / 15.0f, B / 15.0f);

    public static bool operator ==(LightColor a, LightColor b) => a.R == b.R && a.G == b.G && a.B == b.B; 
    public static bool operator !=(LightColor a, LightColor b) => a.R != b.R || a.G != b.G || a.B != b.B;
    public static bool operator <(LightColor a, LightColor b) => a.R < b.R || a.G < b.G || a.B < b.B;
    public static bool operator >(LightColor a, LightColor b) => a.R > b.R || a.G > b.G || a.B > b.B;
    public static bool operator <=(LightColor a, LightColor b) => a.R <= b.R || a.G <= b.G || a.B <= b.B;
    public static bool operator >=(LightColor a, LightColor b) => a.R >= b.R || a.G >= b.G || a.B >= b.B;
    public static LightColor operator +(LightColor a, uint value)
    {
        return new LightColor((ushort)Math.Min(a.R + value, 15), (ushort)Math.Min(a.G + value, 15), (ushort)Math.Min(a.B + value, 15));
    }
    public static LightColor operator -(LightColor a, uint value)
    {
        ushort r = 0;
        ushort g = 0;
        ushort b = 0;
        if (a.R >= value) r = (ushort) (a.R - value);
        if (a.G >= value) g = (ushort) (a.G - value);
        if (a.B >= value) b = (ushort) (a.B - value);
        return new LightColor(r,g,b);
    }
    public static LightColor operator -(LightColor a, LightColor bLight) 
    {
        ushort r = 0;
        ushort g = 0;
        ushort b = 0;
        if (a.R - bLight.R > 0) r = (ushort) (a.R - bLight.R);
        if (a.G - bLight.G > 0) g = (ushort) (a.G - bLight.G);
        if (a.B - bLight.B > 0) b = (ushort) (a.B - bLight.B);
        return new LightColor(r,g,b);
    }
    public static Vector3b ComponentWiseLessThan(LightColor a, LightColor b)
    {
        return (a.R < b.R, a.G < b.G, a.B < b.B);
    }
    public override string ToString()
    {
        return $"({R}, {G}, {B})";
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {

        if (obj is not LightColor) return false;
        LightColor light = (LightColor)obj;
        return light.R == R && light.G == G && light.B == B;

    }

    public override int GetHashCode()
    {
        return LightData.GetHashCode();
    }

    public static LightColor Max(LightColor a, LightColor b) 
    {
        return new LightColor(Math.Max(a.R, b.R), Math.Max(a.G, b.G), Math.Max(a.B, b.B));
    }
    public LightColor(ushort r, ushort g, ushort b) 
    {   
        R = r;
        G = g;
        B = b;
    }

    public LightColor(ushort lightData) 
    {

        LightData = lightData;

    }

}

public struct BlockLight
{

    public Vector3i Position;
    public LightColor LightColor;

    public BlockLight(LightColor lightColor)
    {
        LightColor = lightColor;
    }

    public BlockLight(Vector3i localBlockPosition, LightColor lightColor)
    {
        LightColor = lightColor;
        Position = localBlockPosition;
    }

    public BlockLight(ushort lightData) 
    {
        LightColor = new LightColor(lightData);
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {

        if (obj is not BlockLight) return false;
        BlockLight light = (BlockLight)obj;
        return light.Position == Position && light.LightColor == LightColor;

    }

    public override int GetHashCode()
    {

        return Position.GetHashCode();

    }

}

public struct SunLight
{

    public ushort Value;
    public Vector3i Position;
    public float Normalized => Value / 15.0f;
    public SunLight(Vector3i localBlockPosition, ushort value)
    {
        Value = (ushort) Math.Clamp(value, 0u, 15u);
        Position = localBlockPosition;
    }

}
public class PackedChunk
{

    public Vector3i ChunkPosition;
    public PackedChunkQueueType QueueType = PackedChunkQueueType.PassOne;
    public bool HasPriority = false;
    public bool HasUpdates = false;
    
    public ushort[] BlockData = new ushort[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
    public ushort[] LightData = new ushort[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
    public uint[] SolidMask = new uint[GlobalValues.ChunkSize * GlobalValues.ChunkSize];
    public Queue<BlockLight> BlockLightAdditionQueue = new();
    public Queue<BlockLight> BlockLightRemovalQueue = new();
    public Queue<SunLight> SunlightAdditionQueue = new();
    public Queue<SunLight> SunlightRemovalQueue = new();

    public PackedChunk(Vector3i chunkPosition)
    {
        
        ChunkPosition = chunkPosition;
        
    }

    public Vector3 GetSunlightValue(Vector3i localBlockPosition)
    {
        uint val = (ushort) (LightData[ChunkUtils.VecToIndex(localBlockPosition)] & 15);
        return (val, val, val);
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