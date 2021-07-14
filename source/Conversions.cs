using System;

namespace BedrockServer2000
{
	public static class Conversions
	{
		// TIme convertions
		public static int HourToMilliseconds(int hour) => hour * 3600000;
		public static int MinuteToMilliseconds(int minute) => minute * 60000;
		public static AutoBackupTimeUnit StringToAutoBackupTimeUnit(string timeUnit)
		{
			if (timeUnit.ToLower() == "minute") return AutoBackupTimeUnit.Minute;
			else if (timeUnit.ToLower() == "hour") return AutoBackupTimeUnit.Hour;
			else return AutoBackupTimeUnit.None;
		}
	}
}