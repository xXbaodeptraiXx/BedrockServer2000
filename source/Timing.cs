using System;

namespace BedrockServer2000
{
	public static class Timing
	{
		public static string LogDateTime()
		{
			DateTime now = DateTime.Now;
			string logDateTime = $"[{now.Day}/{now.Month}/{now.Year}-{now.Hour}:{now.Minute}:{now.Second}]";
			// example logDateTime string: "[30/6/2021-13:42:7]"
			return logDateTime;
		}
	}
}