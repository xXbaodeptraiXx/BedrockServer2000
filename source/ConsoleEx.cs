using System;

namespace BedrockServer2000
{
    public static class ConsoleEx
    {
        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
            Logger.Log(message);
        }
    }
}
