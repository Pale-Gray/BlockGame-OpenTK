using System;
using System.Collections.Generic;
using System.Reflection;

namespace Game.BlockProperty;

public class BlockPropertyUtils
{

    public static void AsString(IBlockProperties property)
    {

        foreach (FieldInfo field in property.GetType().GetFields())
        {

            string str = string.Empty;
            str += $"{field.Name} ";
            if (field.GetValue(property) is List<int>)
            {   
                str += "[";
                foreach (object value in (List<int>)field.GetValue(property))
                {

                    str += $"{value}, ";

                }
                str += "]";
            } else if (field.GetValue(property) is List<string>)
            {
                str += "[";
                foreach (object value in (List<string>)field.GetValue(property))
                {

                    str += $"{value}, ";

                }
                str += "]";
            } else
            {

                str += $"{field.GetValue(property)}";

            }
            Console.WriteLine(str);

        }

    }

}   