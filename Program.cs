using System;
using OpenTK.Graphics.OpenGL4;
using Blockgame_OpenTK.Util;
using System.Text.Json;

namespace Blockgame_OpenTK
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
