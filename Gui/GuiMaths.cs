using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.Formats.Asn1;

namespace Blockgame_OpenTK.Gui
{
    internal class GuiMaths
    {

        public static Vector3 AbsoluteToRelative(Vector3 absolutePostion)
        {

            Vector3 relativePosition = absolutePostion;
            relativePosition.X /= Globals.WIDTH;
            relativePosition.X *= 2;
            relativePosition.X -= 1;
            relativePosition.Y /= Globals.HEIGHT;
            relativePosition.Y *= 2;
            relativePosition.Y -= 1;
            return relativePosition;

        }

        public static Vector3 RelativeToAbsolute(Vector3 relativePosition)
        {

            Vector3 absolutePosition = relativePosition;
            absolutePosition.X *= Globals.WIDTH;
            absolutePosition.Y *= Globals.HEIGHT;

            return absolutePosition;

        }

        public static float PixelSizeRelativeToPercentageOfWidth(float percentage)
        {

            return Globals.WIDTH * (percentage / 100);

        }

        public static float PixelSizeRelativeToPercentageOfHeight(float percentage)
        {

            return Globals.HEIGHT * (percentage / 100);

        }

        public static float PixelSizeRelativeToPercentageMax(float percentage)
        {

            return Math.Max(Globals.HEIGHT, Globals.WIDTH) * (percentage / 100);

        }

        public static float PixelSizeRelativeToPercentageMin(float percentage)
        {

            return Math.Min(Globals.HEIGHT, Globals.WIDTH) * (percentage / 100);

        }

        public static float PixelSizeRelativeToPercentageAverage(float percentage)
        {

            float a = Globals.WIDTH * (percentage / 100);
            float b = Globals.HEIGHT * (percentage / 100);

            return (a + b) / 2f;

        }

        public static bool DidCollideWithMousePointer(Vector2 position, Vector2 dimension, Vector2 origin)
        {

            Vector2 mousePosition = Globals.Mouse.Position;

            position = position - (dimension * origin);

            if (mousePosition.X >= position.X && mousePosition.X <= position.X + dimension.X && mousePosition.Y >= position.Y && mousePosition.Y <= position.Y + dimension.Y) return true;

            return false; 

        }

    }
}
