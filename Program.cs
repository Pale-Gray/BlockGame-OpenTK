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
using System.IO;
using System.Resources;
using Blockgame_OpenTK.Font;
using System.Collections.Generic;
using Blockgame_OpenTK.BlockUtil;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using Blockgame_OpenTK.PlayerUtil;
using Blockgame_OpenTK.BlockProperty;
using System.Diagnostics.Contracts;

namespace Blockgame_OpenTK
{
    internal class Program
    {

        private static OpenGLContextHandle _glContext;
        public static async Task Main(string[] args)
        {

            if (args.Length == 0)
            {

                Util.Debugger.Log("Starting in client mode.", Severity.Info);

            }
            if (args.Length == 1)
            {

                if (args[0].ToLower() == "client") Util.Debugger.Log("Starting in client mode.", Severity.Info);

            }
            if (args.Length > 1)
            {

                if (args[0].ToLower() == "server") Util.Debugger.Log("Starting in server mode.", Severity.Info);

            }

            AspenTreeBlockProperties prop = new AspenTreeBlockProperties();
            IBlockProperties prop2 = prop;
            prop2.ToBytes();

            NewProperties prop3 = new NewProperties();
            IBlockProperties prop4 = prop3;
            prop4.ToBytes();

            IBlockProperties[] props = new IBlockProperties[50];

            IBlockProperties propser = null;

            Console.WriteLine(propser is null);

            // account stuff
            /* 
            Console.WriteLine("Please specify a username");
            string username = Console.ReadLine();
            Console.WriteLine("Please specify a password");
            string password = Console.ReadLine();

            using (HttpClient client = new HttpClient())
            {

                Dictionary<string, string> loginInfo = new()
                {

                    {"username", username},
                    {"password", password}

                };

                FormUrlEncodedContent content = new FormUrlEncodedContent(loginInfo);

                HttpResponseMessage response = await client.PostAsync("https://palegray.blog/logingame.php", content);

                if (response.StatusCode == HttpStatusCode.OK)
                {

                    string jsonData = await response.Content.ReadAsStringAsync();
                    AbstractAccountInfo accountInfo = JsonSerializer.Deserialize<AbstractAccountInfo>(jsonData);

                    GlobalValues.AbstractCurrentUser = accountInfo;

                }

            }
            */

            Console.OutputEncoding = Encoding.Unicode;

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);

            ThreadPool.SetMaxThreads(8, 8);
            
            ToolkitOptions toolkitOptions = new ToolkitOptions();
            toolkitOptions.ApplicationName = "Game";
            toolkitOptions.Logger = new ConsoleLogger();
            Toolkit.Init(toolkitOptions);

            OpenGLGraphicsApiHints contextSettings = new OpenGLGraphicsApiHints()
            {

                Version = new Version(4, 6),
                Profile = OpenGLProfile.Core,
                DebugFlag = false,
                DepthBits = ContextDepthBits.Depth24,
                StencilBits = ContextStencilBits.Stencil8

            };

            WindowHandle window = Toolkit.Window.Create(contextSettings);
            _glContext = Toolkit.OpenGL.CreateFromWindow(window);

            Toolkit.OpenGL.SetCurrentContext(_glContext);
            GLLoader.LoadBindings(Toolkit.OpenGL.GetBindingsContext(_glContext));

            BlockGame.Load();

            EventQueue.EventRaised += EventRaised;

            Toolkit.Window.SetTitle(window, "Game");
            Toolkit.Window.SetSize(window, (640, 480));
            Toolkit.Window.SetMode(window, WindowMode.Normal);
            Toolkit.Window.SetCursorCaptureMode(window, CursorCaptureMode.Locked);

            //Toolkit.Window.GetClientSize(window, out int w, out int h);
            //Console.WriteLine($"{w}, {h}");
            // CursorHandle invisibleCursor = Toolkit.Cursor.Create(1, 1, new ReadOnlySpan<byte>(new byte[4]), 0, 0);
            CursorHandle visibleCursor = Toolkit.Cursor.Create(SystemCursorType.Default);
            
            ReadOnlySpan<byte> hd = new ReadOnlySpan<byte>(new byte[] {0, 0, 0, 0});
            CursorHandle c1 = Toolkit.Cursor.Create(1, 1, hd, 0, 0);
            Toolkit.Window.SetCursor(window, c1);
            // Toolkit.Window.SetCursor(window, null);
            {

                // Toolkit.Mouse.GetPosition();
                Toolkit.Mouse.GetPosition(window, out Vector2 position);
                Toolkit.Mouse.GetMouseState(window, out OpenTK.Platform.MouseState state);
                Input.PreviousMouseScroll = state.Scroll;
                Input.PreviousMousePosition = position;

            }

