using System;
using OpenTK.Graphics.Egl;
using VoxelGame.Networking;

namespace VoxelGame;

class Game
{
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            string argument = args[0];
            if (argument == "server")
            {
                Config.Server = new Server("server_settings.json").Start();
                while (Config.IsRunning)
                {
                    Config.Server?.Poll();
                }
                Config.Server?.Stop();
            }

            if (argument == "client")
            {
                string[] hostAndPort = args[1].Split(":");
                Config.Client = new Client();
                Config.Client.Start().JoinServer(hostAndPort[0], int.Parse(hostAndPort[1]));

                while (Config.IsRunning)
                {
                    Config.Client?.Poll();
                }
            }
        }
        else
        {
            Config.Server = new Server("127.0.0.1", 8000).Start(true);
            Config.Client = new Client();
            Config.Client.Start().JoinServer("localhost", 8000);
        
            Console.WriteLine($"Registered block count: {Config.Register.BlockCount}");

            while (Config.IsRunning)
            {
                Config.Server?.Poll();
                Config.Client?.Poll();
            }
            
            Config.Server?.Stop();
        }
    }
}