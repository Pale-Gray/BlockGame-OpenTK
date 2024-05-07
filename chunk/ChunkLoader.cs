using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Threading;
using opentk_proj.util;
using System.Runtime.CompilerServices;

namespace opentk_proj.chunk
{
    internal class ChunkLoader
    {

        static Dictionary<Vector3, Chunk> ChunkDictionary = new Dictionary<Vector3, Chunk>();

        static float elapsedTime = 0;
        static int chunkRadius = 0;
        public static Dictionary<Vector3, Chunk> GetAllChunks()
        {

            return ChunkDictionary;

        }
        public static void Append(Chunk chunk)
        {

            // chunks.Add(new int[] {chunk.cx, chunk.cy, chunk.cz }, chunk);
            // Chunks.Add(ReturnChunkPositionCombined(chunk.cx, chunk.cy, chunk.cz), chunk);
            ChunkDictionary.TryAdd(chunk.ChunkPosition, chunk);
            // Chunks[chunkPosition].QueueGeneration();

        }
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

        public static void SetPriorities(Camera camera)
        {

            

        }
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

            return GetChunkAtPosition(x, y, z);

        }
        public static Chunk GetChunkFromWorldPosition(Camera camera)
        {

            int x = (int) Math.Floor(camera.position.X / 32);
            int y = (int)Math.Floor(camera.position.Y / 32);
            int z = (int)Math.Floor(camera.position.Z / 32);

            return GetChunkAtPosition(x, y, z);

        }

        static float InternalTime = 0;
        static int ChunksChecked = 0;
        public static void StaggeredGenerate(int amountOfChunksToUpdate, float updateDelayInSeconds, float delta, Camera camera)
        {

            InternalTime += delta;
            if (InternalTime >= updateDelayInSeconds)
            {

                // Console.WriteLine("Chunk Generation Tick.");

                for (int i = ChunksChecked; i < ChunksChecked + amountOfChunksToUpdate; i++)
                {

                    try
                    {

                        if (ChunkDictionary.ElementAt(i).Value.GetChunkState() != ChunkState.Ready)
                        {

                            ChunkDictionary.ElementAt(i).Value.Generate();
                            // ChunksChecked++;

                        }

                    } catch
                    {

                        break;

                    }

                }
                if (ChunksChecked >= ChunkDictionary.Count)
                {

                    ChunksChecked = 0;

                } else
                {

                    ChunksChecked += amountOfChunksToUpdate;

                }

                InternalTime = 0;

            }

        }
        public static void DrawAllChunks(Shader shader, Camera camera, float time)
        {

            Chunk[] allChunks = ChunkDictionary.Values.ToArray();
            for (int i = 0; i < allChunks.Length; i++)
            {

                if (allChunks[i].GetChunkState() == ChunkState.Ready && allChunks[i].GetMeshState() == MeshState.Done)
                {

                    allChunks[i].Draw(shader, camera, time);

                }

            }

        }
        public static Chunk GetChunkAtPosition(int cx, int cy, int cz)
        {

            return ChunkDictionary[(cx, cy, cz)];

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

    }
}
