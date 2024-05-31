using System;

namespace Blockgame_OpenTK.Util
{
    enum DebugMessageType 
    { 
    
        Info,
        Warning,
        Error,
        Fatal_Error
    
    };

    internal class DebugMessage
    {

        public static void WriteLine(String text, DebugMessageType type)
        {

            String infoType = EvaluateDebugMessageType(type);

            Console.WriteLine("{0}{1}", infoType, text);

        }

        private static String EvaluateDebugMessageType(DebugMessageType type)
        {

            switch (type) 
            { 
            
                case DebugMessageType.Info:
                    return "[INFO] ";
                case DebugMessageType.Warning:
                    return "[WARN] ";
                case DebugMessageType.Error:
                    return "[ERR] ";
                case DebugMessageType.Fatal_Error:
                    return "[FATAL] ";
                default:
                    return "[INFO] ";
            
            }

        }

    }
}
