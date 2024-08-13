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

namespace Blockgame_OpenTK.ChunkUtil
{
    internal class ChunkLoader
    {

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

        /*
        public static void GenerateThreaded(int chunksPerUpdate, Vector3 cameraPosition)
        {

            CameraPosition = (Vector3i)ChunkUtils.WorldPositionToChunkPosition(cameraPosition);

            int dist = Maths.ManhattanDistance3D(LastCameraPosition, CameraPosition);

            if (dist > MaxRadius/3f)
            {

                ChunkRadius = ChunkRadius - dist;
                LastCameraPosition = CameraPosition;
                Positions = ChunkUtils.RingedColumnPadded(LastCameraPosition, 0, MaxRadius + 1, MaxRadius + 1);
                ChunksDone.Clear();
                RemoveExcessChunks();

            }

            // Console.WriteLine(PositionsExcludingReadyChunks.Length + ", " + PositionsLeftUnchecked);

            // Console.WriteLine(PositionsExcludingReadyChunks.Length);
            PositionsExcludingReadyChunks = Positions.Except(ChunksDone).ToArray();

            if (PositionsExcludingReadyChunks.Length > PositionsLeftUnchecked)
            {

                for (int i = 0; i < PositionsExcludingReadyChunks.Length; i++)
                {

                    if (ChunksUpdated > chunksPerUpdate) { ChunksUpdated = 0; break; }

                    if (ContainsChunk(PositionsExcludingReadyChunks[i]))
                    {

                        if (GetChunk(PositionsExcludingReadyChunks[i]).GetChunkState() == ChunkState.Ready)
                        {

                            if (!ChunksDone.Contains(PositionsExcludingReadyChunks[i]))
                            {

                                ChunksDone.Add(PositionsExcludingReadyChunks[i]);

                            }

                        }

                        if (GetChunk(PositionsExcludingReadyChunks[i]).GetGenerationState() == GenerationState.NotGenerated)
                        {

                            GetChunk(PositionsExcludingReadyChunks[i]).GenerateTerrainThreaded();
                            ChunksUpdated++;

                        }

                        if (GetChunk(PositionsExcludingReadyChunks[i]).GetGenerationState() == GenerationState.Generated)
                        {

                            if (AllNeighborsGenerated(PositionsExcludingReadyChunks[i]))
                            {

                                if (GetChunk(PositionsExcludingReadyChunks[i]).GetMeshState() == MeshState.NotMeshed)
                                {

                                    GetChunk(PositionsExcludingReadyChunks[i]).GenerateMeshThreaded();
                                    ChunksUpdated++;

                                }

                                if (GetChunk(PositionsExcludingReadyChunks[i]).GetMeshState() == MeshState.Meshed && GetChunk(PositionsExcludingReadyChunks[i]).GetChunkState() == ChunkState.NotReady)
                                {

                                    GetChunk(PositionsExcludingReadyChunks[i]).ProcessToRender();
                                    ChunksUpdated++;

                                }

                            }

                        }

                    }
                    else
                    {

                        AddChunk(PositionsExcludingReadyChunks[i]);
                        ChunksUpdated++;

                    }

                }

            }

        }
        public static void GenerateThreadedFilledColumns(int chunksPerUpdate, Vector3 cameraPosition)
        {

            CameraPosition = (Vector3i)ChunkUtils.WorldPositionToChunkPosition(cameraPosition);

            int dist = Maths.ManhattanDistance3D(LastCameraPosition, CameraPosition);

            if (dist > 4)
            {

                ChunkRadius = ChunkRadius - dist;
                LastCameraPosition = CameraPosition;
                UpdateLoadingPositions();
                RemoveExcessChunks();

            }

            if (ChunkRadius > MaxRadius)
            {

                

            } else
            {

                int ChunksDoneWithTerrain = 0;
                foreach (Vector3 TerrainPosition in TerrainPositions)
                {

                    if (ContainsChunk(TerrainPosition))
                    {

                        if (TerrainChunksUpdated > chunksPerUpdate)
                        {

                            TerrainChunksUpdated = 0;
                            break;

                        }

                        if (GetChunk(TerrainPosition).GetGenerationState() == GenerationState.Generated)
                        {

                            ChunksDoneWithTerrain++;

                        }

                        if (GetChunk(TerrainPosition).GetGenerationState() == GenerationState.NotGenerated)
                        {

                            GetChunk(TerrainPosition).GenerateTerrainThreaded();
                            TerrainChunksUpdated++;

                        }

                    }
                    else
                    {

                        AddChunk(TerrainPosition);
                        TerrainChunksUpdated++;

                    }

                }

                if (ChunksDoneWithTerrain == TerrainPositions.Count())
                {

                    // Console.WriteLine("yes");
                    int ChunksDoneWithMeshing = 0;
                    foreach (Vector3 MeshingPosition in MeshingPositions)
                    {

                        if (MeshChunksUpdated > chunksPerUpdate)
                        {

                            MeshChunksUpdated = 0;
                            break;

                        }

                        if (GetChunk(MeshingPosition).GetChunkState() == ChunkState.Ready)
                        {

                            ChunksDoneWithMeshing++;

                        }

                        if (GetChunk(MeshingPosition).GetMeshState() == MeshState.Meshed && GetChunk(MeshingPosition).GetChunkState() == ChunkState.NotReady)
                        {

                            GetChunk(MeshingPosition).ProcessToRender();
                            MeshChunksUpdated++;

                        }

                        if (GetChunk(MeshingPosition).GetMeshState() == MeshState.NotMeshed)
                        {

                            GetChunk(MeshingPosition).GenerateMeshThreaded();
                            MeshChunksUpdated++;

                        }

                    }

                    if (ChunksDoneWithMeshing == MeshingPositions.Count())
                    {

                        ChunkRadius++;
                        UpdateLoadingPositions();

                    }

                }

            }

        }

        */
        public static void UpdateLoadingPositions()
        {

            TerrainPositions = ChunkUtils.RingedColumnPadded(LastCameraPosition, ChunkRadius, 1, MaxRadius + 1);
            MeshingPositions = ChunkUtils.RingedColumn(LastCameraPosition, ChunkRadius, MaxRadius);

        }

