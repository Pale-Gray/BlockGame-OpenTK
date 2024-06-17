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
                    return "[Info] ";
                case DebugMessageType.Warning:
                    return "[Warning] ";
                case DebugMessageType.Error:
                    return "[Error] ";
                case DebugMessageType.Fatal_Error:
                    return "[Fatal] ";
                default:
                    return "[Info] ";
            
            }

        }

    }
}
