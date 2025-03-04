using System;
using System.Diagnostics;
using System.Xml.Linq;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Blockgame_OpenTK.Core.Networking;

public class Client
{

    public bool IsMultiplayer { get; private set; }
    public EventBasedNetListener Listener;
    public NetManager NetworkManager;
    public World World;

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

        Listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {

            PacketType packetType = (PacketType)dataReader.GetByte();

            Console.WriteLine(packetType);

            switch (packetType)
            {

                case PacketType.RequestPlayerUniqueIdPacket:
                    NetDataWriter writer = new NetDataWriter();
                    writer.Put((byte)PacketType.SendPlayerUniqueIdPacket);
                    writer.Put(uid);
                    fromPeer.Send(writer, DeliveryMethod.ReliableOrdered);
                    break;
                case PacketType.BlockPlacePacket:
                    ushort blockId = dataReader.GetUShort();
                    int x = dataReader.GetInt();
                    int y = dataReader.GetInt();
                    int z = dataReader.GetInt();
                    GlobalValues.NewRegister.GetBlockFromId(blockId).OnBlockPlace(World, (x,y,z));
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