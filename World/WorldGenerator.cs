using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Blockgame_OpenTK.Core.World
{
    internal class WorldGenerator
    {

        static int MaxRadius = 8;
        static int CurrentRadius = 0;
        static int MaxChunkUpdates = 50;
        public static ConcurrentQueue<Vector3i> ChunkUpdateQueue = new ConcurrentQueue<Vector3i>();
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

            return pass == 6;

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

                switch (world.WorldChunks[position].QueueType)
                {

                    case QueueType.Final:
                        world.WorldChunks[position].NeedsToRequeue = false;
                        ChunkBuilder.CallOpenGL(world.WorldChunks[position], world.WorldChunks);
                        break;
                    case QueueType.Mesh:

                        if (Maths.ChebyshevDistance3D(position, cameraPosition) <= MaxRadius)
                        {

                            if (!world.WorldChunks[position].IsEmpty && world.WorldChunks[position].CheckIfExposed(world.WorldChunks))
                            {

                                if (NeighborQueueType(position, world, QueueType.Mesh))
                                {
                                    // Console.WriteLine("yes");
                                    ChunkBuilder.MeshThreaded(world.WorldChunks[position], world.WorldChunks, Vector3i.Zero);

                                }

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

                if (world.WorldChunks[position].NeedsToRequeue)
                {

                    ChunkUpdateQueue.Enqueue(position);

                }

                amtUpdated++;

            }

        }

        public static void UpdateAlterQueue(World world)
        {

            int amtUpdated = 0;

            while (ChunkAlterUpdateQueue.Count > 0 && amtUpdated <= MaxChunkUpdates)
            {

                Vector3i position = ChunkAlterUpdateQueue.Dequeue();

                ChunkBuilder.Remesh(world.WorldChunks[position], world.WorldChunks);

            }

        }

        static Stopwatch sw;
        public static void GenerateWorld(World world, Vector3i cameraPosition)
        {

            // Console.WriteLine(ChunkUpdateQueue.Count);

            UpdateQueue(world, Vector3i.Zero);
            UpdateAlterQueue(world);

            if (CurrentRadius <= MaxRadius + 2)
            {

                for (int i = 0; i < ChunksToAdd.Length; i++)
                {

                    world.WorldChunks.Add(ChunksToAdd[i], new Chunk(ChunksToAdd[i]));
                    ChunkUpdateQueue.Enqueue(ChunksToAdd[i]);

                }

                CurrentRadius++;
                ChunksToAdd = ChunkUtils.GenerateRingsOfColumns(CurrentRadius, MaxRadius + 2);

            }

        }

    }
}
