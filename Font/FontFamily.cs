using System.Collections.Generic;

namespace Blockgame_OpenTK.Font;

public struct FontFamily
{

    public List<string> FontPaths = new();

    public FontFamily()
    {
                
                
                
    }

    public void AddFontPath(string path)
    {
                
        FontPaths.Add(path);
                
    }

    public void RemoveFontPath(string path)
    {

        FontPaths.Remove(path);

    }

}