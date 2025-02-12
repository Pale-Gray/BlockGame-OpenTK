using System;
using System.IO;
using System.Text;

namespace Blockgame_OpenTK.Util
{
    enum Severity
    { 
    
        Info,
        Warning,
        Error,
    
    };

    internal class GameLogger
    {

        public static void Log(string message, Severity type = Severity.Info)
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

        public static void ThrowError(string message) {

            Log(message, Severity.Error);
            throw new Exception(message);

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

        public static void SaveToFile(string filename)
        {

            using (FileStream stream = File.OpenWrite($"{filename}.txt"))
            {

                foreach (string logMessage in GlobalValues.LogMessages)
                {
                    stream.Write(Encoding.UTF8.GetBytes(logMessage + Environment.NewLine));
                }
                
            }

            Log($"Saved log to {filename}.txt", Severity.Info);
            
        }

    }
}
