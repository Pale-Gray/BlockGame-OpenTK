using OpenTK.Platform;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK
{
    struct KeyState
    {

        public bool IsKeyDown = false;
        public bool AllowKeyPress = true;

        public KeyState() { }

    }

    struct MouseState
    {

        public bool IsMouseButtonDown = false;
        public bool AllowButtonPress = true;

        public MouseState() { }

    }

    internal class Input
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

        public static Vector2 CurrentMouseScroll = Vector2.Zero;
        public static Vector2 PreviousMouseScroll = Vector2.Zero;
        public static Vector2 ScrollDelta = Vector2.Zero;

        public static Key CurrentKeyDown = Key.Unknown;
        public static Key CurrentKeyPressed = Key.Unknown;

        public static Dictionary<Key, KeyState> KeyStates = new Dictionary<Key, KeyState>();
        public static Dictionary<MouseButton, MouseState> MouseStates = new Dictionary<MouseButton, MouseState>();

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
