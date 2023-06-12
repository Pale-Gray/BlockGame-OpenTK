using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
