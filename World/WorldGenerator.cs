using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace VoxelGame;

public class WorldGenerator
{
    private World _world;
    private List<Thread> _generatorThreads = new();
    private AutoResetEvent _generatorResetEvent = new AutoResetEvent(true);
    private bool _shouldMesh;
    private bool _shouldRun = true;

    public ConcurrentQueue<Vector2i> GenerationQueue = new();
    public ConcurrentQueue<Vector2i> SendChunkDataQueue = new();
    public ConcurrentQueue<Vector2i> UploadQueue = new();
    
    public WorldGenerator(World world, bool shouldMesh = true)
    {
        _world = world;
        _shouldMesh = shouldMesh;
    }

    public WorldGenerator Start()
    {
        for (int i = 0; i < 4; i++)
        {
            _generatorThreads.Add(new Thread(HandleGenerationQueue));
            _generatorThreads[i].Start();
        }
        
        return this;
    }

    public WorldGenerator Stop()
    {
        _shouldRun = false;
        foreach (Thread thr in _generatorThreads)
        {
            while (thr.IsAlive) _generatorResetEvent.Set();
        }

        return this;
    }

    public void Poll()
    {
        if (GenerationQueue.Count > 0) _generatorResetEvent.Set();
        
        while (UploadQueue.TryDequeue(out Vector2i position))
        {
            UploadMesh(_world.Chunks[position]);
        }
    }

