using System;

namespace BedrockServer2000
{
	public static class Conversions
	{
		// TIme convertions
		public static int HourToMilliseconds(int hour) => hour * 3600000;
		public static int MinuteToMilliseconds(int minute) => minute * 60000;

		public static AutoBackupTimeUnit StringToAutoBackupTimeUnit(string timeUnit)
		=> timeUnit.ToLower() switch
		{
			"minute" => AutoBackupTimeUnit.Minute,
			"hour" => AutoBackupTimeUnit.Hour,
			_ => AutoBackupTimeUnit.None
		};

		public static LoggingLevel StringToLoggingLevel(string loggingLevel)
		=> loggingLevel.ToLower() switch
		{
			"debug" => LoggingLevel.Debug,
			"info" => LoggingLevel.Info,
			"warning" => LoggingLevel.Warning,
			"error" => LoggingLevel.Error,
			"fatal" => LoggingLevel.Fatal,
			_ => LoggingLevel.None
		};

		public static ApplicationLoggingLevel StringToApplicationLoggingLevel(string loggingLevel)
		=> loggingLevel.ToLower() switch
		{
			"all" => ApplicationLoggingLevel.All,
			"debug" => ApplicationLoggingLevel.Debug,
			"info" => ApplicationLoggingLevel.Info,
			"warning" => ApplicationLoggingLevel.Warning,
			"error" => ApplicationLoggingLevel.Error,
			"fatal" => ApplicationLoggingLevel.Fatal,
			_ => ApplicationLoggingLevel.None
		};
	}
}