﻿using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Util;
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

        static int MaxRadius = 8;
        static int CurrentRadius = 0;
        static int MaxChunkUpdates = 50;
        public static ConcurrentQueue<Vector3i> ChunkUpdateQueue = new ConcurrentQueue<Vector3i>();
        public static Queue<Vector3i> SaveAndRemoveQueue = new Queue<Vector3i>();
        public static LinkedList<Vector3i> Queue = new LinkedList<Vector3i>();
        public static Queue<Vector3i> ChunkAlterUpdateQueue = new Queue<Vector3i>();
        private static Vector3i[] ChunksToAdd = ChunkUtils.GenerateRingsOfColumns(CurrentRadius, MaxRadius + 2);

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
            ChunkUpdateQueue.Clear();
            ChunksToAdd = ChunkUtils.GenerateRingsOfColumns(CurrentRadius, MaxRadius + 2);

        }
        private static void UpdateQueue(World world, Vector3i cameraPosition)
        {

            int amtUpdated = 0;

            while (ChunkUpdateQueue.Count > 0 && amtUpdated <= MaxChunkUpdates)
            {

                Vector3i position;
                bool did = ChunkUpdateQueue.TryDequeue(out position);
                // Console.WriteLine(Maths.ChebyshevDistance3D(position, cameraPosition));

                if (world.WorldChunks.ContainsKey(position))
                {

                    if (Maths.ChebyshevDistance3D(position, cameraPosition) > MaxRadius + 2)
                    {

                        world.WorldChunks[position].NeedsToRequeue = false;

                    }

                    if (world.WorldChunks[position].NeedsToRequeue)
                    {

                        ChunkUpdateQueue.Enqueue(position);

                    }

                    // Console.WriteLine($"Chunk queue type: {world.WorldChunks[position].QueueType}, distance {Maths.ChebyshevDistance3D(position, cameraPosition)}");

                    switch (world.WorldChunks[position].QueueType)
                    {

                        case QueueType.Remove:
                            world.WorldChunks[position].SaveToFile();
                            world.WorldChunks.Remove(position);
                            break;
                        case QueueType.Final:
                            world.WorldChunks[position].NeedsToRequeue = false;
                            ChunkBuilder.CallOpenGL(world.WorldChunks[position], world.WorldChunks);
                            break;
                        case QueueType.Mesh:
                            if (!world.WorldChunks[position].IsEmpty && world.WorldChunks[position].CheckIfExposed(world.WorldChunks))
                            {

                                if (NeighborQueueType(position, world, QueueType.Mesh))
                                {

                                    ChunkBuilder.MeshThreaded(world.WorldChunks[position], world.WorldChunks, Vector3i.Zero);

                                }

                            } else
                            {

                                world.WorldChunks[position].NeedsToRequeue = false;

                            }
                            break;
                        case QueueType.PassOne:
                            ChunkBuilder.GeneratePassOneThreaded(world.WorldChunks[position]);
                            break;

                    }

                }

                amtUpdated++;

            }

        }

        public static void UpdateAlterQueue(World world)
        {

            int amtUpdated = 0;

            Console.WriteLine(ChunkAlterUpdateQueue.Count);

            while (ChunkAlterUpdateQueue.Count > 0 && amtUpdated < MaxChunkUpdates)
            {

                Vector3i position = ChunkAlterUpdateQueue.Dequeue();
                if (world.WorldChunks[position].CallForRemesh)
                {

                    ChunkBuilder.RemeshThreaded(world.WorldChunks[position], world.WorldChunks, Vector3i.Zero);
                    if (world.WorldChunks[position].CallForRemesh)
                    {

                        ChunkAlterUpdateQueue.Enqueue(position);

                    }

                } else
                {

                    ChunkBuilder.CallOpenGL(world.WorldChunks[position], world.WorldChunks);

                }

                amtUpdated++;

            }

        }

        public static void UpdateSaveAndRemoveQueue(World world)
        {

            int amtUpdated = 0;

            while (SaveAndRemoveQueue.Count > 0 && amtUpdated < MaxChunkUpdates)
            {

                Vector3i position = SaveAndRemoveQueue.Dequeue();

                world.WorldChunks[position].SaveToFile();
                bool removed = world.WorldChunks.Remove(position);

                amtUpdated++;

            }

        }

        static Stopwatch sw;
        static Vector3i PreviousChunkPosition = Vector3i.Zero;
        public static void GenerateWorld(World world, Vector3 playerPosition)
        {

            Vector3i playerChunkPosition = ChunkUtils.PositionToChunk(playerPosition);

            UpdateAlterQueue(world);
            UpdateQueue(world, PreviousChunkPosition);
            // Console.WriteLine(ChunkUpdateQueue.Count);

            if (CurrentRadius <= MaxRadius + 2)
            {

                for (int i = 0; i < ChunksToAdd.Length; i++)
                {

                    if (!world.WorldChunks.ContainsKey(ChunksToAdd[i] + PreviousChunkPosition))
                    {

                        world.WorldChunks.Add(ChunksToAdd[i] + PreviousChunkPosition, new Chunk(ChunksToAdd[i] + PreviousChunkPosition));
                        world.WorldChunks[ChunksToAdd[i] + PreviousChunkPosition].NeedsToRequeue = true;
                        world.WorldChunks[ChunksToAdd[i] + PreviousChunkPosition].QueueType = QueueType.PassOne;
                        ChunkUpdateQueue.Enqueue(ChunksToAdd[i] + PreviousChunkPosition);

                    }

                }

                CurrentRadius++;
                ChunksToAdd = ChunkUtils.GenerateRingsOfColumns(CurrentRadius, MaxRadius + 2);

            }

            if (Maths.ChebyshevDistance3D(playerChunkPosition, PreviousChunkPosition) > 1)
            {

                Console.WriteLine("Chunk position changed");
                CurrentRadius = 0; // CurrentRadius - Maths.ChebyshevDistance3D(playerChunkPosition, PreviousChunkPosition);
                ChunksToAdd = ChunkUtils.GenerateRingsOfColumns(CurrentRadius, MaxRadius + 2);
                PreviousChunkPosition = playerChunkPosition;

                foreach (Chunk chunk in world.WorldChunks.Values)
                {

                    if (Maths.ChebyshevDistance3D(PreviousChunkPosition, chunk.ChunkPosition) > MaxRadius + 2)
                    {

                        chunk.QueueType = QueueType.Remove;
                        ChunkUpdateQueue.Enqueue(chunk.ChunkPosition);

                    }

                }

            }

        }

    }
}