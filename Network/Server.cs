using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;
using Blockgame_OpenTK.BlockProperty;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.PlayerUtil;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.PlayerUtil;
using Blockgame_OpenTK.Util;
using LiteNetLib;
using LiteNetLib.Utils;
using NVorbis.Contracts;
using OpenTK.Mathematics;
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
    public World World = new();
    public Dictionary<int, NewPlayer> ConnectedPlayers = new();
    

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

            PackedWorldGenerator.Initialize();
            PackedWorldGenerator.CurrentWorld = World;
            

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
                    _writer.Put("Maximum players reached.");
                    GameLogger.Log($"{PacketType.DisconnectErrorPacket}: Maximum players reached.");
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

                GameLogger.Log($"Player with uid {ConnectedPlayers[peer.Id].UserId} and netpeer id {peer.Id} has disconnected.");
                ConnectedPlayers.Remove(peer.Id);

            };

            Listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
            {

                PacketType packetType = (PacketType)dataReader.GetByte();
                IPacket packet;

                switch (packetType)
                {
                    case PacketType.SendPlayerUniqueIdPacket:
                        long uid = dataReader.GetLong();
                        GameLogger.Log($"Player with uid {uid} and netpeer id {fromPeer.Id} is trying to join");
                        
                        if (ConnectedPlayers.Where(player => player.Value.UserId == uid).Count() != 0)
                        {
                            // disconnect
                            _writer.Put((byte)PacketType.DisconnectErrorPacket);
                            _writer.Put($"A player with the uid {uid} already exists.");
                            GameLogger.Log($"{PacketType.DisconnectErrorPacket}: A player with the uid {uid} already exists.");
                            fromPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
                            _writer.Reset();
                        } else
                        {
                            // accept
                            ConnectedPlayers.Add(fromPeer.Id, new NewPlayer() { NetPeerId = fromPeer.Id, UserId = uid, DisplayName = "null", Position = (0, 0, 0) });
                            ConnectedPlayers[fromPeer.Id].ResolveAreaDifference((0, 0));
                            GameLogger.Log($"A player with the uid {uid} has successfully joined the game.");
                            packet = new ConnectSuccessPacket();
                            packet.Serialize(_writer);
                            fromPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
                            _writer.Reset();
                        }
                        break;
                    case PacketType.BlockPlacePacket:
                        packet = new BlockPlacePacket();
                        packet.Deserialize(dataReader);
                        break;

                }

                dataReader.Recycle();

            };

        } else
        {

            

        }

    }   

    public void SendChunk(Vector2i chunkPosition)
    {

        foreach (NewPlayer player in ConnectedPlayers.Values)
        {

            Vector2i position = ChunkUtils.PositionToChunk(player.Position).Xz;
            // Console.WriteLine(position);
            if (Maths.ChebyshevDistance2D(position, chunkPosition) <= PackedWorldGenerator.WorldGenerationRadius)
            {
                
                // ColumnSerializer.SerializeColumn(World.WorldColumns[chunkPosition]);

                ChunkSendPacket packet = new ChunkSendPacket();
                packet.Position = chunkPosition;
                packet.Data = ColumnSerializer.SerializeColumnToBytes(World.WorldColumns[chunkPosition]);
                packet.Serialize(_writer);
                NetworkManager.GetPeerById(player.NetPeerId).Send(_writer, DeliveryMethod.ReliableOrdered);
                _writer.Reset();

            }

        }

    }   

    public void Update()
    {

        if (IsMultiplayer)
        {

            NetworkManager.PollEvents();
            PackedWorldGenerator.Poll();



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