using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Blockgame_OpenTK.Core.PlayerUtil;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenTK.Mathematics;
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

    [TomlProperty("world_name")]
    public string WorldName { get; set; }

    public ServerProperties(ServerProperties properties) {}

}
public class NewServer 
{

    private EventBasedNetListener _listener;
    private NetManager _manager;
    private ServerProperties _properties;
    private World _world;
    private Dictionary<int, NewPlayer> _connectedPlayers;
    private NetDataWriter _writer;
    public bool IsNetworked { get; private set; } = false;

    public void Load()
    {

        // mod loading (base)
        GlobalValues.Base.OnLoad(GlobalValues.Register);

        // start threads
        PackedWorldGenerator.Initialize();

        // world loading
        _world = new World(_properties.WorldName);
        PackedWorldGenerator.CurrentWorld = _world;

    }
    public void Start()
    {

        _listener = new EventBasedNetListener();
        _manager = new NetManager(_listener);
        _writer = new NetDataWriter();
        _connectedPlayers = new Dictionary<int, NewPlayer>();
        
        _properties = TomletMain.To<ServerProperties>(File.ReadAllText("server.toml"));

        GameLogger.Log($"String address: {_properties.AddressOrHost}");
        GameLogger.Log($"Port: {_properties.Port}");
        GameLogger.Log($"Max players: {_properties.MaxPlayers}");
        GameLogger.Log($"World save: {_properties.WorldName}");

        _world = new World(_properties.WorldName);
        PackedWorldGenerator.CurrentWorld = _world;
        PackedWorldGenerator.Initialize();

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
            _connectedPlayers.Remove(peer.Id);

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
                    if (_connectedPlayers.Where(player => player.Value.UserId == packet.UserId).Count() != 0)
                    {
                        ConnectRejectPacket reject = new ConnectRejectPacket();
                        reject.Reason = "Someone with the user id is already joined";
                        reject.Serialize(_writer);
                        peer.Send(_writer, DeliveryMethod.ReliableOrdered); 
                        _writer.Reset();
                    } else if (_connectedPlayers.Count >= _properties.MaxPlayers)
                    {
                        ConnectRejectPacket reject = new ConnectRejectPacket();
                        reject.Reason = "Player count exceeded";
                        reject.Serialize(_writer);
                        peer.Send(_writer, DeliveryMethod.ReliableOrdered);
                        peer.Disconnect();
                        _writer.Reset();
                    } else
                    {
                        _connectedPlayers.Add(peer.Id, new NewPlayer() { UserId = packet.UserId, DisplayName = packet.DisplayName, Loader = new PlayerChunkLoader(Vector2i.Zero) }); 
                        _connectedPlayers[peer.Id].Loader.QueuePosition(_world, Vector2i.Zero);
                        _writer.Put((ushort)PacketType.ConnectSuccessPacket);
                        peer.Send(_writer, DeliveryMethod.ReliableOrdered);
                        _writer.Reset();
                    }
                    break;

            }

            reader.Recycle();

        };

    }

    public void Update()
    {

        _manager?.PollEvents();

    }

    public void Unload()
    {



    }

}