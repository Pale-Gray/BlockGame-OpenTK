using System;
using System.IO;
using Game.Core.Chunks;
using LiteNetLib.Utils;
using OpenTK.Mathematics;

namespace Game.Core.Networking;

public enum PacketType : ushort
{

    DisconnectErrorPacket,
    DisconnectSuccessPacket,
    ConnectSuccessPacket,
    ConnectRejectPacket,
    PlayerDataRequestPacket,
    PlayerDataSendPacket,
    BlockPlacePacket,
    LightPlacePacket,
    BlockRemovePacket,
    LightRemovePacket,
    ChunkSendPacket,
    ChunkReceivePacket,
    ChunkRemovePacket

}
public class PacketManager
{

    

}   

public interface IPacket
{

    public PacketType Type { get; } 

    public abstract void Serialize(NetDataWriter writer);

    public abstract void Deserialize(NetDataReader reader);

}

public class DisconnectErrorPacket : IPacket
{

    public PacketType Type => PacketType.DisconnectErrorPacket;

    public string Message;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((ushort)Type);
        writer.Put(Message);
    }

    public void Deserialize(NetDataReader reader)
    {
        Message = reader.GetString();
    }

}

public class ConnectRejectPacket : IPacket
{

    public PacketType Type => PacketType.ConnectRejectPacket;

    public string Reason = "";

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((ushort)Type);
        writer.Put(Reason);
    }

    public void Deserialize(NetDataReader reader)
    {
        Reason = reader.GetString();
    }

}

public class ConnectSuccessPacket : IPacket
{

    public PacketType Type => PacketType.ConnectSuccessPacket;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((ushort)Type);
    }

    public void Deserialize(NetDataReader reader)
    {

    }

}

public class PlayerDataSendPacket : IPacket
{

    public PacketType Type => PacketType.PlayerDataSendPacket;

    public long UserId;
    public string DisplayName;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((ushort)Type);
        writer.Put(UserId);
        writer.Put(DisplayName);
    }

    public void Deserialize(NetDataReader reader)
    {
        UserId = reader.GetLong();
        DisplayName = reader.GetString();
    }

}

public class BlockPlacePacket : IPacket
{

    public PacketType Type => PacketType.BlockPlacePacket;

    public ushort BlockId;
    public Vector3i BlockPosition;
    public void Serialize(NetDataWriter writer)
    {
        writer.Put((ushort)Type);
        writer.Put(BlockId);
        writer.Put(BlockPosition.X);
        writer.Put(BlockPosition.Y);
        writer.Put(BlockPosition.Z);
    }

    public void Deserialize(NetDataReader reader)
    {
        BlockId = reader.GetUShort();
        BlockPosition.X = reader.GetInt();
        BlockPosition.Y = reader.GetInt();
        BlockPosition.Z = reader.GetInt();
    }

}

public class ChunkSendPacket : IPacket
{

    public PacketType Type => PacketType.ChunkSendPacket;

    public Vector2i Position;
    public byte[] Data;
    public void Serialize(NetDataWriter writer)
    {
        writer.Put((ushort)Type);
        writer.Put(Position.X);
        writer.Put(Position.Y);
        writer.Put(Data);
    }

    public void Deserialize(NetDataReader reader)
    {
        Position.X = reader.GetInt();
        Position.Y = reader.GetInt();
        Data = reader.GetRemainingBytes();
    }

}

public class ChunkReceivePacket : IPacket
{

    public PacketType Type => PacketType.ChunkReceivePacket;

    public Vector2i Position;
    public void Serialize(NetDataWriter writer)
    {
        writer.Put((ushort)Type);
        writer.Put(Position.X);
        writer.Put(Position.Y);
    }

    public void Deserialize(NetDataReader reader)
    {
        Position.X = reader.GetInt();
        Position.Y = reader.GetInt();
    }

}

public class ChunkRemovePacket : IPacket
{

    public PacketType Type => PacketType.ChunkRemovePacket;

    public Vector2i ChunkPosition;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((ushort)Type);
        writer.Put(ChunkPosition.X);
        writer.Put(ChunkPosition.Y);
    }

    public void Deserialize(NetDataReader reader)
    {
        ChunkPosition.X = reader.GetInt();
        ChunkPosition.Y = reader.GetInt();
    }

}

