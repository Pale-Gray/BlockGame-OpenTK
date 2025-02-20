using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Font
{
    public class TextLocalizer
    {

        private static Dictionary<string, string> _localLanguage = new Dictionary<string, string>();
        public static void LoadLanguage(string lang)
        {

            _localLanguage = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine("Resources", "Data", "Languages", lang)));

        }

        public static string GetLocalizedString(string key)
        {

            return _localLanguage[key];

        }

    }
}
