using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using System.Diagnostics;

using Blockgame_OpenTK.Util;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using System.Reflection;
using OpenTK.Platform.Windows;

namespace Blockgame_OpenTK.Core.Chunks
{
    internal class ChunkLoader
    {
        /*
        public static Dictionary<Vector3, Chunk> ChunkDictionary = new Dictionary<Vector3, Chunk>();
        public static Dictionary<Vector3, Chunk> ReadyChunks = new Dictionary<Vector3, Chunk>();

        static float elapsedTime = 0;
        static int chunkRadius = 0;
        public static void GenerateChunksWithinRadius(int radius)
        {

            chunkRadius = radius;

            Stopwatch elapsed = Stopwatch.StartNew();
            for (int x = 0; x < radius; x++)
            {

                for (int y = 0; y < radius; y++)
                {

                    for (int z = 0; z < radius; z++)
                    {

                        Append(new Chunk(x,y,z));

                    }

                }

            }

            // Chunks = ChunkDictionary.Values.ToArray();

            // SetPriorities(camera);

            elapsed.Stop();
            TimeSpan elapsedtime = elapsed.Elapsed;
            Console.WriteLine("Finished generating " + ChunkDictionary.Count + " chunks in " + Math.Round(elapsedtime.TotalSeconds, 2) + " seconds.");

        }

        static int ChunksPerTick = 2; // amount of chunks you want to generate per tick
        static float TickLength = 0.1f; // length of tick in seconds
        static float Ticking = 0;
        static int[] ChunkPositions;
        public static void GenerateChunksWithinRadiusStaggered(int radius, float deltaTime)
        {
            Ticking += deltaTime;
            int MaxChunks = radius * radius * radius;

            if (Ticking > TickLength)
            {

                Console.WriteLine("Ticking.");
                Console.WriteLine(ChunkPositions.Length);
                // Console.WriteLine("{0}, {1}, {2}", ChunkPositions[0], ChunkPositions[1], ChunkPositions[2]);
                if ((MaxChunks - ChunkDictionary.Count) < ChunksPerTick)
                {

                    ChunksPerTick = MaxChunks - ChunkDictionary.Count;

                }
                for (int a = 0; a < ChunksPerTick; a++)
                {
                    Console.WriteLine("appending...");
                    // Console.WriteLine("{0}, {1}, {2}", ChunkPositions[0], ChunkPositions[1], ChunkPositions[2]);
                    Append(new Chunk(ChunkPositions[0], ChunkPositions[1], ChunkPositions[2]));
                    ChunkPositions = ChunkPositions.Skip(3).ToArray();

                }
                // ChunkPositions = ChunkPositions.Skip(3).ToArray();
                Ticking = 0;

            }

        }
        public static Chunk GetChunkFromWorldPosition(Vector3 position)
        {

            int x = (int)Math.Floor(position.X / 32);
            int y = (int)Math.Floor(position.Y / 32);
            int z = (int)Math.Floor(position.Z / 32);

            return GetChunkAtPosition((x,y,z));

        }
        public static Chunk GetChunkFromWorldPosition(Camera camera)
        {

            int x = (int) Math.Floor(camera.Position.X / 32);
            int y = (int)Math.Floor(camera.Position.Y / 32);
            int z = (int)Math.Floor(camera.Position.Z / 32);

            try
            {

                return GetChunkAtPosition((x, y, z));

            } catch
            {

                return GetChunkAtPosition(Vector3.Zero);

            }

        }

        static float InternalTime = 0;
        static int AmountOfChunksUpdated = 0;
        static int ChunkRadius = 0;
        static int MaxRadius = 12;
        static int ChunksUpdated = 0;
        static Vector3i LastCameraPosition = Vector3i.Zero;
        static Vector3i CameraPosition = Vector3i.Zero;
        static Vector3[] CurrentRing;

        static int TerrainChunksUpdated = 0;
        static int MeshChunksUpdated = 0;

        static Vector3[] TerrainPositions = ChunkUtils.RingedColumnPadded(LastCameraPosition, ChunkRadius, 1, MaxRadius + 1);
        static Vector3[] MeshingPositions = ChunkUtils.RingedColumn(LastCameraPosition, ChunkRadius, MaxRadius);

        static Vector3[] Positions = ChunkUtils.RingedColumnPadded(LastCameraPosition, 0, MaxRadius + 1, MaxRadius + 1);
        static int PositionsLeftUnchecked = Positions.Except(ChunkUtils.RingedColumnPadded(LastCameraPosition, 0, MaxRadius, MaxRadius)).Count();
        static List<Vector3> ChunksDone = new List<Vector3>();
        static Vector3[] PositionsExcludingReadyChunks = Positions.Except(ChunksDone).ToArray();
        public static void UpdateLoadingPositions()
        {

            TerrainPositions = ChunkUtils.RingedColumnPadded(LastCameraPosition, ChunkRadius, 1, MaxRadius + 1);
            MeshingPositions = ChunkUtils.RingedColumn(LastCameraPosition, ChunkRadius, MaxRadius);

        }
        public static bool NeighborsGenerated(Vector3 position)
        {

            

            return false;

        }
        private static void RemoveExcessChunks()
        {

            foreach (Vector3 ChunkPosition in ChunkDictionary.Keys)
            {

                if (ChunkPosition.X > LastCameraPosition.X + MaxRadius ||
                        ChunkPosition.X < LastCameraPosition.X - MaxRadius ||
                        ChunkPosition.Y > LastCameraPosition.Y + MaxRadius ||
                        ChunkPosition.Y < LastCameraPosition.Y - MaxRadius ||
                        ChunkPosition.Z > LastCameraPosition.Z + MaxRadius ||
                        ChunkPosition.Z < LastCameraPosition.Z - MaxRadius)
                {

                    RemoveAt(ChunkPosition);

                }

            }

        }

        public static Chunk GetChunkAtPosition(Vector3 position)
        {

            return ChunkDictionary[position];

        }
        public static String ReturnChunkPositionCombined(int cx, int cy, int cz)
        {

            // Console.WriteLine(cx.ToString() + "_" + cy.ToString() + "_" + cz.ToString());


            return cx.ToString() + "_" + cy.ToString() + "_" + cz.ToString();

        }
        public static int[] GetChunkPositionFromFile(string pathtofile)
        {

            string[] splitunderscore = pathtofile.Split("/").Last().Split("_");
            string[] numbers = { splitunderscore[0], splitunderscore[1], splitunderscore[2].Split(".")[0] };

            for (int i = 0; i < numbers.Length; i++)
            {

                Console.WriteLine(numbers[i]);

            }


            return new int[] { Int32.Parse(numbers[0]), Int32.Parse(numbers[1]), Int32.Parse(numbers[2]) };

        }
        public static Dictionary<Vector3, Chunk> GetAllChunks()
        {

            return ChunkDictionary;

        }
        public static void Append(Chunk chunk)
        {

            ChunkDictionary.TryAdd(chunk.ChunkPosition, chunk);

        }
        public static bool ContainsChunk(Vector3i position)
        {


            return Chunks.ContainsKey(position);

        }

        public static bool ContainsGeneratedChunk(Vector3i position)
        {

            if (ContainsChunk(position) && GetChunk(position).GetGenerationState() == GenerationState.Generated)
            {

                return true;

            }

            return false;

        }
        public static Chunk GetChunk(Vector3i position)
        {

            return Chunks[position];

        }
        public static void AddChunk(Vector3i position)
        {

            if (!ContainsChunk(position))
            {

                ChunkDictionary.TryAdd(position, new Chunk(position));

            }

        }
        public static void Remove(Chunk chunk)
        {

            ChunkDictionary.Remove(chunk.ChunkPosition);

        }
        public static void RemoveAt(Vector3 position)
        {

            ChunkDictionary.Remove(position);

        }


        public static int Radius = 6;
        static int CurrentRadius = 0;
        static int PassOnePadding = 0;
        static int PassTwoPadding = 0;
        static int CurrentPassOneRadius = 0;
        static int CurrentPassTwoRadius = 0;
        public static int CurrentMeshRadius = 0;
        public static Dictionary<Vector3i, Chunk> Chunks = new Dictionary<Vector3i, Chunk>();
        static Vector3i[] cposs = ChunkUtils.InitialFloodFill(2, 2);
        static int MaximumChunksPerTick = 25;
        static bool DoGenerationUpdateTicking = true;

        static Vector3i previousPlayerChunkPosition = Vector3i.Zero;
        static Vector3i[] ChunksToGeneratePassOne = ChunkUtils.GenerateRingsOfColumns(0, Radius+2);
        static Vector3i[] ChunksToGeneratePassTwo = ChunkUtils.GenerateRingsOfColumns(0, Radius+1);
        static Vector3i[] ChunksToMesh = ChunkUtils.GenerateRingsOfColumns(0, Radius);
        public static List<Vector3i> RemeshQueue = new List<Vector3i>();
        public static List<Vector3i> SaveAndRemoveQueue = new List<Vector3i>();
        public static List<Vector3i> LoadAndMeshQueue = new List<Vector3i>();

        public static Queue<Vector3i> GenOneQueue = new Queue<Vector3i>();
        public static Queue<Vector3i> GenTwoQueue = new Queue<Vector3i>();
        public static Queue<Vector3i> MeshQueue = new Queue<Vector3i>();
        public static Queue<Vector3i> CallQueue = new Queue<Vector3i>();

        public static void UpdatePassOneQueue()
        {

            int chunksGeneratedPassOne = 0;

            while (GenOneQueue.Count > 0 && chunksGeneratedPassOne <= MaximumChunksPerTick)
            {

                Vector3i position = GenOneQueue.Dequeue();
                ChunkBuilder.GeneratePassOneThreaded(Chunks[position]);
                chunksGeneratedPassOne++;

            }

        }

        public static void UpdatePassTwoQueue()
        {

            int chunksGeneratedPassTwo = 0;

            while (GenTwoQueue.Count > 0 && chunksGeneratedPassTwo <= MaximumChunksPerTick)
            {

                Vector3i position = GenTwoQueue.Dequeue();
                ChunkBuilder.GeneratePassTwoThreaded(Chunks[position], Chunks);
                chunksGeneratedPassTwo++;

            }

        }

        public static void UpdateMeshQueue()
        {

            int chunksMeshed = 0;

            while (MeshQueue.Count > 0 && chunksMeshed <= MaximumChunksPerTick)
            {

                Vector3i position = MeshQueue.Dequeue();
                ChunkBuilder.MeshThreaded(Chunks[position], Chunks);
                chunksMeshed++;

            }

        }

        public static void UpdateCallQueue()
        {

            int chunksCalled = 0; 

            while (CallQueue.Count > 0 && chunksCalled <= MaximumChunksPerTick)
            {

                Vector3i position = CallQueue.Dequeue();

                ChunkBuilder.CallOpenGL(Chunks[position]);
                chunksCalled++;

            }

        }

        public static void DebugReset()
        {

            Chunks.Clear();

            GenOneQueue.Clear();
            GenTwoQueue.Clear();
            MeshQueue.Clear();
            CallQueue.Clear();

            ChunksToGeneratePassOne = ChunkUtils.GenerateRingsOfColumns(0, Radius + 2);
            ChunksToGeneratePassTwo = ChunkUtils.GenerateRingsOfColumns(0, Radius + 1);
            ChunksToMesh = ChunkUtils.GenerateRingsOfColumns(0, Radius);

            CurrentPassOneRadius = 0;
            CurrentPassTwoRadius = 0;
            CurrentMeshRadius = 0;

            AmtChunksGenPassOne = 0;
            AmtChunksGenPassTwo = 0;
            AmtChunksMeshed = 0;

            PassOnePadding = 0;
            PassTwoPadding = 0;

            CurrentRadius = 0;

            DoGenerationUpdateTicking = true;

        }

        public static void ResetQueues()
        {

            GenOneQueue.Clear();
            GenTwoQueue.Clear();
            MeshQueue.Clear();
            CallQueue.Clear();

        }

        public static void Gen()
        {

            int amtGenOne = 0;
            int amtGenTwo = 0;
            int amtMeshed = 0;

            UpdateChunkQueue();

            if (CurrentPassOneRadius <= Radius + 2)
            {

                for (int i = 0; i < ChunksToGeneratePassOne.Length; i++)
                {

                    if (!Chunks.ContainsKey(ChunksToGeneratePassOne[i]))
                    {

                        Chunks.Add(ChunksToGeneratePassOne[i], new Chunk(ChunksToGeneratePassOne[i]));
                        GenOneQueue.Enqueue(ChunksToGeneratePassOne[i]);

                    }

                }

                for (int i = 0; i < ChunksToGeneratePassOne.Length; i++)
                {

                    if (Chunks[ChunksToGeneratePassOne[i]].GenerationState == GenerationState.PassOne)
                    {

                        amtGenOne++;

                    }

                }

                if (amtGenOne >= ChunksToGeneratePassOne.Length)
                {

                    Console.WriteLine("inc p1");

                    CurrentPassOneRadius++;
                    ChunksToGeneratePassOne = ChunkUtils.GenerateRingsOfColumns(CurrentPassOneRadius, Radius + 2);

                }

            }
            UpdatePassOneQueue();

            if (CurrentPassOneRadius - CurrentPassTwoRadius > 1)
            {

                for (int i = 0; i < ChunksToGeneratePassTwo.Length; i++)
                {

                    if (Chunks[ChunksToGeneratePassTwo[i]].GenerationState == GenerationState.PassOne && Chunks[ChunksToGeneratePassTwo[i]].QueueMode == QueueMode.NotQueued && AreNeighborsPassOne(ChunksToGeneratePassTwo[i]))
                    {

                        Chunks[ChunksToGeneratePassTwo[i]].QueueMode = QueueMode.Queued;
                        GenTwoQueue.Enqueue(ChunksToGeneratePassTwo[i]);

                    }

                }

                for (int i = 0; i < ChunksToGeneratePassTwo.Length; i++)
                {

                    if (Chunks[ChunksToGeneratePassTwo[i]].GenerationState == GenerationState.Generated)
                    {

                        amtGenTwo++;

                    }

                }

                if (amtGenTwo >= ChunksToGeneratePassTwo.Length)
                {

                    Console.WriteLine("inc p2");

                    CurrentPassTwoRadius++;
                    ChunksToGeneratePassTwo = ChunkUtils.GenerateRingsOfColumns(CurrentPassTwoRadius, Radius+1);

                }

            }
            UpdatePassTwoQueue();

            if (CurrentPassTwoRadius - CurrentMeshRadius > 1)
            {

                for (int i = 0; i < ChunksToMesh.Length; i++)
                {

                    if (Chunks[ChunksToMesh[i]].GenerationState == GenerationState.Generated && Chunks[ChunksToMesh[i]].MeshState == MeshState.NotMeshed && Chunks[ChunksToMesh[i]].QueueMode == QueueMode.NotQueued && AreNeighborsGenerated(ChunksToMesh[i]))
                    {

                        Chunks[ChunksToMesh[i]].QueueMode = QueueMode.Queued;
                        MeshQueue.Enqueue(ChunksToMesh[i]);

                    }

                }

                for (int i = 0; i < ChunksToMesh.Length; i++)
                {

                    if (Chunks[ChunksToMesh[i]].MeshState == MeshState.Meshed)
                    {

                        if (Chunks[ChunksToMesh[i]].ChunkState == ChunkState.NotReady)
                        {

                            ChunkBuilder.CallOpenGL(Chunks[ChunksToMesh[i]]);

                        }

                        amtMeshed++;

                    }

                }

                if (amtMeshed >= ChunksToMesh.Length)
                {

                    Console.WriteLine("inc mesh radius");
                    CurrentMeshRadius++;
                    ChunksToMesh = ChunkUtils.GenerateRingsOfColumns(CurrentMeshRadius, Radius);

                }

            }
            UpdateMeshQueue();

        }

        static Vector3i PreviousPlayerChunkPosition = Vector3i.Zero;

        static int AmtChunksGenPassOne = 0;
        static int AmtChunksGenPassTwo = 0;
        static int AmtChunksMeshed = 0;
        public static void Generate(Vector3 cameraPosition)
        {

            UpdateCallQueue();
            UpdateMeshQueue();
            UpdatePassTwoQueue();
            UpdatePassOneQueue();
            UpdateChunkQueue();
            // Console.WriteLine($"gen one queue: {GenOneQueue.Count}, gen two queue: {GenTwoQueue.Count}, mesh queue: {MeshQueue.Count}");

            int distance = Maths.ChebyshevDistance3D(PreviousPlayerChunkPosition, ChunkUtils.PositionToChunk(cameraPosition));

            if (distance >= 3)
            {

                // PreviousPlayerChunkPosition = ChunkUtils.PositionToChunk(cameraPosition);

            }

            int amtChunksGenPassOne = 0;
            int amtChunksGenPassTwo = 0;
            int amtChunksMeshed = 0;

            if (CurrentPassOneRadius <= Radius + 2)
            {

                for (int i = 0; i < ChunksToGeneratePassOne.Length; ++i)
                {

                    if (!Chunks.ContainsKey(PreviousPlayerChunkPosition + ChunksToGeneratePassOne[i]))
                    {

                        Chunks.Add(PreviousPlayerChunkPosition + ChunksToGeneratePassOne[i], new Chunk(PreviousPlayerChunkPosition + ChunksToGeneratePassOne[i]));

                        if (Chunks[PreviousPlayerChunkPosition + ChunksToGeneratePassOne[i]].GenerationState == GenerationState.NotGenerated && Chunks[PreviousPlayerChunkPosition + ChunksToGeneratePassOne[i]].QueueMode == QueueMode.NotQueued)
                        {

                            Chunks[PreviousPlayerChunkPosition + ChunksToGeneratePassOne[i]].QueueMode = QueueMode.Queued;
                            GenOneQueue.Enqueue(PreviousPlayerChunkPosition + ChunksToGeneratePassOne[i]);

                        }

                    }
                    else
                    {

                        if (Chunks[PreviousPlayerChunkPosition + ChunksToGeneratePassOne[i]].GenerationState == GenerationState.PassOne)
                        {

                            amtChunksGenPassOne++;

                        }

                    }

                }

                // Console.WriteLine($"Amount chunks gen pass one: {amtChunksGenPassOne}");

                if (amtChunksGenPassOne >= ChunksToGeneratePassOne.Length)
                {

                    CurrentPassOneRadius++;
                    ChunksToGeneratePassOne = ChunkUtils.GenerateRingsOfColumns(CurrentPassOneRadius, Radius + 2);
                    Console.WriteLine($"Pass one radius: {CurrentPassOneRadius}");
                    // amtChunksGenPassOne = 0;

                }

            }

            if (CurrentPassTwoRadius <= Radius + 1)
            {

                for (int i = 0; i < ChunksToGeneratePassTwo.Length; i++)
                {

                    if (Chunks.ContainsKey(PreviousPlayerChunkPosition + ChunksToGeneratePassTwo[i]))
                    {

                        if (Chunks[PreviousPlayerChunkPosition + ChunksToGeneratePassTwo[i]].GenerationState == GenerationState.Generated)
                        {

                            amtChunksGenPassTwo++;

                        }

                        if (Chunks[PreviousPlayerChunkPosition + ChunksToGeneratePassTwo[i]].GenerationState == GenerationState.PassOne && AreNeighborsPassOne(PreviousPlayerChunkPosition + ChunksToGeneratePassTwo[i]) && Chunks[PreviousPlayerChunkPosition + ChunksToGeneratePassTwo[i]].QueueMode == QueueMode.NotQueued)
                        {

                            Chunks[PreviousPlayerChunkPosition + ChunksToGeneratePassTwo[i]].QueueMode = QueueMode.Queued;
                            GenTwoQueue.Enqueue(PreviousPlayerChunkPosition + ChunksToGeneratePassTwo[i]);

                        }

                    }

                }

                // Console.WriteLine($"Amount chunks gen pass two: {amtChunksGenPassTwo}");

                if (amtChunksGenPassTwo >= ChunksToGeneratePassTwo.Length)
                {

                    CurrentPassTwoRadius++;
                    ChunksToGeneratePassTwo = ChunkUtils.GenerateRingsOfColumns(CurrentPassTwoRadius, Radius + 1);
                    Console.WriteLine($"Pass two radius: {CurrentPassTwoRadius}");
                    // amtChunksGenPassTwo = 0;

                }

            }

            if (CurrentMeshRadius <= Radius)
            {

                for (int i = 0; i < ChunksToMesh.Length; i++)
                {

                    if (Chunks.ContainsKey(PreviousPlayerChunkPosition + ChunksToMesh[i]))
                    {

                        if (Chunks[PreviousPlayerChunkPosition + ChunksToMesh[i]].GenerationState == GenerationState.Generated && Chunks[PreviousPlayerChunkPosition + ChunksToMesh[i]].MeshState == MeshState.NotMeshed && AreNeighborsGenerated(PreviousPlayerChunkPosition + ChunksToMesh[i]) && Chunks[PreviousPlayerChunkPosition + ChunksToMesh[i]].QueueMode == QueueMode.NotQueued)
                        {

                            // Console.WriteLine("yes");

                            Chunks[PreviousPlayerChunkPosition + ChunksToMesh[i]].QueueMode = QueueMode.Queued;
                            MeshQueue.Enqueue(PreviousPlayerChunkPosition + ChunksToMesh[i]);

                        }

                        if (Chunks[PreviousPlayerChunkPosition + ChunksToMesh[i]].MeshState == MeshState.Meshed && Chunks[PreviousPlayerChunkPosition + ChunksToMesh[i]].QueueMode == QueueMode.NotQueued)
                        {

                            Chunks[PreviousPlayerChunkPosition + ChunksToMesh[i]].QueueMode = QueueMode.Queued;
                            CallQueue.Enqueue(PreviousPlayerChunkPosition + ChunksToMesh[i]);
                            // ChunkBuilder.CallOpenGL(Chunks[PreviousPlayerChunkPosition + ChunksToMesh[i]]);

                        }

                        if (Chunks[PreviousPlayerChunkPosition + ChunksToMesh[i]].MeshState == MeshState.Meshed)
                        {

                            amtChunksMeshed++;

                        }

                    }

                }

                // Console.WriteLine($"Amount chunks meshed: {amtChunksMeshed}");

                if (amtChunksMeshed >= ChunksToMesh.Length)
                {

                    CurrentMeshRadius++;
                    ChunksToMesh = ChunkUtils.GenerateRingsOfColumns(CurrentMeshRadius, Radius);
                    Console.WriteLine($"Current mesh radius: {CurrentMeshRadius}");
                    // amtChunksMeshed = 0;

                }

            }

            // UpdateCallQueue();

        }

        public static bool AreNeighborsGenerated(Vector3i chunkPosition)
        {

            Vector3i up = chunkPosition + Vector3i.UnitY;
            Vector3i down = chunkPosition - Vector3i.UnitY;
            Vector3i left = chunkPosition + Vector3i.UnitX;
            Vector3i right = chunkPosition - Vector3i.UnitX;
            Vector3i back = chunkPosition + Vector3i.UnitZ;
            Vector3i front = chunkPosition - Vector3i.UnitZ; 

            int neighborsGenerated = 0;
            if (Chunks.ContainsKey(up) && Chunks[up].GenerationState >= GenerationState.Generated) neighborsGenerated++;
            if (Chunks.ContainsKey(down) && Chunks[down].GenerationState >= GenerationState.Generated) neighborsGenerated++;
            if (Chunks.ContainsKey(left) && Chunks[left].GenerationState >= GenerationState.Generated) neighborsGenerated++;
            if (Chunks.ContainsKey(right) && Chunks[right].GenerationState >= GenerationState.Generated) neighborsGenerated++;
            if (Chunks.ContainsKey(back) && Chunks[back].GenerationState >= GenerationState.Generated) neighborsGenerated++;
            if (Chunks.ContainsKey(front) && Chunks[front].GenerationState >= GenerationState.Generated) neighborsGenerated++;

            return neighborsGenerated == 6;

        }

        public static bool AreNeighborsPassOne(Vector3i chunkPosition)
        {

            Vector3i up = chunkPosition + Vector3i.UnitY;
            Vector3i down = chunkPosition - Vector3i.UnitY;
            Vector3i left = chunkPosition + Vector3i.UnitX;
            Vector3i right = chunkPosition - Vector3i.UnitX;
            Vector3i back = chunkPosition + Vector3i.UnitZ;
            Vector3i front = chunkPosition - Vector3i.UnitZ;

            int neighborsGenerated = 0;

            if (Chunks.ContainsKey(up) && Chunks[up].GenerationState >= GenerationState.PassOne) neighborsGenerated++;
            if (Chunks.ContainsKey(down) && Chunks[down].GenerationState >= GenerationState.PassOne) neighborsGenerated++;
            if (Chunks.ContainsKey(left) && Chunks[left].GenerationState >= GenerationState.PassOne) neighborsGenerated++;
            if (Chunks.ContainsKey(right) && Chunks[right].GenerationState >= GenerationState.PassOne) neighborsGenerated++;
            if (Chunks.ContainsKey(back) && Chunks[back].GenerationState >= GenerationState.PassOne) neighborsGenerated++;
            if (Chunks.ContainsKey(front) && Chunks[front].GenerationState >= GenerationState.PassOne) neighborsGenerated++;

            // Console.WriteLine(neighborsGenerated == 6);

            return neighborsGenerated == 6;

        }

        public static Dictionary<Vector3i, Chunk> GetChunkNeighbors(Chunk chunk)
        {

            Vector3i up = chunk.ChunkPosition + Vector3i.UnitY;
            Vector3i down = chunk.ChunkPosition - Vector3i.UnitY;
            Vector3i left = chunk.ChunkPosition + Vector3i.UnitX;
            Vector3i right = chunk.ChunkPosition - Vector3i.UnitX;
            Vector3i back = chunk.ChunkPosition + Vector3i.UnitZ;
            Vector3i front = chunk.ChunkPosition - Vector3i.UnitZ;

            Dictionary<Vector3i, Chunk> neighbors = new Dictionary<Vector3i, Chunk>
            {

                { Vector3i.UnitY, Chunks[up] },
                { -Vector3i.UnitY, Chunks[down] },
                { Vector3i.UnitX, Chunks[left] },
                { -Vector3i.UnitX, Chunks[right] },
                { Vector3i.UnitZ, Chunks[back] },
                { -Vector3i.UnitZ, Chunks[front] }

            };

            return neighbors;

        }

        static float t;
        static Stopwatch sw;

        public static void LoadChunks(Vector3 position)
        {

            UpdateLoadAndMeshQueue();
            UpdateSaveAndRemoveQueue();

            Vector3i playerChunkPosition = ChunkUtils.PositionToChunk(position);
            int distanceToPreviousPosition = Maths.ChebyshevDistance3D(previousPlayerChunkPosition, playerChunkPosition);

            if (distanceToPreviousPosition > 1)
            {

                CurrentPassOneRadius = 0;
                CurrentPassTwoRadius = 0;
                CurrentMeshRadius = 0;

                previousPlayerChunkPosition = playerChunkPosition;

                ChunksToGeneratePassOne = ChunkUtils.GenerateRingsOfColumns(CurrentPassOneRadius, Radius + 2);
                ChunksToGeneratePassTwo = ChunkUtils.GenerateRingsOfColumns(CurrentPassTwoRadius, Radius + 1);
                ChunksToMesh = ChunkUtils.GenerateRingsOfColumns(CurrentMeshRadius, Radius);

                DoGenerationUpdateTicking = true;

                foreach (Vector3i chunkPosition in Chunks.Keys)
                {

                    if (Maths.ChebyshevDistance3D(chunkPosition, playerChunkPosition) >= Radius)
                    {

                        if (Chunks[chunkPosition].ChunkState == ChunkState.Ready)
                        {

                            if (!SaveAndRemoveQueue.Contains(chunkPosition))
                            {

                                SaveAndRemoveQueue.Add(chunkPosition);
                                Chunks[chunkPosition].ChunkState = ChunkState.SaveAndRemove;

                            }

                        }

                    }

                }

            }

            if (DoGenerationUpdateTicking)
            {

                // UpdateLoadAndMeshQueue();

                // Console.WriteLine($"{CurrentPassOneRadius}, {CurrentPassTwoRadius}, {CurrentMeshRadius}");

                int chunksUpdated = 0;
                if (CurrentMeshRadius >= Radius)
                {

                    DoGenerationUpdateTicking = false;
                    return;

                }

                for (int passOneIndex = 0; passOneIndex < ChunksToGeneratePassOne.Length; passOneIndex++)
                {

                    if (Chunks.ContainsKey(ChunksToGeneratePassOne[passOneIndex] + previousPlayerChunkPosition))
                    {

                        if (Chunks[ChunksToGeneratePassOne[passOneIndex] + previousPlayerChunkPosition].GenerationState == GenerationState.NotGenerated)
                        {

                            ChunkBuilder.GeneratePassOneThreaded(Chunks[ChunksToGeneratePassOne[passOneIndex] + previousPlayerChunkPosition]);
                            chunksUpdated++;

                        }

                    } else
                    {

                        Chunk chunk = new Chunk(ChunksToGeneratePassOne[passOneIndex] + previousPlayerChunkPosition);

                        if (chunk.CheckForFile())
                        {

                            chunk.GenerationState = GenerationState.Generating;
                            LoadAndMeshQueue.Add(ChunksToGeneratePassOne[passOneIndex] + previousPlayerChunkPosition);
                            Chunks.Add(ChunksToGeneratePassOne[passOneIndex] + previousPlayerChunkPosition, chunk);

                        } else
                        {

                            Chunks.Add(ChunksToGeneratePassOne[passOneIndex] + previousPlayerChunkPosition, chunk);

                        }

                        /*
                        Chunks.Add((ChunksToGeneratePassOne[passOneIndex] + previousPlayerChunkPosition), new NewChunk((ChunksToGeneratePassOne[passOneIndex] + previousPlayerChunkPosition)));
                        if (Chunks[ChunksToGeneratePassOne[passOneIndex] + previousPlayerChunkPosition].CheckForFile())
                        {

                            Chunks[ChunksToGeneratePassOne[passOneIndex] + previousPlayerChunkPosition].GenerationState = GenerationState.Generating;
                            LoadAndMeshQueue.Add(ChunksToGeneratePassOne[passOneIndex] + previousPlayerChunkPosition);

                        }
                        

                    }

                    if (chunksUpdated >= MaximumChunksPerTick)
                    {

                        chunksUpdated = 0;
                        break;

                    }

                }

                if (CurrentPassOneRadius < Radius+2)
                {

                    int amountPassOne = 0;
                    for (int i = 0; i < ChunksToGeneratePassOne.Length; i++)
                    {

                        if (Chunks.ContainsKey(ChunksToGeneratePassOne[i] + previousPlayerChunkPosition))
                        {

                            if (Chunks[ChunksToGeneratePassOne[i] + previousPlayerChunkPosition].GenerationState >= GenerationState.PassOne) amountPassOne++;

                        }

                    }
                    if (amountPassOne == ChunksToGeneratePassOne.Length)
                    {

                        Console.WriteLine("inc pass one radius");
                        CurrentPassOneRadius++;
                        ChunksToGeneratePassOne = ChunkUtils.GenerateRingsOfColumns(CurrentPassOneRadius, Radius + 2);

                    }

                }

                for (int passTwoIndex = 0; passTwoIndex < ChunksToGeneratePassTwo.Length; passTwoIndex++)
                {

                    if (Chunks.ContainsKey(ChunksToGeneratePassTwo[passTwoIndex] + previousPlayerChunkPosition))
                    {

                        if (Chunks[ChunksToGeneratePassTwo[passTwoIndex] + previousPlayerChunkPosition].GenerationState == GenerationState.PassOne)
                        {

                            if (AreNeighborsPassOne(ChunksToGeneratePassTwo[passTwoIndex] + previousPlayerChunkPosition))
                            {

                                // ChunkBuilder.GeneratePassTwo(Chunks[ChunksToGeneratePassTwo[passTwoIndex] + previousPlayerChunkPosition], GetChunkNeighbors(Chunks[ChunksToGeneratePassTwo[passTwoIndex] + previousPlayerChunkPosition]));
                                chunksUpdated++;

                            }

                        }

                    }

                    if (chunksUpdated > MaximumChunksPerTick)
                    {

                        chunksUpdated = 0;
                        break;

                    }

                }
                if (CurrentPassTwoRadius < Radius + 1)
                {

                    int amountPassTwo = 0;
                    for (int i = 0; i < ChunksToGeneratePassTwo.Length; i++)
                    {

                        if (Chunks.ContainsKey(ChunksToGeneratePassTwo[i] + previousPlayerChunkPosition))
                        {

                            if (Chunks[ChunksToGeneratePassTwo[i] + previousPlayerChunkPosition].GenerationState >= GenerationState.Generated) amountPassTwo++;

                        }

                    }
                    if (amountPassTwo == ChunksToGeneratePassTwo.Length)
                    {

                        Console.WriteLine("inc pass two radius");
                        CurrentPassTwoRadius++;
                        ChunksToGeneratePassTwo = ChunkUtils.GenerateRingsOfColumns(CurrentPassTwoRadius, Radius + 1);

                    }

                }

                for (int meshIndex = 0; meshIndex < ChunksToMesh.Length; meshIndex++)
                {

                    if (Chunks.ContainsKey(ChunksToMesh[meshIndex] + previousPlayerChunkPosition))
                    {

                        if (Chunks[ChunksToMesh[meshIndex] + previousPlayerChunkPosition].GenerationState == GenerationState.Generated && Chunks[ChunksToMesh[meshIndex] + previousPlayerChunkPosition].MeshState == MeshState.NotMeshed)
                        {

                            if (AreNeighborsGenerated(ChunksToMesh[meshIndex] + previousPlayerChunkPosition))
                            {

                                ChunkBuilder.MeshThreaded(Chunks[ChunksToMesh[meshIndex] + previousPlayerChunkPosition], GetChunkNeighbors(Chunks[ChunksToMesh[meshIndex] + previousPlayerChunkPosition]));

                            }

                        }

                        if (Chunks[ChunksToMesh[meshIndex] + previousPlayerChunkPosition].MeshState == MeshState.Meshed && Chunks[ChunksToMesh[meshIndex] + previousPlayerChunkPosition].ChunkState == ChunkState.NotReady)
                        {

                            ChunkBuilder.CallOpenGL(Chunks[ChunksToMesh[meshIndex] + previousPlayerChunkPosition]);

                        }

                    }

                    if (chunksUpdated > MaximumChunksPerTick)
                    {

                        chunksUpdated = 0;
                        break;

                    }

                }

                if (CurrentMeshRadius < Radius)
                {

                    int amountMeshed = 0;
                    for (int i = 0; i < ChunksToMesh.Length; i++)
                    {

                        if (Chunks.ContainsKey(ChunksToMesh[i] + previousPlayerChunkPosition))
                        {

                            if (Chunks[ChunksToMesh[i] + previousPlayerChunkPosition].MeshState >= MeshState.Meshed) amountMeshed++;

                        }

                    }
                    if (amountMeshed == ChunksToMesh.Length)
                    {

                        Console.WriteLine("inc pass mesh radius");
                        CurrentMeshRadius++;
                        ChunksToMesh = ChunkUtils.GenerateRingsOfColumns(CurrentMeshRadius, Radius);

                    }

                }

            }

        }

        public static void UpdateLoadAndMeshQueue()
        {

            int max = 15 > LoadAndMeshQueue.Count ? LoadAndMeshQueue.Count : 15;

            for (int i = 0; i < max; i++)
            {

                if (LoadAndMeshQueue.Count > 0)
                {

                    if (Chunks[LoadAndMeshQueue.First()].TryLoad())
                    {

                        Chunks[LoadAndMeshQueue.First()].GenerationState = GenerationState.Generated;
                        LoadAndMeshQueue.Remove(LoadAndMeshQueue.First());

                    }

                }

            }

        }

        public static void UpdateSaveAndRemoveQueue()
        {

            int max = 5 > SaveAndRemoveQueue.Count ? SaveAndRemoveQueue.Count : 5;

            for (int i = 0; i < max; i++)
            {

                if (SaveAndRemoveQueue.Count > 0)
                {

                    Chunks[SaveAndRemoveQueue.First()].SaveToFile();
                    Chunks.Remove(SaveAndRemoveQueue.First());
                    SaveAndRemoveQueue.Remove(SaveAndRemoveQueue.First());

                }

            }

        }

        public static void UpdateChunkQueue()
        {

            if (RemeshQueue.Count > 0)
            {

                ChunkBuilder.Remesh(Chunks[RemeshQueue[0]], Chunks);
                RemeshQueue.RemoveAt(0);

            }

        }

        public static void DrawReadyChunks(Vector3 sunVec, Camera camera)
        {
            
            /*
            foreach (Vector3i chunkPosition in ChunksToGeneratePassOne)
            {

               

                    Game.rmodel.SetScale(32, 32, 32);
                    Game.rmodel.Draw(((Vector3)chunkPosition + (0.5f, 0.5f, 0.5f)) * 32, Vector3.Zero, camera, 0);
                    Game.rmodel.SetScale(1, 1, 1);

               

            }

            foreach (Vector3i chunkPosition in ChunksToGeneratePassTwo)
            {



                Game.rmodel.SetScale(32, 32, 32);
                Game.rmodel.Draw(((Vector3)chunkPosition + (0.5f, 0.5f, 0.5f)) * 32, Vector3.Zero, camera, 0);
                Game.rmodel.SetScale(1, 1, 1);



            }

            foreach (Vector3i chunkPosition in ChunksToMesh)
            {



                Game.rmodel.SetScale(32, 32, 32);
                Game.rmodel.Draw(((Vector3)chunkPosition + (0.5f, 0.5f, 0.5f)) * 32, Vector3.Zero, camera, 0);
                Game.rmodel.SetScale(1, 1, 1);



            }
           

            foreach (Vector3i chunkPosition in Chunks.Keys)
            {

                if (Chunks[chunkPosition].ChunkState == ChunkState.Ready && Chunks[chunkPosition].IsExposed && !Chunks[chunkPosition].IsEmpty)// && Chunks[chunkPosition].IsExposed && !Chunks[chunkPosition].IsEmpty)
                {

                    Chunks[chunkPosition].Draw(sunVec, camera);
                    if (Globals.ShouldRenderBounds)
                    {

                        Game.rmodel.SetScale(32, 32, 32);
                        Game.rmodel.Draw(((Vector3)chunkPosition + (0.5f, 0.5f, 0.5f)) * 32, Vector3.Zero, camera, 0);
                        Game.rmodel.SetScale(1, 1, 1);

                    }

                }

            }

        }

        */

    }

}
