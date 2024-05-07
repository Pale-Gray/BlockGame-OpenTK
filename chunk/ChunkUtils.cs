using OpenTK.Mathematics;
using opentk_proj.util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj.chunk
{
    internal class ChunkUtils
    {

        public static Vector3 WorldPositionToChunkPosition(Vector3 position)
        {

            int x = (int) Math.Floor(position.X / Globals.ChunkSize);
            int y = (int)Math.Floor(position.Y / Globals.ChunkSize);
            int z = (int)Math.Floor(position.Z / Globals.ChunkSize);

            return (x, y, z);

        }













        public static Vector3 getPlayerPositionRelativeToChunk(Vector3 position)
        {

            float x = Math.Max(0, (float)Math.Floor(position.X + 0.5f));
            float y = Math.Max(0, (float)Math.Floor(position.Y + 0.5f));
            float z = Math.Max(0, (float)Math.Floor(position.Z + 0.5f));

            x = Math.Min(Globals.ChunkSize - 1, x);
            y = Math.Min(Globals.ChunkSize - 1, y);
            z = Math.Min(Globals.ChunkSize - 1, z);

            return new Vector3(x, y, z);

        }

        public static Vector3 GetPositionRelativeToChunkPosition(Camera camera)
        {

            int x = (int)Math.Floor(camera.position.X / 32);
            int y = (int)Math.Floor(camera.position.Y / 32);
            int z = (int)Math.Floor(camera.position.Z / 32);

            return (x, y, z);

        }

    }
}
