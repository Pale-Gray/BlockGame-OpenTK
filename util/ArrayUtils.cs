using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace opentk_proj.util
{
    internal class ArrayUtils
    {

        public static float[] BlockFaceShift(float[] array, float x, float y, float z)
        {

            float[] newArray = new float[array.Length];

            float[] offsets =
            {

                0, x, y, z, 0, 0, 0, 0, 0,
                0, x, y, z, 0, 0, 0, 0, 0,
                0, x, y, z, 0, 0, 0, 0, 0,
                0, x, y, z, 0, 0, 0, 0, 0,
                0, x, y, z, 0, 0, 0, 0, 0,
                0, x, y, z, 0, 0, 0, 0, 0,

            };

            for (int i = 0; i < array.Length; i++)
            {

                newArray[i] = array[i] + offsets[i];

            }

            return newArray;

        }

    }
}
