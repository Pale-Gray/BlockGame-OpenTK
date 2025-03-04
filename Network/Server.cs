using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;
using Blockgame_OpenTK.BlockProperty;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.PlayerUtil;
using Blockgame_OpenTK.Util;
using LiteNetLib;
using LiteNetLib.Utils;
using NVorbis.Contracts;
using OpenTK.Platform;
using Tomlet;
using Tomlet.Attributes;

namespace Blockgame_OpenTK.Core.Networking;

public struct ServerProperties
{

    [TomlProperty("address")]
    public string AddressOrHost { get; set; }
    
    [TomlProperty("port")]
    public int Port { get; set; }

    [TomlProperty("max_players")]
    public int MaxPlayers { get; set; }

    public ServerProperties(ServerProperties properties) 
    {

        // Address = IPAddress.Parse(AddressString);
    }

}
public class Server
{

    private RSACryptoServiceProvider _rsa;
    public byte[] PublicKey => _rsa.ExportRSAPublicKey();
    public bool IsMultiplayer;
    public EventBasedNetListener Listener;
    public NetManager NetworkManager;
    public ServerProperties Properties { get; private set; }
    public World World;
    public Dictionary<long, Player> ConnectedPlayers = new();
    public Dictionary<long, NetPeer> ConnectedPlayerPeers = new();
    public Dictionary<long, PlayerChunkArea> PlayerChunkAreas = new();
    private NetDataWriter _writer = new();
    public Server(bool isMultiplayer = false)
    {

        IsMultiplayer = isMultiplayer;
        _rsa = new RSACryptoServiceProvider(2048);

        Listener = new EventBasedNetListener();
        NetworkManager = new NetManager(Listener);

    }

    public void Start()
    {

        if (IsMultiplayer)
        {

            Properties = TomletMain.To<ServerProperties>(File.ReadAllText("server.toml"));

            NetworkManager.IPv6Enabled = false;
            NetworkManager.Start(Properties.AddressOrHost, Properties.AddressOrHost, Properties.Port);

            GameLogger.Log($"Started a server at {Properties.AddressOrHost}:{Properties.Port}");

            Listener.ConnectionRequestEvent += request => 
            {

                GameLogger.Log("Someone is trying to join");

                if (NetworkManager.ConnectedPeersCount >= Properties.MaxPlayers)
                {
                    _writer.Put((byte)PacketType.DisconnectErrorPacket);
                    _writer.Put("Maximum players exceeded");
                    request.Reject(_writer);
                    _writer.Reset();
                } else
                {
                    request.AcceptIfKey("BlockGame");
                }

            };

            Listener.PeerConnectedEvent += peer =>   
            {

                GameLogger.Log("A player has connected");

                _writer.Put((byte)PacketType.RequestPlayerUniqueIdPacket);
                peer.Send(_writer, DeliveryMethod.ReliableOrdered);
                _writer.Reset();

            };

            Listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {

                GameLogger.Log($"A client has disconnected");

            };

            Listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
            {

                PacketType packetType = (PacketType)dataReader.GetByte();

                switch (packetType)
                {
                    case PacketType.SendPlayerUniqueIdPacket:
                        long uid = dataReader.GetLong();
                        GameLogger.Log($"Player with uid {uid} is trying to join");
                        if (ConnectedPlayers.ContainsKey(uid))
                        {
                            _writer.Put((byte)PacketType.DisconnectErrorPacket);
                            _writer.Put("The uid already exists on the server.");
                            GameLogger.Log($"{PacketType.DisconnectErrorPacket}: The uid already exists on the server");
                            fromPeer.Disconnect(_writer);
                            _writer.Reset();
                        } else 
                        {
                            GameLogger.Log($"The player with uid {uid} would be considered joined.");
                            ConnectedPlayers.Add(uid, new Player() { UserId = uid });
                            ConnectedPlayerPeers.Add(uid, fromPeer);
                        }
                        break;
                    case PacketType.BlockPlacePacket:
                        ushort blockId = dataReader.GetUShort();
                        int x = dataReader.GetInt();
                        int y = dataReader.GetInt();
                        int z = dataReader.GetInt();
                        GlobalValues.NewRegister.GetBlockFromId(blockId).OnBlockPlace(World, (x,y,z));

                        _writer.Put((byte)PacketType.BlockPlacePacket);
                        _writer.Put(blockId);
                        _writer.Put(x);
                        _writer.Put(y);
                        _writer.Put(z);
                        NetworkManager.SendToAll(_writer, DeliveryMethod.ReliableOrdered, fromPeer);
                        break;

                }

                dataReader.Recycle();

            };

        } else
        {



        }

    }   

    public void Update()
    {

        if (IsMultiplayer)
        {

            NetworkManager.PollEvents();

            foreach (Player player in ConnectedPlayers.Values)
            {

                

            }

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