using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK
{
    internal unsafe class Program
    {

        // static Window* window;
        static Game game;

        public static void Main(string[] args)
        {

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);

            game = new Game((int)Globals.WIDTH, (int)Globals.HEIGHT, "Game");

            using (game)
            {

                game.Run();

            }

            /*AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);
            GLFW.Init();

            GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 4);
            GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 3);
            GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
            window = GLFW.CreateWindow(640, 480, "Title", null, null);

            GLFW.MakeContextCurrent(window);
            GL.LoadBindings(new GLFWBindingsContext());

            int[] array = new int[3];

            while (!GLFW.WindowShouldClose(window))
            {

                GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

                GL.ClearColor(1, 1, 1, 1);

                GLFW.SwapBuffers(window);
                GLFW.PollEvents();

                if (GLFW.GetTime() >= 2)
                {

                    throw new Exception("Forcing a crash");

                }

            }

            GLFW.Terminate(); */

            //using (Game game = new Game((int)Globals.WIDTH, (int)Globals.HEIGHT, "BlockGame"))
            //{

                // game.UpdateFrequency = 120.0;
                // Console.WriteLine(GL.GetString(StringName.Renderer));
                // game.Run();

            //}
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {

            Exception e = args.ExceptionObject as Exception;

            Console.WriteLine($"Caught an exception, \n {e.Message + "\n" + e.StackTrace}");

            // GLFW.MakeContextCurrent(window);
            // GL.Enable(EnableCap.Blend);
            // GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            string message = "An error occured:\n\n" + e.Message + "\n" + e.StackTrace;
            string[] lines = message.Split(new [] { '\r', '\n' });

            List<FontRenderer> textList = new List<FontRenderer>();

            int fontsize = 10;

            foreach (string line in lines)
            {

                textList.Add(new FontRenderer(fontsize, line));

            }

            for (int i = 0; i < textList.Count(); i++)
            {

                textList[i].SetPosition((-315, 210 - (i * (fontsize + 4)), 0));
                textList[i].SetFontColor((1,1,1));

            }

            FontRenderer fr = new FontRenderer(8, e.Message + "\n" + e.StackTrace);
            // GL.LoadBindings(new GLFWBindingsContext());
            game.CursorState = CursorState.Normal;
            // GLFW.SetWindowSize(game.WindowPtr, 900, 480);
            // GL.Viewport(0, 0, 640, 480);
            game.frameBuffer.UpdateAspect();

            while (!game.IsExiting)
            {

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                // game.Error("hi");
                GL.ClearColor(0.2f, 0, 0.5f, 1);

                foreach (var line in textList)
                {

                    line.Draw();

                }

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
