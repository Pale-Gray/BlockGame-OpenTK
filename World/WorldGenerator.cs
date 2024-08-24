using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Blockgame_OpenTK.ChunkUtil;
using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.World
{
    internal class WorldGenerator
    {

        static int MaxRadius = 8;
        static int MaxChunksPerUpdate = 10;
        static int CurrentRadius = 0;
        static int CurrentPassOneRadius = 0;
        static int CurrentPassTwoRadius = 0;
        static int CurrentMeshPassRadius = 0;

        static Queue<Vector3i> PassOneQueue = new Queue<Vector3i>();
        static Queue<Vector3i> PassTwoQueue = new Queue<Vector3i>();
        static Queue<Vector3i> MeshPassQueue = new Queue<Vector3i>();

        static Vector3i[] ChunksToGeneratePassOne = ChunkUtils.GenerateRingsOfColumns(CurrentPassOneRadius, MaxRadius + 2);
        static Vector3i[] ChunksToGeneratePassTwo = ChunkUtils.GenerateRingsOfColumns(CurrentPassTwoRadius, MaxRadius + 1);
        static Vector3i[] ChunksToDoMeshPass = ChunkUtils.GenerateRingsOfColumns(CurrentMeshPassRadius, MaxRadius);

        static Vector3i[] ChunksToGenerate = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, MaxRadius + 2, 2);
        static int ChunksMeshedLength = ChunkUtils.GenerateRingsOfColumns(CurrentRadius, MaxRadius).Length;

        static Vector3i[] ChunksGenPassOne = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, MaxRadius + 2, 2);
        static Vector3i[] ChunksGenPassTwo = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, MaxRadius + 1, 1);
        static Vector3i[] ChunksMeshPass = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, MaxRadius, 0);

        static Stopwatch sw;

        public static void Reset()
        {

            CurrentRadius = 0;
            CurrentPassOneRadius = 0;
            CurrentPassTwoRadius = 0;
            CurrentMeshPassRadius = 0;

            PassOneQueue = new Queue<Vector3i>();
            PassTwoQueue = new Queue<Vector3i>();
            MeshPassQueue = new Queue<Vector3i>();

            ChunksToGeneratePassOne = ChunkUtils.GenerateRingsOfColumns(CurrentPassOneRadius, MaxRadius + 2);
            ChunksToGeneratePassTwo = ChunkUtils.GenerateRingsOfColumns(CurrentPassTwoRadius, MaxRadius + 1);
            ChunksToDoMeshPass = ChunkUtils.GenerateRingsOfColumns(CurrentMeshPassRadius, MaxRadius);

            ChunksToGenerate = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, MaxRadius + 2, 2);
            ChunksMeshedLength = ChunkUtils.GenerateRingsOfColumns(CurrentRadius, MaxRadius).Length;

            ChunksGenPassOne = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, MaxRadius + 2, 2);
            ChunksGenPassTwo = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, MaxRadius + 1, 1);
            ChunksMeshPass = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, MaxRadius, 0);

        }
        private static void UpdatePassOneQueue(World world)
        {

            int amtUpdated = 0;

            while (PassOneQueue.Count > 0 && amtUpdated <= MaxChunksPerUpdate)
            {

                Vector3i position = PassOneQueue.Dequeue();
                ChunkBuilder.GeneratePassOneThreaded(world.WorldChunks[position]);

                amtUpdated++;

            }

        }

        private static void UpdatePassTwoQueue(World world)
        {

            int amtUpdated = 0;

            while (PassTwoQueue.Count > 0 && amtUpdated <= MaxChunksPerUpdate)
            {

                Vector3i position = PassTwoQueue.Dequeue();
                ChunkBuilder.GeneratePassTwoThreaded(world.WorldChunks[position], world.WorldChunks);
                amtUpdated++;

            }

        }

        private static void UpdateMeshPassQueue(World world)
        {

            int amtUpdated = 0;

            while (MeshPassQueue.Count > 0 && amtUpdated <= MaxChunksPerUpdate)
            {

                Vector3i position = MeshPassQueue.Dequeue();
                ChunkBuilder.MeshThreaded(world.WorldChunks[position], world.WorldChunks);
                amtUpdated++;

            }

        }

        public static void Generate(World world, Vector3i cameraChunkPosition)
        {

            UpdatePassOneQueue(world);
            UpdatePassTwoQueue(world);
            UpdateMeshPassQueue(world);

            int amountMeshed = 0;
            int amountPassOne = 0;
            int amountPassTwo = 0;

            if (sw == null) sw = Stopwatch.StartNew();

            if (CurrentRadius <= MaxRadius)
            {

                for (int i = 0; i < ChunksGenPassOne.Length; i++)
                {

                    if (!world.WorldChunks.ContainsKey(ChunksGenPassOne[i]))
                    {

                        world.WorldChunks.Add(ChunksGenPassOne[i], new Chunk(ChunksGenPassOne[i]));
                        PassOneQueue.Enqueue(ChunksGenPassOne[i]);

                    }
                    else
                    {

                        if (world.WorldChunks[ChunksGenPassOne[i]].GenerationState >= GenerationState.PassOne)
                        {

                            amountPassOne++;

                        }

                    }

                }

                for (int i = 0; i < ChunksGenPassTwo.Length; i++)
                {

                    if (world.WorldChunks.ContainsKey(ChunksGenPassTwo[i]))
                    {

                        if (world.WorldChunks[ChunksGenPassTwo[i]].GenerationState == GenerationState.PassOne)
                        {

                            if (AreNeighborsCertainGenerationState(ChunksGenPassTwo[i], GenerationState.PassOne, world))
                            {

                                if (world.WorldChunks[ChunksGenPassTwo[i]].QueueMode == QueueMode.NotQueued)
                                {

                                    world.WorldChunks[ChunksGenPassTwo[i]].QueueMode = QueueMode.Queued;
                                    PassTwoQueue.Enqueue(ChunksGenPassTwo[i]);

                                }

                            }

                        }

                        if (world.WorldChunks[ChunksGenPassTwo[i]].GenerationState >= GenerationState.Generated)
                        {

                            amountPassTwo++;

                        }

                    }

                }

                for (int i = 0; i < ChunksMeshPass.Length; i++)
                {

                    if (world.WorldChunks.ContainsKey(ChunksMeshPass[i]))
                    {

                        if (world.WorldChunks[ChunksMeshPass[i]].GenerationState == GenerationState.Generated && world.WorldChunks[ChunksMeshPass[i]].MeshState == MeshState.NotMeshed)
                        {

                            if (AreNeighborsCertainGenerationState(ChunksMeshPass[i], GenerationState.Generated, world))
                            {

                                if (world.WorldChunks[ChunksMeshPass[i]].QueueMode == QueueMode.NotQueued)
                                {

                                    world.WorldChunks[ChunksMeshPass[i]].QueueMode = QueueMode.Queued;
                                    MeshPassQueue.Enqueue(ChunksMeshPass[i]);

                                }

                            }

                        }

                        if (world.WorldChunks[ChunksMeshPass[i]].MeshState == MeshState.Meshed)
                        {

                            if (world.WorldChunks[ChunksMeshPass[i]].ChunkState == ChunkState.NotReady)
                            {

                                ChunkBuilder.CallOpenGL(world.WorldChunks[ChunksMeshPass[i]], world.WorldChunks);

                            }

                            amountMeshed++;

                        }

                    }

                }

                if (amountMeshed >= ChunksMeshPass.Length && amountPassOne >= ChunksGenPassOne.Length && amountPassTwo >= ChunksGenPassTwo.Length)
                {

                    CurrentRadius++;
                    ChunksGenPassOne = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, MaxRadius + 2, 2);
                    ChunksGenPassTwo = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, MaxRadius + 1, 1);
                    ChunksMeshPass = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, MaxRadius, 0);

                }

            } else
            {

                sw.Stop();

                Console.WriteLine($"Finished generating chunks with {MaxRadius} radius in {sw.ElapsedMilliseconds}ms, {sw.ElapsedMilliseconds/1000f}s");

            }

        }

        public static void GenerateWorld(World world, Vector3i cameraChunkPosition)
        {

            int amtGenPassOne = 0;
            int amtGenPassTwo = 0;
            int amtGenMeshPass = 0;

            UpdatePassOneQueue(world);
            UpdatePassTwoQueue(world);
            UpdateMeshPassQueue(world);

            // Console.WriteLine($"{CurrentRadius}, {CurrentPassOneRadius}, {CurrentPassTwoRadius}, {CurrentMeshPassRadius}");

            if (CurrentRadius <= MaxRadius)
            {

                if (CurrentMeshPassRadius > CurrentRadius)
                {

                    Console.WriteLine("increasing current radius");
                    CurrentRadius++;

                }

                if (CurrentPassOneRadius <= CurrentRadius+2)
                {

                    for (int i = 0; i < ChunksToGeneratePassOne.Length; i++)
                    {

                        if (!world.WorldChunks.ContainsKey(ChunksToGeneratePassOne[i]))
                        {

                            world.WorldChunks.Add(ChunksToGeneratePassOne[i], new Chunk(ChunksToGeneratePassOne[i]));
                            PassOneQueue.Enqueue(ChunksToGeneratePassOne[i]);

                        } else
                        {

                            if (world.WorldChunks[ChunksToGeneratePassOne[i]].GenerationState >= GenerationState.PassOne)
                            {

                                amtGenPassOne++;

                            }

                        }

                    }

                    if (amtGenPassOne >= ChunksToGeneratePassOne.Length)
                    {

                        Console.WriteLine($"pass one completed: {CurrentPassOneRadius}");
                        CurrentPassOneRadius++;
                        ChunksToGeneratePassOne = ChunkUtils.GenerateRingsOfColumns(CurrentPassOneRadius, MaxRadius + 2);

                    }

                }

                if (CurrentPassTwoRadius <= CurrentRadius+1)
                {

                    for (int i = 0; i < ChunksToGeneratePassTwo.Length; i++)
                    {

                        if (world.WorldChunks.ContainsKey(ChunksToGeneratePassTwo[i]))
                        {

                            if (world.WorldChunks[ChunksToGeneratePassTwo[i]].GenerationState == GenerationState.PassOne && world.WorldChunks[ChunksToGeneratePassTwo[i]].QueueMode == QueueMode.NotQueued && AreNeighborsCertainGenerationState(ChunksToGeneratePassTwo[i], GenerationState.PassOne, world))
                            {

                                world.WorldChunks[ChunksToGeneratePassTwo[i]].QueueMode = QueueMode.Queued;
                                PassTwoQueue.Enqueue(ChunksToGeneratePassTwo[i]);

                            }

                            if (world.WorldChunks[ChunksToGeneratePassTwo[i]].GenerationState >= GenerationState.Generated)
                            {

                                amtGenPassTwo++;

                            }

                        }

                    }

                    if (amtGenPassTwo >= ChunksToGeneratePassTwo.Length)
                    {

                        Console.WriteLine($"current pass two radius: {CurrentPassTwoRadius}");
                        CurrentPassTwoRadius++;
                        ChunksToGeneratePassTwo = ChunkUtils.GenerateRingsOfColumns(CurrentPassTwoRadius, MaxRadius + 1);

                    }

                }

                if (CurrentMeshPassRadius <= CurrentRadius)
                {

                    for (int i = 0; i < ChunksToDoMeshPass.Length; i++)
                    {

                        if (world.WorldChunks.ContainsKey(ChunksToDoMeshPass[i]))
                        {

                            if (world.WorldChunks[ChunksToDoMeshPass[i]].GenerationState == GenerationState.Generated && world.WorldChunks[ChunksToDoMeshPass[i]].MeshState == MeshState.NotMeshed && world.WorldChunks[ChunksToDoMeshPass[i]].QueueMode == QueueMode.NotQueued && AreNeighborsCertainGenerationState(ChunksToDoMeshPass[i], GenerationState.Generated, world))
                            {

                                world.WorldChunks[ChunksToDoMeshPass[i]].QueueMode = QueueMode.Queued;
                                MeshPassQueue.Enqueue(ChunksToDoMeshPass[i]);

                            }

                            if (world.WorldChunks[ChunksToDoMeshPass[i]].MeshState == MeshState.Meshed)
                            {

                                amtGenMeshPass++;
                                if (world.WorldChunks[ChunksToDoMeshPass[i]].ChunkState == ChunkState.NotReady)
                                {

                                    ChunkBuilder.CallOpenGL(world.WorldChunks[ChunksToDoMeshPass[i]], world.WorldChunks);

                                }

                            }

                        }

                    }

                    if (amtGenMeshPass >= ChunksToDoMeshPass.Length)
                    {

                        Console.WriteLine($"current mesh radius: {CurrentMeshPassRadius}");
                        CurrentMeshPassRadius++;
                        ChunksToDoMeshPass = ChunkUtils.GenerateRingsOfColumns(CurrentMeshPassRadius, MaxRadius);

                    }

                }

            }

        }

        private static bool AreNeighborsCertainGenerationState(Vector3i chunkPosition, GenerationState stateToTest, World world)
        {

            int amtState = 0;

            if (world.WorldChunks.ContainsKey(chunkPosition + Vector3i.UnitY) && world.WorldChunks[chunkPosition + Vector3i.UnitY].GenerationState >= stateToTest) amtState++;
            if (world.WorldChunks.ContainsKey(chunkPosition - Vector3i.UnitY) && world.WorldChunks[chunkPosition - Vector3i.UnitY].GenerationState >= stateToTest) amtState++;
            if (world.WorldChunks.ContainsKey(chunkPosition + Vector3i.UnitX) && world.WorldChunks[chunkPosition + Vector3i.UnitX].GenerationState >= stateToTest) amtState++;
            if (world.WorldChunks.ContainsKey(chunkPosition - Vector3i.UnitX) && world.WorldChunks[chunkPosition - Vector3i.UnitX].GenerationState >= stateToTest) amtState++;
            if (world.WorldChunks.ContainsKey(chunkPosition + Vector3i.UnitZ) && world.WorldChunks[chunkPosition + Vector3i.UnitZ].GenerationState >= stateToTest) amtState++;
            if (world.WorldChunks.ContainsKey(chunkPosition - Vector3i.UnitZ) && world.WorldChunks[chunkPosition - Vector3i.UnitZ].GenerationState >= stateToTest) amtState++;

            return amtState == 6;

        }

    }
}
