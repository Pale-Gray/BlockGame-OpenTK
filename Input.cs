using OpenTK.Platform;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Platform.Native.Windows;
using System.Reflection;
using FreeTypeSharp;
using OpenTK.Audio.OpenAL;

namespace Blockgame_OpenTK
{
    public struct KeyState
    {

        public bool IsKeyDown = false;

        public KeyState() { }

    }

    public struct MouseState
    {

        public bool IsMouseButtonDown = false;

        public MouseState() { }

    }

    public struct JoystickState
    {

        public bool IsJoystickButtonDown = false;

        public JoystickState() { }

    }

    public class Input
    {

        public static Key CurrentKey { get; private set; } = Key.Unknown;
        public static MouseButton? CurrentMouseButtonDown { get; private set; } = null;
        public static Vector2 CurrentMousePosition { get; private set; } = Vector2.Zero;
        public static Vector2 MouseDelta { get; private set; } = Vector2.Zero;

        public static Vector2 FocusAwareMouseDelta { get; private set; } = Vector2.Zero;

        private static Vector2 _focusAwareMousePosition = Vector2.Zero;
        public static Vector2 MousePosition { get; private set; } = Vector2.Zero;

        private static Vector2 _mouseScroll = Vector2.Zero;
        public static Vector2 ScrollDelta { get; private set; } = Vector2.Zero;

        public static Key CurrentKeyDown = Key.Unknown;
        public static Key CurrentKeyPressed = Key.Unknown;

        private static Dictionary<Key, KeyState> _previousKeyStates = new();
        private static Dictionary<Key, KeyState> _keyStates = new();
        private static Dictionary<MouseButton, MouseState> _previousMouseStates = new();
        private static Dictionary<MouseButton, MouseState> _mouseStates = new();
        public static Dictionary<JoystickButton, JoystickState> JoystickStates = new();
        public static bool IsMouseFocused { get; private set; } = false;

        public static Vector2 JoystickLeftAxis = Vector2.Zero;
        public static Vector2 JoystickRightAxis = Vector2.Zero;

        public static float LeftTrigger = 0.0f;
        public static float RightTrigger = 0.0f;

        public static List<char> CurrentTypedChars = new List<char>();

        public static JoystickHandle PlayerOneJoystickHandle;

        public static void Initialize(WindowHandle window)
        {

            Toolkit.Mouse.GetPosition(window, out Vector2 currentMousePosition);
            CurrentMousePosition = currentMousePosition;

            Toolkit.Mouse.GetMouseState(window, out OpenTK.Platform.MouseState state);
            _mouseScroll = state.Scroll;

            foreach (Key key in Enum.GetValues(typeof(Key)))
            {

                _keyStates.Add(key, new KeyState());
                _previousKeyStates.Add(key, new KeyState());

            }

            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
            {

                _mouseStates.Add(button, new MouseState());
                _previousMouseStates.Add(button, new MouseState());

            }

            foreach (JoystickButton joystickButton in Enum.GetValues(typeof(JoystickButton)))
            {

                JoystickStates.Add(joystickButton, new JoystickState());

            }

        }

        public static void CheckForController(int joystickIndex)
        {

            Console.WriteLine($"Checking for controller at index {joystickIndex}");
            if (Toolkit.Joystick.IsConnected(joystickIndex))
            {

                PlayerOneJoystickHandle = Toolkit.Joystick.Open(joystickIndex); // first connected controller
                Console.WriteLine("A controller is connected");
                Console.WriteLine(Toolkit.Joystick.GetName(PlayerOneJoystickHandle) ?? "null");
                Toolkit.Joystick.TryGetBatteryInfo(PlayerOneJoystickHandle, out GamepadBatteryInfo batteryInfo);
                Console.WriteLine($"Charge level: {batteryInfo.ChargeLevel} ({batteryInfo.ChargeLevel * 100.0f})");
                Console.WriteLine($"Battery type: {batteryInfo.BatteryType.ToString()}");

            }
            else
            {

                Console.WriteLine($"There is no controller currently connected at index {joystickIndex}");

            }

        }

