using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj.util
{
    internal class Globals
    {

        public static float WIDTH = 640f;
        public static float HEIGHT = 480f;
        public static int ChunkSize = 32;
        public static double Time = 0;
        public static MouseState Mouse = null;
        public static KeyboardState Keyboard = null;

        public static float AtlasResolution = 8;
        public static float Ratio = 1 / AtlasResolution;

        // public static long seed = new Random().Next(int.MaxValue);
        public static long seed = 1245919872491;

    }
}
