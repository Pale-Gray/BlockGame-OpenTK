using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using LiteNetLib;

namespace VoxelGame.Networking;

public class Server : Networked
{
    public bool IsInternal = false;
    
    public Server(string ip, int port) : base(ip, port)
    {
        
    }

    public Server(string settingsFile) : base(settingsFile)
    {
        
    }

    public override void Start()
    {
        if (!IsInternal)
        {
            Config.Register.RegisterBlock("air", new Block());
            Config.Register.RegisterBlock("grass", 
                new Block()
                    .SetBlockModel(new BlockModel()
                        .AddCube(new Cube())
                        .SetTextureFace(0, Direction.Top, "grass_top")
                        .SetTextureSides(0, "grass_side")
                        .SetTextureFace(0, Direction.Bottom, "dirt")));
            Config.Register.RegisterBlock("sand",
                new Block()
                    .SetBlockModel(new BlockModel()
                        .AddCube(new Cube())
                        .SetAllTextureFaces(0, "sand")));
            Config.Register.RegisterBlock("pumpkin",
                new Block()
                    .SetBlockModel(new BlockModel()
                        .AddCube(new Cube())
                        .SetTextureFace(0, Direction.Top, "pumpkin_top")
                        .SetTextureFace(0, Direction.Bottom, "pumpkin_bottom")
                        .SetTextureSides(0, "pumpkin_face")));
        }
        
        Manager.ChannelsCount = 2;
        Manager.Start(HostOrIp, string.Empty, Port);
        Console.WriteLine($"Started server at {HostOrIp}:{Port}");
        World = new World();
        World.Generator.Start();
        
        Listener.ConnectionRequestEvent += request =>
        {
            if (Manager.ConnectedPeersCount >= 20)
            {
                request.Reject();
            }
            else
            {
                request.AcceptIfKey("hello");
            }
        };

        Listener.PeerConnectedEvent += peer =>
        {
            ConnectedPlayers.Add(peer, new Player(Guid.NewGuid().ToString()));
            Console.WriteLine($"Player {ConnectedPlayers[peer].Name} has joined");
        };

        Listener.PeerDisconnectedEvent += (peer, info) =>
        {
            Console.WriteLine($"Player {ConnectedPlayers[peer].Name} disconnected, reason: {info.Reason}");
            ConnectedPlayers.Remove(peer);
        };
        
        Listener.NetworkReceiveEvent += (fromPeer, dataReader, channel, deliveryMethod) =>
        {
            DataReader reader = new DataReader(dataReader.GetRemainingBytes());
            int t = reader.ReadInt32();
            PacketType type = (PacketType)t;
            Console.WriteLine($"{type} packet received");
            switch (type)
            {
                case PacketType.BlockDestroy:
                    BlockDestroyPacket blockDestroy = (BlockDestroyPacket) new BlockDestroyPacket().Deserialize(reader);
                    Config.Register.GetBlockFromId(blockDestroy.Id).OnBlockDestroy(World, blockDestroy.GlobalBlockPosition);
                    
                    SendPacket(blockDestroy, fromPeer);
                    break;
                case PacketType.BlockPlace:
                    BlockPlacePacket packet = (BlockPlacePacket)new BlockPlacePacket().Deserialize(reader);
                    Config.Register.GetBlockFromId(packet.Id).OnBlockPlace(World, packet.GlobalBlockPosition);
                    
                    SendPacket(packet, fromPeer);
                    break;
            }
            
            dataReader.Recycle();
        };
    }

    public override void SendPacket(IPacket packet, NetPeer? excludingPeer = null)
    {
        Writer.Clear();
        Writer.Write((int)packet.Type);
        packet.Serialize(Writer);

        if (excludingPeer != null)
        {
            Manager.SendToAll(Writer.Data, DeliveryMethod.ReliableOrdered, excludingPeer);
        }
        else
        {
            Manager.SendToAll(Writer.Data, DeliveryMethod.ReliableOrdered);
        }
    }
    public override void TickUpdate()
    {
        
    }

    public override void Join()
    {
        throw new NotImplementedException();
    }

    public override void Disconnect()
    {
        throw new NotImplementedException();
    }

    public override void Update()
    {
        Manager.PollEvents();
        World.Generator.Poll();

        foreach (KeyValuePair<NetPeer, Player> playerPair in ConnectedPlayers)
        {
            int rad = 8;
            for (int x = -rad; x <= rad; x++)
            {
                for (int z = -rad; z <= rad; z++)
                {
                    if (!playerPair.Value.LoadedChunks.Contains((x, z)))
                    {
                        if (World.Chunks.ContainsKey((x, z)))
                        {
                            if (World.Chunks[(x,z)].Status == ChunkStatus.Mesh)
                            {
                                Writer.Clear();
                                ChunkDataPacket chunkData = new ChunkDataPacket();
                                chunkData.Position = (x, z);
                                chunkData.Column = World.Chunks[(x, z)];
                                SendPacket(chunkData);
                                
                                // chunkData.Serialize(_writer);
                                // _server.SendToAll(_writer.Data, 1, DeliveryMethod.ReliableUnordered);
                                playerPair.Value.LoadedChunks.Add((x, z));
                            }
                        }
                        else
                        {
                            World.AddChunk((x, z), new Chunk((x, z)));
                            World.Generator.GenerationQueue.Enqueue((x, z));
                        }
                    }
                }
            }
        }
    }

    public override void Stop()
    {
        Manager.Stop();
        World.Generator.Stop();
    }
}