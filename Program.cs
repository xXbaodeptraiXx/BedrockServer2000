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
			// debug line
			Directory.SetCurrentDirectory("/home/bao/bedrock_server");

			serverConfigs.serverRunning = false;
			serverConfigs.backupRunning = false;

			Process.GetCurrentProcess().Exited += new EventHandler(Events.ProgramExited);

			LoadConfigs();

			Console.WriteLine("Server wrapper started.");

			if (serverConfigs.autoStartServer) Command.ProcessCommand("start");

			consoleInputThread = new Thread(ConsoleInput);
			consoleInputThread.Start();
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
			try
			{
				Console.WriteLine("Loading configs");

				serverConfigs.serverExecutableExists = File.Exists("bedrock_server");
				Console.WriteLine($"serverExecutableExists: {serverConfigs.serverExecutableExists}");

				if (ConfigurationManager.AppSettings["autoStartServer"] != "true" && ConfigurationManager.AppSettings["autoStartServer"] != "false")
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["autoStartServer"].Value = "false";
					configuration.Save(ConfigurationSaveMode.Modified);
					serverConfigs.autoStartServer = false;
				}
				else
				{
					if (ConfigurationManager.AppSettings["autoStartServer"] == "true") serverConfigs.autoStartServer = true;
					else serverConfigs.autoStartServer = false;
				}
				Console.WriteLine($"autoStartServer: {serverConfigs.autoStartServer}");

				if (ConfigurationManager.AppSettings["autoBackupOnDate"] != "true" && ConfigurationManager.AppSettings["autoBackupOnDate"] != "false")
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["autoBackupOnDate"].Value = "false";
					configuration.Save(ConfigurationSaveMode.Modified);
					serverConfigs.autoBackupOnDate = false;
				}
				else
				{
					if (ConfigurationManager.AppSettings["autoBackupOnDate"] == "true") serverConfigs.autoBackupOnDate = true;
					else serverConfigs.autoBackupOnDate = false;
				}
				Console.WriteLine($"autoBackupOnDate: {serverConfigs.autoBackupOnDate}");

				if (ConfigurationManager.AppSettings["autoBackupOnDate_Time"] == "" || !DateTime.TryParseExact(ConfigurationManager.AppSettings["autoBackupOnDate_Time"], "H:m:s", null, DateTimeStyles.None, out DateTime result))
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["autoBackupOnDate_Time"].Value = "00:00:00";
					configuration.Save(ConfigurationSaveMode.Modified);
					serverConfigs.autoBackupOnDate_Time = "00:00:00";
				}
				else
					serverConfigs.autoBackupOnDate_Time = ConfigurationManager.AppSettings["autoBackupOnDate_Time"];
				Console.WriteLine($"utoBackupOnDate_Time: {serverConfigs.autoBackupOnDate_Time}");

				if (ConfigurationManager.AppSettings["autoBackupEveryX"] != "true" && ConfigurationManager.AppSettings["autoBackupEveryX"] != "false")
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["autoBackupEveryX"].Value = "false";
					configuration.Save(ConfigurationSaveMode.Modified);
					serverConfigs.autoBackupEveryX = false;
				}
				else
				{
					if (ConfigurationManager.AppSettings["autoBackupEveryX"] == "true") serverConfigs.autoBackupEveryX = true;
					else serverConfigs.autoBackupEveryX = false;
				}
				Console.WriteLine($"autoBackupEveryX: {serverConfigs.autoBackupEveryX}");

				if (!int.TryParse(ConfigurationManager.AppSettings["autoBackupEveryXDuration"], out int importVal))
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["autoBackupEveryXDuration"].Value = "1";
					configuration.Save(ConfigurationSaveMode.Modified);
					serverConfigs.autoBackupEveryXDuration = 1;
				}
				else
					serverConfigs.autoBackupEveryXDuration = Convert.ToInt32(ConfigurationManager.AppSettings["autoBackupEveryXDuration"]);
				Console.WriteLine($"autoBackupEveryXDuration: {serverConfigs.autoBackupEveryXDuration}");

				if (ConfigurationManager.AppSettings["autoBackupEveryXTimeUnit"] != "minute" && ConfigurationManager.AppSettings["autoBackupEveryXTimeUnit"] != "hour")
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["autoBackupEveryXTimeUnit"].Value = "hour";
					configuration.Save(ConfigurationSaveMode.Modified);
					serverConfigs.autoBackupEveryXTimeUnit = "hour";
				}
				else serverConfigs.autoBackupEveryXTimeUnit = ConfigurationManager.AppSettings["autoBackupEveryXTimeUnit"];
				Console.WriteLine($"autoBackupEveryXTimeUnit: {serverConfigs.autoBackupEveryXTimeUnit}");

				if (ConfigurationManager.AppSettings["worldPath"] == "" && Directory.Exists("worlds"))
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
					serverConfigs.worldPath = ConfigurationManager.AppSettings["worldPath"];
				Console.WriteLine($"worldPath: {serverConfigs.worldPath}");

				serverConfigs.backupPath = ConfigurationManager.AppSettings["backupPath"].ToString();
				Console.WriteLine($"backupPath: {serverConfigs.backupPath}");

				if (!int.TryParse(ConfigurationManager.AppSettings["backupLimit"], out importVal))
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["backupLimit"].Value = Convert.ToString(32);
					configuration.Save(ConfigurationSaveMode.Modified);
					serverConfigs.backupLimit = 32;
				}
				else
					serverConfigs.backupLimit = Convert.ToInt32(ConfigurationManager.AppSettings["backupLimit"]);
				Console.WriteLine($"backupLimit: {serverConfigs.backupLimit}");
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
