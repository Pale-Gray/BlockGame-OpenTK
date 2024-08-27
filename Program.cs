using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using OpenTK.Mathematics;
using System.Diagnostics;
using Blockgame_OpenTK.BlockUtil;
using System.Reflection;
using Blockgame_OpenTK.ChunkUtil;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Blockgame_OpenTK
{
    internal unsafe class Program
    {

        static Game game;

        public static void Main(string[] args)
        {

            /*
            Dictionary<Vector3i, Chunk> d = new Dictionary<Vector3i, Chunk>();
            int Radius = 16;
            Stopwatch sw = Stopwatch.StartNew();
            for (int x = -Radius; x <= Radius; x++)
            {

                for (int y = -Radius; y <= Radius; y++)
                {

                    for (int z = -Radius; z <= Radius; z++)
                    {

                        d.Add((x, y, z), new Chunk((x, y, z)));

                    }

                }

            }
            sw.Stop();
            Console.WriteLine($"Filled a dictionary containing {Math.Pow(Radius+1+Radius, 3)} chunks (Radius of {Radius}) in {sw.ElapsedMilliseconds}ms");  
            */

            int work;
            int complete;
            ThreadPool.GetMaxThreads(out work, out complete);
            Console.WriteLine(work);

            ThreadPool.SetMaxThreads(8, 0);

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);

            Console.Write("Please enter a username to connect to the server: ");
            string username = Console.ReadLine();
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            byte[] usernameHashBytes = SHA256.HashData(usernameBytes);
            uint usernameHash = BitConverter.ToUInt32(usernameHashBytes);
            Console.WriteLine($"For debug purposes, the hash of your username is {BitConverter.ToString(usernameHashBytes).Replace("-", string.Empty)}. This is the unique ID to connect to the server.");
            Console.Write("Enter the server IP or name to connect: ");
            string address = Console.ReadLine();
            IPAddress[] addresses = Dns.GetHostAddresses(address);

            Console.WriteLine($"The first address in the addresses of {address} is {addresses[0]}");
            Console.WriteLine("Press any button to continue");
            Console.ReadLine();

            game = new Game((int)Globals.WIDTH, (int)Globals.HEIGHT, "Game");

            using (game)
            {

                game.Run();

            }

        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {

            Exception e = args.ExceptionObject as Exception;

            // Console.WriteLine($"Caught an exception, \n {e.Message + "\n" + e.StackTrace}");

            // string message = "An error occured:\n\n" + e.Message + "\n" + e.StackTrace;
            // string[] lines = message.Split(new [] { '\r', '\n' });

            string message = e.Message;
            string[] stackTrace = e.StackTrace.Split(Environment.NewLine);

            Console.WriteLine(message);
            Console.WriteLine(e.StackTrace);

            game.CursorState = CursorState.Normal;
            game.frameBuffer.UpdateAspect();

            float previousTime = 0;

            GL.Disable(EnableCap.DepthTest);

            while (!game.IsExiting)
            {

                GLFW.GetWindowSize(game.WindowPtr, out int w, out int h);

                GL.Viewport(0, 0, w, h);
                Globals.WIDTH = w;
                Globals.HEIGHT = h;
                TextRenderer.Camera.UpdateProjectionMatrix();

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.ClearColor(0.2f, 0, 0.5f, 1);

                float dt = (float)GLFW.GetTime() - (float)previousTime;
                previousTime = (float)GLFW.GetTime();
                
                Globals.Time += dt;

                float yOffset = 2;
                float lineSpacing = 8;
                int fontSize = 16;

                TextRenderer.RenderLines((0, 2, 0), (1, 1, 1), 18, 2, new string[]
                {

                    "Encountered an error.",
                    e.GetType() + ": " + e.Message,

                }.Concat(stackTrace).ToArray());

                // TextRenderer.RenderText((2, yOffset, 0), (1, 1, 1), fontSize, TextRenderer.FilterText("<0xF00000>Encountered an error.</0xF00000>"));

                // TextRenderer.RenderText((2, yOffset + fontSize + lineSpacing, 0), (1, 1, 1), fontSize, e.GetType() + ": " + message);

                // for (int i = 0; i < stackTrace.Length; i++)
                // {

                //      TextRenderer.RenderText((0, 2*(fontSize + lineSpacing) + yOffset + ( (fontSize + lineSpacing) * i), 0), (1, 1, 1), 16, stackTrace[i]);

                // }

                game.SwapBuffers();
                GLFW.PollEvents();

            }

            GLFW.Terminate();
            Environment.Exit(-1);

        }
        public static void RunWindow()
        {

            GameWindow gw2 = new GameWindow(GameWindowSettings.Default, new NativeWindowSettings() { Size = (640, 480), Title = "hi" });
            gw2.Run();

        }

    }
}
