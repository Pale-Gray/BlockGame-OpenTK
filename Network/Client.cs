using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using LiteNetLib;
using LiteNetLib.Layers;
using LiteNetLib.Utils;

namespace Blockgame_OpenTK.Core.Networking;

public class Client
{

    public bool IsMultiplayer { get; private set; }
    public EventBasedNetListener Listener;
    public NetManager NetworkManager;
    // public World World = new();

    public Client(bool isMultiplayer = false)
    {

        IsMultiplayer = isMultiplayer;
        Listener = new EventBasedNetListener();
        NetworkManager = new NetManager(Listener);

    }

    public void JoinWorld(string worldName)
    {

        IsMultiplayer = false;

    }

    public void JoinWorld(string address, int port, long uid)
    {

        IsMultiplayer = true;
        NetworkManager.Start();
        NetworkManager.Connect(address, port, "BlockGame");
        // PackedWorldGenerator.Initialize();

        Listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {

            PacketType packetType = (PacketType)dataReader.GetUShort();
            IPacket packet;

            Console.WriteLine(packetType);

            switch (packetType)
            {

                case PacketType.PlayerDataRequestPacket:
                    NetDataWriter writer = new NetDataWriter();
                    writer.Put((byte)PacketType.PlayerDataSendPacket);
                    writer.Put(uid);
                    fromPeer.Send(writer, DeliveryMethod.ReliableOrdered);
                    break;
                case PacketType.BlockPlacePacket:
                    packet = new BlockPlacePacket();
                    packet.Deserialize(dataReader);
                    // GlobalValues.Register.GetBlockFromId(((BlockPlacePacket)packet).BlockId).OnBlockPlace(World, ((BlockPlacePacket)packet).BlockPosition);
                    break;
                case PacketType.ChunkSendPacket:
                    packet = new ChunkSendPacket();
                    packet.Deserialize(dataReader);
                    // GameLogger.Log($"Received a {PacketType.ChunkSendPacket} at chunk position {((ChunkSendPacket)packet).Position}");
                    // GameLogger.Log("need to add a chunk");
                    // bool added = World.WorldColumns.TryAdd(((ChunkSendPacket)packet).Position, new ChunkColumn(((ChunkSendPacket)packet).Position) { QueueType = ColumnQueueType.Mesh } );
                    // ColumnSerializer.DeserializeColumnFromBytes(World.WorldColumns[((ChunkSendPacket)packet).Position], ((ChunkSendPacket)packet).Data);
                    // for (int i = 0; i < PackedWorldGenerator.WorldGenerationHeight; i++)
                    // {
                    //     World.WorldColumns[((ChunkSendPacket)packet).Position].Chunks[i].HasUpdates = true;
                    // }
                    // PackedWorldGenerator.ColumnWorldGenerationQueue.EnqueueLast(((ChunkSendPacket)packet).Position);

                    ChunkReceivePacket received = new ChunkReceivePacket();
                    received.Position = ((ChunkSendPacket)packet).Position;
                    NetDataWriter dataWriter = new NetDataWriter();
                    received.Serialize(dataWriter);
                    fromPeer.Send(dataWriter, DeliveryMethod.ReliableOrdered);
                    break;
                case PacketType.ChunkRemovePacket:
                    packet = new ChunkRemovePacket();
                    packet.Deserialize(dataReader);
                    // NetworkingValues.Client.World.WorldColumns.Remove(((ChunkRemovePacket)packet).ChunkPosition, out _);
                    break;

            }

            dataReader.Recycle();

        };

        Listener.PeerDisconnectedEvent += (fromPeer, disconnectInfo) =>
        {

            PacketType packetType = (PacketType)disconnectInfo.AdditionalData.GetByte();
            string reason = disconnectInfo.AdditionalData.GetString();

            if (packetType != PacketType.DisconnectSuccessPacket) GameLogger.Log($"failed to connect with reason {reason}");

        };

    }

    public void Update()
    {

        if (IsMultiplayer)
        {

            NetworkManager.PollEvents();

        } else
        {



        }

    }

    public void Stop()
    {

        if (IsMultiplayer)
        {

            NetworkManager.Stop();

        } else
        {



        }   

    }

}