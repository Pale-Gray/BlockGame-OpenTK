using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;

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

    }
}
