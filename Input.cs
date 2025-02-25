using OpenTK.Platform;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Platform.Native.Windows;
using System.Reflection;

namespace Blockgame_OpenTK
{
    public struct KeyState
    {

        public bool IsKeyDown = false;
        public bool AllowKeyPress = true;

        public KeyState() { }

    }

    public struct MouseState
    {

        public bool IsMouseButtonDown = false;
        public bool AllowButtonPress = true;

        public MouseState() { }

    }

    public struct JoystickState
    {

        public bool IsJoystickButtonDown = false;
        public bool AllowJoystickButtonPress = true;

        public JoystickState() { }

    }

    public class Input
    {

        public static bool IsCurrentKeyPressed = true;
        public static bool IsAnyCurrentKeyPressed = true;
        public static bool IsCurrentKeyRepeat = false;
        public static Key CurrentKey = Key.Unknown;
        public static Key LastKeyPressed = Key.Unknown;

        public static bool IsCurrentMouseButtonPressed = true;
        public static bool IsAnyCurrentMouseButtonPressed = true;
        public static MouseButton? CurrentButtonDown = null;
        public static MouseButton? CurrentButtonPressed = null;

        public static Vector2 CurrentMousePosition = Vector2.Zero;
        public static Vector2 PreviousMousePosition = Vector2.Zero;
        public static Vector2 MouseDelta = Vector2.Zero;

        public static Vector2 MousePosition = Vector2.Zero;

        public static Vector2 CurrentMouseScroll = Vector2.Zero;
        public static Vector2 PreviousMouseScroll = Vector2.Zero;
        public static Vector2 ScrollDelta = Vector2.Zero;

        public static Key CurrentKeyDown = Key.Unknown;
        public static Key CurrentKeyPressed = Key.Unknown;

        public static Dictionary<Key, KeyState> KeyStates = new Dictionary<Key, KeyState>();
        public static Dictionary<MouseButton, MouseState> MouseStates = new Dictionary<MouseButton, MouseState>();
        public static Dictionary<JoystickButton, JoystickState> JoystickStates = new Dictionary<JoystickButton, JoystickState>();

        public static Vector2 JoystickLeftAxis = Vector2.Zero;
        public static Vector2 JoystickRightAxis = Vector2.Zero;

        public static float LeftTrigger = 0.0f;
        public static float RightTrigger = 0.0f;

        public static List<string> CurrentTypedStrings = new List<string>();
        public static List<char> CurrentTypedChars = new List<char>();

        public static JoystickHandle PlayerOneJoystickHandle;

        public static void Initialize()
        {

            foreach (Key key in Enum.GetValues(typeof(Key)))
            {

                KeyStates.Add(key, new KeyState());

            }

            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
            {

                MouseStates.Add(button, new MouseState());

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
                // Toolkit.Joystick.SetVibration(PlayerOneJoystickHandle, 0.0f, 0.0f);

            }
            else
            {

                Console.WriteLine($"There is no controller currently connected at index {joystickIndex}");

            }

        }

        public static bool IsLeftTriggerDown(float threshhold = 0.5f)
        {

            if (LeftTrigger > 0.0f) { Console.WriteLine(LeftTrigger); }
            return LeftTrigger >= threshhold;

        }

        public static bool IsRightTriggerDown(float threshhold = 0.5f)
        {

            if (RightTrigger > 0.0f) { Console.WriteLine(RightTrigger); }
            return RightTrigger >= threshhold;

        }

        public static bool IsJoystickButtonDown(JoystickButton joystickButton)
        {

            return JoystickStates[joystickButton].IsJoystickButtonDown;

        }

        public static bool IsJoystickButtonPressed(JoystickButton joystickButton)
        {

            if (JoystickStates[joystickButton].IsJoystickButtonDown && JoystickStates[joystickButton].AllowJoystickButtonPress)
            {

                JoystickState state = JoystickStates[joystickButton];
                state.AllowJoystickButtonPress = false;
                JoystickStates[joystickButton] = state;
                return true;

            }

            return false;

        }

        public static bool IsAnyKeyDown()
        {

            foreach (Key key in KeyStates.Keys)
            {

                if (KeyStates[key].IsKeyDown)
                {

                    CurrentKeyDown = key;
                    return true;

                }

            }

            return false;

        }

        public static bool IsKeyDown(Key key)
        {

            if (KeyStates[key].IsKeyDown) return true;

            return false;

        }

        public static bool IsAnyKeyPressed()
        {

            foreach (Key key in KeyStates.Keys)
            {

                if (KeyStates[key].IsKeyDown && KeyStates[key].AllowKeyPress)
                {

                    KeyState keyState = KeyStates[key];
                    keyState.AllowKeyPress = false;
                    CurrentKeyPressed = key;
                    return true;

                }

            }

            return false;

        }

        public static bool IsKeyPressed(Key key)
        {

            if (KeyStates[key].IsKeyDown && KeyStates[key].AllowKeyPress)
            {

                KeyState keyState = KeyStates[key];
                keyState.AllowKeyPress = false;
                KeyStates[key] = keyState;
                return true;

            }

            return false;

        }

        public static bool IsAnyMouseButtonDown()
        {

            // return CurrentButton != null;
            foreach (MouseButton button in MouseStates.Keys)
            {

                if (MouseStates[button].IsMouseButtonDown)
                {

                    CurrentButtonDown = button;
                    return true;

                }

            }

            return false;

        }

        public static bool IsMouseButtonDown(MouseButton button)
        {

            if (MouseStates[button].IsMouseButtonDown) return true;

            return false;

        }

        public static bool IsAnyMouseButtonPressed()
        {

            foreach (MouseButton button in MouseStates.Keys)
            {

                if (MouseStates[button].IsMouseButtonDown && MouseStates[button].AllowButtonPress)
                {

                    MouseState state = MouseStates[button];
                    state.AllowButtonPress = false;
                    MouseStates[button] = state;
                    CurrentButtonPressed = button;
                    return true;

                }

            }

            return false;

        }

        public static bool IsMouseButtonPressed(MouseButton button)
        {

            if (MouseStates[button].IsMouseButtonDown && MouseStates[button].AllowButtonPress)
            {

                MouseState state = MouseStates[button];
                state.AllowButtonPress = false;
                MouseStates[button] = state;
                return true;

            }

            return false;

        }

    }

}
