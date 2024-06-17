using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Util
{
    public struct AABB
    {

        Vector3 Position, Offset, Dimension, Origin;

        public AABB(Vector3 position, Vector3 dimension, Vector3 origin)
        {

            Position = position;
            Offset = Vector3.Zero;
            Dimension = dimension;
            Origin = origin;

        }
        public void OffsetPosition(Vector3 offset)
        {

            Offset = offset;

        }

        public bool CollideWith(AABB boundingBox)
        {

            Vector3 ActualPosition = Position + Offset - Origin;
            Vector3 Extent = ActualPosition + Dimension;

            Vector3 ActualPosition2 = boundingBox.Position + boundingBox.Offset - boundingBox.Origin;
            Vector3 Extent2 = ActualPosition2 + boundingBox.Dimension;

            Vector3 PositionDifference = ActualPosition2 - ActualPosition;
            Vector3 ExtentDifference = Extent2 - Extent;

            if (ActualPosition.X >= ActualPosition2.X && ActualPosition.X <= Extent2.X &&
                ActualPosition.Y >= ActualPosition2.Y && ActualPosition.Y <= Extent2.Y &&
                ActualPosition.Z >= ActualPosition2.Z && ActualPosition.Z <= Extent2.Z ||
                Extent.X >= ActualPosition2.X && Extent.X <= Extent2.X &&
                Extent.Y >= ActualPosition2.Y && Extent.Y <= Extent2.Y &&
                Extent.Z >= ActualPosition2.Z && Extent.Z <= Extent2.Z)
            {
                return true;

            }

            return false;

        }

    }
}
