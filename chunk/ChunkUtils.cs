﻿using OpenTK.Mathematics;
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
        public static Vector3 getPlayerPositionRelativeToChunk(Vector3 position)
        {

            float x = Math.Max(0, (float)Math.Floor(position.X + 0.5f));
            float y = Math.Max(0, (float)Math.Floor(position.Y + 0.5f));
            float z = Math.Max(0, (float)Math.Floor(position.Z + 0.5f));

            x = Math.Min(Constants.ChunkSize - 1, x);
            y = Math.Min(Constants.ChunkSize - 1, y);
            z = Math.Min(Constants.ChunkSize - 1, z);

            return new Vector3(x, y, z);

        }

    }
}