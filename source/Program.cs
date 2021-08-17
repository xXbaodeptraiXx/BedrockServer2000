﻿using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

namespace BedrockServer2000
{
	public static class Program
	{
		// configs
		public const string AppName = "bs2k";
		public const string AppVersion = "preAlpha-1.2";

		// this private dictionary contains the default configs for the server
		private static Dictionary<object, object> DefaultServerConfigs { get; set; } = new Dictionary<object, object>()
		{
			{"autoStartServer", false},
			{ "autoBackupOnDate", false},
			{ "autoBackupOnDate_Time", "00:00:00"},
			{ "autoBackupEveryX", false},
			{ "autoBackupEveryXDuration", 1},
			{ "autoBackupEveryXTimeUnit", AutoBackupTimeUnit.Hour},
			{ "serverStopTimeout", 20},
			{ "banListScanInterval", 15},
			{ "worldPath", ""},
			{ "backupPath", ""},
			{ "backupLimit", 32},
			{ "banList", new string[0]},
			{ "applicationLoggingLevel", ApplicationLoggingLevel.All},
			{ "logPath", ""}
		};
		// this public dictionary contains the configs for the server (modifiable)
		public static Dictionary<object, object> ServerConfigs { get; set; } = DefaultServerConfigs;

		// server process
		public static Process serverProcess;
		public static StreamWriter serverInput;

		// timers
		public static Timer autoBackupEveryXTimer;
		public static Timer exitTImeoutTImer;
		public static Timer banlistScanTImer;

		// server properties
		public static readonly DateTime SessionStartTime = DateTime.Now;
		public static List<Player> Players { get; set; } = new List<Player>();
		public static bool ServerExecutableExists { get; set; } = false;
		public static bool ServerRunning { get; set; } = false;
		public static bool BackupRunning { get; set; } = false;
		public static bool LoadRunning { get; set; } = false;
		public static bool ServerWasRunningBefore { get; set; } = false;
		public static bool ExitCompleted { get; set; } = true;
		public static bool PlayerActivitySinceLastBackup { get; set; } = false;

		// requests
		public static bool LoadRequest { get; set; } = false;
		public static bool ExitRequest { get; set; } = false;
		public static bool BackupFileListRequest { get; set; } = false;
		public static string BackupFileLUst { get; set; } = "";

