using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenTK.Mathematics;

namespace VoxelGame.Networking;

public class Server
{
    public string Ip;
    public int Port;

    private EventBasedNetListener _listener;
    private NetManager _server;
    private NetDataWriter _writer = new NetDataWriter();

    private World _world;
    private WorldGenerator _worldGenerator;

    private Dictionary<NetPeer, Player> _connectedPlayers = new();
    
    public Server(string ip, int port)
    {
        Ip = ip;
        Port = port;

        _listener = new EventBasedNetListener();
        _server = new NetManager(_listener);
    }

    public Server Start(bool isInternal = false)
    {
        if (isInternal)
        {
            // load assets
        }
        
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
            PacketType type = (PacketType)dataReader.GetInt();
            switch (type)
            {
                case PacketType.BlockDestroy:
                    
                    BlockDestroyPacket blockDestroy = (BlockDestroyPacket) new BlockDestroyPacket().Deserialize(dataReader);
                    Console.WriteLine($"SERVER: block needs to be destroyed: {blockDestroy.GlobalBlockPosition}");
                    _world.SetBlockId(blockDestroy.GlobalBlockPosition, 0);
                    
                    _writer.Reset();
                    blockDestroy.Serialize(_writer);
                    _server.SendToAll(_writer, DeliveryMethod.ReliableUnordered);
                    break;
            }
            
            dataReader.Recycle();
        };
        
        return this;
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
                                _writer.Reset();
                                ChunkDataPacket chunkData = new ChunkDataPacket();
                                chunkData.Position = (x, z);
                                chunkData.Column = _world.ChunkColumns[(x, z)];
                                chunkData.Serialize(_writer);
                                _server.SendToAll(_writer, DeliveryMethod.ReliableUnordered);
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