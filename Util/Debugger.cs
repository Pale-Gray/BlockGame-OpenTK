using System;

namespace Blockgame_OpenTK.Util
{
    enum Severity
    { 
    
        Info,
        Warning,
        Error,
    
    };

    internal class Debugger
    {

        public static void Log(string message, Severity type)
        {

            String infoType = EvaluateDebugMessageType(type);

            string log = $"[{infoType}] {message}";

            GlobalValues.LogMessages.Add(log);
            Console.WriteLine(log);

        }

        private static String EvaluateDebugMessageType(Severity type)
        {

            switch (type) 
            { 
            
                case Severity.Info:
                    return "INFO";
                case Severity.Warning:
                    return "WARNING";
                case Severity.Error:
                    return "ERROR";
                default:
                    return "INFO";
            
            }

        }

    }
}
