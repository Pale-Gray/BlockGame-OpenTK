using System;

namespace opentk_proj
{
    internal class Maths
    {

        public static float Dist(float x1, float y1, float x2, float y2)
        {

            return (float) (Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2-y1),2)));

        }
        public static float Dist3D(float x1, float y1, float z1, float x2, float y2, float z2)
        {

            return (float)(Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2) + Math.Pow((z2-z1),2)));

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