        /*
        public static void GenerateThreadedColumn(int chunksPerUpdate, Vector3 cameraPosition)
        {

            Vector3i CameraPosition = (Vector3i)ChunkUtils.WorldPositionToChunkPosition(cameraPosition);

            // ChunkRadiusDifference = ChunkRadius - 1 - Maths.ManhattanDistance3D(LastCameraPosition, CameraPosition);

            if (LastCameraPosition != CameraPosition)
            {

                LastCameraPosition = CameraPosition;
                ChunkRadius = 0;
                CurrentRing = ChunkUtils.SingleRing(LastCameraPosition, ChunkRadius);

            }

            if (ChunkRadius > MaxRadius)
            {

                ChunkRadius = MaxRadius;

            }

            foreach (Vector3 RingPosition in CurrentRing)
            {

                foreach (Vector3 ColumnPosition in ChunkUtils.Column(RingPosition, MaxRadius)) 
                {

                    if (ChunksUpdated > chunksPerUpdate)
                    {

                        ChunksUpdated = 0;
                        goto OutOfLoop;

                    }

                    if (!ContainsChunk(ColumnPosition))
                    {

                        AddChunk(ColumnPosition);
                        ChunksUpdated++;

                    } else
                    {


                        Console.WriteLine("calling");

                        // if (ChunkDictionary[ColumnPosition].IsSent == false)
                        // {

                        //     Console.WriteLine("calling");
                            // ChunkDictionary[ColumnPosition].UpdateChunk();
                            // ChunkDictionary[ColumnPosition].UpdateChunkThreaded();
                        //     ChunkDictionary[ColumnPosition].UpdateChunk();
                       //     ChunksUpdated++;

                        // }

                    }

                }

            }

            CurrentRing = ChunkUtils.SingleRing(LastCameraPosition, ChunkRadius);
            ChunkRadius++;
        // CurrentRing = ChunkUtils.SingleRing(LastCameraPosition, ChunkRadius);

        OutOfLoop:

            ChunkRadius = ChunkRadius;

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
        */
        /* 
        public static bool AllNeighborsGenerated(Vector3 position)
        {

            if (ContainsChunk(position + (1,0,0)) && ContainsChunk(position - (1,0,0)) &&
                ContainsChunk(position + (0,1,0)) && ContainsChunk(position - (0,1,0)) &&
                ContainsChunk(position + (0,0,1)) && ContainsChunk(position - (0,0,1)))
            {

                if (GetChunk(position + (1,0,0)).GetGenerationState() == GenerationState.Generated && GetChunk(position - (1, 0, 0)).GetGenerationState() == GenerationState.Generated &&
                    GetChunk(position + (0, 1, 0)).GetGenerationState() == GenerationState.Generated && GetChunk(position - (0, 1, 0)).GetGenerationState() == GenerationState.Generated &&
                    GetChunk(position + (0, 0, 1)).GetGenerationState() == GenerationState.Generated && GetChunk(position - (0, 0, 1)).GetGenerationState() == GenerationState.Generated)
                {

                    return true;

                }

            }

            return false;

        }

        */
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

