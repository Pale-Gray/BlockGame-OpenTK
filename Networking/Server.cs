using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using LiteNetLib;
using OpenTK.Mathematics;
using VoxelGame.Util;

namespace VoxelGame.Networking;

public class Server : Networked
{
    public bool IsInternal = false;
    public NetPeer? InternalServerPeer = null;
    
    public Server(string ip, int port) : base(ip, port)
    {
        
    }

    public Server(string settingsFile) : base(settingsFile)
    {
        
    }

    public override void Start()
    {
        if (!IsInternal) BaseGame.OnLoad();
        
        Manager.Start(HostOrIp, string.Empty, Port);
        Console.WriteLine($"Started server at {HostOrIp}:{Port}");

        Config.World = new World();
        Config.World.Generator.Start();
        
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
            if (IsInternal && InternalServerPeer == null)
            {
                Console.WriteLine("internal peer connected");
                InternalServerPeer = peer;
            }
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
            switch (type)
            {
                case PacketType.PlayerMove:
                    PlayerMovePacket playerMove = (PlayerMovePacket)new PlayerMovePacket().Deserialize(reader);
                    if (ConnectedPlayers.ContainsKey(fromPeer)) ConnectedPlayers[fromPeer].Position = playerMove.Position;
                    break;
                case PacketType.PlayerJoin:
                    PlayerJoinPacket playerJoin = (PlayerJoinPacket)new PlayerJoinPacket().Deserialize(reader);
                    ConnectedPlayers.Add(fromPeer, new Player(playerJoin.Id.ToString(), playerJoin.Id));
                    Console.WriteLine($"Player {playerJoin.Id} joined");
                    break;
                case PacketType.BlockDestroy:
                    BlockDestroyPacket blockDestroy = (BlockDestroyPacket) new BlockDestroyPacket().Deserialize(reader);
                    Config.Register.GetBlockFromId(blockDestroy.Id).OnBlockDestroy(Config.World, blockDestroy.GlobalBlockPosition);
                    
                    SendPacket(blockDestroy, fromPeer);
                    break;
                case PacketType.BlockPlace:
                    BlockPlacePacket packet = (BlockPlacePacket)new BlockPlacePacket().Deserialize(reader);
                    Config.Register.GetBlockFromId(packet.Id).OnBlockPlace(Config.World, packet.GlobalBlockPosition);
                    
                    SendPacket(packet, fromPeer);
                    break;
            }
            
            dataReader.Recycle();
        };
    }

    public override void TickUpdate()
    {
        
    }

    public override void Join(bool isInternal = false)
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
        // Config.World.Generator.Poll();
        
        foreach (KeyValuePair<NetPeer, Player> playerPair in ConnectedPlayers)
        {
            int rad = 8;
            
            Player player = playerPair.Value;
            if (player.ChunkPosition == null || ChunkMath.ChebyshevDistance(player.ChunkPosition.Value, ChunkMath.GlobalToChunk(ChunkMath.PositionToBlockPosition(player.Position)).Xz) >= 2)
            {
                player.ChunkPosition = ChunkMath.GlobalToChunk(ChunkMath.PositionToBlockPosition(player.Position)).Xz;
                
                for (int x = -rad; x <= rad; x++)
                {
                    for (int z = -rad; z <= rad; z++)
                    {
                        Vector2i pos = (x, z) + player.ChunkPosition.Value;
                        player.ChunksToLoad.Add(pos);
                    }
                }
            }

            foreach (Vector2i position in player.ChunksToLoad)
            {
                if (Config.World.Chunks.TryGetValue(position, out Chunk chunk))
                {
                    if (chunk.Status == ChunkStatus.Done)
                    {
                        if (!InternalServerPeer?.Equals(playerPair.Key) ?? true)
                        {
                            ChunkDataPacket chunkData = new ChunkDataPacket();
                            chunkData.Position = position;
                            chunkData.Column = Config.World.Chunks[position];
                            SendPacketTo(chunkData, playerPair.Key);
                        }

                        player.ChunksToLoad.Remove(position);
                    }
                    else
                    {
                        player.LoadQueue.Enqueue(position);
                    }
                }
                else
                {
                    if (!Config.World.Chunks.ContainsKey(position)) Config.World.Chunks.TryAdd(position, new Chunk(position));
                    Config.World.Generator.EnqueueChunk(position, ChunkStatus.Empty, false);
                }
            }
        }
    }

    public override void Stop()
    {
        Manager.Stop();
        Config.World.Generator.Stop();
    }
}