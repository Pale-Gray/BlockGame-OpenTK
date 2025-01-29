using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.PlayerUtil;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.Vulkan;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Worlds;

public class PackedWorldGenerator
{

    public static int WorldGenerationRadius = 8;
    public static int WorldGenerationHeight = 4;
    public static int MaxChunkUploadCount = 5;

    private static int _currentRadius = 0;

    public static PackedChunkWorld CurrentWorld;

    public static ThreadSafeDoubleEndedQueue<Vector3i> PackedChunkWorldGenerationQueue = new();
    public static ThreadSafeDoubleEndedQueue<Vector3i> PackedChunkWorldUploadQueue = new();
    
    private static List<Thread> _chunkGenerationThreads = new();
    
    private static ConcurrentDictionary<int, AutoResetEvent> _chunkGenerationAutoResetEvents = new();

    public static AutoResetEvent _resetEvent = new(true);

    private static void HandleUploadQueue()
    {
        
        int currentCount = 0;
        while (currentCount < MaxChunkUploadCount)
        {

            if (PackedChunkWorldUploadQueue.TryDequeueFirst(out Vector3i chunkPosition))
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
            while (PackedChunkWorldGenerationQueue.TryDequeueFirst(out Vector3i chunkPosition))
            {
               
                switch (CurrentWorld.PackedWorldChunks[chunkPosition].QueueType)
                {
                    case PackedChunkQueueType.PassOne:
                        PackedChunkBuilder.GeneratePassOne(CurrentWorld.PackedWorldChunks[chunkPosition]);
                        break;
                    case PackedChunkQueueType.Mesh:
                        if (Maths.ChebyshevDistance3D((chunkPosition.X, 0, chunkPosition.Z), Vector3i.Zero) < WorldGenerationRadius && Math.Abs(chunkPosition.Y) < WorldGenerationHeight)
                        {
                            if (AreNeighborsTheSameQueueType(chunkPosition, PackedChunkQueueType.Mesh))
                            {
                                PackedChunkBuilder.Mesh(CurrentWorld.GetChunkNeighbors(chunkPosition), CurrentWorld.PackedWorldChunks[chunkPosition]);
                            }
                            else
                            {
                                PackedChunkWorldGenerationQueue.EnqueueLast(chunkPosition);
                            }
                        }
                        break;
                }
            
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
            foreach (AutoResetEvent e in _chunkGenerationAutoResetEvents.Values) e.Set();
        }
       
        HandleUploadQueue();
        
        foreach (PackedChunkMesh m in CurrentWorld.PackedWorldMeshes.Values) m.Draw(player);

    }
    
    public static void QueueGeneration()
    {
        
        if (_currentRadius <= WorldGenerationRadius)
        {
            
            foreach (Vector3i position in WorldGeneratorUtilities.GetColumnRing(_currentRadius, WorldGenerationHeight, Vector3i.Zero))
            {
                
                CurrentWorld.PackedWorldChunks.TryAdd(position, new PackedChunk(position));
                CurrentWorld.PackedWorldMeshes.TryAdd(position, new PackedChunkMesh(position));
                
                PackedChunkWorldGenerationQueue.EnqueueLast(position);
            }

            _currentRadius++;

        }
        
    }

    private static bool AreNeighborsTheSameQueueType(Vector3i chunkPosition, PackedChunkQueueType queueType)
    {

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
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
        return true;

    }
    
}