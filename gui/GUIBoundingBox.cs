using OpenTK.Mathematics;
using opentk_proj.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj.gui
{
    internal class GUIBoundingBox
    {

        public Vector2 Position;
        Vector2 RelativePosition;
        public Vector2 Dimensions;

        Vector2 OriginOffset;

        public Vector2 Corner;
        public Vector2 CornerDimensionOffset;
        Vector2 CoordinateOffset;
        public GUIBoundingBox(GUIElement GUI, OriginType originType)
        {

            switch (originType)
            {

                case OriginType.BottomLeft:
                    OriginOffset = new Vector2(0f, 0f);
                    break;
                case OriginType.BottomRight:
                    OriginOffset = new Vector2(1f, 0f);
                    break;
                case OriginType.TopLeft:
                    OriginOffset = new Vector2(0f, 1f);
                    break;
                case OriginType.TopRight:
                    OriginOffset = new Vector2(1f, 1f);
                    break;
                case OriginType.Center:
                    OriginOffset = new Vector2(0.5f, 0.5f);
                    break;
                default:
                    OriginOffset = new Vector2(0f, 0f);
                    break;

            }

            Position = GUI.PositionNoOffset;
            Dimensions = GUI.Dimensions;

            Corner = Position - (OriginOffset*Dimensions);
            CornerDimensionOffset = Corner + Dimensions;

        }

        public void Update(GUIElement GUI)
        {

            Position = GUI.PositionNoOffset;
            Dimensions = GUI.Dimensions;

            Corner = Position - (OriginOffset * Dimensions);
            CornerDimensionOffset = Corner + Dimensions;

        }

    }
}