        /*
        public static void DrawAllReadyChunks(Shader shader, Camera camera, float time)
        {

            foreach (Chunk chunk in ChunkDictionary.Values)
            {

                // Console.WriteLine($"{chunk.GetChunkState()}, {chunk.GetMeshState()}, {chunk.GetGenerationState()}");

                if (chunk.ContainsMesh() && chunk.GetChunkState() == ChunkState.Ready)
                {

                    // Console.WriteLine("Drawing");
                    chunk.Draw(shader, camera, time);

                }

            }

        }

        */
        /*
        public static void DrawAllChunks(Shader shader, Camera camera, float time)
        {

            foreach (Chunk c in ChunkDictionary.Values)
            {

                if (c.GetChunkState() == ChunkState.Ready)
                {

                    c.Draw(shader, camera, time);

                }

            }

        }
        */

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
        public static NewChunk GetChunk(Vector3i position)
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


        static int Radius = 5;
        static int CurrentRadius = 0;
        static int CurrentPassOneRadius = 0;
        static int CurrentPassTwoRadius = 0;
        static int CurrentMeshRadius = 0;
        public static Dictionary<Vector3i, NewChunk> Chunks = new Dictionary<Vector3i, NewChunk>();
        static Vector3i[] cposs = ChunkUtils.InitialFloodFill(2, 2);
        static int MaximumChunksPerTick = 25;
        static bool DoGenerationUpdateTicking = true;

        // static Vector3i[] ChunksToGeneratePassOne = ChunkUtils.InitialFloodFill(Radius, CurrentRadius + 2);
        // static Vector3i[] ChunksToGeneratePassTwo = ChunkUtils.InitialFloodFill(Radius, CurrentRadius + 1);
        // static Vector3i[] ChunksToMesh = ChunkUtils.InitialFloodFill(Radius, CurrentRadius);

        // static Vector3i[] ChunksToGeneratePassOne = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, Radius, 2);
        // static Vector3i[] ChunksToGeneratePassTwo = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, Radius, 1);
        // static Vector3i[] ChunksToMesh = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, Radius, 0);
        static Vector3i previousPlayerChunkPosition = Vector3i.Zero;
        static Vector3i[] ChunksToGeneratePassOne = ChunkUtils.GenerateRingsOfColumns(CurrentPassOneRadius, Radius+2);
        static Vector3i[] ChunksToGeneratePassTwo = ChunkUtils.GenerateRingsOfColumns(CurrentPassTwoRadius, Radius+1);
        static Vector3i[] ChunksToMesh = ChunkUtils.GenerateRingsOfColumns(CurrentMeshRadius, Radius);
        public static List<Vector3i> RemeshQueue = new List<Vector3i>();
        public static List<Vector3i> SaveAndRemoveQueue = new List<Vector3i>();
        public static List<Vector3i> LoadAndMeshQueue = new List<Vector3i>();

        public static bool AreNeighborsGenerated(NewChunk chunk)
        {

            Vector3i up = chunk.ChunkPosition + Vector3i.UnitY;
            Vector3i down = chunk.ChunkPosition - Vector3i.UnitY;
            Vector3i left = chunk.ChunkPosition + Vector3i.UnitX;
            Vector3i right = chunk.ChunkPosition - Vector3i.UnitX;
            Vector3i back = chunk.ChunkPosition + Vector3i.UnitZ;
            Vector3i front = chunk.ChunkPosition - Vector3i.UnitZ;

            int neighborsGenerated = 0;
            if (Chunks.ContainsKey(up) && Chunks[up].GenerationState >= GenerationState.Generated) neighborsGenerated++;
            if (Chunks.ContainsKey(down) && Chunks[down].GenerationState >= GenerationState.Generated) neighborsGenerated++;
            if (Chunks.ContainsKey(left) && Chunks[left].GenerationState >= GenerationState.Generated) neighborsGenerated++;
            if (Chunks.ContainsKey(right) && Chunks[right].GenerationState >= GenerationState.Generated) neighborsGenerated++;
            if (Chunks.ContainsKey(back) && Chunks[back].GenerationState >= GenerationState.Generated) neighborsGenerated++;
            if (Chunks.ContainsKey(front) && Chunks[front].GenerationState >= GenerationState.Generated) neighborsGenerated++;

            return neighborsGenerated == 6;

        }

