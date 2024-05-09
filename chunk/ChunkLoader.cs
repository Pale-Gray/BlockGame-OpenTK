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
using System.Xml;

namespace opentk_proj.chunk
{
    internal class ChunkLoader
    {

        public static Dictionary<Vector3, Chunk> ChunkDictionary = new Dictionary<Vector3, Chunk>();

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
        public static void Remove(Chunk chunk)
        {

            ChunkDictionary.Remove(chunk.ChunkPosition);

        }

        public static void RemoveAt(Vector3 position)
        {

            ChunkDictionary.Remove(position);

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

            return GetChunkAtPosition((x,y,z));

        }
        public static Chunk GetChunkFromWorldPosition(Camera camera)
        {

            int x = (int) Math.Floor(camera.position.X / 32);
            int y = (int)Math.Floor(camera.position.Y / 32);
            int z = (int)Math.Floor(camera.position.Z / 32);

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
        static int ChunkRadius = 1;
        static int MaxRadius = 6;
        static int ChunksReady = 0;
        public static void StaggeredGenerate(int amountOfChunksToUpdate, float updateDelayInSeconds, float delta, Camera camera)
        {

            InternalTime += delta;
            // Console.WriteLine("Chunks loaded: {0}, Chunks in dict: {1}", ChunksReady, ChunkDictionary.Count);

            if (InternalTime >= updateDelayInSeconds)
            {

                Vector3 cameraChunkPosition = ChunkUtils.WorldPositionToChunkPosition(camera.position);

                // Console.WriteLine("Current Radius of rendering: {0}", ChunkRadius);

                if (ChunkRadius > MaxRadius)
                {

                    ChunkRadius = 1;

                }

                for (int x = -ChunkRadius + (int)cameraChunkPosition.X; x <= ChunkRadius+(int)cameraChunkPosition.X; x++)
                {

                    for (int y = -ChunkRadius + (int)cameraChunkPosition.Y; y <= ChunkRadius+(int)cameraChunkPosition.Y; y++)
                    {

                        for (int z = -ChunkRadius + (int)cameraChunkPosition.Z; z <= ChunkRadius + (int)cameraChunkPosition.Z; z++)
                        {

                            if (!ChunkDictionary.ContainsKey((x, y, z)))
                            {

                                Append(new Chunk(x, y, z));

                            }
                            if (ChunkDictionary[(x, y, z)].GetChunkState() == ChunkState.NotReady)
                            {

                                // Console.WriteLine(AmountOfChunksUpdated);
                                if (AmountOfChunksUpdated >= amountOfChunksToUpdate)
                                {

                                    AmountOfChunksUpdated = 0;
                                    goto Jump;

                                }
                                ChunkDictionary[(x, y, z)].Generate();
                                AmountOfChunksUpdated++;
                                if (ChunkDictionary[(x,y,z)].GetChunkState() == ChunkState.Ready)
                                {

                                    ChunksReady++;
                                    // Console.WriteLine("Chunks ready: {0}, amount in dict: {1}", ChunksReady, ChunkDictionary.Count);

                                }

                            }

                        }

                    }

                }


            Jump:

                foreach (Chunk c in ChunkDictionary.Values)
                {

                    if (c.ChunkPosition.X < -MaxRadius + cameraChunkPosition.X || c.ChunkPosition.X > MaxRadius + cameraChunkPosition.X ||
                        c.ChunkPosition.Y < -MaxRadius + cameraChunkPosition.Y || c.ChunkPosition.Y > MaxRadius + cameraChunkPosition.Y ||
                        c.ChunkPosition.Z < -MaxRadius + cameraChunkPosition.Z || c.ChunkPosition.Z > MaxRadius + cameraChunkPosition.Z)
                    {

                        Remove(c);

                    }

                }

                if (ChunksReady >= ChunkDictionary.Count)
                {

                    ChunkRadius++;

                }
                // ChunkRadius++;
                InternalTime = 0;

            }

        }

        static int ChunksUpdated = 0;
        public static void StaggeredGenerateThreaded(int amountOfChunksToUpdate, float updateDelayInSeconds, float delta)
        {

            InternalTime += delta;
            if (InternalTime >= updateDelayInSeconds)
            {

                foreach (Chunk c in ChunkDictionary.Values)
                {

                    if (ChunksUpdated >= amountOfChunksToUpdate)
                    {

                        ChunksUpdated = 0;
                        goto End;

                    }

                    // if (c.GetChunkState() == ChunkState.NotReady)
                    // {

                    //     ThreadPool.QueueUserWorkItem(new WaitCallback(c.GenerateThreaded));
                    //     goto End;

                    // }

                    if (c.IsSent == false)
                    {

                        c.UpdateChunk();
                        ChunksUpdated++;

                    }

                }

                End:

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

    }
}
