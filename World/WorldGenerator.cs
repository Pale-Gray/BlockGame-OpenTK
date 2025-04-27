using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.Core.Chunks;
using Game.Core.PlayerUtil;
using Game.Util;
using OpenTK.Mathematics;

namespace Game.Core.Worlds;

public class WorldGenerator
{

    public static int WorldGenerationPadding = 3;
    public static int WorldGenerationRadius = 8 + WorldGenerationPadding; // padding
    public static int WorldGenerationHeight = 8; // The height starting from y = 0
    public static int MaxChunkUploadCount = 999999999;
    public static int GenerationThreadCount = 10;
    public static bool IsSmoothLightingEnabled = true;
    private static ManualResetEvent _generationResetEvent = new ManualResetEvent(true);
    public static ConcurrentQueue<Vector2i> LowPriorityWorldGenerationQueue = new();
    public static ConcurrentQueue<Vector2i> HighPriorityWorldGenerationQueue = new();
    public static ConcurrentQueue<Vector2i> UnloadQueue = new();
    public static ConcurrentQueue<Vector2i> UploadQueue = new();
    public static Thread[] _generationThreads = new Thread[GenerationThreadCount];
    public static Dictionary<int, ManualResetEvent> _manualResetEvents = new();

    public static void Unload()
    {

        if (_manualResetEvents.Count != GenerationThreadCount) return;

        for (int i = 0; i < GenerationThreadCount; i++)
        {

            while (_generationThreads[i].IsAlive) _manualResetEvents[_generationThreads[i].ManagedThreadId].Set();
            _manualResetEvents.Remove(_generationThreads[i].ManagedThreadId);

        }

    }
    private static void HandleUnloadQueue()
    {

        while (UnloadQueue.TryDequeue(out Vector2i position))
        {

            if (HighPriorityWorldGenerationQueue.Contains(position) || LowPriorityWorldGenerationQueue.Contains(position) || GameState.World.WorldColumns[position].IsUpdating)
            {

                UnloadQueue.Enqueue(position);

            } else
            {

                if (GameState.World.WorldColumns.TryRemove(position, out ChunkColumn column) && column.QueueType >= QueueType.Mesh)
                {

                    ColumnSerializer.SerializeColumn(column);

                } 

            }

        }

    }
    private static void HandleUploadQueue()
    {

        if (UploadQueue.TryDequeue(out Vector2i columnPosition))
        {
        
            if (NetworkingValues.Server?.IsNetworked ?? false)
            {

                NetworkingValues.Server.SendChunk(columnPosition);

            } else
            {

                if (GameState.World.WorldColumns.TryGetValue(columnPosition, out ChunkColumn column))
                {

                    ColumnBuilder.Upload(column);

                }

            }

        }

    }
    private static void HandleGenerationQueue()
    {

        while (GlobalValues.IsRunning)
        {
            
            _manualResetEvents[Thread.CurrentThread.ManagedThreadId].WaitOne();
            if (HighPriorityWorldGenerationQueue.TryDequeue(out Vector2i columnPosition) || LowPriorityWorldGenerationQueue.TryDequeue(out columnPosition))
            {

                if (!GameState.World.WorldColumns.ContainsKey(columnPosition)) continue;
                switch (GameState.World.WorldColumns[columnPosition].QueueType)
                {

                    case QueueType.PassOne:
                        if (!GameState.World.WorldColumns.ContainsKey(columnPosition)) break;
                        ColumnBuilder.GeneratePassOne(GameState.World.WorldColumns[columnPosition]);
                        break;
                    case QueueType.SunlightCalculation:
                        if (!GameState.World.WorldColumns.ContainsKey(columnPosition)) break;
                        {
                            bool shouldQueue = false;
                            foreach (Player player in NetworkingValues.Server?.ConnectedPlayers.Values)
                            {

                                if (Maths.ChebyshevDistance2D(columnPosition, player.Loader.PlayerPosition) < WorldGenerationRadius)
                                {

                                    shouldQueue = true;
                                    break;

                                }

                            }
                            if (shouldQueue)
                            {

                                if (AreNeighborColumnsTheSameQueueType(columnPosition, QueueType.SunlightCalculation))
                                {
                                    ColumnBuilder.PrecalculateSunlight(GameState.World.WorldColumns, columnPosition);
                                } else
                                {
                                    if (GameState.World.WorldColumns[columnPosition].HasPriority)
                                    {
                                        HighPriorityWorldGenerationQueue.Enqueue(columnPosition);
                                    } else
                                    {
                                        LowPriorityWorldGenerationQueue.Enqueue(columnPosition);
                                    }
                                }

                            }
                        }
                        break;
                    case QueueType.LightPropagation:
                        if (!GameState.World.WorldColumns.ContainsKey(columnPosition)) break;
                        {
                            bool shouldQueue = false;
                            foreach (Player player in NetworkingValues.Server?.ConnectedPlayers.Values)
                            {

                                if (Maths.ChebyshevDistance2D(columnPosition, player.Loader.PlayerPosition) < WorldGenerationRadius - 1)
                                {

                                    shouldQueue = true;
                                    break;

                                }

                            }
                            if (shouldQueue)
                            {
                                if (AreNeighborColumnsTheSameQueueType(columnPosition, QueueType.LightPropagation))
                                {
                                    ColumnBuilder.PropagateLights(GameState.World.WorldColumns, columnPosition);
                                } else
                                {
                                    if (GameState.World.WorldColumns[columnPosition].HasPriority)
                                    {
                                        HighPriorityWorldGenerationQueue.Enqueue(columnPosition);
                                    } else
                                    {
                                        LowPriorityWorldGenerationQueue.Enqueue(columnPosition);
                                    }
                                }
                            }
                        }
                        break;
                    case QueueType.Mesh:
                        if (!GameState.World.WorldColumns.ContainsKey(columnPosition)) break;
                        if (NetworkingValues.Server?.IsNetworked ?? false)
                        {
                            GameState.World.WorldColumns[columnPosition].QueueType = QueueType.Upload;
                            LowPriorityWorldGenerationQueue.Enqueue(columnPosition);
                        } else 
                        {
                            bool shouldQueue = false;
                            foreach (Player player in NetworkingValues.Server?.ConnectedPlayers.Values)
                            {

                                if (Maths.ChebyshevDistance2D(columnPosition, player.Loader.PlayerPosition) < WorldGenerationRadius - 2)
                                {

                                    shouldQueue = true;
                                    break;

                                }

                            }
                            if (shouldQueue)
                            {
                                if (AreNeighborColumnsTheSameQueueType(columnPosition, QueueType.Mesh))
                                {
                                    ColumnBuilder.Mesh(GameState.World.WorldColumns, columnPosition);
                                } else
                                {
                                    if (GameState.World.WorldColumns[columnPosition].HasPriority)
                                    {
                                        HighPriorityWorldGenerationQueue.Enqueue(columnPosition);
                                    } else
                                    {
                                        LowPriorityWorldGenerationQueue.Enqueue(columnPosition);
                                    }
                                }
                            }
                        }
                        break;
                    case QueueType.Unload:
                        if (!GameState.World.WorldColumns.ContainsKey(columnPosition)) break;
                        if (GameState.World.WorldColumns.TryRemove(columnPosition, out ChunkColumn column))
                        {

                            ColumnSerializer.SerializeColumn(column);

                        }
                        break;

                }

            }
            if (LowPriorityWorldGenerationQueue.Count == 0 && HighPriorityWorldGenerationQueue.Count == 0) _manualResetEvents[Thread.CurrentThread.ManagedThreadId].Reset();

        }
        
    }
    private static bool AreNeighborColumnsTheSameQueueType(Vector2i position, QueueType queueType)
    {

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if ((x,z) != Vector2i.Zero) 
                {
                    if (!GameState.World.WorldColumns.ContainsKey(position + (x,z)) 
                    || !IsColumnTheSameQueueType(GameState.World.WorldColumns[position + (x,z)], queueType)) return false;
                }
            }
        }
        return true;

    }
    private static bool IsColumnTheSameQueueType(ChunkColumn column, QueueType queueType)
    {

        return column.QueueType >= queueType && column.QueueType != QueueType.Unload;

    }
    public static void Initialize()
    {

        for (int i = 0; i < GenerationThreadCount; i++)
        {

            _generationThreads[i] = new Thread(HandleGenerationQueue);
            _manualResetEvents.Add(_generationThreads[i].ManagedThreadId, new ManualResetEvent(true));
            _generationThreads[i].Start();

        }
        
    }

    public static void Update()
    {

        if (LowPriorityWorldGenerationQueue.Count > 0 || HighPriorityWorldGenerationQueue.Count > 0)
        {

            for (int i = 0; i < GenerationThreadCount; i++)
            {

                if (!_manualResetEvents[_generationThreads[i].ManagedThreadId].WaitOne(0)) _manualResetEvents[_generationThreads[i].ManagedThreadId].Set();

            }

        }
       
        HandleUploadQueue();
        
        foreach (ChunkColumn column in GameState.World.WorldColumns.Values)
        {

            bool shouldUnload = true;
            foreach (Player player in NetworkingValues.Server?.ConnectedPlayers.Values)
            {

                if (Maths.ChebyshevDistance2D(player.Loader.PlayerPosition, column.Position) <= WorldGenerationRadius) shouldUnload = false;

            }

            if (shouldUnload)
            {

                if (column.QueueType < QueueType.Mesh) continue; 

                column.FreeResources();
                column.QueueType = QueueType.Unload;
                LowPriorityWorldGenerationQueue.Enqueue(column.Position);

            }

        }

    }

}