using OpenTK.Mathematics;
using opentk_proj.block;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj.util
{
    internal class Ray
    {

        public Vector3 Ray_Position = new Vector3(0, 0, 0);
        public Vector3 Ray_Direction = new Vector3(0, 0, 0);

        public Vector3 rpos;
        public Ray(Vector3 position, Vector3 direction) {

            Ray_Position = position;
            Ray_Direction = direction;

        }
        public Ray(float posx, float posy, float posz, float dirx, float diry, float dirz)
        {

            Ray_Position = new Vector3(posx, posy, posz);
            Ray_Direction = new Vector3(dirx, diry, dirz);

        }

        public void Update(Vector3 position, Vector3 direction)
        {

            Ray_Position = position;
            Ray_Direction = direction;

        }
        public void Update(float posx, float posy, float posz, float dirx, float diry, float dirz)
        {

            Ray_Position = new Vector3(posx, posy, posz);
            Ray_Direction = new Vector3(dirx, diry, dirz);

        }
        public bool Hit_Imperformant(int[,,] map, float length, float step_size)
        {

            for (float i = 0; i < length; i += step_size)
            {

                rpos = Ray_Position + Ray_Direction;
                if (rpos.X > map.GetLength(0) || rpos.Y > map.GetLength(1) || rpos.Z > map.GetLength(2) || rpos.X < 0 || rpos.Y < 0 || rpos.Z < 0)
                {
                    return false;
                }

                int floorx = (int) Math.Floor(rpos.X);
                int floory = (int) Math.Floor(rpos.Y);
                int floorz = (int) Math.Floor(rpos.Z);

                if (map[floorx,floory,floorz] == Blocks.Air.ID)
                {

                    Ray_Direction += (Ray_Direction*step_size);

                } else
                {
                    rpos = Ray_Position + Ray_Direction;
                    return true;

                }

            }
            return false;

        }

    }
}
