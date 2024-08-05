using OpenTK.Mathematics;
using System;

namespace Blockgame_OpenTK.Util
{
    internal class Maths
    {

        public static float MapValueToMinMax(float value, float min, float max)
        {

            return ((value) * (max - min)) + min;

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

        public static float ToRadians(float degrees)
        {

            return (degrees * (float)Math.PI) / 180f;

        }

        public static float ToDegrees(float radians)
        {

            return (radians * 180f) / (float)Math.PI;


        }

        public static float Lerp(float p1, float p2, float t)
        {

            switch (t)
            {

                case 1:
                    return p2;
                    break;
                case 0:
                    return p1;
                    break;
                default:
                    return (1 - t) * p1 + t * p2;
                    break;

            }

        }

        public static float Smooth(float v)
        {

            return v * v * v * (v * (v * 6 - 15) + 10);

        }

    }
}