        public static bool AreNeighborsPassOne(NewChunk chunk)
        {

            Vector3i up = chunk.ChunkPosition + Vector3i.UnitY;
            Vector3i down = chunk.ChunkPosition - Vector3i.UnitY;
            Vector3i left = chunk.ChunkPosition + Vector3i.UnitX;
            Vector3i right = chunk.ChunkPosition - Vector3i.UnitX;
            Vector3i back = chunk.ChunkPosition + Vector3i.UnitZ;
            Vector3i front = chunk.ChunkPosition - Vector3i.UnitZ;

            int neighborsGenerated = 0;
            if (Chunks.ContainsKey(up) && Chunks[up].GenerationState >= GenerationState.PassOne) neighborsGenerated++;
            if (Chunks.ContainsKey(down) && Chunks[down].GenerationState >= GenerationState.PassOne) neighborsGenerated++;
            if (Chunks.ContainsKey(left) && Chunks[left].GenerationState >= GenerationState.PassOne) neighborsGenerated++;
            if (Chunks.ContainsKey(right) && Chunks[right].GenerationState >= GenerationState.PassOne) neighborsGenerated++;
            if (Chunks.ContainsKey(back) && Chunks[back].GenerationState >= GenerationState.PassOne) neighborsGenerated++;
            if (Chunks.ContainsKey(front) && Chunks[front].GenerationState >= GenerationState.PassOne) neighborsGenerated++;

            return neighborsGenerated == 6;

        }

