using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.Vulkan;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace Blockgame_OpenTK.Core.Worlds
{
    internal class WorldGenerator
    {

        public static int MaxRadius = 8;
        static int CurrentRadius = 0;
        static int MaxChunkUpdates = 100;
        static int MaxUploads = 10;
        private static Vector3i[] ChunksToAdd = ChunkUtils.GenerateRingsOfColumns(CurrentRadius, MaxRadius);
        private static int _totalQueued = 0;

        public static ConcurrentQueue<Vector2i> ConcurrentSunlightColumnUpdateQueue = new ConcurrentQueue<Vector2i>();
        public static ConcurrentQueue<Vector3i> ConcurrentChunkUpdateQueue = new ConcurrentQueue<Vector3i>();
        public static ConcurrentQueue<Vector3i> ConcurrentChunkUploadQueue = new ConcurrentQueue<Vector3i>();

        private static void UpdateConcurrentUploadQueue(World world)
        {

            int amountUpdated = 0;
            while (ConcurrentChunkUploadQueue.Count > 0 && amountUpdated < MaxUploads)
            {

                if (ConcurrentChunkUploadQueue.TryDequeue(out Vector3i chunkPosition))
                {

                    ChunkBuilder.CallOpenGL(world.WorldChunks[chunkPosition]);

                }
                amountUpdated++;

            }

        }

        private static void UpdateSunlightQueue(World world)
        {

            int amountUpdated = 0;
            while (ConcurrentSunlightColumnUpdateQueue.Count > 0 && amountUpdated < MaxChunkUpdates)
            {

                if (ConcurrentSunlightColumnUpdateQueue.TryDequeue(out Vector2i columnPosition))
                {

                    if (IsColumnTheSameQueueType(world, columnPosition, QueueType.SunlightGeneration))
                    {
                        
                        ChunkBuilder.CalculateSunlightColumnThreaded(world, columnPosition, 0);

                    } else
                    {

                        ConcurrentSunlightColumnUpdateQueue.Enqueue(columnPosition);

                    }

                }
                amountUpdated++;

            } 

        }
        private static void UpdateConcurrentQueue(World world)
        {

            int amountUpdated = 0;
            while (ConcurrentChunkUpdateQueue.Count > 0 && amountUpdated < MaxChunkUpdates)
            {

                if (ConcurrentChunkUpdateQueue.TryDequeue(out Vector3i chunkPosition))
                {

                    if (!world.WorldChunks[chunkPosition].IsUpdating)
                    {

                        switch (world.WorldChunks[chunkPosition].QueueType)
                        {

                            case QueueType.PassOne:
                                ChunkBuilder.GeneratePassOneThreaded(world.WorldChunks[chunkPosition]);
                                break;
                            case QueueType.Mesh:
                                if (Maths.ChebyshevDistance3D(chunkPosition, Vector3i.Zero) < MaxRadius)
                                {

                                    if (AreNeighborsTheSameQueueType(world, chunkPosition, QueueType.Mesh))
                                    {

                                        ChunkBuilder.MeshThreaded(world.WorldChunks[chunkPosition], world, Vector3i.Zero);

                                    }
                                    else
                                    {

                                        ConcurrentChunkUpdateQueue.Enqueue(chunkPosition);

                                    }

                                }
                                break;

                        }

                    } else { ConcurrentChunkUpdateQueue.Enqueue(chunkPosition); }

                }
                amountUpdated++;

            }

        }

        private static bool IsChunkTheSameQueueType(World world, Vector3i samplePosition, QueueType queueType)
        {

            return world.WorldChunks.ContainsKey(samplePosition) && world.WorldChunks[samplePosition].QueueType >= queueType;

        }

        private static bool IsColumnTheSameQueueType(World world, Vector2i columnPosition, QueueType queueType)
        {

            int count = 0;
            for (int chunkY = MaxRadius; chunkY >= -MaxRadius; chunkY--)
            {

                if (world.WorldChunks.ContainsKey((columnPosition.X, chunkY, columnPosition.Y)) && world.WorldChunks[(columnPosition.X, chunkY, columnPosition.Y)].QueueType >= queueType)
                {

                    count++;

                }

            }

            return count == (MaxRadius + MaxRadius + 1);

        }

        private static bool AreEightNeighborsTheSameQueueType(World world, Vector3i chunkPosition, QueueType queueType)
        {

            int count = 0;
            for (int x = -1; x <= 1; x++)
            {

                for (int z = -1; z <= 1; z++)
                {

                    if ((x,0,z) != Vector3i.Zero)
                    {

                        if (world.WorldChunks.ContainsKey(chunkPosition + (x, 0, z)) && world.WorldChunks[chunkPosition + (x, 0, z)].QueueType >= queueType) count++;

                    }

                }

            }
            return count == 8;

        }

        private static bool AreNeighborsTheSameQueueType(World world, Vector3i chunkPosition, QueueType queueType)
        {

            int count = 0;
            for (int x = -1; x <= 1; x++)
            {

                for (int y = -1; y <= 1; y++)
                {

                    for (int z = -1; z <= 1; z++)
                    {

                        if (world.WorldChunks.ContainsKey(chunkPosition + (x, y, z)) && world.WorldChunks[chunkPosition + (x, y, z)].QueueType >= queueType) count++;


                    }

                }

            }
            return count == 27;

        }

        private static bool NeighborQueueType(Vector3i position, World world, QueueType queueType)
        {

            int pass = 0;

            if (world.WorldChunks.ContainsKey(position + Vector3i.UnitY) && world.WorldChunks[position + Vector3i.UnitY].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position - Vector3i.UnitY) && world.WorldChunks[position - Vector3i.UnitY].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position + Vector3i.UnitX) && world.WorldChunks[position + Vector3i.UnitX].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position - Vector3i.UnitX) && world.WorldChunks[position - Vector3i.UnitX].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position + Vector3i.UnitZ) && world.WorldChunks[position + Vector3i.UnitZ].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position - Vector3i.UnitZ) && world.WorldChunks[position - Vector3i.UnitZ].QueueType >= queueType) pass++;

            if (world.WorldChunks.ContainsKey(position + Vector3i.UnitY + Vector3i.UnitX + Vector3i.UnitZ) && world.WorldChunks[position + Vector3i.UnitY + Vector3i.UnitX + Vector3i.UnitZ].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position + Vector3i.UnitX + Vector3i.UnitZ) && world.WorldChunks[position + Vector3i.UnitX + Vector3i.UnitZ].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position - Vector3i.UnitY + Vector3i.UnitX + Vector3i.UnitZ) && world.WorldChunks[position - Vector3i.UnitY + Vector3i.UnitX + Vector3i.UnitZ].QueueType >= queueType) pass++;

            if (world.WorldChunks.ContainsKey(position + Vector3i.UnitY - Vector3i.UnitX + Vector3i.UnitZ) && world.WorldChunks[position + Vector3i.UnitY - Vector3i.UnitX + Vector3i.UnitZ].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position - Vector3i.UnitX + Vector3i.UnitZ) && world.WorldChunks[position - Vector3i.UnitX + Vector3i.UnitZ].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position - Vector3i.UnitY - Vector3i.UnitX + Vector3i.UnitZ) && world.WorldChunks[position - Vector3i.UnitY - Vector3i.UnitX + Vector3i.UnitZ].QueueType >= queueType) pass++;

            if (world.WorldChunks.ContainsKey(position + Vector3i.UnitY + Vector3i.UnitX - Vector3i.UnitZ) && world.WorldChunks[position + Vector3i.UnitY + Vector3i.UnitX - Vector3i.UnitZ].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position + Vector3i.UnitX - Vector3i.UnitZ) && world.WorldChunks[position + Vector3i.UnitX - Vector3i.UnitZ].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position - Vector3i.UnitY + Vector3i.UnitX - Vector3i.UnitZ) && world.WorldChunks[position - Vector3i.UnitY + Vector3i.UnitX - Vector3i.UnitZ].QueueType >= queueType) pass++;

            if (world.WorldChunks.ContainsKey(position + Vector3i.UnitY - Vector3i.UnitX - Vector3i.UnitZ) && world.WorldChunks[position + Vector3i.UnitY - Vector3i.UnitX - Vector3i.UnitZ].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position - Vector3i.UnitX - Vector3i.UnitZ) && world.WorldChunks[position - Vector3i.UnitX - Vector3i.UnitZ].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position - Vector3i.UnitY - Vector3i.UnitX - Vector3i.UnitZ) && world.WorldChunks[position - Vector3i.UnitY - Vector3i.UnitX - Vector3i.UnitZ].QueueType >= queueType) pass++;

            if (world.WorldChunks.ContainsKey(position + Vector3i.UnitY + Vector3i.UnitX) && world.WorldChunks[position + Vector3i.UnitY + Vector3i.UnitX].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position - Vector3i.UnitY + Vector3i.UnitX) && world.WorldChunks[position - Vector3i.UnitY + Vector3i.UnitX].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position + Vector3i.UnitY - Vector3i.UnitX) && world.WorldChunks[position + Vector3i.UnitY - Vector3i.UnitX].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position - Vector3i.UnitY - Vector3i.UnitX) && world.WorldChunks[position - Vector3i.UnitY - Vector3i.UnitX].QueueType >= queueType) pass++;

            if (world.WorldChunks.ContainsKey(position + Vector3i.UnitY + Vector3i.UnitZ) && world.WorldChunks[position + Vector3i.UnitY + Vector3i.UnitZ].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position - Vector3i.UnitY + Vector3i.UnitZ) && world.WorldChunks[position - Vector3i.UnitY + Vector3i.UnitZ].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position + Vector3i.UnitY - Vector3i.UnitZ) && world.WorldChunks[position + Vector3i.UnitY - Vector3i.UnitZ].QueueType >= queueType) pass++;
            if (world.WorldChunks.ContainsKey(position - Vector3i.UnitY - Vector3i.UnitZ) && world.WorldChunks[position - Vector3i.UnitY - Vector3i.UnitZ].QueueType >= queueType) pass++;

            return pass == 26; // originally 6...

        }

        public static void Reset()
        {

            CurrentRadius = 0;
            // ChunkUpdateQueue.Clear();
            ConcurrentChunkUploadQueue.Clear();
            ConcurrentChunkUpdateQueue.Clear();
            ChunksToAdd = ChunkUtils.GenerateRingsOfColumns(CurrentRadius, MaxRadius + 2);

        }

        static Stopwatch sw;
        static Vector3i PreviousChunkPosition = Vector3i.Zero;
        static bool ShouldGenerate = true;
        public static void GenerateWorld(World world, Vector3 playerPosition)
        {

            Vector3i playerChunkPosition = ChunkUtils.PositionToChunk(playerPosition);

            // UpdateAlterQueue(world);
            // UpdateOpenglCallQueue(world);
            // UpdateQueue(world, PreviousChunkPosition);
            UpdateConcurrentQueue(world);
            UpdateSunlightQueue(world);
            UpdateConcurrentUploadQueue(world);

            if (CurrentRadius <= MaxRadius)
            {

                for (int i = 0; i < ChunksToAdd.Length; i++)
                {

                    if (!world.WorldChunks.ContainsKey(ChunksToAdd[i] + PreviousChunkPosition))
                    {

                        world.WorldChunks.TryAdd(ChunksToAdd[i] + PreviousChunkPosition, new Chunk(ChunksToAdd[i] + PreviousChunkPosition));
                        ConcurrentChunkUpdateQueue.Enqueue(ChunksToAdd[i] + PreviousChunkPosition);

                    }

                }

                CurrentRadius++;
                ChunksToAdd = ChunkUtils.GenerateRingsOfColumns(CurrentRadius, MaxRadius);

            }

            if (Maths.ChebyshevDistance3D(playerChunkPosition, PreviousChunkPosition) > 2)
            {

                // CurrentRadius = 0;
                // ChunksToAdd = ChunkUtils.GenerateRingsOfColumns(CurrentRadius, MaxRadius);
                // PreviousChunkPosition = playerChunkPosition;

            }

        }

    }
}
