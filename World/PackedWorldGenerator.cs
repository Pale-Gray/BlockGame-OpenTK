using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Networking;
using Blockgame_OpenTK.PlayerUtil;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.Vulkan;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Worlds;

public class PackedWorldGenerator
{

    public static int WorldGenerationRadius = 8;
    public static int WorldGenerationHeight = 8; // The height starting from y = 0
    public static int MaxChunkUploadCount = 5;

    private static int _currentRadius = 0;

    public static World CurrentWorld;

    public static ThreadSafeDoubleEndedQueue<Vector3i> PackedChunkWorldGenerationQueue = new();
    public static ThreadSafeDoubleEndedQueue<Vector3i> PackedChunkWorldUploadQueue = new();

    public static ThreadSafeDoubleEndedQueue<Vector2i> ColumnWorldGenerationQueue = new();
    public static ThreadSafeDoubleEndedQueue<Vector2i> ColumnWorldUploadQueue = new();
    
    private static List<Thread> _chunkGenerationThreads = new();
    private static ConcurrentDictionary<int, AutoResetEvent> _chunkGenerationAutoResetEvents = new();

    public static void Unload()
    {
        
        foreach (Thread t in _chunkGenerationThreads)
        {
            while (t.IsAlive)
            {
                _chunkGenerationAutoResetEvents[t.ManagedThreadId].Set();
            }
        }
        _chunkGenerationThreads.Clear();
        _chunkGenerationAutoResetEvents.Clear();

    }
    private static void HandleUploadQueue()
    {

        if (ColumnWorldUploadQueue.TryDequeueFirst(out Vector2i columnPosition))
        {
        
            // Console.WriteLine("uploading");
            if (NetworkingValues.Server?.IsNetworked ?? false)
            {

                NetworkingValues.Server.SendChunk(columnPosition);

            } else
            {

                ColumnBuilder.Upload(CurrentWorld.WorldColumns[columnPosition]);

            }
            // NetworkingValues.Server?.SendChunk(columnPosition);
            // NetworkingValues.Client?.SendChunk(columnPosition);
            /*
            if (NetworkingValues.Server != null)
            {
                // NetworkingValues.Server.SendChunk(columnPosition);
            } else
            {
                // Console.WriteLine("uploading");
                ColumnBuilder.Upload(CurrentWorld.WorldColumns[columnPosition]);
            }
            */

        }

    }
    private static void HandleGenerationQueue()
    {

        while (GlobalValues.IsRunning)
        {
            
            _chunkGenerationAutoResetEvents[Thread.CurrentThread.ManagedThreadId].WaitOne();
            while (ColumnWorldGenerationQueue.TryDequeueFirst(out Vector2i columnPosition))
            {

                switch (CurrentWorld.WorldColumns[columnPosition].QueueType)
                {

                    case ColumnQueueType.PassOne:
                        ColumnBuilder.GeneratePassOne(CurrentWorld.WorldColumns[columnPosition]);
                        break;
                    case ColumnQueueType.Mesh:
                        if (NetworkingValues.Server?.IsNetworked ?? false)
                        {

                            CurrentWorld.WorldColumns[columnPosition].QueueType = ColumnQueueType.Upload;
                            ColumnWorldUploadQueue.EnqueueLast(columnPosition);

                        } else 
                        {

                            if (Maths.ChebyshevDistance2D(columnPosition, Vector2i.Zero) < WorldGenerationRadius)
                            {

                                if (AreNeighborColumnsTheSameQueueType(columnPosition, ColumnQueueType.Mesh))
                                {
                                    // Stopwatch sw = Stopwatch.StartNew();
                                    // Console.WriteLine("meshing");
                                    ColumnBuilder.Mesh(GetSurroundingColumns(columnPosition));
                                    // sw.Stop();
                                    // Console.WriteLine($"column meshing took {sw.Elapsed.TotalMilliseconds}ms");
                                } else
                                {
                                    if (CurrentWorld.WorldColumns[columnPosition].HasPriority)
                                    {
                                        ColumnWorldGenerationQueue.EnqueueBehindFirst(columnPosition);
                                    } else 
                                    {
                                        ColumnWorldGenerationQueue.EnqueueLast(columnPosition);
                                    }
                                }

                            }

                        }
                        break;

                }

            }

        }
        
    }

