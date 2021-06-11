using System;
using System.Globalization;
using System.Configuration;
using System.IO;

namespace BedrockServer2000
{
	public class ServerConfigs
	{
		public bool serverExecutableExists { get; set; }
		public bool backupRunning { get; set; }
		public bool serverRunning { get; set; }
		public bool loadRequest { get; set; }
		public bool exitRequest { get; set; }
		public bool serverWasRunningBefore { get; set; }

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
				Console.WriteLine("Loading configs");

				serverExecutableExists = File.Exists("bedrock_server");
				Console.WriteLine($"serverExecutableExists: {serverExecutableExists}");

				if (ConfigurationManager.AppSettings["autoStartServer"] != "true" && ConfigurationManager.AppSettings["autoStartServer"] != "false")
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["autoStartServer"].Value = "false";
					configuration.Save(ConfigurationSaveMode.Modified);
					autoStartServer = false;
				}
				else
				{
					if (ConfigurationManager.AppSettings["autoStartServer"] == "true") autoStartServer = true;
					else autoStartServer = false;
				}
				Console.WriteLine($"autoStartServer: {autoStartServer}");

				if (ConfigurationManager.AppSettings["autoBackupOnDate"] != "true" && ConfigurationManager.AppSettings["autoBackupOnDate"] != "false")
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["autoBackupOnDate"].Value = "false";
					configuration.Save(ConfigurationSaveMode.Modified);
					autoBackupOnDate = false;
				}
				else
				{
					if (ConfigurationManager.AppSettings["autoBackupOnDate"] == "true") autoBackupOnDate = true;
					else autoBackupOnDate = false;
				}
				Console.WriteLine($"autoBackupOnDate: {autoBackupOnDate}");

				if (ConfigurationManager.AppSettings["autoBackupOnDate_Time"] == "" || !DateTime.TryParseExact(ConfigurationManager.AppSettings["autoBackupOnDate_Time"], "H:m:s", null, DateTimeStyles.None, out DateTime result))
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["autoBackupOnDate_Time"].Value = "00:00:00";
					configuration.Save(ConfigurationSaveMode.Modified);
					autoBackupOnDate_Time = "00:00:00";
				}
				else
					autoBackupOnDate_Time = ConfigurationManager.AppSettings["autoBackupOnDate_Time"];
				Console.WriteLine($"utoBackupOnDate_Time: {autoBackupOnDate_Time}");

				if (ConfigurationManager.AppSettings["autoBackupEveryX"] != "true" && ConfigurationManager.AppSettings["autoBackupEveryX"] != "false")
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["autoBackupEveryX"].Value = "false";
					configuration.Save(ConfigurationSaveMode.Modified);
					autoBackupEveryX = false;
				}
				else
				{
					if (ConfigurationManager.AppSettings["autoBackupEveryX"] == "true") autoBackupEveryX = true;
					else autoBackupEveryX = false;
				}
				Console.WriteLine($"autoBackupEveryX: {autoBackupEveryX}");

				if (!int.TryParse(ConfigurationManager.AppSettings["autoBackupEveryXDuration"], out int importVal))
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["autoBackupEveryXDuration"].Value = "1";
					configuration.Save(ConfigurationSaveMode.Modified);
					autoBackupEveryXDuration = 1;
				}
				else
					autoBackupEveryXDuration = Convert.ToInt32(ConfigurationManager.AppSettings["autoBackupEveryXDuration"]);
				Console.WriteLine($"autoBackupEveryXDuration: {autoBackupEveryXDuration}");

				if (ConfigurationManager.AppSettings["autoBackupEveryXTimeUnit"] != "minute" && ConfigurationManager.AppSettings["autoBackupEveryXTimeUnit"] != "hour")
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["autoBackupEveryXTimeUnit"].Value = "hour";
					configuration.Save(ConfigurationSaveMode.Modified);
					autoBackupEveryXTimeUnit = "hour";
				}
				else autoBackupEveryXTimeUnit = ConfigurationManager.AppSettings["autoBackupEveryXTimeUnit"];
				Console.WriteLine($"autoBackupEveryXTimeUnit: {autoBackupEveryXTimeUnit}");

				if (ConfigurationManager.AppSettings["worldPath"] == "" && Directory.Exists("worlds"))
				{
					if (Directory.GetDirectories("worlds").Length >= 1)
					{
						Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
						configuration.Save(ConfigurationSaveMode.Modified);
						configuration.AppSettings.Settings["worldPath"].Value = Directory.GetDirectories("worlds")[0];
						configuration.Save(ConfigurationSaveMode.Modified);
						worldPath = Directory.GetDirectories("worlds")[0];
					}
				}
				else
					worldPath = ConfigurationManager.AppSettings["worldPath"];
				Console.WriteLine($"worldPath: {worldPath}");

				backupPath = ConfigurationManager.AppSettings["backupPath"].ToString();
				Console.WriteLine($"backupPath: {backupPath}");

				if (!int.TryParse(ConfigurationManager.AppSettings["backupLimit"], out importVal))
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["backupLimit"].Value = Convert.ToString(32);
					configuration.Save(ConfigurationSaveMode.Modified);
					backupLimit = 32;
				}
				else
					backupLimit = Convert.ToInt32(ConfigurationManager.AppSettings["backupLimit"]);
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