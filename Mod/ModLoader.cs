using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Game.Util;

namespace Game.Core.Modding;

public class ModLoader
{

    public static void LoadMods()
    {

        Base.OnLoad(GlobalValues.Register);

        foreach (string directory in Directory.GetDirectories("Mods"))
        {

            foreach (string file in Directory.GetFiles(directory))
            {

                if (file.EndsWith(".dll"))
                {

                    Assembly mod = Assembly.LoadFile(Path.GetFullPath(file));
                    Type modLoadingClass = mod.GetExportedTypes().Where(type => type.GetInterface("IModLoader") != null).First();
                    modLoadingClass.GetMethod("OnLoad").Invoke(null, [GlobalValues.Register]);

                }

            }

        }

    }

}