using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace VoxelGame.Util;

public class Logger
{
    private static FileStream _fileStream;
    public static bool DoDisplayMessages = true;

    private const string InfoString = "[INFO] ";
    private const string WarnString = "[WARN] ";
    private const string ErrString = "[ERR] ";
    
    public static void Init(string filepath)
    {
        _fileStream = File.Open(filepath, FileMode.Create);
    }

    public static void Free()
    {
        _fileStream.Close();
        _fileStream.Dispose();
    }

    public static void Info(string message)
    {
        
    }

    public static string FancyPrint<T>(T obj)
    {
        string fancy = string.Empty;

        if (obj.GetType().IsClass) fancy += "class ";
        if (obj.GetType().IsValueType) fancy += "struct ";
        fancy += obj.GetType().Name;
        fancy += ":\n\n";
        foreach (FieldInfo field in obj.GetType().GetFields())
        {
            fancy += $"field {field.Name}: \n{field.GetValue(obj)}\n";
        }

        fancy += "\n";
        foreach (PropertyInfo property in obj.GetType().GetProperties())
        {
            fancy += $"property {property.Name}: \n{property.GetValue(obj)}\n";
        }

        fancy += "\n";
        foreach (MethodInfo method in obj.GetType().GetMethods())
        {
            fancy += $"{method.ReturnType.Name} {method.Name}(";
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                fancy += $"{parameter.ParameterType} {parameter.Name}, ";
            }

            fancy = fancy.TrimEnd();
            fancy = fancy.TrimEnd(',');
            fancy += ");\n";
        }

        return fancy;
    }
}