    private static bool AreNeighborColumnsTheSameQueueType(Vector2i position, ColumnQueueType queueType)
    {

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if ((x,z) != Vector2i.Zero) 
                {
                    if (!CurrentWorld.WorldColumns.ContainsKey(position + (x,z)))
                    {
                        return false;
                    } else if (!IsColumnTheSameQueueType(CurrentWorld.WorldColumns[position + (x,z)], queueType)) return false;
                    // if (!CurrentWorld.WorldColumns.ContainsKey(position + (x,z))) return false;
                } // else if (!IsColumnTheSameQueueType(CurrentWorld.WorldColumns[position + (x,z)], queueType)) return false;
            }
        }
        return true;

    }

    private static bool IsColumnTheSameQueueType(ChunkColumn column, ColumnQueueType queueType)
    {

        return column.QueueType >= queueType;

    }

    private static ConcurrentDictionary<Vector2i, ChunkColumn> GetSurroundingColumns(Vector2i position)
    {

        ConcurrentDictionary<Vector2i, ChunkColumn> columns = new();

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                columns.TryAdd((x,z), CurrentWorld.WorldColumns[position + (x,z)]);
            }
        }

        return columns;

    }

    public static void Initialize()
    {

        for (int i = 0; i < 5; i++)
        {
            
            _chunkGenerationThreads.Add(new Thread(HandleGenerationQueue));
            _chunkGenerationAutoResetEvents.TryAdd(_chunkGenerationThreads[i].ManagedThreadId, new AutoResetEvent(true));
            
            _chunkGenerationThreads[i].Start();
            
        }
        
    }

    public static void Update()
    {

        if (ColumnWorldGenerationQueue.Count > 0)
        {

            for (int i = 0; i < (ColumnWorldGenerationQueue.Count >= _chunkGenerationAutoResetEvents.Count ? _chunkGenerationAutoResetEvents.Count : ColumnWorldGenerationQueue.Count); i++) {
                _chunkGenerationAutoResetEvents.ElementAt(i).Value.Set();
            }
            
        }
       
        HandleUploadQueue();
        
        // foreach (PackedChunkMesh m in CurrentWorld.PackedWorldMeshes.Values) m.Draw(player);
        /*
        foreach (ChunkColumn column in CurrentWorld.WorldColumns.Values)
        {

            for (int i = 0; i < column.ChunkMeshes.Length; i++)
            {

                column.ChunkMeshes[i].Draw(player);
                
            }

        }
        */

    }

    static bool c = false;

    public static List<Vector2i> GetArea(int radius, Vector2i playerPosition)
    {

        List<Vector2i> area = new();

        for (int x = -radius; x <= radius; x++)
        {

            for (int z = -radius; z <= radius; z++)
            {

                area.Add((x,z));

            }

        }

        return area;

    }

    static int currentIndex = 0;
    static Queue<Vector2i> _queue = new();
    static Vector2i[] _offset = { (-1, 0), (1, 0), (0, 1), (0, -1) };
    static bool hello = false;

    public static void QueueGeneration()
    {

        if (!hello) 
        {

            _queue.Enqueue(Vector2i.Zero); 
            CurrentWorld.WorldColumns.TryAdd(Vector2i.Zero, new ChunkColumn(Vector2i.Zero));
            ColumnWorldGenerationQueue.EnqueueLast(Vector2i.Zero);

            hello = true;

        }

        int c = 0;
        while (_queue.Count != 0 && c < 20)
        {

            if (_queue.TryDequeue(out Vector2i res))
            {

                for (int i = 0; i < _offset.Length; i++)
                {

                    if (Maths.ChebyshevDistance2D(res + _offset[i], Vector2i.Zero) <= WorldGenerationRadius && CurrentWorld.WorldColumns.TryAdd(res + _offset[i], new ChunkColumn(res + _offset[i])))
                    {

                        ColumnWorldGenerationQueue.EnqueueLast(res + _offset[i]);
                        _queue.Enqueue(res + _offset[i]);

                    }

                }

            }
            c++;

        }   

        /*
        int maxPolledChunks = 8;

        if (!c)
        {

            List<Vector2i> positions = GetArea(WorldGenerationRadius, Vector2i.Zero);

            if (currentIndex < positions.Count)
            {

                int i = 0;
                while (i <= maxPolledChunks && currentIndex < positions.Count)
                {

                    CurrentWorld.WorldColumns.TryAdd(positions[currentIndex], new ChunkColumn(positions[currentIndex]));
                    ColumnWorldGenerationQueue.EnqueueLast(positions[currentIndex]);
                    currentIndex++;
                    i++;

                }

            } else
            {

                c = true;

            }

        }

        /*
        if (_currentRadius <= WorldGenerationRadius)
        {

            foreach (Vector2i columnPosition in ColumnUtils.GetRing(_currentRadius))
            {

                CurrentWorld.WorldColumns.TryAdd(columnPosition, new ChunkColumn(columnPosition));

                ColumnWorldGenerationQueue.EnqueueLast(columnPosition);

            }

            _currentRadius++;

        }
        */
        
    }

}