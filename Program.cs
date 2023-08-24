using System;
using OpenTK.Graphics.OpenGL4;

using opentk_proj.util;

namespace opentk_proj
{
    internal class Program
    {
        static void Main(string[] args)
        {

            using (Game game = new Game((int)Constants.WIDTH, (int)Constants.HEIGHT, "Game"))
            {
                game.Run();

            }

        }

    }
}
