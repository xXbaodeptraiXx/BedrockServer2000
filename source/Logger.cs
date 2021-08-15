using System;
using System.IO;
using System.Text;

namespace BedrockServer2000
{
    static class Logger
    {
        public static StreamWriter writer;

        public static void Log(string logMessage)
        {
            writer.WriteLine($"{Timing.LogDateTime()}  :{logMessage}");
            writer.WriteLine("-------------------------------");  
        }
    }
}
