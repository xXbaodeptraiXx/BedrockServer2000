using System;

namespace BedrockServer2000
{
	public class Timing
	{
		public static string LogDateTime()
		{
			DateTime now = DateTime.Now;
			string logDateTime = $"[{now.Day}/{now.Month}/{now.Year}-{now.Hour}:{now.Minute}:{now.Second}]";
			return logDateTime;
		}
	}
}