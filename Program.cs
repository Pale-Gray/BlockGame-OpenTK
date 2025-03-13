using System;
using System.Text;
using OpenTK.Platform;
using OpenTK.Graphics;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.IO;
using Tomlet;
using Tomlet.Models;
using Blockgame_OpenTK.Audio;
using Blockgame_OpenTK.Core.Networking;

namespace Blockgame_OpenTK
{
    public class Program
    {

        private static OpenGLContextHandle _glContext;
        private static WindowHandle _window;
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

            if (args.Length == 0)
            {
                GameLogger.Log("Starting in client mode");
                InitializeWindow(contextSettings);
                EventQueue.EventRaised += EventRaised;
                Input.Initialize(_window);
                BlockGame.Load();
            } else
            {
                if (args.Length >= 1)
                {
                    if (args[0].ToLower() == "server")
                    {
                        GameLogger.Log("Starting in server mode.");
                        // make server
                        NetworkingValues.Server = new PhysicalServer();
                        NetworkingValues.Server.Start();
                    } else if (args[0].ToLower() == "client")
                    {
                        GameLogger.Log("Starting in client mode.");
                        InitializeWindow(contextSettings);
                        EventQueue.EventRaised += EventRaised;
                        Input.Initialize(_window);
                        BlockGame.Load();
                    } else
                    {
                        GameLogger.Log("There were no valid arguments, so starting in client mode.");
                        InitializeWindow(contextSettings);
                        EventQueue.EventRaised += EventRaised;
                        Input.Initialize(_window);
                        BlockGame.Load();
                    }
                }
            }

            GlobalValues.CurrentTime = Stopwatch.GetTimestamp();

            while (GlobalValues.IsRunning)
            {

                GlobalValues.Time += GlobalValues.DeltaTime;

                long currentTime = Stopwatch.GetTimestamp();
                GlobalValues.DeltaTime = (GlobalValues.CurrentTime - currentTime) / Stopwatch.Frequency;
                GlobalValues.CurrentTime = currentTime;
// 
                NetworkingValues.Server?.Update();
                NetworkingValues.Client?.Update(); 
// 
                Toolkit.Window.ProcessEvents(false);

                // call general render loop in client update method? 
                if (NetworkingValues.Server is not PhysicalServer)
                {

                    NetworkingValues.Client?.Update();
                    BlockGame.Render();

                    Input.Poll(_window);
                    AudioPlayer.Poll();
                    Toolkit.OpenGL.SwapBuffers(_glContext);

                }

            }

            if (NetworkingValues.Server is not PhysicalServer)
            {
                AudioPlayer.Unload();
                BlockGame.Unload();
                Toolkit.Window.Destroy(_window);
            }

            GameLogger.SaveToFile("log");

            // if (NetworkingValues.Server is not PhysicalServer)
            // {
// 
            //     AudioPlayer.Unload();
            //     BlockGame.Unload();
            //     Toolkit.Window.Destroy(_window);
// 
            // }
            
            // GameLogger.SaveToFile("log");

        }

        static void InitializeWindow(OpenGLGraphicsApiHints contextSettings)
        {

            _window = Toolkit.Window.Create(contextSettings);
            _glContext = Toolkit.OpenGL.CreateFromWindow(_window);

            Toolkit.OpenGL.SetCurrentContext(_glContext);
            GLLoader.LoadBindings(Toolkit.OpenGL.GetBindingsContext(_glContext));

            Toolkit.Window.SetTitle(_window, "Game");
            Toolkit.Window.SetSize(_window, (640, 480));
            Toolkit.Window.SetMode(_window, WindowMode.Normal);
            
            GameLogger.Log($"Supports raw mouse? {Toolkit.Mouse.SupportsRawMouseMotion}");
            CursorHandle visibleCursor = Toolkit.Cursor.Create(SystemCursorType.Default);

            ReadOnlySpan<byte> icon = new ReadOnlySpan<byte>([ 255, 255, 255, 255, 0, 0, 0, 255, 0, 0, 0, 255, 255, 255, 255, 255]);
            IconHandle handle = Toolkit.Icon.Create(2, 2, icon);
            Toolkit.Window.SetIcon(_window, handle);

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
