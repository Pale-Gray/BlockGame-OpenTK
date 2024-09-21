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

    internal class Input
    {

        public static bool IsCurrentKeyPressed = true;
        public static bool IsAnyCurrentKeyPressed = true;
        public static bool IsCurrentKeyRepeat = false;
        public static Key CurrentKey = Key.Unknown;
        public static Key LastKeyPressed = Key.Unknown;

        public static bool IsCurrentMouseButtonPressed = true;
        public static bool IsAnyCurrentMouseButtonPressed = true;
        public static MouseButton? CurrentButton = null;
        public static MouseButton? LastButtonPressed = null;

        public static Vector2 CurrentMousePosition = Vector2.Zero;
        public static Vector2 PreviousMousePosition = Vector2.Zero;
        public static Vector2 MouseDelta = Vector2.Zero;

        public static Key CurrentKeyDown = Key.Unknown;
        public static Key CurrentKeyPressed = Key.Unknown;

        public static Dictionary<Key, KeyState> KeyStates = new Dictionary<Key, KeyState>();

        public static void Initialize()
        {

            foreach (Key key in Enum.GetValues(typeof(Key)))
            {

                KeyStates.Add(key, new KeyState());

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

            return CurrentButton != null;

        }

        public static bool IsMouseButtonDown(MouseButton button)
        {

            if (CurrentButton == button) return true;

            return false;

        }

        public static bool IsAnyMouseButtonPressed()
        {

            if (CurrentButton != null && IsAnyCurrentMouseButtonPressed)
            {

                IsAnyCurrentMouseButtonPressed = false;
                return true;

            }

            return false;

        }

        public static bool IsMouseButtonPressed(MouseButton button)
        {

            if (CurrentButton == button && IsCurrentMouseButtonPressed)
            {

                IsCurrentMouseButtonPressed = false;
                return true;

            }

            return false;

        }

    }

}
