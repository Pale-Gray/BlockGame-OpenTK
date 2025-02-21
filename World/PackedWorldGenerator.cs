using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    public static int WorldGenerationRadius = 7;
    public static int WorldGenerationHeight = 8; // The height starting from 0
    public static int MaxChunkUploadCount = 5;

    private static int _currentRadius = 0;

    public static PackedChunkWorld CurrentWorld;

    public static ThreadSafeDoubleEndedQueue<Vector3i> PackedChunkWorldGenerationQueue = new();
    public static ThreadSafeDoubleEndedQueue<Vector3i> PackedChunkWorldUploadQueue = new();
    
    private static List<Thread> _chunkGenerationThreads = new();
    
    private static ConcurrentDictionary<int, AutoResetEvent> _chunkGenerationAutoResetEvents = new();
    public static ConcurrentQueue<BlockLight> LightAdditionQueue = new();
    public static ConcurrentQueue<BlockLight> LightRemovalQueue = new();

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
        
        int currentCount = 0;
        while (currentCount < MaxChunkUploadCount && GlobalValues.IsRunning)
        {

            if (PackedChunkWorldUploadQueue.TryDequeueFirst(out Vector3i chunkPosition) && GlobalValues.IsRunning)
            {
                
                PackedChunkBuilder.Upload(chunkPosition);
                
            }
            currentCount++;

        }

    }
    private static void HandleGenerationQueue()
    {

        while (GlobalValues.IsRunning)
        {
            
            _chunkGenerationAutoResetEvents[Thread.CurrentThread.ManagedThreadId].WaitOne();
            while (PackedChunkWorldGenerationQueue.TryDequeueFirst(out Vector3i chunkPosition) && GlobalValues.IsRunning)
            {

                switch (CurrentWorld.PackedWorldChunks[chunkPosition].QueueType)
                {
                    case PackedChunkQueueType.PassOne:
                        PackedChunkBuilder.GeneratePassOne(CurrentWorld.PackedWorldChunks[chunkPosition]);
                        break;
                    case PackedChunkQueueType.SunlightCalculation:
                        if (Maths.ChebyshevDistance3D((chunkPosition.X, 0, chunkPosition.Z), Vector3i.Zero) < WorldGenerationRadius)
                        {
                            if (IsColumnTheSameQueueType(CurrentWorld, chunkPosition.Xz, PackedChunkQueueType.SunlightCalculation) && AreColumnNeighborsTheSameQueueType(CurrentWorld, chunkPosition.Xz, PackedChunkQueueType.SunlightCalculation)) 
                            { 
                                PackedChunkBuilder.QueueSunlightValues(CurrentWorld, CurrentWorld.PackedWorldChunks[chunkPosition]);
                            } else 
                            {
                                if (CurrentWorld.PackedWorldChunks[chunkPosition].HasPriority)
                                {
                                    PackedChunkWorldGenerationQueue.EnqueueBehindFirst(chunkPosition);
                                }
                                else
                                {
                                    PackedChunkWorldGenerationQueue.EnqueueLast(chunkPosition);
                                }
                            }
                        }
                        break;
                    case PackedChunkQueueType.LightPropagation:
                        if (Maths.ChebyshevDistance3D((chunkPosition.X, 0, chunkPosition.Z), Vector3i.Zero) < WorldGenerationRadius - 1 && chunkPosition.Y > 0 && chunkPosition.Y < WorldGenerationHeight) {
                            if (IsColumnTheSameQueueType(CurrentWorld, chunkPosition.Xz, PackedChunkQueueType.LightPropagation) && AreColumnNeighborsTheSameQueueType(CurrentWorld, chunkPosition.Xz, PackedChunkQueueType.LightPropagation)) 
                            { 
                                PackedChunkBuilder.ComputeLights(CurrentWorld.GetChunkNeighbors(chunkPosition), CurrentWorld.PackedWorldChunks[chunkPosition]);
                            } else 
                            {
                                if (CurrentWorld.PackedWorldChunks[chunkPosition].HasPriority)
                                {
                                    PackedChunkWorldGenerationQueue.EnqueueBehindFirst(chunkPosition);
                                }
                                else
                                {
                                    PackedChunkWorldGenerationQueue.EnqueueLast(chunkPosition);
                                }
                            }
                        }
                        break;
                    case PackedChunkQueueType.Mesh:
                        if (Maths.ChebyshevDistance3D((chunkPosition.X, 0, chunkPosition.Z), Vector3i.Zero) < WorldGenerationRadius - 2 && chunkPosition.Y > 0 && chunkPosition.Y < WorldGenerationHeight)
                        {
                            if (AreNeighborsTheSameQueueType(chunkPosition, PackedChunkQueueType.Mesh))
                            {
                                PackedChunkBuilder.Mesh(CurrentWorld.GetChunkNeighbors(chunkPosition), CurrentWorld.PackedWorldChunks[chunkPosition]);
                            }
                            else
                            {   
                                if (CurrentWorld.PackedWorldChunks[chunkPosition].HasPriority)
                                {
                                    PackedChunkWorldGenerationQueue.EnqueueBehindFirst(chunkPosition);
                                }
                                else
                                {
                                    PackedChunkWorldGenerationQueue.EnqueueLast(chunkPosition);
                                }
                            }
                        }
                        break;
                }
                
            }

        }
        
    }

    public static void SetAllResetEvents()
    {

        _chunkGenerationAutoResetEvents.Clear();

    }

    public static void CheckIfThreadsAreAlive()
    {
        
        for (int i = 0; i < _chunkGenerationThreads.Count; i++)
        {
            if (_chunkGenerationThreads[i].IsAlive)
            {
                Console.WriteLine($"Thread {_chunkGenerationThreads[i].ManagedThreadId} is alive");
            }
            else
            {
                Console.WriteLine($"Thread {_chunkGenerationThreads[i].ManagedThreadId} is not alive");
            }
        }

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

        if (PackedChunkWorldGenerationQueue.Count > 0)
        {

            int amountOfNeighborPairs = (PackedChunkWorldGenerationQueue.Count % 27) + 1;
            for (int i = 0; i < (amountOfNeighborPairs > _chunkGenerationAutoResetEvents.Count ? _chunkGenerationAutoResetEvents.Count : amountOfNeighborPairs); i++) {
                _chunkGenerationAutoResetEvents.ElementAt(i).Value.Set();
            }

            // foreach (AutoResetEvent e in _chunkGenerationAutoResetEvents.Values) e.Set();
        }
       
        HandleUploadQueue();
        
        foreach (PackedChunkMesh m in CurrentWorld.PackedWorldMeshes.Values) m.Draw(player);

    }
    private static List<Vector3i> _currentRing = WorldGeneratorUtilities.GetColumnRing(_currentRadius, WorldGenerationHeight, Vector3i.Zero);
    private static int _currentIndex = 0;
    public static void QueueGeneration()
    {
        
        if (_currentRadius <= WorldGenerationRadius)
        {
            
            /*
            if (_currentIndex < _currentRing.Count)
            {

                int amountUpated = 0;
                while (amountUpated < 25 && _currentIndex < _currentRing.Count)
                {

                    CurrentWorld.PackedWorldChunks.TryAdd(_currentRing[_currentIndex], new PackedChunk(_currentRing[_currentIndex]));
                    CurrentWorld.PackedWorldMeshes.TryAdd(_currentRing[_currentIndex], new PackedChunkMesh(_currentRing[_currentIndex]));
                    PackedChunkWorldGenerationQueue.EnqueueLast(_currentRing[_currentIndex]);
                    _currentIndex++;
                    amountUpated++;

                }

            } else 
            {

                _currentRadius++;
                _currentRing = WorldGeneratorUtilities.GetColumnRing(_currentRadius, WorldGenerationHeight, Vector3i.Zero);

            }
            */
            
            foreach (Vector3i position in WorldGeneratorUtilities.GetColumnRing(_currentRadius, WorldGenerationHeight, Vector3i.Zero))
            {
                
                CurrentWorld.PackedWorldChunks.TryAdd(position, new PackedChunk(position));
                CurrentWorld.PackedWorldMeshes.TryAdd(position, new PackedChunkMesh(position));
                CurrentWorld.MaxColumnBlockHeight.TryAdd(position.Xz, new uint[GlobalValues.ChunkSize * GlobalValues.ChunkSize]);
                
                PackedChunkWorldGenerationQueue.EnqueueLast(position);
            }

            _currentRadius++;

        }
        
    }

    private static bool AreColumnNeighborsTheSameQueueType(PackedChunkWorld world, Vector2i columnPosition, PackedChunkQueueType queueType)
    {

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if ((x,z) != Vector2i.Zero)
                {
                    if (!IsColumnTheSameQueueType(world, columnPosition + (x,z), queueType)) return false;
                }
            }
        }
        return true;

    }

    private static bool AreNeighborsTheSameQueueType(Vector3i chunkPosition, PackedChunkQueueType queueType)
    {

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if ((x, y, z) != Vector3i.Zero)
                    {
                        if (CurrentWorld.PackedWorldChunks.ContainsKey(chunkPosition + (x,y,z)))
                        {
                            if (CurrentWorld.PackedWorldChunks[chunkPosition + (x,y,z)].QueueType < queueType) return false;
                        }
                        else
                        {
                            return false;
                        }  
                    }
                }
            }
        }
        return true;

    }

    private static bool IsColumnTheSameQueueType(PackedChunkWorld world, Vector2i columnPosition, PackedChunkQueueType queueType)
    {

        for (int y = 0; y <= WorldGenerationHeight; y++)
        {
            if (world.PackedWorldChunks.ContainsKey((columnPosition.X, y, columnPosition.Y)) && world.PackedWorldChunks[(columnPosition.X, y, columnPosition.Y)].QueueType < queueType) return false;
        }
        return true;

    }
    
}