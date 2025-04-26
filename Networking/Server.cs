using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Game.Core.Chunks;
using Game.Core.Modding;
using Game.Core.PlayerUtil;
using Game.Core.Worlds;
using Game.Util;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenTK.Mathematics;
using Tomlet;
using Tomlet.Attributes;

namespace Game.Core.Networking;

public struct ServerProperties
{

    [TomlProperty("address")]
    public string AddressOrHost { get; set; }
    
    [TomlProperty("port")]
    public int Port { get; set; }

    [TomlProperty("max_players")]
    public int MaxPlayers { get; set; }

    [TomlProperty("world_name")]
    public string WorldName { get; set; }

    public ServerProperties(ServerProperties properties) {}

}
public class Server 
{

    private EventBasedNetListener _listener;
    private NetManager _manager;
    private ServerProperties _properties;
    public Dictionary<int, Player> ConnectedPlayers = new ();
    private NetDataWriter _writer;
    public bool IsNetworked { get; private set; } = false;

    public void StartSingleplayer(string worldName, Player player)
    {

        IsNetworked = false;

        /*
            TODO
            load shit involving world generator,
            dont load blocks or anything, the client already did.
            add unauthed player to the "connected players" list.
            queue the player's position for generation.
        */

        player.Loader = new PlayerChunkLoader((0, 0));
        ConnectedPlayers.Add(0, player);

        GameState.World = new World(worldName);
        WorldGenerator.Initialize();

        // ConnectedPlayers[0].Loader.QueuePosition(World, (0,0));
        ConnectedPlayers[0].Loader.Tick(GameState.World, true);

    }
    public void StartMultiplayer()
    {

        IsNetworked = true;

        _listener = new EventBasedNetListener();
        _manager = new NetManager(_listener);
        _writer = new NetDataWriter();
        ConnectedPlayers = new Dictionary<int, Player>();
        
        _properties = TomletMain.To<ServerProperties>(File.ReadAllText("server.toml"));

        // mod loading (base)
        // GlobalValues.Base.OnLoad(GlobalValues.Register);
        ModLoader.Load();

        GameLogger.Log($"String address: {_properties.AddressOrHost}");
        GameLogger.Log($"Port: {_properties.Port}");
        GameLogger.Log($"Max players: {_properties.MaxPlayers}");
        GameLogger.Log($"World save: {_properties.WorldName}");

        GameState.World = new World(_properties.WorldName);
        WorldGenerator.Initialize();

        _manager.IPv6Enabled = false;
        _manager.Start(_properties.AddressOrHost, _properties.AddressOrHost, _properties.Port);
        GameLogger.Log($"Started server at {_properties.AddressOrHost}:{_properties.Port}");

        // network shit
        _listener.PeerConnectedEvent += (peer) =>
        {

            GameLogger.Log($"Peer id {peer.Id} joined.");   
            _writer.Put((ushort)PacketType.PlayerDataRequestPacket);
            peer.Send(_writer, DeliveryMethod.ReliableOrdered);
            _writer.Reset();

        };

        _listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {

            GameLogger.Log($"Peer id {peer.Id} disconnected.");
            ConnectedPlayers.Remove(peer.Id);

        };

        _listener.ConnectionRequestEvent += (request) => 
        {

            GameLogger.Log("A player has requested to join.");
            request.AcceptIfKey("BlockGame");

        };

        _listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
        {

            PacketType packetType = (PacketType)reader.GetUShort();

            switch (packetType)
            {
                
                case PacketType.PlayerDataSendPacket:
                    PlayerDataSendPacket packet = new PlayerDataSendPacket();
                    packet.Deserialize(reader);
                    GameLogger.Log($"{packet.UserId}, {packet.DisplayName}");
                    if (ConnectedPlayers.Where(player => player.Value.UserId == packet.UserId).Count() != 0)
                    {
                        ConnectRejectPacket reject = new ConnectRejectPacket();
                        reject.Reason = "Someone with the user id is already joined";
                        reject.Serialize(_writer);
                        peer.Send(_writer, DeliveryMethod.ReliableOrdered); 
                        _writer.Reset();
                    } else if (ConnectedPlayers.Count >= _properties.MaxPlayers)
                    {
                        ConnectRejectPacket reject = new ConnectRejectPacket();
                        reject.Reason = "Player count exceeded";
                        reject.Serialize(_writer);
                        peer.Send(_writer, DeliveryMethod.ReliableOrdered);
                        peer.Disconnect();
                        _writer.Reset();
                    } else
                    {
                        ConnectedPlayers.Add(peer.Id, new Player() { UserId = packet.UserId, DisplayName = packet.DisplayName, Loader = new PlayerChunkLoader(Vector2i.Zero) }); 
                        // ConnectedPlayers[peer.Id].Loader.QueuePosition(GameState.World, Vector2i.Zero);
                        _writer.Put((ushort)PacketType.ConnectSuccessPacket);
                        peer.Send(_writer, DeliveryMethod.ReliableOrdered);
                        _writer.Reset();
                    }
                    break;

            }

            reader.Recycle();

        };

    }

    public void TickUpdate()
    {

        GameState.World?.TickUpdate();

    }

    public void Update()
    {

        WorldGenerator.Update();

        foreach (Player player in ConnectedPlayers.Values)
        {

            player.Loader.PlayerPosition = ColumnUtils.PositionToChunk(VectorMath.Floor(player.Position));
            player.Loader.Tick(GameState.World);

        }

        _manager?.PollEvents();

    }

    public void Unload()
    {



    }

    public void SendChunk(Vector2i position)
    {

        foreach (Player player in ConnectedPlayers.Values)
        {

            if (!player.SentChunks.Contains(position))
            {

                player.SentChunks.Add(position);
                ChunkSendPacket packet = new ChunkSendPacket();
                packet.Position = position;
                packet.Data = ColumnSerializer.SerializeColumnToBytes(GameState.World.WorldColumns[position]);
                packet.Serialize(_writer);
                _manager.GetPeerById(player.NetPeerId).Send(_writer, DeliveryMethod.ReliableOrdered);
                _writer.Reset();

            }

        }

    }

}