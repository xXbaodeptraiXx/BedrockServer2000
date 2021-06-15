using System;

namespace BedrockServer2000
{
	public class CustomConsoleColor
	{
		public static void SetColor_WorkStart()
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
		}

		public static void SetColor_Work()
		{
			Console.ForegroundColor = ConsoleColor.Blue;
		}

		public static void SetColor_Success()
		{
			Console.ForegroundColor = ConsoleColor.Green;
		}

		public static void SetColor_Warning()
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
		}

		public static void SetColor_Error()
		{
			Console.ForegroundColor = ConsoleColor.Red;
		}
	}
}