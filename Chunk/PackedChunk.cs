using System;
using System.Collections.Generic;
using Blockgame_OpenTK.BlockProperty;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Chunks;

public enum PackedChunkQueueType : int
{
    PassOne = 0,
    LightPropagation = 1,
    Mesh = 2,
    Renderable = 3
}

public struct LightColor
{
    public static LightColor Zero => new LightColor(0, 0, 0);
    public ushort LightData { get; private set; }
    public ushort R 
    {
        get => (ushort) ((LightData >> 12) & 15);
        set => LightData = (ushort) ((LightData & 0b0000111111111111) | ((ushort)Math.Clamp(value, 0u, 15u) << 12)); 
    }

    public ushort G 
    {
        get => (ushort) ((LightData >> 8) & 15);
        set => LightData = (ushort) ((LightData & 0b1111000011111111) | ((ushort)Math.Clamp(value, 0u, 15u) << 8)); 
    }

    public ushort B 
    {
        get => (ushort) ((LightData >> 4) & 15);
        set => LightData = (ushort) ((LightData & 0b1111111100001111) | ((ushort)Math.Clamp(value, 0u, 15u) << 4)); 
    }

    public int MaxValue => Math.Max(R, Math.Max(G, B));
    public Vector3 Normalized => (R / 15.0f, G / 15.0f, B / 15.0f);

    public static bool operator ==(LightColor a, LightColor b) => a.R == b.R && a.G == b.G && a.B == b.B; 
    public static bool operator !=(LightColor a, LightColor b) => a.R != b.R || a.G != b.G || a.B != b.B;
    public static bool operator <(LightColor a, LightColor b) => a.R < b.R || a.G < b.G || a.B < b.B;
    public static bool operator >(LightColor a, LightColor b) => a.R > b.R || a.G > b.G || a.B > b.B;
    public static bool operator <=(LightColor a, LightColor b) => a.R <= b.R || a.G <= b.G || a.B <= b.B;
    public static bool operator >=(LightColor a, LightColor b) => a.R >= b.R || a.G >= b.G || a.B >= b.B;

    public static LightColor operator -(LightColor a, LightColor b) 
    {
        return new LightColor((ushort)Math.Max(a.R - b.R, 0), (ushort)Math.Max(a.G - b.G, 0), (ushort)Math.Max(a.B - b.B, 0));
    }
    public override string ToString()
    {
        return $"({R}, {G}, {B})";
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

    public BlockLight(ushort lightData) 
    {
        LightColor = new LightColor(lightData);
    }

}
public class PackedChunk
{

    public Vector3i ChunkPosition;
    public PackedChunkQueueType QueueType = PackedChunkQueueType.PassOne;
    public bool HasPriority = false;
    
    public ushort[] BlockData = new ushort[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
    public ushort[] LightData = new ushort[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
    public uint[] SolidMask = new uint[GlobalValues.ChunkSize * GlobalValues.ChunkSize];
    public Queue<BlockLight> BlockLightAdditionQueue = new();
    public Queue<BlockLight> BlockLightRemovalQueue = new();

    public PackedChunk(Vector3i chunkPosition)
    {
        
        ChunkPosition = chunkPosition;
        
    }

    public Vector3 GetSunlightValue(Vector3i localBlockPosition)
    {
        uint val = (ushort) (LightData[ChunkUtils.VecToIndex(localBlockPosition)] & 15);
        return (val, val, val);
    }

}