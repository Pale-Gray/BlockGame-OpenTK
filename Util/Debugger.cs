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
            // Console.WriteLine(log);
            switch (infoType)
            {
                case "Info":
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case "Warning":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case "Error":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            Console.Write($"[{infoType}] ");

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(message);

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
