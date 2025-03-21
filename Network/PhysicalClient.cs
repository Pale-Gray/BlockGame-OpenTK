using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Image;
using Blockgame_OpenTK.Core.PlayerUtil;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenTK.Core.Utility;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Networking;

public class PhysicalClient : IClient
{

    private NetManager _manager;
    private EventBasedNetListener _listener;
    private List<NewPlayer> _players;
    private NetDataWriter _writer = new NetDataWriter();
    private NewPlayer _player;
    private World _world;

    private Pimage _image = new Pimage();

    public void Start(string addressOrHost = "", int port = 0, NewPlayer player = null)
    {
        
        _player = player;

        _listener = new EventBasedNetListener();
        _manager = new NetManager(_listener);

        _world = new World("");
        PackedWorldGenerator.CurrentWorld = _world;

        PackedWorldGenerator.Initialize();

        //mod loading (base)
        GlobalValues.Base.OnLoad(GlobalValues.Register);

        _manager.Start();
        _manager.Connect(addressOrHost, port, "BlockGame");

        _listener.PeerConnectedEvent += (peer) =>
        {

            // GameLogger.Log("Connected to server.");

        };

        _listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {

            PacketType packetType = default;//(PacketType)disconnectInfo.AdditionalData.GetUShort();

            switch (packetType)
            {
                case PacketType.ConnectRejectPacket:
                    // ConnectRejectPacket packet = new ConnectRejectPacket();
                    // packet.Deserialize(disconnectInfo.AdditionalData);
                    // GameLogger.Log($"{packet.Type}: {packet.Reason}");
                    break;
            }

            disconnectInfo.AdditionalData.Recycle();

        };

        _listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
        {

            PacketType packetType = (PacketType)reader.GetUShort();

            switch (packetType)
            {
            
                case PacketType.PlayerDataRequestPacket:
                    PlayerDataSendPacket packet = new PlayerDataSendPacket();
                    packet.UserId = _player.UserId;
                    packet.DisplayName = _player.DisplayName;
                    packet.Serialize(_writer);
                    peer.Send(_writer, DeliveryMethod.ReliableOrdered);
                    break;
                case PacketType.ConnectSuccessPacket:
                    GameLogger.Log("Connected to server.");
                    break;
                case PacketType.ConnectRejectPacket:
                    ConnectRejectPacket rejectPacket = new ConnectRejectPacket();
                    rejectPacket.Deserialize(reader);
                    GameLogger.Log($"{rejectPacket.Type}: {rejectPacket.Reason}");
                    _manager.DisconnectAll();
                    break;
                case PacketType.ChunkSendPacket:
                    ChunkSendPacket sendPacket = new ChunkSendPacket();
                    sendPacket.Deserialize(reader);
                    _world.WorldColumns.TryAdd(sendPacket.Position, new ChunkColumn(sendPacket.Position) { QueueType = ColumnQueueType.Mesh });
                    ColumnSerializer.DeserializeColumnFromBytes(_world.WorldColumns[sendPacket.Position], sendPacket.Data);
                    for (int i = 0; i < PackedWorldGenerator.WorldGenerationHeight; i++)
                    {
                        _world.WorldColumns[sendPacket.Position].Chunks[i].HasUpdates = true;
                    }
                    // column.QueueType = ColumnQueueType.Mesh;
                    // bool added = _world.WorldColumns.TryAdd(sendPacket.Position, column);
                    // Console.WriteLine(added);
                    PackedWorldGenerator.ColumnWorldGenerationQueue.EnqueueLast(sendPacket.Position);
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
        throw new System.NotImplementedException();
    }

    public void Update()
    {

        _manager.PollEvents();
        PackedWorldGenerator.Update();
        _world.Draw(_player);

    }

    public void SendChunk(Vector2i position)
    {

        ColumnBuilder.Upload(_world.WorldColumns[position]);

    }

}