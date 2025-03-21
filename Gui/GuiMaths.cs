using Game.Util;
using OpenTK.Mathematics;
using System;
using System.Formats.Asn1;

namespace Game.Gui
{
    public class GuiMaths
    {

        public static Vector3 AbsoluteToRelative(Vector3 absolutePostion)
        {

            Vector3 relativePosition = absolutePostion;
            relativePosition.X /= GlobalValues.Width;
            relativePosition.X *= 2;
            relativePosition.X -= 1;
            relativePosition.Y /= GlobalValues.Height;
            relativePosition.Y *= 2;
            relativePosition.Y -= 1;
            return relativePosition;

        }

        public static Vector3 RelativeToAbsolute(Vector3 relativePosition)
        {

            Vector3 absolutePosition = relativePosition;
            absolutePosition.X *= GlobalValues.Width;
            absolutePosition.Y *= GlobalValues.Height;

            return absolutePosition;

        }

        public static float PixelSizeRelativeToPercentageOfWidth(float percentage)
        {

            return GlobalValues.Width * (percentage / 100);

        }

        public static float PixelSizeRelativeToPercentageOfHeight(float percentage)
        {

            return GlobalValues.Height * (percentage / 100);

        }

        public static float PixelSizeRelativeToPercentageMax(float percentage)
        {

            return Math.Max(GlobalValues.Height, GlobalValues.Width) * (percentage / 100);

        }

        public static float PixelSizeRelativeToPercentageMin(float percentage)
        {

            return Math.Min(GlobalValues.Height, GlobalValues.Width) * (percentage / 100);

        }

        public static float PixelSizeRelativeToPercentageAverage(float percentage)
        {

            float a = GlobalValues.Width * (percentage / 100);
            float b = GlobalValues.Height * (percentage / 100);

            return (a + b) / 2f;

        }

        public static bool DidCollideWithMousePointer(Vector2 position, Vector2 dimension, Vector2 origin)
        {

            Vector2 mousePosition = Input.CurrentMousePosition;

            position = position - (dimension * origin);

            if (mousePosition.X >= position.X && mousePosition.X <= position.X + dimension.X && mousePosition.Y >= position.Y && mousePosition.Y <= position.Y + dimension.Y) return true;

            return false;

        }

    }
}