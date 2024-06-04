// Hit_Triangle is based off of this wikipedia article:
// https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm#Java_implementation
//

using OpenTK.Mathematics;
using System;

using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK.Util
{
    internal class Ray
    {
        static float[] verts = {

            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, // front
             0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

            0.5f, -0.5f, -0.5f,  0.0f, 0.0f, // right
            0.5f, -0.5f, 0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, 0.5f,  1.0f, 1.0f,
            0.5f,  0.5f, 0.5f,  1.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

            0.5f, -0.5f, 0.5f, 0.0f, 0.0f, // back 
            -0.5f, -0.5f, 0.5f, 1.0f, 0.0f,
            -0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            -0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 0.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 0.0f, 0.0f,

            -0.5f, -0.5f, 0.5f,  0.0f, 0.0f, // left
            -0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, 0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, 0.5f,  0.0f, 0.0f,

            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, // top
            0.5f, 0.5f, -0.5f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            -0.5f, 0.5f, 0.5f, 0.0f, 1.0f,
            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f,

            0.5f, -0.5f, -0.5f, 0.0f, 0.0f, // bottom
            -0.5f, -0.5f, -0.5f, 1.0f, 0.0f,
            -0.5f, -0.5f, 0.5f, 1.0f, 1.0f,
            -0.5f, -0.5f, 0.5f, 1.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 0.0f, 1.0f,
            0.5f, -0.5f, -0.5f, 0.0f, 0.0f

        };

        private const double epsilon = 0.0000001;

        public Vector3 Ray_Position = new Vector3(0, 0, 0);
        public Vector3 Ray_Direction = new Vector3(0, 0, 0);

        public Vector3 rpos;

        public bool hit;
        public Vector3 hit_position;

        public Vector3 HitPositionXY;
        public Vector3 HitPositionYX;
        public Vector3 HitPositionXZ;
        public Vector3 HitPositionZY;
        public Vector3 HitPositionYZ;
        public Vector3 HitPositionZX;

        public Vector3 HitPositionXYZ;
        public Vector3 HitPositionZYX;
        public Vector3 HitPositionYXZ;

        public Vector3 RHitPositionXY;
        public Vector3 RHitPositionYX;
        public Vector3 RHitPositionXZ;
        public Vector3 RHitPositionZY;
        public Vector3 RHitPositionYZ;
        public Vector3 RHitPositionZX;

        public Vector3 RHitPositionXYZ;
        public Vector3 RHitPositionZYX;
        public Vector3 RHitPositionYXZ;

        public Vector3 XIntersect;
        public Vector3 YIntersect;
        public Vector3 ZIntersect;
        public Vector3 Intersect;

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

        // Model hitdisplay;

        public float scalefactor = -1;

        // uses chunk data instead of meshes etc... more performant but can only trace cubes and not different meshes
        // only checks if no air, so meshes on the block data grid that are not exactly cubes still work, but the bounds will be CUBES

        // travels along the slope of a line.
        public void StepChunkData(Chunk chunk, Camera camera)
        {
            // hitdisplay = new Model(verts, "../../../res/textures/debug.png", "../../../res/shaders/model.vert", "../../../res/shaders/model.frag");
            int[,,] chunkData;
            if (chunk == null)
            {

                chunkData = new int[32, 32, 32];

            }
            else
            {

                chunkData = chunk.blockdata;

            }
            HitChunkData(chunk, camera);
            int steps = 8;
            float em = 0.0001f;
            for (int i = 0; i < steps; i++)
            {

                Vector3 SamplePosition = ChunkUtils.getPlayerPositionRelativeToChunk(Ray_Position);


                if (chunkData[(int)SamplePosition.X, (int)SamplePosition.Y, (int)SamplePosition.Z] == 0)
                {

                    Ray_Position += Intersect + (Ray_Direction*em);

                }
                // Ray_Position += Intersect + (Ray_Direction*em);
                HitChunkData(chunk, camera);
                // hitdisplay.SetScale(0.1f,0.1f,0.1f);
                // hitdisplay.Draw(Ray_Position + Intersect, camera, 0);

            }
            // hitdisplay.Draw(Ray_Position, camera, 0);
            Console.WriteLine(Ray_Position + Intersect);

        }
        public void HitChunkData(Chunk chunk, Camera camera)
        {
            int[,,] chunkData;
            if (chunk == null)
            {

                chunkData = new int[32, 32, 32];

            } else
            {

                chunkData = chunk.blockdata;

            }
            // Ray_Position; Ray_Direction;
            // Vector3 RayPositionModOne = (Ray_Position.X % 1, Ray_Position.Y % 1, Ray_Position.Z % 1);

            // due to cubes being from -0.5f to 0.5f, need to offset.
            Vector3 RayPositionOffsetted = Ray_Position - (0.5f,0.5f,0.5f);
            // direction doesnt need to be offset.
            Vector3 RayDirectionOffsetted = Ray_Direction;

            Vector3 PointB = RayPositionOffsetted + RayDirectionOffsetted;

            float XYSlope = (PointB.Y - RayPositionOffsetted.Y) / (PointB.X - RayPositionOffsetted.X);
            float XZSlope = (PointB.Z - RayPositionOffsetted.Z) / (PointB.X - RayPositionOffsetted.X);

            float ZXSlope = (PointB.X - RayPositionOffsetted.X) / (PointB.Z - RayPositionOffsetted.Z);
            float ZYSlope = (PointB.Y - RayPositionOffsetted.Y) / (PointB.Z - RayPositionOffsetted.Z);

            float YXSlope = (PointB.X - RayPositionOffsetted.X) / (PointB.Y - RayPositionOffsetted.Y);
            float YZSlope = (PointB.Z - RayPositionOffsetted.Z) / (PointB.Y - RayPositionOffsetted.Y);

            // float ZYSlope = (PointB.Y - RayPositionOffsetted.Y) / (PointB.Z - RayPositionOffsetted.Z);

            Vector3 DistToRay = ((float)Math.Ceiling(RayPositionOffsetted.X), (float)Math.Ceiling(RayPositionOffsetted.Y), (float)Math.Ceiling(RayPositionOffsetted.Z))
                - RayPositionOffsetted;// - RayPositionOffsetted;

            Vector3 RDistToRay = ((float)Math.Floor(RayPositionOffsetted.X), (float)Math.Floor(RayPositionOffsetted.Y), (float)Math.Floor(RayPositionOffsetted.Z))
                - RayPositionOffsetted;

            // Console.WriteLine("{0}, {1}", Ray_Position, DistToRay.X);

            Vector3 XYIntersect = (DistToRay.X, DistToRay.X * XYSlope, 0.0f);
            Vector3 XZIntersect = (DistToRay.X, 0.0f, DistToRay.X * XZSlope);

            Vector3 RXYIntersect = (RDistToRay.X, RDistToRay.X * XYSlope, 0.0f);
            Vector3 RXZIntersect = (RDistToRay.X, 0.0f, RDistToRay.X * XZSlope);

            Vector3 XYZIntersect = (DistToRay.X, DistToRay.X * XYSlope, DistToRay.X * XZSlope);
            Vector3 RXYZIntersect = (RDistToRay.X, RDistToRay.X * XYSlope, RDistToRay.X * XZSlope);

            Vector3 ZYIntersect = (0.0f, DistToRay.Z * ZYSlope, DistToRay.Z);
            Vector3 ZXIntersect = (DistToRay.Z * ZXSlope, 0.0f, DistToRay.Z);

            Vector3 RZYIntersect = (0.0f, RDistToRay.Z * ZYSlope, RDistToRay.Z);
            Vector3 RZXIntersect = (RDistToRay.Z * ZXSlope, 0.0f, RDistToRay.Z);

            Vector3 ZYXIntersect = (DistToRay.Z * ZXSlope, DistToRay.Z * ZYSlope, DistToRay.Z);
            Vector3 RZYXIntersect = (RDistToRay.Z * ZXSlope, RDistToRay.Z * ZYSlope, RDistToRay.Z);

            Vector3 YXIntersect = (DistToRay.Y * YXSlope, DistToRay.Y, 0.0f);
            Vector3 YZIntersect = (0.0f, DistToRay.Y, DistToRay.Y * YZSlope);

            Vector3 RYXIntersect = (RDistToRay.Y * YXSlope, RDistToRay.Y, 0.0f);
            Vector3 RYZIntersect = (0.0f, RDistToRay.Y, RDistToRay.Y * YZSlope);

            Vector3 YXZIntersect = (DistToRay.Y * YXSlope, DistToRay.Y, DistToRay.Y * YZSlope);
            Vector3 RYXZIntersect = (RDistToRay.Y * YXSlope, RDistToRay.Y, RDistToRay.Y * YZSlope);

            HitPositionXY = XYIntersect;
            HitPositionXZ = XZIntersect;
            HitPositionZY = ZYIntersect;
            HitPositionZX = ZXIntersect;
            HitPositionYX = YXIntersect;
            HitPositionYZ = YZIntersect;

            HitPositionXYZ = XYZIntersect;
            HitPositionZYX = ZYXIntersect;
            HitPositionYXZ = YXZIntersect;

            RHitPositionXY = RXYIntersect;
            RHitPositionXZ = RXZIntersect;
            RHitPositionZY = RZYIntersect;
            RHitPositionZX = RZXIntersect;
            RHitPositionYX = RYXIntersect;
            RHitPositionYZ = RYZIntersect;

            RHitPositionXYZ = RXYZIntersect;
            RHitPositionZYX = RZYXIntersect;
            RHitPositionYXZ = RYXZIntersect;

            XIntersect = camera.ForwardVector.X < 0 ? RHitPositionXYZ : HitPositionXYZ;
            YIntersect = camera.ForwardVector.Y < 0 ? RHitPositionYXZ : HitPositionYXZ;
            ZIntersect = camera.ForwardVector.Z < 0 ? RHitPositionZYX : HitPositionZYX;

            // Vector3.min
            // Intersect = Vector3.ComponentMin(Vector3.ComponentMin(XIntersect, YIntersect), ZIntersect);
            Intersect = Vector3.MagnitudeMin(Vector3.MagnitudeMin(YIntersect, XIntersect), ZIntersect);

        }
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

                if (map[floorx,floory,floorz] == Blocks.GetIDFromBlock(Blocks.Air))
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
