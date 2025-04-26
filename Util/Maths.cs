using OpenTK.Mathematics;
using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Game.Util
{
    public class Maths
    {
        
        public static bool TryStringHexColorToColor4(string hexColor, out Color4<Rgba> color)
        {

            try
            {
                string hexBytes = hexColor.Substring(2, 8);

                if (byte.TryParse(hexBytes.Substring(0, 2), NumberStyles.HexNumber, null, out byte r) &&
                                                byte.TryParse(hexBytes.Substring(2, 2), NumberStyles.HexNumber, null, out byte g) &&
                                                byte.TryParse(hexBytes.Substring(4, 2), NumberStyles.HexNumber, null, out byte b) &&
                                                byte.TryParse(hexBytes.Substring(6, 2), NumberStyles.HexNumber, null, out byte a))
                {

                    color = new Color4<Rgba>(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
                    return true;

                }

                color = Color4.Black;
                return false;
            } catch
            {
                color = Color4.Black;
                return false;
            }

        }

        public static float Rand(int seed, int a, int b)
        {

            uint bitcastSeed = Unsafe.BitCast<int, uint>(seed);
            uint bitcastA = Unsafe.BitCast<int, uint>(a);
            uint bitcastB = Unsafe.BitCast<int, uint>(b);

            uint value = bitcastSeed << 15 | bitcastA >> 5 | bitcastB << 2;
            value ^= bitcastA >> 15;
            value = value << 4;
            value |= bitcastB >> 40;
            value ^= bitcastA & bitcastB;
            value ^= bitcastB << 15 | bitcastA >> 27;
            value = value << 14;

            return unchecked(value / (float)uint.MaxValue);

        }

        public static Color4<Rgba> Mix(Color4<Rgba> a, Color4<Rgba> b, float mixValue)
        {

            return new Color4<Rgba>(Lerp(a.X, b.X, mixValue), Lerp(a.Y, b.Y, mixValue), Lerp(a.Z, b.Z, mixValue), Lerp(a.W, b.W, mixValue));

        }
        public static int VecToIndex(int x, int z, int size)
        {

            return x + (z * size);

        }
        public static int VecToIndex(int x, int y, int z, int size)
        {

            return x + (y * size) + (z * size * size);

        }

        public static int VecToIndex(int x, int y, int z, int sx, int sy)
        {

            return x + (y * sx) + (z * sx * sy);

        }
        public static uint Random2(uint seed, int x, int y)
        {

            uint a = Unsafe.As<int, uint>(ref x);
            uint b = Unsafe.As<int, uint>(ref y);

            if (seed == 0) seed = 12451233 ^ 234881893 >> 34888199 ^ 234551233 << 334888 << 12223 << 112333 ^ 1199672934;
            if (a == 0) a = 1 << 48129 >> 45891234 << 23411111 ^ 100944999;
            if (b == 0) b = 1 << 1200052 >> 192984591 ^ 2399921;

            a = a ^ (a << 16);
            b = b ^ (b << 16);
            a = a ^ (a >> 14);
            b = b ^ (b >> 14);
            a = a ^ (a << 5);
            b = b ^ (b >> 5);

            return unchecked(seed*a*b);

        }

        public static int Mod(float a, float n) => (int) (a - n * Math.Floor(a / Math.Abs(n)));
        public static float FloatRandom2(uint seed, int x, int y) => Random2(seed, x, y) / (float)uint.MaxValue;
        public static float Noise2(uint seed, float x, float y)
        {

            float bottomLeft = FloatRandom2(seed, (int)Math.Floor(x), (int)Math.Floor(y));
            float bottomRight = FloatRandom2(seed, (int)Math.Floor(x) + 1, (int)Math.Floor(y));
            float topLeft = FloatRandom2(seed, (int)Math.Floor(x), (int)Math.Floor(y) + 1);
            float topRight = FloatRandom2(seed, (int)Math.Floor(x) + 1, (int)Math.Floor(y) + 1);

            float xPart = x % 1;
            float yPart = y % 1;
            if (xPart < 0) xPart += 1;
            if (yPart < 0) yPart += 1;

            float topLerp = Slerp(topLeft, topRight, xPart);
            float bottomLerp = Slerp(bottomLeft, bottomRight, xPart);

            return Slerp(bottomLerp, topLerp, yPart);

        }

        public static float ValueNoise2Octaves(uint seed, float x, float y, int octaves)
        {

            float val = 0;

            for (float i = 1; i <= octaves; i++)
            {

                val += Noise2(seed, x*i, y*i) / (i);

            }

            // val /= (octaves);

            return val;

        }

        // from https://easings.net/#easeOutCubic
        // transforms t value into eased value.
        public static float EaseOutCubic(float t)
        {   

            return (float)(1.0 - Math.Pow(1.0 - t, 3));

        }

        public static float Slerp(float a, float b, float t)
        {

            t = t * t * t * (10f + t * (-15f + t * 6f));

            return Lerp(a, b, t);

        }

        public static float CosLerp(float a, float b, float t)
        {

            double t2 = (1 - Math.Cos(t * Math.PI)) / 2.0;
            return (a * (1 - (float)t2) + b * (float)t2);

        }

        public static double DoubleLerp(double a, double b, double t)
        {

            return (1 - t) * b + t * b;

        }

        public static double DoubleCosLerp(double a, double b, double t)
        {

            double t2 = (1 - Math.Cos(t * Math.PI))/2.0;
            return (a * (1 - t2) + b * t2);

        }

        public static float MapValueToMinMax(float value, float min, float max)
        {

            return ((value) * (max - min)) + min;

        }

        public static int ChebyshevDistance2D(Vector2i pointA, Vector2i pointB)
        {

            return Math.Max(Math.Abs(pointB.X - pointA.X), Math.Abs(pointB.Y - pointA.Y));

        }

        public static int ChebyshevDistance3D(Vector3 pointA, Vector3 pointB)
        {

            return (int)(Math.Max(Math.Abs(pointB.X - pointA.X), Math.Max(Math.Abs(pointB.Y - pointA.Y), Math.Abs(pointB.Z - pointA.Z))));

        }

        public static int ManhattanDistance3D(Vector3 pointA, Vector3 pointB)
        {

            return (int)Math.Abs(pointA.X - pointB.X) + (int)Math.Abs(pointA.Y - pointB.Y) + (int)Math.Abs(pointA.Z - pointB.Z);

        }
        public static float Dist(float x1, float y1, float x2, float y2)
        {

            return (float) (Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2-y1),2)));

        }
        public static float Dist3D(float x1, float y1, float z1, float x2, float y2, float z2)
        {

            return (float)(Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2) + Math.Pow((z2-z1),2)));

        }

        public static float Dist3D(Vector3 pos1, Vector3 pos2)
        {

            return (float)(Math.Sqrt(Math.Pow((pos2.X - pos1.X), 2) + Math.Pow((pos2.Y - pos1.Y), 2) + Math.Pow((pos2.Z - pos1.Z), 2)));

        }

        public static Vector3 ToRadians(Vector3 degrees)
        {

            return (ToRadians(degrees.X), ToRadians(degrees.Y), ToRadians(degrees.Z));

        }
        public static float ToRadians(float degrees)
        {

            return (degrees * (float)Math.PI) / 180f;

        }

        public static float ToDegrees(float radians)
        {

            return (radians * 180f) / (float)Math.PI;


        }

        public static float Lerp(float a, float b, float t)
        {

            return (a * (1-t) + b*t);

        }

        public static float Smooth(float v)
        {

            return v * v * v * (v * (v * 6 - 15) + 10);

        }

    }
}
