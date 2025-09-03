using System;
using OpenTK.Mathematics;
using VoxelGame.Util;

namespace VoxelGame.Networking;

public interface IPacket
{
    public PacketType Type { get; set; }
    public void Serialize(DataWriter writer);
    public IPacket Deserialize(DataReader reader);
}

public enum PacketType
{
    ChunkData,
    PlayerJoin,
    PlayerMove,
    BlockDestroy,
    BlockPlace
}

public struct PlayerJoinPacket() : IPacket
{
    public PacketType Type { get; set; } = PacketType.PlayerJoin;
    public Guid Id;
    public void Serialize(DataWriter writer)
    {
        writer.Write(Id.ToString());
    }

    public IPacket Deserialize(DataReader reader)
    {
        Id = Guid.Parse(reader.ReadString());
        return this;
    }
}

public struct PlayerMovePacket() : IPacket
{
    public PacketType Type { get; set; } = PacketType.PlayerMove;
    public Vector3 Position;
    public void Serialize(DataWriter writer)
    {
        writer.Write(Position.X);
        writer.Write(Position.Y);
        writer.Write(Position.Z);
    }

    public IPacket Deserialize(DataReader reader)
    {
        Position = (reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
        return this;
    }
}

public struct BlockPlacePacket() : IPacket
{
    public PacketType Type { get; set; } = PacketType.BlockPlace;
    public ushort Id;
    public Vector3i GlobalBlockPosition;

    public void Serialize(DataWriter writer)
    {
        writer.Write(Id);
        writer.Write(GlobalBlockPosition.X);
        writer.Write(GlobalBlockPosition.Y);
        writer.Write(GlobalBlockPosition.Z);
    }

    public IPacket Deserialize(DataReader reader)
    {
        Id = reader.ReadUInt16();
        GlobalBlockPosition = (reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        
        return this;
    }
}

public struct BlockDestroyPacket() : IPacket
{
    public PacketType Type { get; set; } = PacketType.BlockDestroy;
    public ushort Id;
    public Vector3i GlobalBlockPosition;
    
    public void Serialize(DataWriter writer)
    {
        writer.Write(Id);
        writer.Write(GlobalBlockPosition.X);
        writer.Write(GlobalBlockPosition.Y);
        writer.Write(GlobalBlockPosition.Z);
    }

    public IPacket Deserialize(DataReader reader)
    {
        Id = reader.ReadUInt16();
        GlobalBlockPosition = (reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        
        return this;
    }
}

public struct ChunkDataPacket : IPacket
{
    public PacketType Type { get; set; } = PacketType.ChunkData;
    public Vector2i Position;
    public Chunk Column;
    
    public void Serialize(DataWriter writer)
    {
        writer.Write(Position.X);
        writer.Write(Position.Y);
        for (int i = 0; i < Config.ColumnSize; i++)
        {
            writer.WriteValues(RunLengthEncoder.Encode(Column.ChunkSections[i].Data).ToArray());
            writer.WriteValues(Column.ChunkSections[i].SolidData);
            writer.WriteValues(Column.ChunkSections[i].TransparentData);
        }
    }

    public IPacket Deserialize(DataReader reader)
    {
        Position = (reader.ReadInt32(), reader.ReadInt32());
        Column = new Chunk(Position);
        for (int i = 0; i < Config.ColumnSize; i++)
        {
            Column.ChunkSections[i].Position = (Position.X, i, Position.Y);
            Column.ChunkSections[i].Data = RunLengthEncoder.Decode(reader.ReadUInt16Values()).ToArray();
            Column.ChunkSections[i].SolidData = reader.ReadUInt32Values();
            Column.ChunkSections[i].TransparentData = reader.ReadUInt32Values();
        }
        
        return this;
    }
    
    public ChunkDataPacket() {}
}

