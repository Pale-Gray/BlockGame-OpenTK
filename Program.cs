using System;
using System.Text;
using System.Threading;
using OpenTK.Platform;
using OpenTK.Graphics;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.IO;
using Blockgame_OpenTK.BlockProperty;
using Blockgame_OpenTK.Core.Worlds;
using Tomlet;
using Tomlet.Models;
using GameLogger = Blockgame_OpenTK.Util.GameLogger;
using Direction = Blockgame_OpenTK.BlockUtil.Direction;
using Blockgame_OpenTK.Audio;

namespace Blockgame_OpenTK
{
    internal class Program
    {

        private static OpenGLContextHandle _glContext;
        public static void Main(string[] args)
        {
            
            if (args.Length == 0)
            {
                GameLogger.Log("Starting in client mode.", Severity.Info);
            }
            if (args.Length == 1)
            {
                if (args[0].ToLower() == "client") Util.GameLogger.Log("Starting in client mode.", Severity.Info);
            }
            if (args.Length > 1)
            {
                if (args[0].ToLower() == "server") Util.GameLogger.Log("Starting in server mode.", Severity.Info);
            }
            TomletMain.RegisterMapper(

                vector =>
                {
                    TomlArray arr = new TomlArray();
                    arr.Add(vector.X);
                    arr.Add(vector.Y);
                    arr.Add(vector.Z);
                    return arr;
                },

                value =>
                {
                    if (value is not TomlArray) return Vector3.Zero;
                    if (((TomlArray)value).Count != 3) return Vector3.Zero;

                    float x = float.Parse(((TomlArray)value)[0].StringValue);
                    float y = float.Parse(((TomlArray)value)[1].StringValue);
                    float z = float.Parse(((TomlArray)value)[2].StringValue);
                    
                    return (x, y, z);

                } 
            );
            
            TomletMain.RegisterMapper(
                direction =>
                {
                    switch (direction)
                    {
                        case Direction.Top:
                            return new TomlString("top");
                        case Direction.Bottom:
                            return new TomlString("bottom");
                        case Direction.Left:
                            return new TomlString("left");
                        case Direction.Right:
                            return new TomlString("right");
                        case Direction.Front:
                            return new TomlString("front");
                        case Direction.Back:
                            return new TomlString("back");
                        default:
                            return new TomlString("none");
                    }
                },

                value =>
                {

                    if (value is not TomlString) return Direction.None;
                    switch (((TomlString)value).Value)
                    {
                        case "top":
                            return Direction.Top;
                        case "bottom":
                            return Direction.Bottom;
                        case "left":
                            return Direction.Left;
                        case "right":
                            return Direction.Right;
                        case "front":
                            return Direction.Front;
                        case "back":
                            return Direction.Back;
                        default:
                            return Direction.None;
                    }

                }
            );

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
            toolkitOptions.Logger = null;//new ConsoleLogger();
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
            GameLogger.Log($"Supports raw mouse? {Toolkit.Mouse.SupportsRawMouseMotion.ToString()}", Severity.Info);
            CursorHandle visibleCursor = Toolkit.Cursor.Create(SystemCursorType.Default);
            
            ReadOnlySpan<byte> hd = new ReadOnlySpan<byte>(new byte[] {0, 0, 0, 0});
            CursorHandle c1 = Toolkit.Cursor.Create(1, 1, hd, 0, 0);
            Toolkit.Window.SetCursor(window, c1);
            {
                
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
                
                if (Input.IsKeyDown(Key.LeftControl))
                {

                    if (Input.IsKeyPressed(Key.V))
                    {

                        string text = Toolkit.Clipboard.GetClipboardText() ?? string.Empty;
                        text = text.Replace(Environment.NewLine, "\r").Replace("\r", "\n");
                        Input.CurrentTypedChars.AddRange(text);

                    }
                    
                }
                
                if (GlobalValues.IsCursorLocked)
                {
                    Toolkit.Mouse.GetPosition(window, out Vector2 position);
                    Input.MouseDelta = position - Input.PreviousMousePosition;
                    Input.PreviousMousePosition = position;
                }
                
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

                // Input.MouseDelta = Vector2.Zero;

                GlobalValues.Time += GlobalValues.DeltaTime;

                Toolkit.Window.ProcessEvents(false);
                
                // if (Input.MouseDelta != Vector2.Zero) Console.WriteLine(Input.MouseDelta);

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

                    Input.CurrentTypedChars.Clear();

                }

                Toolkit.OpenGL.SwapBuffers(_glContext);

            }
            
            PackedWorldGenerator.Unload();
            
            BlockGame.Unload();
            AudioPlayer.Unload();
            Toolkit.Window.Destroy(window);
            GameLogger.SaveToFile("log");

        }

        static void EventRaised(PalHandle? handle, PlatformEventType type, EventArgs args)
        {

            if (args is CloseEventArgs closeEventArgs)
            {
                
                // Toolkit.Window.Destroy(closeEventArgs.Window);
                // PackedWorldGenerator.Unload();
                // PackedWorldGenerator.SetAllResetEvents();
                GlobalValues.IsRunning = false;
                // Environment.Exit(0);

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

            if (args is TextInputEventArgs textInput)
            {

                Input.CurrentTypedChars.AddRange(textInput.Text.Replace(Environment.NewLine, "\r").Replace("\r", "\n"));

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
