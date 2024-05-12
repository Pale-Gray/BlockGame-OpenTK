using opentk_proj.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace opentk_proj
{
    internal class Program
    {

        public static void Main(string[] args)
        {

            using (Game game = new Game((int)Globals.WIDTH, (int)Globals.HEIGHT, "BlockGame"))
            {

                game.UpdateFrequency = 120.0;
                Console.WriteLine(GL.GetString(StringName.Renderer));
                game.Run();

            }

        }

    }
}
