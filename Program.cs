using opentk_proj.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj
{
    internal class Program
    {

        public static void Main(string[] args)
        {

            using (Game game = new Game((int)Globals.WIDTH, (int)Globals.HEIGHT, "BlockGame"))
            {

                game.UpdateFrequency = 120.0;
                game.Run();

            }

        }

    }
}
