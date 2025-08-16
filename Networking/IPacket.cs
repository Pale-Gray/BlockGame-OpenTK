using System.Numerics;
using LiteNetLib.Utils;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace VoxelGame.Networking;

public interface IPacket
{
    public PacketType Type { get; set; }
    public void Serialize(NetDataWriter writer);
    public IPacket Deserialize(NetDataReader reader);
}

public enum PacketType
{
    ChunkData,
    PlayerMove,
    BlockDestroy,
}

public struct PlayerMovePacket : IPacket
{
    public PacketType Type { get; set; } = PacketType.PlayerMove;
    public Vector3 PositionTo;
    
    public void Serialize(NetDataWriter writer)
    {
        writer.Put((int)Type);
        writer.Put(PositionTo.X);
        writer.Put(PositionTo.Y);
        writer.Put(PositionTo.Z);
    }

    public IPacket Deserialize(NetDataReader reader)
    {
        PositionTo = (reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        
        return this;
    }

    public PlayerMovePacket() {}
}

public struct BlockDestroyPacket() : IPacket
{
    public PacketType Type { get; set; } = PacketType.BlockDestroy;
    public Vector3i GlobalBlockPosition;
    
    public void Serialize(NetDataWriter writer)
    {
        writer.Put((int)Type);
        writer.Put(GlobalBlockPosition.X);
        writer.Put(GlobalBlockPosition.Y);
        writer.Put(GlobalBlockPosition.Z);
    }

    public IPacket Deserialize(NetDataReader reader)
    {
        GlobalBlockPosition = (reader.GetInt(), reader.GetInt(), reader.GetInt());
        
        return this;
    }
}

public struct ChunkDataPacket : IPacket
{
    public PacketType Type { get; set; } = PacketType.ChunkData;
    public Vector2i Position;
    public Chunk Column;
    
    public void Serialize(NetDataWriter writer)
    {
        writer.Put((int)Type);
        writer.Put(Position.X);
        writer.Put(Position.Y);
        for (int i = 0; i < Config.ColumnSize; i++)
        {
            for (int l = 0; l < Column.ChunkSections[i].Data.Length; l++)
            {
                writer.Put(Column.ChunkSections[i].Data[l]);
            }
        }
    }

    public IPacket Deserialize(NetDataReader reader)
    {
        Position = (reader.GetInt(), reader.GetInt());
        Column = new Chunk(Position);
        for (int i = 0; i < Config.ColumnSize; i++)
        {
            Column.ChunkSections[i].Position = (Position.X, i, Position.Y);
            for (int l = 0; l < Column.ChunkSections[i].Data.Length; l++)
            {
                Column.ChunkSections[i].Data[l] = reader.GetUShort();
            }
        }
        
        return this;
    }
    
    public ChunkDataPacket() {}
}

