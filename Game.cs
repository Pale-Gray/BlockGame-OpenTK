using System;
using System.Diagnostics;
using System.Linq;
using OpenTK.Graphics.Egl;
using VoxelGame.Networking;
using VoxelGame.Util;

namespace VoxelGame;

class Game
{
    private static float ElapsedTime;
    
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            string argument = args[0];
            if (argument == "server")
            {
                Config.Server = new Server("server_settings.json");
                Config.Server.Start();
            }

            if (argument == "client")
            {
                string[] hostAndPort = args[1].Split(":");
                Config.Client = new Client();
                Config.Client.Start();
                Config.Client.HostOrIp = hostAndPort[0];
                Config.Client.Port = int.Parse(hostAndPort[1]);
                Config.Client.Join();
            }
        }
        else
        {
            Config.Server = new Server("127.0.0.1", 8000);
            Config.Server.IsInternal = true;
            Config.Server.Start();
            
            Config.Client = new Client();
            Config.Client.Start();
            Config.Client.HostOrIp = "localhost";
            Config.Client.Port = 8000;
            Config.Client.Join();
        }
        
        Config.Timer.Start();
        float t = 0;
        float elapsed = 0;
        
        while (Config.IsRunning)
        {
            while (elapsed >= 1 / Config.Tickrate)
            {
                Config.LastTicksPerSecond++;
                Config.Server?.TickUpdate();
                Config.Client?.TickUpdate();
                elapsed -= 1 / Config.Tickrate;
            }
            
            Config.Server?.Update();
            Config.Client?.Update();

            Config.DeltaTime = (float) Config.Timer.Elapsed.TotalSeconds;
            Config.FrameTimesOfLastSecond.Add((float) Config.Timer.Elapsed.TotalMilliseconds);
            Config.ElapsedTime += Config.DeltaTime;
            t += Config.DeltaTime;
            elapsed += Config.DeltaTime;

            if (t >= 1.0f)
            {
                Console.WriteLine($"TPS: {Config.LastTicksPerSecond}, expected: {Config.Tickrate}, tick loss: {Config.Tickrate - Config.LastTicksPerSecond}");
                Config.LastTicksPerSecond = 0;

                Config.MinimumFps = float.Round(16.6f / Config.FrameTimesOfLastSecond.Max() * 60.0f);
                Config.MaximumFps = float.Round(16.6f / Config.FrameTimesOfLastSecond.Min() * 60.0f);
                Config.AverageFps = float.Round(16.6f / Config.FrameTimesOfLastSecond.Average() * 60.0f);
                Config.FrameTimesOfLastSecond.Clear();
                t -= 1.0f;
            }
            
            Config.Timer.Restart();
        }
            
        Config.Server?.Stop();
    }
}