        public static void Poll(WindowHandle window)
        {

            if (CurrentTypedChars.Count > 0) CurrentTypedChars.Clear();

            Toolkit.Mouse.GetPosition(window, out Vector2 mousePosition);
            if (IsMouseFocused)
            {
                FocusAwareMouseDelta = _focusAwareMousePosition - mousePosition;
                _focusAwareMousePosition = mousePosition;  
            }      

            MouseDelta = MousePosition - mousePosition;
            MousePosition = mousePosition;

            Toolkit.Mouse.GetMouseState(window, out OpenTK.Platform.MouseState state);
            
            IsMouseFocused = Toolkit.Window.GetCursorCaptureMode(window) == CursorCaptureMode.Locked;
            
            ScrollDelta = _mouseScroll - state.Scroll;
            _mouseScroll = state.Scroll;

            foreach (KeyValuePair<Key, KeyState> keyState in _keyStates)
            {

                _previousKeyStates[keyState.Key] = keyState.Value;

            }

            foreach (KeyValuePair<MouseButton, MouseState> mouseState in _mouseStates)
            {

                _previousMouseStates[mouseState.Key] = mouseState.Value;

            }

        }

        // TODO: fix for abstraction
        public static bool IsLeftTriggerDown(float threshhold = 0.5f)
        {

            if (LeftTrigger > 0.0f) { Console.WriteLine(LeftTrigger); }
            return LeftTrigger >= threshhold;

        }

        // TODO: fix for abstraction
        public static bool IsRightTriggerDown(float threshhold = 0.5f)
        {

            if (RightTrigger > 0.0f) { Console.WriteLine(RightTrigger); }
            return RightTrigger >= threshhold;

        }

        // TODO: fix for abstraction
        public static bool IsJoystickButtonDown(JoystickButton joystickButton)
        {

            return JoystickStates[joystickButton].IsJoystickButtonDown;

        }

        // TODO: fix for abstraction
        public static bool IsJoystickButtonPressed(JoystickButton joystickButton)
        {

            if (JoystickStates[joystickButton].IsJoystickButtonDown)//  && JoystickStates[joystickButton].AllowJoystickButtonPress)
            {

                JoystickState state = JoystickStates[joystickButton];
                // state.AllowJoystickButtonPress = false;
                JoystickStates[joystickButton] = state;
                return true;

            }

            return false;

        }

        public static bool IsAnyKeyDown()
        {

            if (CurrentKeyDown != Key.Unknown) return true;

            return false;

        }

        public static bool IsKeyDown(Key key)
        {

            if (_keyStates[key].IsKeyDown) return true;

            return false;

        }

        public static bool IsAnyKeyPressed()
        {

            if (CurrentKeyDown != Key.Unknown && !_previousKeyStates[CurrentKeyDown].IsKeyDown) return true;

            return false;

        }

        public static bool IsKeyPressed(Key key)
        {

            if (_keyStates[key].IsKeyDown && !_previousKeyStates[key].IsKeyDown) return true;

            return false;

        }

        public static bool IsAnyMouseButtonDown()
        {

            if (CurrentMouseButtonDown != null) return true;

            return false;

        }

        public static bool IsMouseButtonDown(MouseButton button)
        {

            if (_mouseStates[button].IsMouseButtonDown) return true;

            return false;

        }

        public static bool IsAnyMouseButtonPressed()
        {

            if (CurrentMouseButtonDown != null && !_previousMouseStates[CurrentMouseButtonDown??MouseButton.Button1].IsMouseButtonDown) return true;

            return false;

        }

        public static bool IsMouseButtonPressed(MouseButton button)
        {

            if (_mouseStates[button].IsMouseButtonDown && !_previousMouseStates[button].IsMouseButtonDown) return true;

            return false;

        }

        public static void OnKeyDown(Key key)
        {

            KeyState state = _keyStates[key];
            CurrentKeyDown = key;
            state.IsKeyDown = true;
            _keyStates[key] = state;

        }

        public static void OnKeyUp(Key key)
        {

            KeyState state = _keyStates[key];
            CurrentKeyDown = Key.Unknown;
            state.IsKeyDown = false;
            _keyStates[key] = state;

        }

        public static void OnMouseDown(MouseButton button)
        {

            MouseState state = _mouseStates[button];
            CurrentMouseButtonDown = button;
            state.IsMouseButtonDown = true;
            _mouseStates[button] = state;

        }

        public static void OnMouseUp(MouseButton button)
        {

            MouseState state = _mouseStates[button];
            CurrentMouseButtonDown = null;
            state.IsMouseButtonDown = false;
            _mouseStates[button] = state;

        }

    }

}
