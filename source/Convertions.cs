using System;

namespace BedrockServer2000
{
	public class Conversions
	{
		// TIme convertions
		public static int HourToMilliseconds(int hour) => hour * 3600000;
		public static int MinuteToMilliseconds(int minute) => minute * 60000;

		// Converts backup folder formatted as "day_month_year-hour_minute_second" to DateTime value
		public static DateTime GetBackupDate(string directoryName)
		{
			string date = directoryName.Split("-", StringSplitOptions.RemoveEmptyEntries)[0];
			string time = directoryName.Split("-", StringSplitOptions.RemoveEmptyEntries)[1];

			int year = Convert.ToInt32(date.Split("_", StringSplitOptions.RemoveEmptyEntries)[2]);
			int month = Convert.ToInt32(date.Split("_", StringSplitOptions.RemoveEmptyEntries)[1]);
			int day = Convert.ToInt32(date.Split("_", StringSplitOptions.RemoveEmptyEntries)[0]);

			int hour = Convert.ToInt32(time.Split("_", StringSplitOptions.RemoveEmptyEntries)[0]);
			int minute = Convert.ToInt32(time.Split("_", StringSplitOptions.RemoveEmptyEntries)[1]);
			int second = Convert.ToInt32(time.Split("_", StringSplitOptions.RemoveEmptyEntries)[2]);

			return new DateTime(year, month, day, hour, minute, second);
		}
	}
}