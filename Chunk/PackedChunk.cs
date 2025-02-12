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
public class PackedChunk
{

    public Vector3i ChunkPosition;
    public PackedChunkQueueType QueueType = PackedChunkQueueType.PassOne;
    public bool HasPriority = false;
    
    public ushort[] BlockData = new ushort[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
    public ushort[] LightData = new ushort[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
    public uint[] SolidMask = new uint[GlobalValues.ChunkSize * GlobalValues.ChunkSize];

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