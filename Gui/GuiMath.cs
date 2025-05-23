﻿using Game.Util;
using OpenTK.Mathematics;

namespace Game.GuiRendering
{
    public class GuiMath
    {

        public static Vector2 RelativeToAbsolute(float x, float y)
        {

            return new Vector2(x, y) * (GlobalValues.Width, GlobalValues.Height);

        }

        public static Vector2 RelativeToAbsolute(Vector2 relativePosition, Vector2 referenceDimensions)
        {

            return relativePosition * referenceDimensions;

        }

    }
}