        public static Dictionary<Vector3i, NewChunk> GetChunkNeighbors(NewChunk chunk)
        {

            Vector3i up = chunk.ChunkPosition + Vector3i.UnitY;
            Vector3i down = chunk.ChunkPosition - Vector3i.UnitY;
            Vector3i left = chunk.ChunkPosition + Vector3i.UnitX;
            Vector3i right = chunk.ChunkPosition - Vector3i.UnitX;
            Vector3i back = chunk.ChunkPosition + Vector3i.UnitZ;
            Vector3i front = chunk.ChunkPosition - Vector3i.UnitZ;

            Dictionary<Vector3i, NewChunk> neighbors = new Dictionary<Vector3i, NewChunk>
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

                            if (!SaveAndRemoveQueue.Contains(chunkPosition)) SaveAndRemoveQueue.Add(chunkPosition);

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

                        NewChunk chunk = new NewChunk(ChunksToGeneratePassOne[passOneIndex] + previousPlayerChunkPosition);

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
                        */

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

                            if (AreNeighborsPassOne(Chunks[ChunksToGeneratePassTwo[passTwoIndex] + previousPlayerChunkPosition]))
                            {

                                ChunkBuilder.GeneratePassTwo(Chunks[ChunksToGeneratePassTwo[passTwoIndex] + previousPlayerChunkPosition], GetChunkNeighbors(Chunks[ChunksToGeneratePassTwo[passTwoIndex] + previousPlayerChunkPosition]));
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

                            if (AreNeighborsGenerated(Chunks[ChunksToMesh[meshIndex] + previousPlayerChunkPosition]))
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

        public static void Load(Camera camera)
        {

            if (DoGenerationUpdateTicking)
            {

                int chunksUpdated = 0;
                if (sw == null)
                {

                    sw = Stopwatch.StartNew();

                }

                for (int poindex = 0; poindex < ChunksToGeneratePassOne.Length; poindex++)
                {

                    if (!Chunks.ContainsKey(ChunksToGeneratePassOne[poindex]))
                    {

                        Chunks.Add(ChunksToGeneratePassOne[poindex], new NewChunk(ChunksToGeneratePassOne[poindex]));

                    }
                    else
                    {

                        if (Chunks[ChunksToGeneratePassOne[poindex]].GenerationState == GenerationState.NotGenerated)
                        {

                            ChunkBuilder.GeneratePassOneThreaded(Chunks[ChunksToGeneratePassOne[poindex]]);

                        }

                    }

                    if (chunksUpdated > MaximumChunksPerTick)
                    {

                        chunksUpdated = 0;
                        return;

                    }

                }
                // chunksUpdated = 0;

                for (int ptindex = 0; ptindex < ChunksToGeneratePassTwo.Length; ptindex++)
                {

                    if (Chunks[ChunksToGeneratePassTwo[ptindex]].GenerationState == GenerationState.PassOne)
                    {

                        if (AreNeighborsPassOne(Chunks[ChunksToGeneratePassTwo[ptindex]]))
                        {

                            ChunkBuilder.GeneratePassTwo(Chunks[ChunksToGeneratePassTwo[ptindex]], GetChunkNeighbors(Chunks[ChunksToGeneratePassTwo[ptindex]]));
                            chunksUpdated++;

                        }

                    }

                    if (chunksUpdated > MaximumChunksPerTick)
                    {

                        chunksUpdated = 0;
                        return;

                    }

                }
                // chunksUpdated = 0;
                for (int mindex = 0; mindex < ChunksToMesh.Length; mindex++)
                {

                    if (Chunks[ChunksToMesh[mindex]].GenerationState == GenerationState.Generated && Chunks[ChunksToMesh[mindex]].MeshState == MeshState.NotMeshed)
                    {

                        if (AreNeighborsGenerated(Chunks[ChunksToMesh[mindex]]))
                        {

                            ChunkBuilder.MeshThreaded(Chunks[ChunksToMesh[mindex]], GetChunkNeighbors(Chunks[ChunksToMesh[mindex]]));
                            chunksUpdated++;

                        }

                    }

                    if (Chunks[ChunksToMesh[mindex]].MeshState == MeshState.Meshed && Chunks[ChunksToMesh[mindex]].ChunkState == ChunkState.NotReady)
                    {

                        ChunkBuilder.CallOpenGL(Chunks[ChunksToMesh[mindex]]);

                    }

                    if (chunksUpdated > MaximumChunksPerTick)
                    {

                        chunksUpdated = 0;
                        return;

                    }

                }

                int chunksReady = 0;
                int chunksPassOne = 0;
                int chunksPassTwo = 0;
                int chunksGenerated = 0;
                int chunksMeshed = 0;
                for (int i = 0; i < ChunksToMesh.Length; i++)
                {


                    if (Chunks[ChunksToMesh[i]].ChunkState == ChunkState.Ready)
                    {

                        chunksReady++;

                    }

                }

                if (chunksReady == ChunksToMesh.Length)
                {

                    if (CurrentRadius == Radius)
                    {

                        DoGenerationUpdateTicking = false;

                    }

                    CurrentRadius++;
                    ChunksToMesh = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, Radius, 0);
                    ChunksToGeneratePassTwo = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, Radius, 1);
                    ChunksToGeneratePassOne = ChunkUtils.GenerateRingsOfColumnsWithPadding(CurrentRadius, Radius, 2);

                }

            } else
            {

                sw.Stop();
                // Console.WriteLine($"Generated a {Radius} radius, {Math.Pow(Radius + 1 + Radius, 3)}, 32 cubic chunks, in {sw.ElapsedMilliseconds/1000f} seconds, {sw.ElapsedMilliseconds}ms.");

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

            if (SaveAndRemoveQueue.Count > 0)
            {

                Chunks[SaveAndRemoveQueue.First()].SaveToFile();
                Chunks.Remove(SaveAndRemoveQueue.First());
                SaveAndRemoveQueue.Remove(SaveAndRemoveQueue.First());

            }

        }

        public static void UpdateChunkQueue()
        {

            if (RemeshQueue.Count > 0)
            {

                ChunkBuilder.Remesh(Chunks[RemeshQueue[0]], GetChunkNeighbors(Chunks[RemeshQueue[0]]));
                RemeshQueue.RemoveAt(0);

            }

        }

        public static void DrawReadyChunks(Vector3 sunVec, Camera camera)
        {

            foreach (Vector3i chunkPosition in Chunks.Keys)
            {

                if (Chunks[chunkPosition].GetChunkState() == ChunkState.Ready && Chunks[chunkPosition].IsExposed && !Chunks[chunkPosition].IsEmpty)
                {

                    Chunks[chunkPosition].Draw(sunVec, camera);

                }

            }

        }

    }
}
