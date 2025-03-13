using System;
using System.Net;
using Blockgame_OpenTK.Audio;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Gui;
using Blockgame_OpenTK.Core.PlayerUtil;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.Util;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Blockgame_OpenTK.Core.Networking;

public class NewClient
{

    private NetManager _manager;
    private EventBasedNetListener _listener;
    private NetDataWriter _writer = new NetDataWriter();
    private World _world;
    public bool IsNetworked { get; private set; } = false;
    public NewPlayer Player { get; private set; }
    public void Load()
    {

        BlockGame.Load();
        GlobalValues.Base.OnLoad(GlobalValues.Register);

    }

    public void JoinWorld(string worldSave, NewPlayer player)
    {

        // singleplayer
        IsNetworked = false;

        Player = player;



    }

    public void JoinWorld(string addressOrHost, int port, NewPlayer player)
    {

        // multiplayer
        IsNetworked = true;

        Player = player;

        _listener = new EventBasedNetListener();
        _manager = new NetManager(_listener);

        _world = new World("");
        PackedWorldGenerator.CurrentWorld = _world;
        PackedWorldGenerator.Initialize();

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
                    packet.UserId = Player.UserId;
                    packet.DisplayName = Player.DisplayName;
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

    public void Update()
    {

        GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
        GL.ClearColor(new Color4<Rgba>(0, 0, 0, 1.0f));

        Player?.UpdateInputs();
        _world?.Draw(Player);
        PackedWorldGenerator.Update();

        GuiRenderer.Begin("thing");
        GuiRenderer.RenderElement(GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time), (float)Math.Cos(GlobalValues.Time)) / 2), (50, 50), (0.5f, 0.5f));
        GuiRenderer.RenderElement(GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time + 0.1), (float)Math.Cos(GlobalValues.Time + 0.1)) / 2), (50, 50), (0.5f, 0.5f), Color4.Bisque);
        GuiRenderer.RenderElement(GuiMath.RelativeToAbsolute(0.5f, 0.5f) + (GuiMath.RelativeToAbsolute((float)Math.Sin(GlobalValues.Time + 0.25), (float)Math.Cos(GlobalValues.Time + 0.25)) / 2), (50, 50), (0.5f, 0.5f), Color4.Purple);
        GuiRenderer.RenderTextbox(GuiMath.RelativeToAbsolute(0.5f, 0.4f), new Vector2i(200, 24), (0.5f, 0.5f), "Address", out string addressString, Color4.White);
        GuiRenderer.RenderTextbox(GuiMath.RelativeToAbsolute(0.5f, 0.6f), (200, 24), (0.5f, 0.5f), "User Id", out string userIdString, Color4.White);
        if (GuiRenderer.RenderButton(GuiMath.RelativeToAbsolute(0.5f, 0.7f), (150, 24), (0.5f, 0.5f), "Join Server", Color4.White))
        {

            string[] network = addressString.Split(':');

            if (network.Length != 2) 
            {
                
                GameLogger.Log("invalid address");

            } else
            {

                if (!int.TryParse(network[1], out int port))
                {
                    GameLogger.Log("port is invalid");
                } else if (!long.TryParse(userIdString, out long uid))
                {
                    GameLogger.Log("uid is invalid");
                } else
                {
                    NetworkingValues.Client.JoinWorld(network[0], port, new NewPlayer() { UserId = uid, DisplayName = "Poo" });
                }

            }

        }
        GuiRenderer.End();

        _manager?.PollEvents();

    }

    public void Unload()
    {

        PackedWorldGenerator.Unload();

        BlockGame.Unload();

    }

}