            // Toolkit.Joystick.Initialize(toolkitOptions);
            // Input.CheckForController(0);

            GlobalValues.PreviousTime = Stopwatch.GetTimestamp();
            double secondValue = 0;
            double frameTimeOverOneSecond = 0;
            double numTicks = 0;
            while (GlobalValues.IsRunning)
            {

                if (secondValue >= 1.0)
                {

                    GlobalValues.AverageFps = (int) Math.Truncate(((frameTimeOverOneSecond / numTicks) * 60) * 100);
                    secondValue--;
                    frameTimeOverOneSecond = 0;
                    numTicks = 0;

                }
                frameTimeOverOneSecond += GlobalValues.DeltaTime;
                numTicks++;
                secondValue += GlobalValues.DeltaTime;

                Stopwatch sw = Stopwatch.StartNew();
                if (Input.PlayerOneJoystickHandle != null)
                {

                    float joystickLeftAxisX = Toolkit.Joystick.GetAxis(Input.PlayerOneJoystickHandle, JoystickAxis.LeftXAxis);
                    float joystickLeftAxisY = Toolkit.Joystick.GetAxis(Input.PlayerOneJoystickHandle, JoystickAxis.LeftYAxis);
                    float joystickRightAxisX = Toolkit.Joystick.GetAxis(Input.PlayerOneJoystickHandle, JoystickAxis.RightXAxis);
                    float joystickRightAxisY = Toolkit.Joystick.GetAxis(Input.PlayerOneJoystickHandle, JoystickAxis.RightYAxis);

                    Input.JoystickLeftAxis.X = joystickLeftAxisX;
                    Input.JoystickLeftAxis.Y = joystickLeftAxisY;
                    Input.JoystickRightAxis.X = joystickRightAxisX;
                    Input.JoystickRightAxis.Y = joystickRightAxisY;
                    if (Math.Abs(joystickLeftAxisX) < Toolkit.Joystick.LeftDeadzone)
                    {

                        Input.JoystickLeftAxis.X = 0.0f;

                    }
                    if (Math.Abs(joystickLeftAxisY) < Toolkit.Joystick.LeftDeadzone)
                    {

                        Input.JoystickLeftAxis.Y = 0.0f;

                    }
                    if (Math.Abs(joystickRightAxisX) < Toolkit.Joystick.RightDeadzone)
                    {

                        Input.JoystickRightAxis.X = 0.0f;

                    }
                    if (Math.Abs(joystickRightAxisY) < Toolkit.Joystick.RightDeadzone)
                    {

                        Input.JoystickRightAxis.Y = 0.0f;

                    }

                    Input.LeftTrigger = Toolkit.Joystick.GetAxis(Input.PlayerOneJoystickHandle, JoystickAxis.LeftTrigger);
                    Input.RightTrigger = Toolkit.Joystick.GetAxis(Input.PlayerOneJoystickHandle, JoystickAxis.RightTrigger);
                    if (Input.LeftTrigger < Toolkit.Joystick.TriggerThreshold)
                    {

                        Input.LeftTrigger = 0.0f;

                    }
                    if (Input.RightTrigger < Toolkit.Joystick.TriggerThreshold)
                    {

                        Input.RightTrigger = 0.0f;

                    }

                    foreach (JoystickButton joystickButton in Input.JoystickStates.Keys)
                    {

                        JoystickState joystickButtonState = Input.JoystickStates[joystickButton];
                        bool isJoystickButtonDown = Toolkit.Joystick.GetButton(Input.PlayerOneJoystickHandle, joystickButton);
                        joystickButtonState.IsJoystickButtonDown = isJoystickButtonDown;
                        if (!joystickButtonState.AllowJoystickButtonPress && !isJoystickButtonDown) joystickButtonState.AllowJoystickButtonPress = true;
                        Input.JoystickStates[joystickButton] = joystickButtonState;

                    }

                }

                GlobalValues.CurrentTime = Stopwatch.GetTimestamp();
                GlobalValues.DeltaTime = (GlobalValues.CurrentTime - GlobalValues.PreviousTime) / Stopwatch.Frequency;
                GlobalValues.PreviousTime = GlobalValues.CurrentTime;

                Toolkit.Mouse.GetMouseState(window, out OpenTK.Platform.MouseState state);
                Input.CurrentMouseScroll = state.Scroll;
                Input.ScrollDelta = Input.CurrentMouseScroll - Input.PreviousMouseScroll;
                Input.PreviousMouseScroll = Input.CurrentMouseScroll;

                Input.MouseDelta = Vector2.Zero;

                GlobalValues.Time += GlobalValues.DeltaTime;

                Toolkit.Window.ProcessEvents(false);

                if (Toolkit.Window.IsWindowDestroyed(window))
                {

                    BlockGame.Unload();
                    GlobalValues.IsRunning = false;
                    // break;

                }

                if (Input.IsKeyPressed(Key.Escape))
                {

                    // Console.WriteLine("yes");
                    if (Toolkit.Window.GetCursorCaptureMode(window) == CursorCaptureMode.Locked)
                    {

                        Toolkit.Window.SetCursorCaptureMode(window, CursorCaptureMode.Normal);
                        Toolkit.Window.SetCursor(window, visibleCursor);
                        GlobalValues.IsCursorLocked = false;
                        
                    } else
                    {

                        Toolkit.Window.SetCursorCaptureMode(window, CursorCaptureMode.Locked);
                        ReadOnlySpan<byte> hiddenCursor = new ReadOnlySpan<byte>(new byte[] {0, 0, 0, 0});
                        CursorHandle cursor = Toolkit.Cursor.Create(1, 1, hiddenCursor, 0, 0);
                        Toolkit.Window.SetCursor(window, cursor);
                        GlobalValues.IsCursorLocked = true;

                    }

                }

                BlockGame.Render();

                if (Input.CurrentTypedChars.Count > 0)
                {

                    foreach (char c in Input.CurrentTypedChars)
                    {
                        Console.Write(c);
                    }
                    Console.WriteLine();
                    Input.CurrentTypedChars.Clear();

                }

                Toolkit.OpenGL.SwapBuffers(_glContext);

                sw.Stop();

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

                BlockGame.UpdateScreenSize(windowResizeEventArgs);

                Toolkit.OpenGL.SwapBuffers(_glContext);

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

            if (args is MouseButtonDownEventArgs mouseDown)
            {

                MouseState state = Input.MouseStates[mouseDown.Button];
                state.IsMouseButtonDown = true;
                Input.MouseStates[mouseDown.Button] = state;

            }

            if (args is MouseButtonUpEventArgs mouseUp)
            {

                MouseState state = Input.MouseStates[mouseUp.Button];
                state.IsMouseButtonDown = false;
                state.AllowButtonPress = true;
                Input.CurrentButtonDown = null;
                Input.CurrentButtonPressed = null;
                Input.MouseStates[mouseUp.Button] = state;

            }

            if (args is MouseMoveEventArgs mouseMove)
            {

                Input.CurrentMousePosition = mouseMove.ClientPosition;
                Input.MouseDelta = Input.CurrentMousePosition - Input.PreviousMousePosition;
                Input.PreviousMousePosition = Input.CurrentMousePosition;

            }

            if (args is TextInputEventArgs textInput)
            {

                // Console.WriteLine(textInput.Text);
                // char[] chars = textInput.Text.ToCharArray();
                Input.CurrentTypedChars.AddRange(textInput.Text);

                // for (int i = 0; i < chars.Length; i++)
                // {
                //     Console.Write(chars[i]);
                // }
                // Console.WriteLine();

                // Input.CurrentTypedStrings.Add(textInput.Text);

            }

        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {

            Console.WriteLine("an exception occured");

            Exception e = args.ExceptionObject as Exception;

            string message = e.Message;
            string[] stackTrace = e.StackTrace.Split(Environment.NewLine);

            using (FileStream stream = new FileStream("log.txt", FileMode.Create))
            {

                foreach (string line in GlobalValues.LogMessages)
                {

                    stream.Write(Encoding.UTF8.GetBytes($"{line}\n"));

                }

                stream.Write(Encoding.UTF8.GetBytes($"{e.GetType()}: {e.Message}\n"));
                stream.Write(Encoding.UTF8.GetBytes(e.StackTrace));

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
                int _fontSize = 16;

                TextRenderer.RenderLines((0, 2, 0), TextRenderer.TopLeft, (1, 1, 1), 18, 2, new string[]
                {

                    "Encountered an error.",
                    e.GetType() + ": " + e.Message,

                }.Concat(stackTrace).ToArray());

                // TextRenderer.RenderText((2, yOffset, 0), (1, 1, 1), _fontSize, TextRenderer.FilterText("<0xF00000>Encountered an error.</0xF00000>"));

                // TextRenderer.RenderText((2, yOffset + _fontSize + lineSpacing, 0), (1, 1, 1), _fontSize, e.GetType() + ": " + message);

                // for (int i = 0; i < stackTrace.Length; i++)
                // {

                //      TextRenderer.RenderText((0, 2*(_fontSize + lineSpacing) + yOffset + ( (_fontSize + lineSpacing) * i), 0), (1, 1, 1), 16, stackTrace[i]);

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
