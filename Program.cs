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

                // Console.WriteLine(Environment.StackTrace);
                // Console.WriteLine(Globals.Time);

                // TextRenderer.RenderText(12, "Hello");
                // TextRenderer.RenderText(GuiMaths.RelativeToAbsolute((-1,0,0)), (1,0,0), 24, TextRenderer.FilterText("Hello <0x00FFFF>World!</0x00FFFF>"));
                // TextRenderer.RenderText(GuiMaths.RelativeToAbsolute((-1, 0.5f, 0)), (0, 0, 0), (int) GuiMaths.PixelSizeRelativeToPercentageAverage(2), TextRenderer.FilterText("<wiggle>Wiggly italics</wiggle>"));

                // TextRenderer.RenderTextWithShadow(GuiMaths.RelativeToAbsolute((-1, -0.5f, 0)), (3, -3, 0), (1,0,0), (0.25f,0.25f,0.25f), 24, TextRenderer.FilterText("<italic>I am formatted text with shadow!</italic>"));

                TextRenderer.RenderText(GuiMaths.RelativeToAbsolute((-1, 1, 0)) - (0, 24, 0), (1, 1, 1), 16, TextRenderer.FilterText("<0xF00000>Encountered an error.</0xF00000>"));

                TextRenderer.RenderText(GuiMaths.RelativeToAbsolute((-1, 1, 0)) - (0, 50, 0), (1, 1, 1), 15, message);

                for (int i = 0; i < stackTrace.Length; i++)
                {

                    TextRenderer.RenderText(GuiMaths.RelativeToAbsolute((-1, 1, 0)) - (0, 72, 0) - ((Vector3.UnitY * 24) * i), (1, 1, 1), 16, stackTrace[i]);

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
