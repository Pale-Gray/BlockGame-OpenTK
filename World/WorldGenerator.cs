using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenTK.Graphics.Vulkan;
using OpenTK.Mathematics;

namespace VoxelGame;

public class WorldGenerator
{
    private World _world;
    private List<Thread> _generatorThreads = new();
    private AutoResetEvent _generatorResetEvent = new AutoResetEvent(true);
    public bool ShouldMesh;
    private bool _shouldRun = true;

    public ConcurrentQueue<Vector2i> HighPriorityGenerationQueue = new();
    public ConcurrentQueue<Vector2i> LowPriorityGenerationQueue = new();
    public ConcurrentQueue<Vector2i> UploadQueue = new();
    
    public WorldGenerator(World world, bool shouldMesh = true)
    {
        _world = world;
        ShouldMesh = shouldMesh;
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
        if (LowPriorityGenerationQueue.Count > 0 || HighPriorityGenerationQueue.Count > 0) _generatorResetEvent.Set();
        
        while (UploadQueue.TryDequeue(out Vector2i position))
        {
            _world.Chunks[position].Mutex.WaitOne();
            if (_world.Chunks[position].Status == ChunkStatus.Upload) UploadMesh(_world.Chunks[position]);
            _world.Chunks[position].Mutex.ReleaseMutex();
        }
    }

    private void HandleGenerationQueue()
    {
        while (_shouldRun)
        {
            _generatorResetEvent.WaitOne();
            while (HighPriorityGenerationQueue.TryDequeue(out Vector2i position) || LowPriorityGenerationQueue.TryDequeue(out position))
            {
                Chunk column = _world.Chunks[position];
                column.Mutex.WaitOne();
                switch (column.Status)
                {
                    case ChunkStatus.Empty:
                        GenerateColumn(column);
                        break;
                    case ChunkStatus.Mesh:
                        if (ShouldMesh)
                        {
                            if (!column.IsMeshIncomplete)
                            {
                                for (int x = -1; x <= 1; x++)
                                {
                                    for (int z = -1; z <= 1; z++)
                                    {
                                        if (x == 0 && z == 0) continue;
                                        if (_world.Chunks.TryGetValue(position + (x, z), out Chunk chunk) && chunk.IsMeshIncomplete)
                                        {
                                            chunk.Mutex.WaitOne();
                                            for (int i = 0; i < Config.ColumnSize; i++) chunk.ChunkMeshes[i].ShouldUpdate = true;
                                            chunk.Status = ChunkStatus.Mesh;
                                            LowPriorityGenerationQueue.Enqueue(position + (x, z));
                                            // GenerateMesh(_world, chunk);
                                            chunk.Mutex.ReleaseMutex();
                                            // for (int i = 0; i < Config.ColumnSize; i++) chunk.ChunkMeshes[i].ShouldUpdate = true;
                                            // chunk.Status = ChunkStatus.Mesh;
                                            // LowPriorityGenerationQueue.Enqueue(position + (x, z));
                                        }
                                    }
                                }
                            }

                            if (column.IsMeshIncomplete) column.IsMeshIncomplete = false;
                            if (!HasAllNeighbors(position)) column.IsMeshIncomplete = true;
                            GenerateMesh(_world, column);
                        }
                        break;
                }
                column.Mutex.ReleaseMutex();
            }
        }
    }

    public bool HasAllNeighbors(Vector2i position)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (!_world.Chunks.ContainsKey(position + (x, z))) return false;
            }
        }
        return true;
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
        float seaLevel = 256.0f;
        float maxAscent = 64.0f;
        float median = maxAscent / 2.0f;

        float[] arr = Noise.Value3(0, new Vector3i(4), (Config.ChunkSize, Config.ChunkSize * Config.ColumnSize, Config.ChunkSize), new Vector3i(column.Position.X, 0, column.Position.Y) * Config.ChunkSize, 16.0f, true, 4, out Vector3i arraySize);
        
        Stopwatch sw = Stopwatch.StartNew();
        for (int x = 0; x < Config.ChunkSize; x++)
        {
            for (int z = 0; z < Config.ChunkSize; z++)
            {
                Vector3i globalPosition = new Vector3i(x, 0, z) + (new Vector3i(column.Position.X, 0, column.Position.Y) * Config.ChunkSize);

                float roughness = (Noise.Value2(1, (Vector2)globalPosition.Xz / 64.0f, true, 2) + 1.0f) * 0.5f;
                float heightness = (Noise.Value2(2, (Vector2)globalPosition.Xz / 64.0f, true, 2) + 1.0f) * 0.5f;
                float continentality = Noise.Value2(3, (Vector2)globalPosition.Xz / 128.0f, true, 4);
                continentality = (continentality + 1.0f) * 0.5f;

                float heightnessValue = float.Lerp(-32, 32, heightness * continentality);
                float continentalityValue = float.Lerp(-seaLevel / 2.0f, 64, continentality);
                roughness *= continentality;
                for (int y = Config.ChunkSize * Config.ColumnSize - 1; y >= 0; y--)
                {
                    globalPosition.Y = y;
                    float height = Remap(globalPosition.Y, (seaLevel + (median * (1.0f - roughness))) + (heightnessValue * roughness) + continentalityValue, (seaLevel + 1.0f + (maxAscent - (median * (1.0f - roughness)))) + (heightnessValue * roughness) + continentalityValue);
                    height = (1.0f - height);
                    
                    // float density = ((Noise.Value3(0, (Vector3)globalPosition / 16.0f, true, 4) + 1.0f) * 0.5f);
                    float density = (Noise.Value3(arr, new Vector3(x, y, z) / 4.0f, arraySize) + 1.0f) * 0.5f;
                    if (density + height >= 0.0f)
                    {
                        Chunk.SetBlock(column, (x,y,z), Config.Register.GetBlockFromNamespace("stone"));
                    } else if (y <= seaLevel)
                    {
                        Chunk.SetBlock(column, (x,y,z), Config.Register.GetBlockFromNamespace("water"));
                    }
                }
            }
        }

        column.Status = ChunkStatus.Mesh;
        if (column.HasPriority) HighPriorityGenerationQueue.Enqueue(column.Position);
        else LowPriorityGenerationQueue.Enqueue(column.Position);
        sw.Stop();
        Config.LastGenTime = (float) sw.Elapsed.TotalMilliseconds;
        Config.GenTimes.Add(Config.LastGenTime);
    }

    float Remap(float a, float v1, float v2)
    {
        return (a - v1) * (1.0f / (v2 - v1));
    }
    
    public void GenerateMesh(World world, Chunk column)
    {
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < Config.ColumnSize; i++)
        {
            ChunkSectionMesh mesh = column.ChunkMeshes[i];
            if (!mesh.ShouldUpdate || column.ChunkSections[i].IsEmpty) continue;
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
        sw.Stop();
        Config.LastMeshTime = (float)sw.Elapsed.TotalMilliseconds;
        Config.MeshTimes.Add(Config.LastMeshTime);
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
        _world.Chunks[position].Mutex.WaitOne();
        _world.Chunks[position].HasPriority = hasPriority;
        _world.Chunks[position].Status = chunkStatus;
        if (hasPriority) HighPriorityGenerationQueue.Enqueue(position);
        else LowPriorityGenerationQueue.Enqueue(position);
        _world.Chunks[position].Mutex.ReleaseMutex();
    }
}