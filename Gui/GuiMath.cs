using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Gui
{
    public class GuiMath
    {

        public static Vector2 RelativeToAbsolute(float x, float y)
        {

            return new Vector2(x, y) * (GlobalValues.WIDTH, GlobalValues.HEIGHT);

        }

        public static Vector2 RelativeToAbsolute(Vector2 relativePosition, Vector2 referenceDimensions)
        {

            return relativePosition * referenceDimensions;

        }

    }
}