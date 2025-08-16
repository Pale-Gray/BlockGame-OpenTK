using System;
using VoxelGame.Networking;

namespace VoxelGame;

class Game
{
    static void Main(string[] args)
    {
        Config.Server = new Server("127.0.0.1", 8000).Start();
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