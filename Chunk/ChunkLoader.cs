using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using System.Diagnostics;

using Blockgame_OpenTK.Util;
using System.IO;

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
        public static void UpdateLoadingPositions()
        {

            TerrainPositions = ChunkUtils.RingedColumnPadded(LastCameraPosition, ChunkRadius, 1, MaxRadius + 1);
            MeshingPositions = ChunkUtils.RingedColumn(LastCameraPosition, ChunkRadius, MaxRadius);

        }
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
        public static bool ContainsChunk(Vector3 position)
        {


            return ChunkDictionary.ContainsKey(position);

        }

        public static bool ContainsGeneratedChunk(Vector3 position)
        {

            if (ContainsChunk(position) && GetChunk(position).GetGenerationState() == GenerationState.Generated)
            {

                return true;

            }

            return false;

        }
        public static Chunk GetChunk(Vector3 position)
        {

            return ChunkDictionary[position];

        }
        public static void AddChunk(Vector3 position)
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


        static int Radius = 8;
        static int CurrentRadius = 0;
        static Dictionary<Vector3i, NewChunk> Chunks = new Dictionary<Vector3i, NewChunk>();

        public static void Load()
        {

            foreach (Vector3i chunkPosition in Chunks.Keys)
            {

                // Console.WriteLine(chunkPosition);
                if (Chunks[chunkPosition].GetChunkState() != ChunkState.Ready)
                {

                    if (Chunks[chunkPosition].GetGenerationState() != GenerationState.Generated)
                    {

                        ChunkBuilder.GenerateThreaded(Chunks[chunkPosition]);
                        goto End;

                    } else
                    {

                        if (Chunks[chunkPosition].GetMeshState() != MeshState.Meshed)
                        {

                            ChunkBuilder.MeshThreaded(Chunks[chunkPosition]);
                            goto End;

                        } else
                        {

                            ChunkBuilder.CallOpenGL(Chunks[chunkPosition]);

                        }

                    }

                } else
                {

                    if (!Chunks.ContainsKey(chunkPosition + Vector3i.UnitX) && chunkPosition.X <= Radius)
                    {

                        // NewChunk chunk = new NewChunk(chunkPosition + Vector3i.UnitX);
                        // Console.WriteLine(chunk.GetChunkPosition());
                        Chunks.Add(chunkPosition + Vector3i.UnitX, new NewChunk(chunkPosition + Vector3i.UnitX));
                        goto End;

                    }

                }

            }

            End:

            if (!Chunks.ContainsKey(Vector3i.Zero)) // Replace Vector3i.Zero with the current camera position rounded to the chunk position;
            {

                NewChunk chunk = new NewChunk((0,0,0));
                Chunks.Add(chunk.GetChunkPosition(), chunk);

            }

        }

        public static void DrawReadyChunks(Camera camera)
        {

            foreach (Vector3i chunkPosition in Chunks.Keys)
            {

                // Console.WriteLine(chunkPosition);
                if (Chunks[chunkPosition].GetChunkState() == ChunkState.Ready)
                {

                    Chunks[chunkPosition].Draw(camera);

                }

            }

        }

    }
}
