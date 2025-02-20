using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Util
{
    public class VectorMath
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

        public static Vector2 CosLerp(Vector2 a, Vector2 b, float t)
        {

            return (Maths.CosLerp(a.X, b.X, t), Maths.CosLerp(a.Y, b.Y, t));

        }

        public static Vector2 EaseOutCubic(Vector2 a, Vector2 b, float t)
        {

            t = Maths.EaseOutCubic(t);
            return Vector2.Lerp(a, b, t);

        }

    }
}
