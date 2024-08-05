using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Util
{
    internal class Noise
    {

        public static float Hash(int seed, int a, int b)
        {

            a ^= a >> 16;
            b ^= b >> 16;
            a *= 0x21f0aaad;
            b *= 0x7feb352d;
            a ^= a >> 15;
            b ^= b >> 15;
            a *= 0xd35a2d9;
            b *= 0x846ca68;
            a ^= a >> 15;
            b ^= b >> 15;
            return (float) (a * b) / 2147483648.0f;

        }

        public static double Lerp(double a, double b, double t)
        {

            return (1 - t) * a + t * b;

        }

        public static double Noise2(int seed, float x, float y)
        {

            double topLeft = Hash(seed, (int)Math.Floor(x), (int)Math.Ceiling(y));
            double topRight = Hash(seed, (int)Math.Ceiling(x), (int)Math.Ceiling(y));
            double bottomLeft = Hash(seed, (int)Math.Floor(x), (int)Math.Floor(y));
            double bottomRight = Hash(seed, (int)Math.Ceiling(x), (int)Math.Floor(y));

            double xValue = x % 1f;
            double yValue = y % 1f;

            double topEdge = Lerp(topLeft, topRight, xValue);
            double bottomEdge = Lerp(bottomLeft, bottomRight, xValue);

            return Lerp(topEdge, bottomEdge, yValue);

        }

    }
}
