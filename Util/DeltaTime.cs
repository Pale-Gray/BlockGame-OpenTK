using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Game.Util
{
    public class DeltaTime
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
