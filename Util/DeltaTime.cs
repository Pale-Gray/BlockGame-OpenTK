using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Blockgame_OpenTK.Util
{
    internal class DeltaTime
    {
        private static double LastTime = GLFW.GetTime();
        public static double Get()
        {

            double ThisTime = GLFW.GetTime();
            double DeltaTime = (ThisTime - LastTime);
            LastTime = ThisTime;

            return DeltaTime;

        }

    }
}
