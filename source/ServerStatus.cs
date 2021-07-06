using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Collections;

namespace BedrockServer2000
{
	public class ServerStatus
	{
		// server properties
		public List<Player> Players { get; set; } = new List<Player>();

		// server status
		public bool ServerExecutableExists { get; set; } = false;
		public bool ServerRunning { get; set; } = false;
		public bool BackupRunning { get; set; } = false;
		public bool ServerWasRunningBefore { get; set; } = false;
		public bool ExitCompleted { get; set; } = true;
		public bool PlayerActivitySinceLastBackup { get; set; } = false;

		// requests
		public bool LoadRequest { get; set; } = false;
		public bool ExitRequest { get; set; } = false;
		public bool BackupFileListRequest { get; set; } = false;
		public string BackupFileLUst { get; set; } = "";

		// server configs
		Hashtable ServerConfigs = new Hashtable();
		// this hashtable contains the configs for the server, data is taken from the 2 configuration files, bs2k.conf and bs2k.banlist
		// current available keys and values are:
		// autoStartServer : boolean value
		// autoBackupOnDate : boolean value
		// autoBackupOnDate_TIme : DateTIme value
		// autoBackupEveryX : boolean value
		// autoBackupEveryXDuration : positive integer value
		// autoBackupEveryXTImeUnit : 



		public bool AutoStartServer { get; set; }

		public bool AutoBackupOnDate { get; set; }
		public string AutoBackupOnDate_Time { get; set; }

		public bool AutoBackupEveryX { get; set; }
		public int AutoBackupEveryXDuration { get; set; }
		public string AutoBackupEveryXTimeUnit { get; set; }

		public string WorldPath { get; set; }
		public string BackupPath { get; set; }
		public int BackupLimit { get; set; }

		public string[] BanList { get; set; }

		public void LoadServerConfigs()
		{
			Console.WriteLine($"{Timing.LogDateTime()} Loading configs");

			ServerExecutableExists = File.Exists("bedrock_server");
			Console.WriteLine($"serverExecutableExists: {ServerExecutableExists}");

			if (Configs.GetValue("autoStartServer") != "true" && Configs.GetValue("autoStartServer") != "false")
			{
				Configs.SetValue("autoStartServer", "false");
				AutoStartServer = false;
			}
			else
			{
				if (Configs.GetValue("autoStartServer") == "true") AutoStartServer = true;
				else AutoStartServer = false;
			}
			Console.WriteLine($"autoStartServer: {AutoStartServer}");

			if (Configs.GetValue("autoBackupOnDate") != "true" && Configs.GetValue("autoBackupOnDate") != "false")
			{
				Configs.SetValue("autoBackupOnDate", "false");
				AutoBackupOnDate = false;
			}
			else
			{
				if (Configs.GetValue("autoBackupOnDate") == "true") AutoBackupOnDate = true;
				else AutoBackupOnDate = false;
			}
			Console.WriteLine($"autoBackupOnDate: {AutoBackupOnDate}");

			if (Configs.GetValue("autoBackupOnDate_Time") == "" || !DateTime.TryParseExact(Configs.GetValue("autoBackupOnDate_Time"), "H:m:s", null, DateTimeStyles.None, out DateTime result))
			{
				Configs.SetValue("autoBackupOnDate_Time", "00:00:00");
				AutoBackupOnDate_Time = "00:00:00";
			}
			else
				AutoBackupOnDate_Time = Configs.GetValue("autoBackupOnDate_Time");
			Console.WriteLine($"utoBackupOnDate_Time: {AutoBackupOnDate_Time}");

			if (Configs.GetValue("autoBackupEveryX") != "true" && Configs.GetValue("autoBackupEveryX") != "false")
			{
				Configs.SetValue("autoBackupEveryX", "false");
				AutoBackupEveryX = false;
			}
			else
			{
				if (Configs.GetValue("autoBackupEveryX") == "true") AutoBackupEveryX = true;
				else AutoBackupEveryX = false;
			}
			Console.WriteLine($"autoBackupEveryX: {AutoBackupEveryX}");

			if (!int.TryParse(Configs.GetValue("autoBackupEveryXDuration"), out int importVal))
			{
				Configs.SetValue("autoBackupEveryXDuration", "1");
				AutoBackupEveryXDuration = 1;
			}
			else
				AutoBackupEveryXDuration = Convert.ToInt32(Configs.GetValue("autoBackupEveryXDuration"));
			Console.WriteLine($"autoBackupEveryXDuration: {AutoBackupEveryXDuration}");

			if (Configs.GetValue("autoBackupEveryXTimeUnit") != "minute" && Configs.GetValue("autoBackupEveryXTimeUnit") != "hour")
			{
				Configs.SetValue("autoBackupEveryXTimeUnit", "hour");
				AutoBackupEveryXTimeUnit = "hour";
			}
			else AutoBackupEveryXTimeUnit = Configs.GetValue("autoBackupEveryXTimeUnit");
			Console.WriteLine($"autoBackupEveryXTimeUnit: {AutoBackupEveryXTimeUnit}");

			if (Configs.GetValue("worldPath") == "" && Directory.Exists("worlds"))
			{
				if (Directory.GetDirectories("worlds").Length >= 1)
				{
					Configs.SetValue("worldPath", Directory.GetDirectories("worlds")[0]);
					WorldPath = Directory.GetDirectories("worlds")[0];
				}
			}
			else
				WorldPath = Configs.GetValue("worldPath");
			Console.WriteLine($"worldPath: {WorldPath}");

			BackupPath = Configs.GetValue("backupPath");
			Console.WriteLine($"backupPath: {BackupPath}");

			if (!int.TryParse(Configs.GetValue("backupLimit"), out importVal))
			{
				Configs.SetValue("backupLimit", "32");
				BackupLimit = 32;
			}
			else if (importVal < 1)
			{
				Configs.SetValue("backupLimit", "32");
				BackupLimit = 32;
			}
			else
				BackupLimit = Convert.ToInt32(Configs.GetValue("backupLimit"));
			Console.WriteLine($"backupLimit: {BackupLimit}");

			if (!File.Exists($"{Program.appName}.banlist"))
			{
				Console.WriteLine("Ban list file not found.");

				File.Create($"{Program.appName}.banlist");
				Console.WriteLine("Empty ban list file generated.");
			}
			BanList = File.ReadAllLines($"{Program.appName}.banlist");
			Console.WriteLine($"Ban list loaded.");
			if (BanList.Length >= 1)
			{
				Console.Write("Banned players: {");
				for (int i = 0; i < BanList.Length; i += 1)
				{
					Console.Write(BanList[i]);
					if (i != BanList.Length - 1) Console.Write(", ");
				}
				Console.WriteLine("}");

				Events.BanlistScanTimer_Tick(null);
			}
			else Console.WriteLine("Ban list is empty.");
		}
	}
}