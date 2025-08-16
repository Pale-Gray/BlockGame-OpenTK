using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenTK.Graphics.Vulkan.VulkanVideoCodecH265stdEncode;
using OpenTK.Mathematics;

namespace VoxelGame.Networking;

public class Server
{
    public string Ip;
    public int Port;

    private EventBasedNetListener _listener;
    private NetManager _server;
    private DataWriter _writer = new DataWriter();

    private World _world;
    private WorldGenerator _worldGenerator;

    private Dictionary<NetPeer, Player> _connectedPlayers = new();

    public NetPeer? CurrentReceivingPeer { get; private set; } = null;
    
    public Server(string ip, int port)
    {
        Ip = ip;
        Port = port;

        _listener = new EventBasedNetListener();
        _server = new NetManager(_listener);
    }

    public Server(string settingsFile)
    {
        using (FileStream stream = File.OpenRead("server_settings.json"))
        {
            Dictionary<string, object> serverSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(stream)!;
            Ip = serverSettings["ip"].ToString()!;
            Port = int.Parse(serverSettings["port"].ToString()!);
        }
        
        _listener = new EventBasedNetListener();
        _server = new NetManager(_listener);
    }

    public Server Start(bool isInternal = false)
    {
        if (!isInternal)
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
        
        _server.ChannelsCount = 2;
        _server.Start(Ip, string.Empty, Port);
        Console.WriteLine($"Started server at {Ip}:{Port}");
        _world = new World();
        _worldGenerator = new WorldGenerator(_world, false).Start();
        
        _listener.ConnectionRequestEvent += request =>
        {
            if (_server.ConnectedPeersCount >= 20)
            {
                request.Reject();
            }
            else
            {
                request.AcceptIfKey("hello");
            }
        };

        _listener.PeerConnectedEvent += peer =>
        {
            _connectedPlayers.Add(peer, new Player(Guid.NewGuid().ToString()));
            Console.WriteLine($"Player {_connectedPlayers[peer].Name} has joined");
        };

        _listener.PeerDisconnectedEvent += (peer, info) =>
        {
            Console.WriteLine($"Player {_connectedPlayers[peer].Name} disconnected, reason: {info.Reason}");
            _connectedPlayers.Remove(peer);
        };
        
        _listener.NetworkReceiveEvent += (fromPeer, dataReader, channel, deliveryMethod) =>
        {
            CurrentReceivingPeer = fromPeer;
            
            DataReader reader = new DataReader(dataReader.GetRemainingBytes());
            int t = reader.ReadInt32();
            PacketType type = (PacketType)t;
            Console.WriteLine($"{type} packet received");
            switch (type)
            {
                case PacketType.BlockDestroy:
                    BlockDestroyPacket blockDestroy = (BlockDestroyPacket) new BlockDestroyPacket().Deserialize(reader);
                    Config.Register.GetBlockFromId(blockDestroy.Id).OnBlockDestroy(_world, blockDestroy.GlobalBlockPosition);
                    
                    SendPacket(blockDestroy, fromPeer);
                    break;
                case PacketType.BlockPlace:
                    BlockPlacePacket packet = (BlockPlacePacket)new BlockPlacePacket().Deserialize(reader);
                    Config.Register.GetBlockFromId(packet.Id).OnBlockPlace(_world, packet.GlobalBlockPosition);
                    
                    SendPacket(packet, fromPeer);
                    break;
            }
            
            dataReader.Recycle();
        };
        
        return this;
    }
    
    public void SendPacket(IPacket packet, NetPeer? fromPeer = null)
    {
        _writer.Clear();
        _writer.Write((int)packet.Type);
        packet.Serialize(_writer);

        if (fromPeer != null)
        {
            _server.SendToAll(_writer.Data, DeliveryMethod.ReliableOrdered, fromPeer);
        }
        else
        {
            _server.SendToAll(_writer.Data, DeliveryMethod.ReliableOrdered);
        }
    }

    public void Poll()
    {
        _server.PollEvents();
        _worldGenerator.Poll();
        
        Stopwatch sw = Stopwatch.StartNew();
        foreach (var pair in _world.ChunkColumns)
        {
            sw.Stop();
            sw.Start();
        }
        sw.Stop();
        // Console.WriteLine($"Looped over the whole world ({_world.ChunkColumns.Count} chunks) in {double.Round(sw.Elapsed.TotalMilliseconds, 2)}ms");

        foreach (KeyValuePair<NetPeer, Player> playerPair in _connectedPlayers)
        {
            int rad = 8;
            for (int x = -rad; x <= rad; x++)
            {
                for (int z = -rad; z <= rad; z++)
                {
                    if (!playerPair.Value.LoadedChunks.Contains((x, z)))
                    {
                        if (_world.ChunkColumns.ContainsKey((x, z)))
                        {
                            if (_world.ChunkColumns[(x,z)].Status == ChunkStatus.Mesh)
                            {
                                _writer.Clear();
                                ChunkDataPacket chunkData = new ChunkDataPacket();
                                chunkData.Position = (x, z);
                                chunkData.Column = _world.ChunkColumns[(x, z)];
                                SendPacket(chunkData);
                                
                                // chunkData.Serialize(_writer);
                                // _server.SendToAll(_writer.Data, 1, DeliveryMethod.ReliableUnordered);
                                playerPair.Value.LoadedChunks.Add((x, z));
                            }
                        }
                        else
                        {
                            _world.AddColumn((x, z), new Chunk((x, z)));
                            _worldGenerator.GenerationQueue.Enqueue((x, z));
                        }
                    }
                }
            }
        }
    }

    public void Stop()
    {
        _server.Stop();
        _worldGenerator.Stop();
    }
}