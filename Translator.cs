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
                    Enum.TryParse(GlobalValues.Settings.Keymap[key].Split(".")[1] == "Right" ? "Button2" : GlobalValues.Settings.Keymap[key].Split(".")[1] == "Left" ? "Button1" : "Button0", out m);

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
            GlobalValues.Settings = settings;

        }

    }
}
