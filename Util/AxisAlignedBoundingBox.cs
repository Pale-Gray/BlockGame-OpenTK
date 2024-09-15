using OpenTK.Mathematics;
using System;

namespace Blockgame_OpenTK.Util
{
    internal class AxisAlignedBoundingBox
    {

        public Vector3 Position;
        public Vector3 Dimensions;
        public Vector3 Origin;

        public float StaticFriction = 0.0f;
        public float DynamicFriction = 0.0f;
        public AxisAlignedBoundingBox(Vector3 position, Vector3 dimensions, Vector3 origin)
        {

            Position = position;
            Dimensions = dimensions;
            Origin = origin;

        }

        public AxisAlignedBoundingBox GetOffsetBoundingBox(Vector3 offset)
        {

            AxisAlignedBoundingBox bound = new AxisAlignedBoundingBox(Position + offset, Dimensions, Origin);
            bound.StaticFriction = StaticFriction;
            bound.DynamicFriction = DynamicFriction;
            return bound;

        }

        public bool CollideWith(AxisAlignedBoundingBox boundingBox2)
        {

            Vector3 min = Position;
            Vector3 max = min + Dimensions;

            Vector3 boxMin = boundingBox2.Position;
            Vector3 boxMax = boxMin + boundingBox2.Dimensions;

            if (max.X > boxMin.X && min.X < boxMax.X && max.Y > boxMin.Y && min.Y < boxMax.Y && max.Z > boxMin.Z && min.Z < boxMax.Z)
            {

                return true;

            }

                return false;

        }

        public Vector3 GetDepthVector(AxisAlignedBoundingBox bound)
        {

            Vector3 min = Position;
            Vector3 max = min + Dimensions;

            Vector3 boxMin = bound.Position;
            Vector3 boxMax = boxMin + bound.Dimensions;

            Vector3 xOffset = Vector3.Zero;
            Vector3 yOffset = Vector3.Zero;
            Vector3 zOffset = Vector3.Zero;

            if (max.X > boxMin.X && min.X < boxMin.X)
            {

                xOffset = (boxMin.X - max.X, 0.0f, 0.0f);

            } else
            {

                xOffset = (boxMax.X - min.X, 0.0f, 0.0f); 

            }
            if (max.Y > boxMin.Y && min.Y < boxMin.Y)
            {

                yOffset = (0.0f, boxMin.Y - max.Y, 0.0f);

            }
            else
            {

                yOffset = (0.0f, Math.Abs(boxMax.Y - min.Y), 0.0f);

            }
            if (max.Z > boxMin.Z && min.Z < boxMin.Z)
            {

                zOffset = (0.0f, 0.0f, boxMin.Z - max.Z);

            }
            else
            {

                zOffset = (0.0f, 0.0f, boxMax.Z - min.Z);

            }

            return Vector3.MagnitudeMin(xOffset, Vector3.MagnitudeMin(yOffset, zOffset));

        }

    }
}
