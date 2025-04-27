using System;
using System.Text;
using OpenTK.Platform;
using OpenTK.Graphics;
using Game.Util;
using System.Diagnostics;
using System.IO;
using Game.Audio;
using Game.Core.Networking;

namespace Game
{
    public class Program
    {

        private static OpenGLContextHandle _glContext;
        private static WindowHandle _window;
        public static void Main(string[] args)
        {

            Console.WriteLine(Game.e.Namespace.GetPrefix("Game.GrassBlock"));
            Console.WriteLine(Game.e.Namespace.GetSuffix("Game.GrassBlock"));

            Console.WriteLine(Path.Combine("Hello", "Fucking", "World", "hi.txt"));

            // Console.WriteLine(Path.GetDirectoryName(Path.Combine("bin", "Release", "net9.0", "Blockgame-OpenTK.dll")));

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

            // ImageLoader.LoadPng("Resources/Textures/test.png");
            Console.OutputEncoding = Encoding.Unicode;

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);
            
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
                NetworkingValues.Client = new Client();
            } else
            {
                if (args.Length >= 1)
                {
                    if (args[0].ToLower() == "server")
                    {
                        GameLogger.Log("Starting in server mode.");
                        // make server
                        NetworkingValues.Server = new Server();
                        NetworkingValues.Server.StartMultiplayer();
                    } else
                    {
                        GameLogger.Log("There were no valid arguments, so starting in client mode.");
                        NetworkingValues.Client = new Client();
                    }
                }
            }

            GlobalValues.CurrentTime = Stopwatch.GetTimestamp();

            if (NetworkingValues.Client != null)
            {

                EventQueue.EventRaised += EventRaised;
                InitializeWindow(contextSettings);

                Input.Initialize(_window);

            }

            NetworkingValues.Client?.Load();

            double currentTickTime = 0.0;

            while (GlobalValues.IsRunning)
            {

                while (currentTickTime >= 1 / GlobalValues.TickRate)
                {
                    NetworkingValues.Server?.TickUpdate();
                    NetworkingValues.Client?.TickUpdate();
                    currentTickTime -= 1 / GlobalValues.TickRate;
                }
                
                GlobalValues.Time += GlobalValues.DeltaTime;
                currentTickTime += GlobalValues.DeltaTime;

                long currentTime = Stopwatch.GetTimestamp();
                GlobalValues.DeltaTime = (currentTime - GlobalValues.CurrentTime) / Stopwatch.Frequency;
                GlobalValues.CurrentTime = currentTime;

                Toolkit.Window.ProcessEvents(false);

                NetworkingValues.Server?.Update();
                NetworkingValues.Client?.Update();

                if (NetworkingValues.Client != null)
                {

                    // if (Input.FocusAwareMouseDelta != Vector2.Zero) Console.WriteLine(Input.FocusAwareMouseDelta);

                    if (Input.IsKeyPressed(Key.Escape))
                    {

                        if (Input.IsMouseFocused)
                        {

                            Console.WriteLine("unfocusing");
                            Toolkit.Window.SetCursor(_window, Toolkit.Cursor.Create(SystemCursorType.Default));
                            Toolkit.Window.SetCursorCaptureMode(_window, CursorCaptureMode.Normal);

                        } else
                        {

                            Console.WriteLine("focusing");
                            Span<byte> arr = new byte[4];
                            CursorHandle handle = Toolkit.Cursor.Create(1, 1, arr, 0, 0);
                            Toolkit.Window.SetCursor(_window, handle);
                            Toolkit.Window.SetCursorCaptureMode(_window, CursorCaptureMode.Locked);

                        }

                    }

                    AudioPlayer.Poll();
                    Input.Poll(_window);
                    Toolkit.OpenGL.SwapBuffers(_glContext);

                }

            }
            
            NetworkingValues.Server?.Unload();
            NetworkingValues.Client?.Unload();

            if (NetworkingValues.Client != null)
            {

                // AudioPlayer.Unload();
                Toolkit.Window.Destroy(_window);

            }

            GameLogger.SaveToFile("log");

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
            Toolkit.Window.SetCursorCaptureMode(_window, CursorCaptureMode.Normal);
            Toolkit.OpenGL.SetSwapInterval(0);
            
            GameLogger.Log($"Supports raw mouse? {Toolkit.Mouse.SupportsRawMouseMotion}");
            CursorHandle visibleCursor = Toolkit.Cursor.Create(SystemCursorType.Default);

            ReadOnlySpan<byte> icon = new ReadOnlySpan<byte>([ 255, 255, 255, 255, 0, 0, 0, 255, 0, 0, 0, 255, 255, 255, 255, 255]);
            IconHandle handle = Toolkit.Icon.Create(2, 2, icon);
            // Toolkit.Window.SetIcon(_window, handle);

        }

        static void EventRaised(PalHandle handle, PlatformEventType type, EventArgs args)
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

                GlobalValues.Width = windowResizeEventArgs.NewClientSize.X;
                GlobalValues.Height = windowResizeEventArgs.NewClientSize.Y;

                // BlockGame.UpdateScreenSize(windowResizeEventArgs);
                NetworkingValues.Client?.OnResize(windowResizeEventArgs.Window);

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

            if (args is MouseMoveEventArgs mouseMove)
            {

                // Console.WriteLine(mouseMove.ClientPosition);

                // Input.OnMouseMove(mouseMove.ClientPosition);

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

        public static void CreateFile(string pathToFile)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(pathToFile));
            File.Create(pathToFile);

            Console.WriteLine($"Created file at {pathToFile}");
        }

    }
}
