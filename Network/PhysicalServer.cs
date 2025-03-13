using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.PlayerUtil;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.PlayerUtil;
using Blockgame_OpenTK.Util;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenTK.Mathematics;
using Tomlet;
using Tomlet.Attributes;
using Tomlet.Exceptions;

namespace Blockgame_OpenTK.Core.Networking;

public class PhysicalServer : IServer
{
    private NetManager _manager;
    private EventBasedNetListener _listener;
    private NetDataWriter _writer = new NetDataWriter();
    private ServerProperties _properties;
    private World _world;
    private Dictionary<int, NewPlayer> _connectedPlayers = new();
    private double _time = 0.0;
    public void Start()
    {

        _listener = new EventBasedNetListener();
        _manager = new NetManager(_listener);
        
        _properties = TomletMain.To<ServerProperties>(File.ReadAllText("server.toml"));

        GameLogger.Log($"String address: {_properties.AddressOrHost}");
        GameLogger.Log($"Port: {_properties.Port}");
        GameLogger.Log($"Max players: {_properties.MaxPlayers}");
        GameLogger.Log($"World save: {_properties.WorldName}");

        _manager.IPv6Enabled = false;
        _manager.Start(_properties.AddressOrHost, _properties.AddressOrHost, _properties.Port);
        GameLogger.Log($"Started server at {_properties.AddressOrHost}:{_properties.Port}");

        // mod loading (base)
        GlobalValues.Base.OnLoad(GlobalValues.Register);

        // start threads
        PackedWorldGenerator.Initialize();

        // world loading
        _world = new World(_properties.WorldName);
        PackedWorldGenerator.CurrentWorld = _world;

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

    public void Stop()
    {
        throw new System.NotImplementedException();
    }

    public void TickUpdate()
    {
        
        

    }

    public void Update()
    {
        
        if (_time >= 1 / 30.0)
        {
            _time -= 1 / 30.0;
            TickUpdate();
        }
        _manager.PollEvents();
        foreach (NewPlayer player in _connectedPlayers.Values)
        {
            player.Loader.Tick(_world);
        }
        PackedWorldGenerator.Update();
        _time += GlobalValues.DeltaTime;

    }

    public void SendChunk(Vector2i position)
    {

        foreach (NewPlayer player in _connectedPlayers.Values)
        {

            if (!player.SentChunks.Contains(position)) 
            {

                player.SentChunks.Add(position);
                ChunkSendPacket packet = new ChunkSendPacket();
                packet.Position = position;
                packet.Data = ColumnSerializer.SerializeColumnToBytes(_world.WorldColumns[position]);
                packet.Serialize(_writer);
                _manager.GetPeerById(player.NetPeerId).Send(_writer, DeliveryMethod.ReliableOrdered);
                _writer.Reset();

            }

        }

    }

}