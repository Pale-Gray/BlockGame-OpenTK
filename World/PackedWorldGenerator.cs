using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.PlayerUtil;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.Vulkan;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Worlds;

public class PackedWorldGenerator
{

    public static int WorldGenerationRadius = 4;
    public static int WorldGenerationHeight = 8; // The height starting from 0
    public static int MaxChunkUploadCount = 5;

    private static int _currentRadius = 0;

    public static PackedChunkWorld CurrentWorld;

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

            ColumnBuilder.Upload(CurrentWorld.WorldColumns[columnPosition]);

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
                        if (Maths.ChebyshevDistance2D(columnPosition, Vector2i.Zero) < WorldGenerationRadius)
                        {

                            if (AreNeighborColumnsTheSameQueueType(columnPosition, ColumnQueueType.Mesh))
                            {
                                Stopwatch sw = Stopwatch.StartNew();
                                ColumnBuilder.Mesh(GetSurroundingColumns(columnPosition));
                                sw.Stop();
                                Console.WriteLine($"meshing took {Math.Round(sw.Elapsed.TotalMilliseconds, 2)}ms");
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
                if ((x,z) != Vector2i.Zero) if (!IsColumnTheSameQueueType(CurrentWorld.WorldColumns[position + (x,z)], queueType)) return false;
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

    public static void Tick(Player player)
    {

        if (ColumnWorldGenerationQueue.Count > 0)
        {

            for (int i = 0; i < (ColumnWorldGenerationQueue.Count > _chunkGenerationAutoResetEvents.Count ? _chunkGenerationAutoResetEvents.Count : ColumnWorldGenerationQueue.Count); i++) {
                _chunkGenerationAutoResetEvents.ElementAt(i).Value.Set();
            }

            
        }
       
        HandleUploadQueue();
        
        // foreach (PackedChunkMesh m in CurrentWorld.PackedWorldMeshes.Values) m.Draw(player);
        foreach (ChunkColumn column in CurrentWorld.WorldColumns.Values)
        {

            for (int i = 0; i < column.ChunkMeshes.Length; i++)
            {

                column.ChunkMeshes[i].Draw(player);
                
            }

        }

    }

    public static void QueueGeneration()
    {
        
        if (_currentRadius <= WorldGenerationRadius)
        {

            foreach (Vector2i columnPosition in ColumnUtils.GetRing(_currentRadius))
            {

                CurrentWorld.WorldColumns.TryAdd(columnPosition, new ChunkColumn(columnPosition));

                ColumnWorldGenerationQueue.EnqueueLast(columnPosition);

            }

            _currentRadius++;

        }
        
    }

}