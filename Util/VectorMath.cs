using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Util
{
    internal class VectorMath
    {

        public static Vector3i Floor(Vector3 vec)
        {

            Vector3 vector3 = vec;
            vector3.X = (float) Math.Floor(vector3.X);
            vector3.Y = (float) Math.Floor(vector3.Y);
            vector3.Z = (float) Math.Floor(vector3.Z);

            return (Vector3i) vector3;

        }

        public static Vector3i Ceil(Vector3 vec)
        {

            Vector3 vector3 = vec;
            vector3.X = (float)Math.Ceiling(vector3.X);
            vector3.Y = (float)Math.Ceiling(vector3.Y);
            vector3.Z = (float)Math.Ceiling(vector3.Z);

            return (Vector3i)vector3;

        }

    }
}
