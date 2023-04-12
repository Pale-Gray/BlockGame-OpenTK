using System;

namespace opentk_proj
{
    internal class Program
    {
        static void Main(string[] args)
        {

            using (Game game = new Game(640, 480, "Game"))
            {

                game.Run();

            }

        }

    }
}
