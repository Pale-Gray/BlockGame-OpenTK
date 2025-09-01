using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using LiteNetLib;
using OpenTK.Mathematics;

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
            Config.Register.RegisterBlock("water", new Block().SetBlockModel(new BlockModel().AddCube(new Cube()).SetAllTextureFaces(0, "water")));
        }
        
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
            // ConnectedPlayers.Add(peer, new Player(Guid.NewGuid().ToString()));
            // Console.WriteLine($"Player {ConnectedPlayers[peer].Name} has joined");
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
            
            Player player = playerPair.Value;
            if (player.ChunkPosition == null || ChunkMath.ChebyshevDistance(player.ChunkPosition.Value, ChunkMath.GlobalToChunk(ChunkMath.PositionToBlockPosition(player.Position)).Xz) >= 2)
            {
                player.ChunkPosition = ChunkMath.GlobalToChunk(ChunkMath.PositionToBlockPosition(player.Position)).Xz;
                HashSet<Vector2i> previousHashSet = new HashSet<Vector2i>(player.ChunksToLoad);
                player.ChunksToLoad.Clear();
                for (int x = -rad; x <= rad; x++)
                {
                    for (int z = -rad; z <= rad; z++)
                    {
                        Vector2i pos = (x, z) + player.ChunkPosition.Value;
                        if (!previousHashSet.Contains(pos))
                        {
                            player.ChunksToLoad.Add(pos);
                        }
                    }
                }
            }

            foreach (Vector2i position in player.ChunksToLoad)
            {
                if (World.Chunks.TryGetValue(position, out Chunk chunk))
                {
                    if (chunk.Status == ChunkStatus.Mesh)
                    {
                        ChunkDataPacket chunkData = new ChunkDataPacket();
                        chunkData.Position = position;
                        chunkData.Column = World.Chunks[position];
                        SendPacketTo(chunkData, playerPair.Key);
                        
                        player.ChunksToLoad.Remove(position);
                    }
                }
                else
                {
                    World.Chunks.TryAdd(position, new Chunk(position));
                    World.Generator.EnqueueChunk(position, ChunkStatus.Empty, false);
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