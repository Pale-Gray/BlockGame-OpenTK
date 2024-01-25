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

            for (int i = 0; i < Chunks.Count; i++)
            {

                if (Maths.Dist3D((float) allChunks[i].cx, (float) allChunks[i].cy, (float) allChunks[i].cz, x, y, z) <= radius)
                {

                    allChunks[i].Draw(shader, camera, time);

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
            
            return Chunks.GetValueOrDefault(ReturnChunkPositionCombined(cx, cy, cz), null);
        }
        private static int ReturnChunkPositionCombined(int cx, int cy, int cz)
        {

            return Convert.ToInt32("" + cx + cy + cz);

        }

    }
}
