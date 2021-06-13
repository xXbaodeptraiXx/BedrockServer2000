using System;
using System.Globalization;
using System.IO;

namespace BedrockServer2000
{
	public class ServerConfig
	{
		public bool serverExecutableExists { get; set; } = false;
		public bool serverRunning { get; set; } = false;
		public bool backupRunning { get; set; } = false;
		public bool loadRequest { get; set; } = false;
		public bool exitRequest { get; set; } = false;
		public bool serverWasRunningBefore { get; set; } = false;

		public bool autoStartServer { get; set; }

		public bool autoBackupOnDate { get; set; }
		public string autoBackupOnDate_Time { get; set; }

		public bool autoBackupEveryX { get; set; }
		public int autoBackupEveryXDuration { get; set; }
		public string autoBackupEveryXTimeUnit { get; set; }

		public string worldPath { get; set; }
		public string backupPath { get; set; }
		public int backupLimit { get; set; }

		public void LoadConfigs()
		{
			try
			{
				Console.WriteLine($"{Timing.LogDateTime()} Loading configs");

				serverExecutableExists = File.Exists("bedrock_server");
				Console.WriteLine($"serverExecutableExists: {serverExecutableExists}");

				if (Configs.GetValue("autoStartServer") != "true" && Configs.GetValue("autoStartServer") != "false")
				{
					Configs.SetValue("autoStartServer", "false");
					autoStartServer = false;
				}
				else
				{
					if (Configs.GetValue("autoStartServer") == "true") autoStartServer = true;
					else autoStartServer = false;
				}
				Console.WriteLine($"autoStartServer: {autoStartServer}");

				if (Configs.GetValue("autoBackupOnDate") != "true" && Configs.GetValue("autoBackupOnDate") != "false")
				{
					Configs.SetValue("autoBackupOnDate", "false");
					autoBackupOnDate = false;
				}
				else
				{
					if (Configs.GetValue("autoBackupOnDate") == "true") autoBackupOnDate = true;
					else autoBackupOnDate = false;
				}
				Console.WriteLine($"autoBackupOnDate: {autoBackupOnDate}");

				if (Configs.GetValue("autoBackupOnDate_Time") == "" || !DateTime.TryParseExact(Configs.GetValue("autoBackupOnDate_Time"), "H:m:s", null, DateTimeStyles.None, out DateTime result))
				{
					Configs.SetValue("autoBackupOnDate_Time", "00:00:00");
					autoBackupOnDate_Time = "00:00:00";
				}
				else
					autoBackupOnDate_Time = Configs.GetValue("autoBackupOnDate_Time");
				Console.WriteLine($"utoBackupOnDate_Time: {autoBackupOnDate_Time}");

				if (Configs.GetValue("autoBackupEveryX") != "true" && Configs.GetValue("autoBackupEveryX") != "false")
				{
					Configs.SetValue("autoBackupEveryX", "false");
					autoBackupEveryX = false;
				}
				else
				{
					if (Configs.GetValue("autoBackupEveryX") == "true") autoBackupEveryX = true;
					else autoBackupEveryX = false;
				}
				Console.WriteLine($"autoBackupEveryX: {autoBackupEveryX}");

				if (!int.TryParse(Configs.GetValue("autoBackupEveryXDuration"), out int importVal))
				{
					Configs.SetValue("autoBackupEveryXDuration", "1");
					autoBackupEveryXDuration = 1;
				}
				else
					autoBackupEveryXDuration = Convert.ToInt32(Configs.GetValue("autoBackupEveryXDuration"));
				Console.WriteLine($"autoBackupEveryXDuration: {autoBackupEveryXDuration}");

				if (Configs.GetValue("autoBackupEveryXTimeUnit") != "minute" && Configs.GetValue("autoBackupEveryXTimeUnit") != "hour")
				{
					Configs.SetValue("autoBackupEveryXTimeUnit", "hour");
					autoBackupEveryXTimeUnit = "hour";
				}
				else autoBackupEveryXTimeUnit = Configs.GetValue("autoBackupEveryXTimeUnit");
				Console.WriteLine($"autoBackupEveryXTimeUnit: {autoBackupEveryXTimeUnit}");

				if (Configs.GetValue("worldPath") == "" && Directory.Exists("worlds"))
				{
					if (Directory.GetDirectories("worlds").Length >= 1)
					{
						Configs.SetValue("worldPath", Directory.GetDirectories("worlds")[0]);
						worldPath = Directory.GetDirectories("worlds")[0];
					}
				}
				else
					worldPath = Configs.GetValue("worldPath");
				Console.WriteLine($"worldPath: {worldPath}");

				backupPath = Configs.GetValue("backupPath");
				Console.WriteLine($"backupPath: {backupPath}");

				if (!int.TryParse(Configs.GetValue("backupLimit"), out importVal))
				{
					Configs.SetValue("backupLimit", "32");
					backupLimit = 32;
				}
				else if (importVal < 1)
				{
					Configs.SetValue("backupLimit", "32");
					backupLimit = 32;
				}
				else
					backupLimit = Convert.ToInt32(Configs.GetValue("backupLimit"));
				Console.WriteLine($"backupLimit: {backupLimit}");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.Source);
				Console.WriteLine(e.StackTrace);
				Console.WriteLine(e.Data);
			}
		}
	}
}