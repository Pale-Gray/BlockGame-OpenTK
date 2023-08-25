using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj.util
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
