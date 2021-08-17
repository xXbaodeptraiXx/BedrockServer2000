using System;

namespace BedrockServer2000
{
    public static class ConsoleEx
    {
        public static StreamWriter writer;
        
        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
            Logger.Log(message);
        }

        public static void Log(string logMessage)
        {
            writer.WriteLine($"{Timing.LogDateTime()}  :{logMessage}");
            writer.WriteLine("-------------------------------");  
        }
    }
}
