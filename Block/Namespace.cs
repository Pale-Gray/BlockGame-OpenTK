using System.Runtime.CompilerServices;
using LiteNetLib;

namespace Game.e;
public class Namespace
{

    public static string GetPrefix(string name)
    {

        for (int i = 0; i < name.Length; i++)
        {

            if (name[i] == '.')
            {
                return name.Substring(0, i);
            }

        }
        return "Error";

    }

    public static string GetSuffix(string name)
    {

        for (int i = name.Length - 1; i >= 0; i--)
        {

            if (name[i] == '.')
            {

                return name.Substring(i + 1, name.Length - (i + 1));

            }

        }
        return "Error";

    }

}