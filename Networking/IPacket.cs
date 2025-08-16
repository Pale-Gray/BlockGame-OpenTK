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
    PlayerMove,
    BlockDestroy,
    BlockPlace
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
        }
        
        return this;
    }
    
    public ChunkDataPacket() {}
}

