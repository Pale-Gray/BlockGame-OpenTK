using System;
using System.Text;
using System.Threading;
using OpenTK.Platform;
using OpenTK.Core.Utility;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace Blockgame_OpenTK
{
    internal unsafe class Program
    { 

        public static void Main(string[] args)
        {

            Console.OutputEncoding = Encoding.Unicode;

            // AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);
            Input.Initialize();

            ThreadPool.SetMaxThreads(8, 8);
            
            ToolkitOptions toolkitOptions = new ToolkitOptions();
            toolkitOptions.ApplicationName = "Game";
            toolkitOptions.Logger = new ConsoleLogger();
            Toolkit.Init(toolkitOptions);

            OpenGLGraphicsApiHints contextSettings = new OpenGLGraphicsApiHints()
            {

                Version = new Version(4, 6),
                Profile = OpenGLProfile.Core,
                DebugFlag = true,
                DepthBits = ContextDepthBits.Depth24,
                StencilBits = ContextStencilBits.Stencil8

            };

            WindowHandle window = Toolkit.Window.Create(contextSettings);
            OpenGLContextHandle glContext = Toolkit.OpenGL.CreateFromWindow(window);

            Toolkit.OpenGL.SetCurrentContext(glContext);
            GLLoader.LoadBindings(Toolkit.OpenGL.GetBindingsContext(glContext));

            BlockGame.Load();

            EventQueue.EventRaised += EventRaised;

            Toolkit.Window.SetTitle(window, "Game");
            Toolkit.Window.SetSize(window, 640, 480);
            Toolkit.Window.SetMode(window, WindowMode.Normal);
            Toolkit.Window.SetCursorCaptureMode(window, CursorCaptureMode.Locked);
            //Toolkit.Window.GetClientSize(window, out int w, out int h);
            //Console.WriteLine($"{w}, {h}");
            // CursorHandle invisibleCursor = Toolkit.Cursor.Create(1, 1, new ReadOnlySpan<byte>(new byte[4]), 0, 0);
            CursorHandle visibleCursor = Toolkit.Cursor.Create(SystemCursorType.Default);
            Toolkit.Window.SetCursor(window, null);

            {

                Toolkit.Mouse.GetPosition(out int x, out int y);
                Input.PreviousMousePosition = (x, y);

            }
            GlobalValues.PreviousTime = Stopwatch.GetTimestamp();
            while (true)
            {

                GlobalValues.CurrentTime = Stopwatch.GetTimestamp();
                GlobalValues.DeltaTime = (GlobalValues.CurrentTime - GlobalValues.PreviousTime) / Stopwatch.Frequency;
                GlobalValues.PreviousTime = GlobalValues.CurrentTime;

                Input.MouseDelta = Vector2.Zero;

                GlobalValues.Time += GlobalValues.DeltaTime;

                Toolkit.Window.ProcessEvents(false);

                if (Toolkit.Window.IsWindowDestroyed(window))
                {

                    BlockGame.Unload();
                    break;

                }

                if (Input.IsKeyPressed(Key.Escape))
                {

                    Console.WriteLine("yes");
                    if (Toolkit.Window.GetCursorCaptureMode(window) == CursorCaptureMode.Locked)
                    {

                        Toolkit.Window.SetCursorCaptureMode(window, CursorCaptureMode.Normal);
                        Toolkit.Window.SetCursor(window, visibleCursor);
                        GlobalValues.IsCursorLocked = false;
                        
                    } else
                    {

                        Toolkit.Window.SetCursorCaptureMode(window, CursorCaptureMode.Locked);
                        Toolkit.Window.SetCursor(window, null);
                        GlobalValues.IsCursorLocked = true;

                    }

                }

                BlockGame.Render();

                Toolkit.OpenGL.SwapBuffers(glContext);

            }

        }

        static void EventRaised(PalHandle? handle, PlatformEventType type, EventArgs args)
        {

            if (args is CloseEventArgs closeEventArgs)
            {

                Toolkit.Window.Destroy(closeEventArgs.Window);

            }

            if (args is WindowResizeEventArgs windowResizeEventArgs)
            {
                // BlockGame.UpdateScreenSize(windowResizeEventArgs.Window);

                Console.WriteLine("resizing");
                BlockGame.UpdateScreenSize(windowResizeEventArgs);

            }

            if (args is WindowFramebufferResizeEventArgs framwbufferResizeEventArgs)
            {

                BlockGame.UpdateFramebufferSize(framwbufferResizeEventArgs);

            }

            if (args is KeyDownEventArgs keyDown)
            {

                KeyState keyState = Input.KeyStates[keyDown.Key];
                keyState.IsKeyDown = true;
                Input.KeyStates[keyDown.Key] = keyState;

            }

            if (args is KeyUpEventArgs keyUp)
            {

                KeyState keyState = Input.KeyStates[keyUp.Key];
                keyState.IsKeyDown = false;
                keyState.AllowKeyPress = true;
                Input.CurrentKeyDown = Key.Unknown;
                Input.CurrentKeyPressed = Key.Unknown;
                Input.KeyStates[keyUp.Key] = keyState;

            }

            if (args is MouseMoveEventArgs mouseMove)
            {


                Input.CurrentMousePosition = mouseMove.Position;
                Input.MouseDelta = Input.CurrentMousePosition - Input.PreviousMousePosition;
                Input.PreviousMousePosition = Input.CurrentMousePosition;

            }

        }

        /*
        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {

            Exception e = args.ExceptionObject as Exception;

            string message = e.Message;
            string[] stackTrace = e.StackTrace.Split(Environment.NewLine);

            Console.WriteLine(message);
            Console.WriteLine(e.StackTrace);

            game.CursorState = CursorState.Normal;
            game.frameBuffer.UpdateAspect();

            float previousTime = 0;

            GL.Disable(EnableCap.DepthTest);

            using (FileStream stream = new FileStream("log.txt", FileMode.Create))
            {

                foreach (string line in GlobalValues.LogMessages)
                {

                    stream.Write(Encoding.UTF8.GetBytes($"{line}\n"));

                }

                stream.Write(Encoding.UTF8.GetBytes($"{e.GetType()}: {e.Message}\n"));
                stream.Write(Encoding.UTF8.GetBytes(e.StackTrace));

            }

            while (!game.IsExiting)
            {

                GLFW.GetWindowSize(game.WindowPtr, out int w, out int h);

                GL.Viewport(0, 0, w, h);
                GlobalValues.WIDTH = w;
                GlobalValues.HEIGHT = h;
                TextRenderer.Camera.UpdateProjectionMatrix();

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.ClearColor(0.2f, 0, 0.5f, 1);

                float dt = (float)GLFW.GetTime() - (float)previousTime;
                previousTime = (float)GLFW.GetTime();
                
                GlobalValues.Time += dt;

                float yOffset = 2;
                float lineSpacing = 8;
                int fontSize = 16;

                TextRenderer.RenderLines((0, 2, 0), TextRenderer.TopLeft, (1, 1, 1), 18, 2, new string[]
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
        */

    }
}
