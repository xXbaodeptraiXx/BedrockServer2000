namespace BedrockServer2000
{
	public class Timing
	{
		public static int HourToMilliseconds(int hour) => hour * 3600000;
		public static int MinuteToMilliseconds(int minute) => minute * 60000;
	}
}