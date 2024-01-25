using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj.chunk
{
    internal class ChunkLoader
    {

        static Dictionary<int, Chunk> Chunks = new Dictionary<int, Chunk>();

        public static void Append(Chunk chunk)
        {

            // chunks.Add(new int[] {chunk.cx, chunk.cy, chunk.cz }, chunk);
            Chunks.Add(ReturnChunkPositionCombined(chunk.cx, chunk.cy, chunk.cz), chunk);

        }
        public static void GenerateChunksWithinRadius(int radius)
        {

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

        }
        public static void DrawChunksLimited()
        {



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
            
            return Chunks.GetValueOrDefault(ReturnChunkPositionCombined(cx, cy, cz), null);
        }
        private static int ReturnChunkPositionCombined(int cx, int cy, int cz)
        {

            return Convert.ToInt32("" + cx + cy + cz);

        }

    }
}
