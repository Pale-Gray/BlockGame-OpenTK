using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj
{
    internal class Noise
    {

        static float topleft = (float)RandomNumberGenerator.GetInt32(0, 100) / 100f;
        static float topright = (float)RandomNumberGenerator.GetInt32(0, 100) / 100f;
        static float bottomleft = (float)RandomNumberGenerator.GetInt32(0, 100) / 100f;
        static float bottomright = (float)RandomNumberGenerator.GetInt32(0, 100) / 100f;

        public static void GenNewGrid()
        {

            topleft = topright;
            topright = (float)RandomNumberGenerator.GetInt32(0, 100) / 100f;
            bottomleft = bottomright;
            bottomright = (float)RandomNumberGenerator.GetInt32(0, 100) / 100f;

        }
        public static float GetValue2D(float x, float y)
        {

            float interpx1 = Maths.Smooth(Maths.Lerp(topleft, topright, x));
            float interpx2 = Maths.Smooth(Maths.Lerp(bottomleft, bottomright, x));

            return Maths.Lerp(interpx1, interpx2, y);

        }

    }
}