		public static void ReloadConfigs()
		{
			// add config keys and their default values to the hashtable if they don't exist
			Console.WriteLine($"{Timing.LogDateTime()} Locating Server executable...");
			ServerExecutableExists = File.Exists("bedrock_server") || File.Exists("bedrock_server.exe");
			Console.WriteLine($"serverExecutableExists={ServerExecutableExists}");

			Console.WriteLine($"{Timing.LogDateTime()} Loading configurations...");

			// autoStartServer
			if (Configs.GetValue("autoStartServer") != "true" && Configs.GetValue("autoStartServer") != "false")
			{
				Configs.SetValue("autoStartServer", ((bool)DefaultServerConfigs["autoStartServer"]).ToString().ToLower());
				ServerConfigs["autoStartServer"] = (bool)DefaultServerConfigs["autoStartServer"];
			}
			else ServerConfigs["autoStartServer"] = Configs.GetValue("autoStartServer") == "true";

			// autoBackupOnDate
			if (Configs.GetValue("autoBackupOnDate") != "true" && Configs.GetValue("autoBackupOnDate") != "false")
			{
				Configs.SetValue("autoBackupOnDate", ((bool)DefaultServerConfigs["autoBackupOnDate"]).ToString().ToLower());
				ServerConfigs["autoBackupOnDate"] = (bool)DefaultServerConfigs["autoBackupOnDate"];
			}
			else ServerConfigs["autoBackupOnDate"] = Configs.GetValue("autoBackupOnDate") == "true";

			// autoBackupOnDate_Time
			if (Configs.GetValue("autoBackupOnDate_Time") == "" || !DateTime.TryParseExact(Configs.GetValue("autoBackupOnDate_Time"), "H:m:s", null, DateTimeStyles.None, out DateTime result))
			{
				Configs.SetValue("autoBackupOnDate_Time", (string)DefaultServerConfigs["autoBackupOnDate_Time"]);
				ServerConfigs["autoBackupOnDate_Time"] = (string)DefaultServerConfigs["autoBackupOnDate_Time"];
			}
			else ServerConfigs["autoBackupOnDate_Time"] = Configs.GetValue("autoBackupOnDate_Time");

			// autoBackupEveryX
			if (Configs.GetValue("autoBackupEveryX") != "true" && Configs.GetValue("autoBackupEveryX") != "false")
			{
				Configs.SetValue("autoBackupEveryX", ((bool)DefaultServerConfigs["autoBackupEveryX"]).ToString().ToLower());
				ServerConfigs["autoBackupEveryX"] = (bool)DefaultServerConfigs["autoBackupEveryX"];
			}
			else ServerConfigs["autoBackupEveryX"] = Configs.GetValue("autoBackupEveryX") == "true";

			// serverStopTimeout
			if (!int.TryParse(Configs.GetValue("serverStopTimeout"), out int importValue))
			{
				Configs.SetValue("serverStopTimeout", Convert.ToString((int)DefaultServerConfigs["serverStopTimeout"]));
				ServerConfigs["serverStopTimeout"] = (int)DefaultServerConfigs["serverStopTimeout"];
			}
			else ServerConfigs["serverStopTimeout"] = Convert.ToInt32(Configs.GetValue("serverStopTimeout"));

			// banListScanInterval
			if (!int.TryParse(Configs.GetValue("banListScanInterval"), out importValue))
			{
				Configs.SetValue("banListScanInterval", Convert.ToString((int)DefaultServerConfigs["banListScanInterval"]));
				ServerConfigs["banListScanInterval"] = (int)DefaultServerConfigs["banListScanInterval"];
			}
			else ServerConfigs["banListScanInterval"] = Convert.ToInt32(Configs.GetValue("banListScanInterval"));

			// autoBackupEveryXDuration
			if (!int.TryParse(Configs.GetValue("autoBackupEveryXDuration"), out importValue))
			{
				Configs.SetValue("autoBackupEveryXDuration", Convert.ToString((int)DefaultServerConfigs["autoBackupEveryXDuration"]));
				ServerConfigs["autoBackupEveryXDuration	"] = (int)DefaultServerConfigs["autoBackupEveryXDuration"];
			}
			else ServerConfigs["autoBackupEveryXDuration"] = Convert.ToInt32(Configs.GetValue("autoBackupEveryXDuration"));

			// autoBackupEveryXTimeUnit
			if (Configs.GetValue("autoBackupEveryXTimeUnit") != AutoBackupTimeUnit.Minute.ToString().ToLower() && Configs.GetValue("autoBackupEveryXTimeUnit") != AutoBackupTimeUnit.Hour.ToString().ToLower())
			{
				Configs.SetValue("autoBackupEveryXTimeUnit", ((AutoBackupTimeUnit)DefaultServerConfigs["autoBackupEveryXTimeUnit"]).ToString().ToLower());
				ServerConfigs["autoBackupEveryXTimeUnit"] = (AutoBackupTimeUnit)DefaultServerConfigs["autoBackupEveryXTimeUnit"];
			}
			else ServerConfigs["autoBackupEveryXTimeUnit"] = Conversions.StringToAutoBackupTimeUnit(Configs.GetValue("autoBackupEveryXTimeUnit"));

			// worldPath
			if (Configs.GetValue("worldPath") == "" && Directory.Exists("worlds"))
			{
				if (Directory.GetDirectories("worlds").Length >= 1)
				{
					Configs.SetValue("worldPath", Directory.GetDirectories("worlds")[0]);
					ServerConfigs["worldPath"] = Directory.GetDirectories("worlds")[0];
				}
			}
			else ServerConfigs["worldPath"] = Configs.GetValue("worldPath");

			// backupPath
			ServerConfigs["backupPath"] = Configs.GetValue("backupPath");

			// backupLimit
			if (!int.TryParse(Configs.GetValue("backupLimit"), out importValue))
			{
				Configs.SetValue("backupLimit", DefaultServerConfigs["backupLimit"].ToString());
				ServerConfigs["backupLimit"] = Convert.ToInt32(DefaultServerConfigs["backupLimit"]);
			}
			else if (importValue < 1)
			{
				Configs.SetValue("backupLimit", DefaultServerConfigs["backupLimit"].ToString());
				ServerConfigs["backupLimit"] = Convert.ToInt32(DefaultServerConfigs["backupLimit"]);
			}
			else ServerConfigs["backupLimit"] = Convert.ToInt32(Configs.GetValue("backupLimit"));

			Console.WriteLine($"{Timing.LogDateTime()} Configurations loaded.");

			// banList
			if (!File.Exists($"{AppName}.banlist"))
			{
				Console.WriteLine($"{Timing.LogDateTime()} Ban list file not found.");

				File.WriteAllText($"{AppName}.banlist", "");
				Console.WriteLine($"{Timing.LogDateTime()} Empty ban list file generated.");
			}
			ServerConfigs["banList"] = File.ReadAllLines($"{AppName}.banlist");
			Console.WriteLine($"{Timing.LogDateTime()} Ban list loaded.");

			Command.ProcessCommand("configs");
		}

		private static void InitializeComponents()
		{
			// timers
			autoBackupEveryXTimer = new Timer(Events.AutoBackupEveryXTimer_TIck);
			exitTImeoutTImer = new Timer(Events.ExitTImeoutTImer_Tick);
			banlistScanTImer = new Timer(Events.BanlistScanTimer_Tick);

			// server process events
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Events.OnProgramExit);
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(Events.OnProgramExit);

			ReloadConfigs();
		}

		static void Main()
		{
			if (!File.Exists($"{AppName}.conf"))
			{
				Console.WriteLine("Configuration file not found.");
				return;
			}

			InitializeComponents();

			Console.WriteLine($"{Timing.LogDateTime()} Server wrapper started. ({AppName}:{AppVersion})");

			if ((bool)ServerConfigs["autoStartServer"]) Command.ProcessCommand("start");

			// start the banlist scan timer
			banlistScanTImer.Change((int)ServerConfigs["banListScanInterval"] * 1000, (int)ServerConfigs["banListScanInterval"] * 1000);

			// console input loop
			while (true)
			{
				Command.ProcessCommand(Console.ReadLine());
				while (LoadRequest || LoadRunning)
				{
					if (!LoadRequest && !LoadRunning) break;
				}
			}
		}
	}
}
