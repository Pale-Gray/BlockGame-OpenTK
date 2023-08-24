// Hit_Triangle is based off of this wikipedia article:
// https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm#Java_implementation
//

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

        private const double epsilon = 0.0000001;

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


        public float scalefactor = -1;
        public bool Hit_Triangle(float[] vertices, int offset, NakedModel model, BoundingBox boundingBox)
        {

            scalefactor = -1;

            // Matrix3d model3d = new Matrix3d(model.model.M11, model.model.M12, model.model.M13, model.model.M21, model.model.M22, model.model.M23, model.model.M31, model.model.M32, model.model.M33);

            Vector3d vert0 = new Vector3d((double)vertices[offset+0], (double)vertices[offset + 1], (double)vertices[offset + 2]);
            Vector3d vert1 = new Vector3d((double)vertices[offset + 3], (double)vertices[offset + 4], (double)vertices[offset + 5]);
            Vector3d vert2 = new Vector3d((double)vertices[offset + 6], (double)vertices[offset + 7], (double)vertices[offset + 8]);

            Vector3d edge1, edge2, h, s, q;

            double a, f, u, v;

            edge1 = vert1 - vert0;
            edge2 = vert2 - vert0;
            
            h = Vector3d.Cross((Vector3d)Ray_Direction, edge2);
            a = Vector3d.Dot(edge1, h);
            if (a > -epsilon && a < epsilon)
            {

                //Console.WriteLine("the vector is parallel to the triangle");
                return false;

            }

            f = 1.0 / a;
            s = Ray_Position - vert0;
            u = f * (Vector3d.Dot(s, h));

            if (u < 0.0 || u > 1.0)
            {

                //Console.WriteLine("U is less than 0 or greater than 1.");
                return false;

            }

            q = Vector3d.Cross(s, edge1);
            v = f * Vector3d.Dot((Vector3d)Ray_Direction, q);

            if (v < 0.0 || u+v > 1.0)
            {

                //Console.WriteLine("V is less than 0 or UV is greater than 1");
                return false;

            }

            double t = f * Vector3d.Dot(edge2, q);
            //Console.WriteLine(Vector3d.Dot(edge2, q));
            if (t > epsilon)
            {

                // Console.WriteLine("There has been an intersection.");
                scalefactor = (float) t;
                return true;

            } else
            {

                return false;

            }

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
