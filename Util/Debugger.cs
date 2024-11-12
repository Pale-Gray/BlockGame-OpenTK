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

            string infoType = EvaluateDebugMessageType(type);

            string log = $"[{infoType}] {message}";

            GlobalValues.LogMessages.Add(log);
            Console.WriteLine(log);

        }

        private static string EvaluateDebugMessageType(Severity type)
        {

            switch (type) 
            { 
            
                case Severity.Info:
                    return "Info";
                case Severity.Warning:
                    return "Warning";
                case Severity.Error:
                    return "Error";
                default:
                    return "Info";
            
            }

        }

    }
}
