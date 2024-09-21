using Blockgame_OpenTK.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK.Platform;
using System.Text.Json;

namespace Blockgame_OpenTK
{
    internal class Translator
    {

        struct Settings
        {

            public Dictionary<string, string> keyMap { get; set; }

        }

        public static void LoadKeymap()
        {



        }
        public static bool ResolveKeymap(string key)
        {

            if (GlobalValues.Settings.Keymap.ContainsKey(key))
            {

                if (GlobalValues.Settings.Keymap[key].Split(".")[0] == "Keydown")
                {

                    Key k;
                    Enum.TryParse(GlobalValues.Settings.Keymap[key].Split(".")[1], out k);

                    if (Input.IsKeyDown(k))
                    {

                        return true;

                    }

                }

                if (GlobalValues.Settings.Keymap[key].Split(".")[0] == "Keypress")
                {

                    Key k;
                    Enum.TryParse(GlobalValues.Settings.Keymap[key].Split(".")[1], out k);

                    if (Input.IsKeyPressed(k))
                    {

                        return true;

                    }

                }

                if (GlobalValues.Settings.Keymap[key].Split(".")[0] == "Mouse")
                {

                    MouseButton m;
                    Enum.TryParse(GlobalValues.Settings.Keymap[key].Split(".")[1], out m);

                    if (Input.IsMouseButtonDown(m))
                    {

                        return true;

                    }

                }

            }

            return false;

        }

        public static void LoadGameSettings()
        {


            GameSettings settings = JsonSerializer.Deserialize<GameSettings>(File.ReadAllText("settings.json"));

            List<string> translatedKeyMap = new List<string>();
            string[] keysInKeyMap = settings.Keymap.Keys.ToArray();

            GlobalValues.Settings = settings;

            /*
            foreach (string bind in settings.Keymap.Values)
            {

                Key key = (Key) Enum.Parse(typeof(Key), bind.Split(".")[1]);

                // Keys newKey = (Keys)Enum.Parse(typeof(Keys), GLFW.GetKeyName(key, 0));

                Key newKey = Key.Unknown;

                try
                {

                    if (char.IsAscii(GLFW.GetKeyName(key, 0).ToCharArray()[0]))
                    {

                        if (char.IsAsciiLetter(GLFW.GetKeyName(key, 0).ToCharArray()[0]))
                        {

                            newKey = (Keys)Enum.Parse(typeof(Keys), GLFW.GetKeyName(key, 0).ToUpper());

                        }
                        else
                        {

                            newKey = (Keys)Enum.Parse(typeof(Keys), GLFW.GetKeyName(key, 0));

                        }

                    }

                } catch
                {

                    newKey = key;

                }

                translatedKeyMap.Add(newKey.ToString());

                Console.WriteLine($"{key.ToString()}, {GLFW.GetKeyName(key, 0)}. new key: {newKey.ToString()}");

            }

            for (int i = 0; i < keysInKeyMap.Length; i++)
            {

                string firstValue = settings.Keymap[keysInKeyMap[i]].Split(".")[0];

                settings.Keymap[keysInKeyMap[i]] = string.Join(".", firstValue, translatedKeyMap[i]);

            }
            */


            // GlobalValues.Settings = settings;

        }

    }
}
