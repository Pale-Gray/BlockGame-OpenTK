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

namespace opentk_proj.chunk
{
    internal class ChunkLoader
    {

        static Dictionary<String, Chunk> Chunks = new Dictionary<String, Chunk>();
        static float elapsedTime = 0;
        static 
        public Dictionary<String, Chunk> GetAllChunks()
        {

            return Chunks;

        }
        public static void Append(Chunk chunk)
        {

            // chunks.Add(new int[] {chunk.cx, chunk.cy, chunk.cz }, chunk);
            // Chunks.Add(ReturnChunkPositionCombined(chunk.cx, chunk.cy, chunk.cz), chunk);
            Chunks.TryAdd(ReturnChunkPositionCombined(chunk.cx, chunk.cy, chunk.cz), chunk);

        }
        public static void RemoveChunk(string key)
        {

            Chunks.Remove(key);

        }

        public static void LoadFromSaveFile(string pathToSaveFile)
        {

            if (!File.Exists(pathToSaveFile))
            {

                GenerateChunksWithinRadius(8);

            } else
            {

                // load from file

            }

        }
        public static void GenerateChunksWithinRadius(int radius)
        {

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

            elapsed.Stop();
            TimeSpan elapsedtime = elapsed.Elapsed;
            Console.WriteLine("Finished generating " + radius * radius * radius + " chunks in " + Math.Round(elapsedtime.TotalSeconds, 2) + " seconds.");

        }
        public static Chunk GetChunkFromPosition(Vector3 position)
        {

            int x = (int)Math.Floor(position.X / 32);
            int y = (int)Math.Floor(position.Y / 32);
            int z = (int)Math.Floor(position.Z / 32);

            return GetChunkAtPosition(x, y, z);

        }
        public static Chunk GetChunkFromPosition(Camera camera)
        {

            int x = (int) Math.Floor(camera.position.X / 32);
            int y = (int)Math.Floor(camera.position.Y / 32);
            int z = (int)Math.Floor(camera.position.Z / 32);

            return GetChunkAtPosition(x, y, z);

        }
        public static void DrawChunksLimited(float radius, Shader shader, Camera camera, float time)
        {

            Chunk[] allChunks = Chunks.Values.ToArray();

            float x = (float)Math.Floor(camera.position.X / 32);
            float y = (float)Math.Floor(camera.position.Y / 32);
            float z = (float)Math.Floor(camera.position.Z / 32);

            /* for (int i = 0; i < Chunks.Count; i++)
            {

                if (Maths.Dist3D((float) allChunks[i].cx, (float) allChunks[i].cy, (float) allChunks[i].cz, x, y, z) <= radius)
                {

                    allChunks[i].Draw(shader, camera, time);

                }

            } */

            for (int cx = 0; cx < radius; cx++) {

                for (int cy = 0; cy < radius; cy++)
                {

                    for (int cz = 0; cz < radius; cz++)
                    {

                        Vector3 cameraposition = ChunkUtils.getPlayerPositionRelativeToChunk(camera.position);

                        int px = (int) (cameraposition.X - radius/2);
                        int py = (int) (cameraposition.Y - radius/2);
                        int pz = (int) (cameraposition.Z - radius/2);

                        try
                        {
                            Chunks[ReturnChunkPositionCombined(px + cx, py + cy, pz + cz)].Draw(shader, camera, time);
                        }
                        catch { }
                    }

                }

            }


        }

        public static void DrawChunksSmarter(int timeoutseconds, float radius, Shader shader, Camera camera, float time, float deltaTime)
        {

            elapsedTime += deltaTime;

            
            if (elapsedTime > timeoutseconds)
            {

                /* for (int i = 0; i < Chunks.Count; i++)
                {

                    string key = Chunks.Keys.ToArray()[i];

                    Chunks[key].Save("../../../res/cdat/" + key + ".cdat");

                } */

                for (int cx = 0; cx < radius; cx++)
                {

                    for (int cy = 0; cy < radius; cy++)
                    {

                        for (int cz = 0; cz < radius; cz++)
                        {

                            int startX = (int)(ChunkUtils.GetPositionRelativeToChunkPosition(camera).X - radius/2);
                            int startY = (int)(ChunkUtils.GetPositionRelativeToChunkPosition(camera).Y - radius/2);
                            int startZ = (int)(ChunkUtils.GetPositionRelativeToChunkPosition(camera).Z - radius/2);
                            
                            if (Chunks.ContainsKey(ReturnChunkPositionCombined(startX + cx, startY + cy, startZ + cz)))
                            {

                                break;

                            } else
                            {

                                Append(new Chunk("../../../res/cdat/" + ReturnChunkPositionCombined(startX + cx, startY + cy, startZ + cz) + ".cdat"));

                            }


                            /* if (!File.Exists("../../../res/cdat/" + ReturnChunkPositionCombined(startX + cx, startY + cy, startZ + cz) + ".cdat"))
                            {

                                Append(new Chunk(startX + cx, startY + cy, startZ + cz));

                            } else
                            {


                                Append(new Chunk("../../../res/cdat/" + ReturnChunkPositionCombined(startX + cx, startY + cy, startZ + cz) + ".cdat"));

                            } */

                        }

                    }

                }
                elapsedTime = 0;

            }

        }
        public static void DrawChunksSmart(float radius, Shader shader, Camera camera, float time)
        {



            Chunk[] allChunks = Chunks.Values.ToArray();

            float x = (float)Math.Floor(camera.position.X / 32);
            float y = (float)Math.Floor(camera.position.Y / 32);
            float z = (float)Math.Floor(camera.position.Z / 32);

            for (int ccx = 0; ccx < radius; ccx++)
            {

                for (int ccy = 0; ccy < radius; ccy++)
                {

                    for (int ccz = 0;  ccz < radius; ccz++)
                    {

                        if (Maths.Dist3D(Chunks[ReturnChunkPositionCombined(ccx, ccy, ccz)].cx, Chunks[ReturnChunkPositionCombined(ccx, ccy, ccz)].cy, Chunks[ReturnChunkPositionCombined(ccx, ccy, ccz)].cz, x, y, z) <= radius)
                        {

                            Chunks[ReturnChunkPositionCombined(ccx, ccy, ccz)].Draw(shader, camera, time);

                        }

                    }

                }


            }

        }
        public static void DrawChunks(Shader shader, Camera camera, float time)
        {

            Chunk[] allChunks = Chunks.Values.ToArray();

            for (int i = 0; i < Chunks.Count; i++)
            {

                allChunks[i].Draw(shader, camera, time);

            }

        }
        public static Chunk GetChunkAtPosition(int cx, int cy, int cz)
        {

            try
            {
                return Chunks.GetValueOrDefault(ReturnChunkPositionCombined(cx, cy, cz), null);
            }
            catch { return null; }

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
