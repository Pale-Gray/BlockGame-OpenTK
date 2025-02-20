using System.Collections.Generic;
using System.IO;
using Tomlet;

namespace Blockgame_OpenTK.Core.Language;

public class LanguageManager
{

    private static Dictionary<string, string> _translationKeys = new();
    public static void LoadLanguage(string path)
    {

        _translationKeys = TomletMain.To<Dictionary<string, string>>(File.ReadAllText(path));

    }

    public static string GetTranslation(string translationKey)
    {

        if (_translationKeys.TryGetValue(translationKey, out string value)) return value;
        return translationKey;

    }

}   