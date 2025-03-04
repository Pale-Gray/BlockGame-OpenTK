using System;
using System.Text;
using OpenTK.Platform;
using OpenTK.Graphics;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.IO;
using Blockgame_OpenTK.Core.Worlds;
using Tomlet;
using Tomlet.Models;
using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Audio;
using Blockgame_OpenTK.Core.Networking;
using System.Collections.Generic;
using Blockgame_OpenTK.Core.Chunks;
using System.Net;
using System.IO.Pipes;
using System.Threading;

namespace Blockgame_OpenTK
{
    public class Program
    {

        private static OpenGLContextHandle _glContext;
        public static void Main(string[] args)
        {

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
                        case BlockUtil.Direction.Top:
                            return new TomlString("top");
                        case BlockUtil.Direction.Bottom:
                            return new TomlString("bottom");
                        case BlockUtil.Direction.Left:
                            return new TomlString("left");
                        case BlockUtil.Direction.Right:
                            return new TomlString("right");
                        case BlockUtil.Direction.Front:
                            return new TomlString("front");
                        case BlockUtil.Direction.Back:
                            return new TomlString("back");
                        default:
                            return new TomlString("none");
                    }
                },

                value =>
                {

                    if (value is not TomlString) return BlockUtil.Direction.None;
                    switch (((TomlString)value).Value)
                    {
                        case "top":
                            return BlockUtil.Direction.Top;
                        case "bottom":
                            return BlockUtil.Direction.Bottom;
                        case "left":
                            return BlockUtil.Direction.Left;
                        case "right":
                            return BlockUtil.Direction.Right;
                        case "front":
                            return BlockUtil.Direction.Front;
                        case "back":
                            return BlockUtil.Direction.Back;
                        default:
                            return BlockUtil.Direction.None;
                    }

                }
            );

            if (args.Length == 0)
            {
                GameLogger.Log("Starting in client mode");
                NetworkingValues.Client = new Client();
            } else
            {
                if (args.Length >= 1)
                {
                    if (args[0].ToLower() == "server")
                    {
                        GameLogger.Log("Starting in server mode.");
                        // GlobalValues.Server = new Server(true);
                        NetworkingValues.Server = new Server(true);
                        NetworkingValues.Server.Start();
                    } else if (args[0].ToLower() == "client")
                    {
                        GameLogger.Log("Starting in client mode.");
                        NetworkingValues.Client = new Client();
                    } else
                    {
                        GameLogger.Log("There were no valid arguments, so starting in client mode.");
                        NetworkingValues.Client = new Client();
                    }
                }
            }

            // NetworkingValues.Client = new Client(false);
            // GlobalValues.Client = new Client(true);

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

            // ThreadPool.SetMaxThreads(8, 8);
            
            ToolkitOptions toolkitOptions = new ToolkitOptions();
            toolkitOptions.ApplicationName = "Game";
            toolkitOptions.Logger = null;
            Toolkit.Init(toolkitOptions);

            OpenGLGraphicsApiHints contextSettings = new OpenGLGraphicsApiHints()
            {

                Version = new Version(4, 6),
                Profile = OpenGLProfile.Core,
                DebugFlag = false,
                DepthBits = ContextDepthBits.Depth24,
                StencilBits = ContextStencilBits.Stencil8

            };

            // Server existant means this is a server!
            if (NetworkingValues.Server != null)
            {

                Console.WriteLine("loading blockgame necesary stuf");
                BlockGame.Load(true);

                GlobalValues.CurrentTime = Stopwatch.GetTimestamp();

                while (true)
                {

                    long currTime = Stopwatch.GetTimestamp();
                    GlobalValues.DeltaTime = (GlobalValues.CurrentTime - currTime) / Stopwatch.Frequency;
                    GlobalValues.CurrentTime = currTime;

                    NetworkingValues.Server.Update();

                }

            }

            // Client existant means this is a client!
            if (NetworkingValues.Client != null)
            {

                WindowHandle window = Toolkit.Window.Create(contextSettings);
                _glContext = Toolkit.OpenGL.CreateFromWindow(window);

                Toolkit.OpenGL.SetCurrentContext(_glContext);
                GLLoader.LoadBindings(Toolkit.OpenGL.GetBindingsContext(_glContext));

                Toolkit.Window.SetTitle(window, "Game");
                Toolkit.Window.SetSize(window, (640, 480));
                Toolkit.Window.SetMode(window, WindowMode.Normal);
                
                GameLogger.Log($"Supports raw mouse? {Toolkit.Mouse.SupportsRawMouseMotion}");
                CursorHandle visibleCursor = Toolkit.Cursor.Create(SystemCursorType.Default);

                ReadOnlySpan<byte> icon = new ReadOnlySpan<byte>([ 255, 255, 255, 255, 0, 0, 0, 255, 0, 0, 0, 255, 255, 255, 255, 255]);
                IconHandle handle = Toolkit.Icon.Create(2, 2, icon);
                Toolkit.Window.SetIcon(window, handle);

                Toolkit.Mouse.GetPosition(window, out Vector2 position);
                Toolkit.Mouse.GetMouseState(window, out OpenTK.Platform.MouseState state);

                GlobalValues.PreviousTime = Stopwatch.GetTimestamp();

                Input.Initialize(window);

                BlockGame.Load(false);

                EventQueue.EventRaised += EventRaised;

                string typed = "";

                while (GlobalValues.IsRunning)
                {

                    // joystick input stuff
                    /*
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
                    */

                    GlobalValues.CurrentTime = Stopwatch.GetTimestamp();
                    GlobalValues.DeltaTime = (GlobalValues.CurrentTime - GlobalValues.PreviousTime) / Stopwatch.Frequency;
                    GlobalValues.PreviousTime = GlobalValues.CurrentTime;

                    GlobalValues.Time += GlobalValues.DeltaTime;

                    Toolkit.Window.ProcessEvents(false);

                    if (Input.IsKeyPressed(Key.Escape))
                    {

                        if (Input.IsMouseFocused)
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

                    Input.Poll(window);

                }

                PackedWorldGenerator.Unload();
            
                BlockGame.Unload();
                AudioPlayer.Unload();
                Toolkit.Window.Destroy(window);

            }
            
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

                Input.OnKeyDown(keyDown.Key);

            }

            if (args is KeyUpEventArgs keyUp)
            {

                Input.OnKeyUp(keyUp.Key);

            }

            if (args is MouseButtonDownEventArgs mouseDown)
            {
                
                Input.OnMouseDown(mouseDown.Button);

            }

            if (args is MouseButtonUpEventArgs mouseUp)
            {

                Input.OnMouseUp(mouseUp.Button);

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

    }
}