    private void HandleGenerationQueue()
    {
        while (_shouldRun)
        {
            _generatorResetEvent.WaitOne();
            while (GenerationQueue.TryDequeue(out Vector2i position))
            {
                Chunk column = _world.Chunks[position];
                switch (column.Status)
                {
                    case ChunkStatus.Empty:
                        GenerateColumn(column);
                        break;
                    case ChunkStatus.Mesh:
                        if (_shouldMesh)
                        {
                            if (ChunkMath.ChebyshevDistance(position, Vector2i.Zero) < 8)
                            {
                                if (AreNeighborsTheSameStatus(position, ChunkStatus.Mesh))
                                {
                                    GenerateMesh(_world, column);
                                }
                                else
                                {
                                    GenerationQueue.Enqueue(position);
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
    
    public bool AreNeighborsTheSameStatus(Vector2i position, ChunkStatus status)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if ((x, z) != Vector2i.Zero)
                {
                    if (!_world.Chunks.ContainsKey(position + (x, z)) ||
                        _world.Chunks[position + (x, z)].Status < status)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    
    public void GenerateColumn(Chunk column)
    {
        for (int x = 0; x < Config.ChunkSize; x++)
        {
            for (int z = 0; z < Config.ChunkSize; z++)
            {
                Vector2i globalPosition = (x + (column.Position.X * Config.ChunkSize), z + (column.Position.Y * Config.ChunkSize));
                int height = (int) (32.0 * Noise.ValueNoise2(0, (Vector2)globalPosition / 64.0f));
                for (int y = Config.ChunkSize * Config.ColumnSize - 1; y >= 0; y--)
                {
                    if (y <= 128 + height)
                    {
                        Chunk.SetBlock(column, (x,y,z), Config.Register.GetBlockFromNamespace("pumpkin"));
                    }
                }
            }
        }

        column.Status = ChunkStatus.Mesh;
        GenerationQueue.Enqueue(column.Position);
    }
    
    public void GenerateMesh(World world, Chunk column)
    {
        for (int i = 0; i < Config.ColumnSize; i++)
        {
            ChunkSectionMesh mesh = column.ChunkMeshes[i];
            if (!mesh.ShouldUpdate) continue;
            mesh.Vertices.Clear();
            mesh.Indices.Clear();
            
            for (int x = 0; x < Config.ChunkSize; x++)
            {
                for (int y = 0; y < Config.ChunkSize; y++)
                {
                    for (int z = 0; z < Config.ChunkSize; z++)
                    {
                        Vector3i globalBlockPosition = (x, y, z) + new Vector3i(column.Position.X, i, column.Position.Y) * Config.ChunkSize;
                        ushort id = column.ChunkSections[i].GetBlockId(x, y, z);
                        if (id != 0)
                        {
                            Config.Register.GetBlockFromId(id).OnBlockMesh(_world, globalBlockPosition);
                            /*
                            if (y == Config.ChunkSize - 1 && i == Config.ColumnSize - 1)
                            {
                                Config.Register.GetBlockFromId(id).Model.AddFace(mesh.Vertices, Direction.Top, (x,y,z));
                            }
                            else if (column.ChunkSections[i + ChunkMath.GlobalToChunk((x,y + 1,z)).Y].GetBlockId(ChunkMath.GlobalToLocal((x,y +1,z))) == 0)
                            {
                                Config.Register.GetBlockFromId(id).Model.AddFace(mesh.Vertices, Direction.Top, (x,y,z));
                            }
                            
                            if (y == 0 && i == 0)
                            {
                                Config.Register.GetBlockFromId(id).Model.AddFace(mesh.Vertices, Direction.Bottom, (x,y,z));
                            } else if (column.ChunkSections[i + ChunkMath.GlobalToChunk((x, y - 1, z)).Y ].GetBlockId(ChunkMath.GlobalToLocal((x, y - 1, z))) == 0)
                            {
                                Config.Register.GetBlockFromId(id).Model.AddFace(mesh.Vertices, Direction.Bottom, (x,y,z));
                            }
                            
                            if (world.Chunks[column.Position + ChunkMath.GlobalToChunk((x, y, z - 1)).Xz].ChunkSections[i]
                                    .GetBlockId(ChunkMath.GlobalToLocal((x, y, z - 1))) == 0)
                            {
                                Config.Register.GetBlockFromId(id).Model.AddFace(mesh.Vertices, Direction.Front, (x,y,z));
                            }
                            
                            if (world.Chunks[column.Position + ChunkMath.GlobalToChunk((x, y, z + 1)).Xz].ChunkSections[i]
                                    .GetBlockId(ChunkMath.GlobalToLocal((x, y, z + 1))) == 0)
                            {
                                Config.Register.GetBlockFromId(id).Model.AddFace(mesh.Vertices, Direction.Back, (x,y,z));
                            }
                            
                            if (world.Chunks[column.Position + ChunkMath.GlobalToChunk((x - 1, y, z)).Xz].ChunkSections[i]
                                    .GetBlockId(ChunkMath.GlobalToLocal((x - 1, y, z))) == 0)
                            {
                                Config.Register.GetBlockFromId(id).Model.AddFace(mesh.Vertices, Direction.Left, (x,y,z));
                            }
                            
                            if (world.Chunks[column.Position + ChunkMath.GlobalToChunk((x+1, y, z)).Xz].ChunkSections[i]
                                    .GetBlockId(ChunkMath.GlobalToLocal((x+1, y, z))) == 0)
                            {
                                Config.Register.GetBlockFromId(id).Model.AddFace(mesh.Vertices, Direction.Right, (x,y,z));
                            }
                            */
                        }
                    }
                }
            }

            for (int m = 0; m < mesh.Vertices.Count; m += 4)
            {
                mesh.Indices.AddRange(0 + m, 1 + m, 2 + m, 2 + m, 3 + m, 0 + m);
            }
        }
        
        column.Status = ChunkStatus.Upload;
        UploadQueue.Enqueue(column.Position);
    }
    
    public void UploadMesh(Chunk column)
    {
        for (int i = 0; i < Config.ColumnSize; i++)
        {
            ChunkSectionMesh mesh = column.ChunkMeshes[i];
            if (!mesh.ShouldUpdate) continue;
            mesh.Update();
        }
        
        column.Status = ChunkStatus.Done;
    }

    public void EnqueueChunk(Vector2i position, ChunkStatus chunkStatus, bool hasPriority)
    {
        _world.Chunks[position].Status = chunkStatus;
        GenerationQueue.Enqueue(position);
    }
}