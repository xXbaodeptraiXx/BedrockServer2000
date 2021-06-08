using System;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Configuration;

namespace BedrockServer2000
{
	class Program
	{
		public static ServerConfigs serverConfigs = new ServerConfigs();

		public static Process bedrockServerProcess;
		public static StreamWriter bedrockServerInputStream;

		public static Thread consoleInputThread;

		public static Timer autoBackupEveryXTimer;

		static void Main()
		{
			serverConfigs.serverRunning = false;
			serverConfigs.backupRunning = false;

			LoadConfigs();

			Console.WriteLine("Server wrapper started.");

			consoleInputThread = new Thread(ConsoleInput);
			consoleInputThread.Start();

			if (serverConfigs.autoStartServer) Command.ProcessCommand("start");
		}

		public static void ConsoleInput()
		{
			while (true)
			{
				Command.ProcessCommand(Console.ReadLine());
			}
		}

		public static void LoadConfigs()
		{
			serverConfigs.serverExecutableExists = File.Exists("bedrock_server.exe");

			if (ConfigurationManager.AppSettings["autoStartServer"].ToString() != "true" && ConfigurationManager.AppSettings["autoStartServer"].ToString() != "false")
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoStartServer"].Value = "false";
				configuration.Save(ConfigurationSaveMode.Modified);
				serverConfigs.autoStartServer = false;
			}
			else
			{
				if (ConfigurationManager.AppSettings["autoStartServer"].ToString() == "true") serverConfigs.autoStartServer = true;
				else serverConfigs.autoStartServer = false;
			}

			if (ConfigurationManager.AppSettings["worldPath"].ToString() == "" && Directory.Exists("worlds"))
			{
				if (Directory.GetDirectories("worlds").Length >= 1)
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["worldPath"].Value = Directory.GetDirectories("worlds")[0];
					configuration.Save(ConfigurationSaveMode.Modified);
					serverConfigs.worldPath = Directory.GetDirectories("worlds")[0];
				}
			}
			else
				serverConfigs.worldPath = ConfigurationManager.AppSettings["worldPath"].ToString();

			if (!int.TryParse(ConfigurationManager.AppSettings["backupLimit"].ToString(), out int importVal))
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["backupLimit"].Value = Convert.ToString(32);
				configuration.Save(ConfigurationSaveMode.Modified);
				serverConfigs.backupLimit = 32;
			}
			else
				serverConfigs.backupLimit = Convert.ToInt32(ConfigurationManager.AppSettings["backupLimit"]);

			if (ConfigurationManager.AppSettings["autoBackupOnDate"].ToString() != "true" && ConfigurationManager.AppSettings["autoBackupOnDate"].ToString() != "false")
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupOnDate"].Value = "false";
				configuration.Save(ConfigurationSaveMode.Modified);
				serverConfigs.autoBackupOnDate = false;
			}
			else
			{
				if (ConfigurationManager.AppSettings["autoBackupOnDate"].ToString() == "true") serverConfigs.autoBackupOnDate = true;
				else serverConfigs.autoBackupOnDate = false;
			}

			if (ConfigurationManager.AppSettings["autoBackupOnDate_Time"].ToString() == "" || !DateTime.TryParseExact(ConfigurationManager.AppSettings["autoBackupOnDate_Time"].ToString(), "H:m:s", null, DateTimeStyles.None, out DateTime result))
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupOnDate_Time"].Value = "00:00:00";
				configuration.Save(ConfigurationSaveMode.Modified);
				serverConfigs.autoBackupOnDate_Time = "00:00:00";
			}
			else
				serverConfigs.autoBackupOnDate_Time = ConfigurationManager.AppSettings["autoBackupOnDate_Time"].ToString();

			if (ConfigurationManager.AppSettings["autoBackupEveryX"].ToString() != "true" && ConfigurationManager.AppSettings["autoBackupEveryX"].ToString() != "false")
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupEveryX"].Value = "false";
				configuration.Save(ConfigurationSaveMode.Modified);
				serverConfigs.autoBackupEveryX = false;
			}
			else
			{
				if (ConfigurationManager.AppSettings["autoBackupEveryX"].ToString() == "true") serverConfigs.autoBackupEveryX = true;
				else serverConfigs.autoBackupEveryX = false;
			}

			if (!int.TryParse(ConfigurationManager.AppSettings["autoBackupEveryXDuration"].ToString(), out importVal))
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupEveryXDuration"].Value = "1";
				configuration.Save(ConfigurationSaveMode.Modified);
				serverConfigs.autoBackupEveryXDuration = 1;
			}
			else
				serverConfigs.autoBackupEveryXDuration = Convert.ToInt32(ConfigurationManager.AppSettings["autoBackupEveryXDuration"].ToString());

			if (ConfigurationManager.AppSettings["autoBackupEveryXTimeUnit"].ToString() != "minute" && ConfigurationManager.AppSettings["autoBackupEveryXTimeUnit"].ToString() != "hour")
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupEveryXTimeUnit"].Value = "hour";
				configuration.Save(ConfigurationSaveMode.Modified);
				serverConfigs.autoBackupEveryXTimeUnit = "hour";
			}
			else serverConfigs.autoBackupEveryXTimeUnit = ConfigurationManager.AppSettings["autoBackupEveryXTimeUnit"].ToString();

			serverConfigs.backupPath = ConfigurationManager.AppSettings["backupPath"].ToString();
		}
	}
}
