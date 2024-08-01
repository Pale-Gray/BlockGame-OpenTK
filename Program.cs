using System;
using OpenTK.Graphics.OpenGL4;
using Blockgame_OpenTK.Util;
using System.Text.Json;
using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.ChunkUtil;
using System.Diagnostics;
using OpenTK.Mathematics;
using OpenTK;

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
