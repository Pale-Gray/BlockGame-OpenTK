﻿using System.Security.Cryptography;

using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK.ChunkUtil
{
    internal class Noise
    {

        static float topleft = RandomNumberGenerator.GetInt32(0, 100) / 100f;
        static float topright = RandomNumberGenerator.GetInt32(0, 100) / 100f;
        static float bottomleft = RandomNumberGenerator.GetInt32(0, 100) / 100f;
        static float bottomright = RandomNumberGenerator.GetInt32(0, 100) / 100f;

        public static void GenNewGrid()
        {

            topleft = topright;
            topright = RandomNumberGenerator.GetInt32(0, 100) / 100f;
            bottomleft = bottomright;
            bottomright = RandomNumberGenerator.GetInt32(0, 100) / 100f;

        }
        public static float GetValue2D(float x, float y)
        {

            float interpx1 = Maths.Smooth(Maths.Lerp(topleft, topright, x));
            float interpx2 = Maths.Smooth(Maths.Lerp(bottomleft, bottomright, x));

            return Maths.Lerp(interpx1, interpx2, y);

        }

    }
}
