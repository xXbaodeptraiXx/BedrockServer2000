using System;
using System.Threading;
using System.Configuration;
using System.Globalization;
using System.IO;

namespace BedrockServer2000
{
	public class Command
	{
		public static void ProcessCommand(string command)
		{
			string formattedCommand = command.Trim().ToLower();

			if (formattedCommand == "commands") ShowHelp("");
			else if (formattedCommand.Split().Length > 1 && formattedCommand.Split()[0] == "commands") ShowHelp(formattedCommand.Remove(0, 9));
			else if (formattedCommand == "backup") Backup.PerformBackup(Program.serverConfigs);
			else if (formattedCommand == "start") StartServer();
			else if (formattedCommand == "stop") StopServer();
			else if (formattedCommand == "reload") Program.LoadConfigs();
			else if (formattedCommand.Split().Length > 1)
			{
				if (formattedCommand.Split().Length == 3)
					if (formattedCommand.Split()[0] == "set") Set(formattedCommand.Split()[1], formattedCommand.Split()[2]);
			}
			else if (formattedCommand == "exit") RunExitProcedure();
			else Program.bedrockServerInputStream.WriteLine(formattedCommand);
		}

		private static void ShowHelp(string args)
		{
			if (args == "")
			{
				Console.WriteLine(@"Commands:
			- commands : show this message
			- backup : backup the world file (available even when the server is not running)
			- start : start the server
			- stop : stop the server
			- reload : reload the configs from the configuration file
			- ^set [config_key] [config_value] : change server wrapper configs
			- exit : stop the server wrapper
			* Commands with ^ before their names can be used with 'commands [comand]' to show more information.
			  + Example: 'commands set'
			Other commands are processed by the bedrock server software");
			}
			else if (args == "set")
			{
				Console.WriteLine(@"Commands > set:
				Purpose: change server wrapper configs
				Syntax: set [config_key] [config_value]
				- Available config keys and their available config values:
				  + autoStartServer [true / false]
				  + autoBackupOnDate [true / false]
				  + autoBackupOnDate_Time [time (H:M:S): example: 17:30:00]
				  + autoBackupEveryX [true / false]
				  + autoBackupEveryXDuration [positive integer]
				  + autoBackupEveryXTimeUnit [string: minute / hour]
				  + worldPath [path to the world folder]
				  + backupPath [path to the backup folder]
				  + backupLimit [positive integer]

				Examples:
				  + set worldPath C:\\bedrock_server\\world backups
				  + set autoStartServer true
				  + set backupLimit 32
				  + set autoBackupEveryXTimeUnit hour
				");
			}
			else ShowSyntaxError();
		}

		private static void StartServer()
		{
			if (!Program.serverConfigs.serverExecutableExists)
			{
				Console.WriteLine("Server executable not found, can't start server.");
				return;
			}
			Program.serverConfigs.serverRunning = true;

			Program.bedrockServerProcess = new System.Diagnostics.Process();
			Program.bedrockServerProcess.StartInfo.FileName = "bedrock_server";
			Console.WriteLine("Using this terminal: " + Program.bedrockServerProcess.StartInfo.FileName);

			Program.bedrockServerProcess.StartInfo.UseShellExecute = false;
			Program.bedrockServerProcess.StartInfo.CreateNoWindow = true;
			Program.bedrockServerProcess.StartInfo.RedirectStandardInput = true;
			Program.bedrockServerProcess.StartInfo.RedirectStandardOutput = true;
			Program.bedrockServerProcess.StartInfo.RedirectStandardError = true;

			Program.bedrockServerProcess.EnableRaisingEvents = true;
			Program.bedrockServerProcess.Exited += new EventHandler(Events.bedrockServerProcess_Exited);
			Program.bedrockServerProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(Events.bedrockServerProcess_ErrorDataReceived);
			Program.bedrockServerProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(Events.bedrockServerProcess_OutputDataReceived);

			Program.bedrockServerProcess.Start();

			Program.bedrockServerInputStream = Program.bedrockServerProcess.StandardInput;
			Program.bedrockServerProcess.BeginOutputReadLine();
			Program.bedrockServerProcess.BeginErrorReadLine();

			if (Program.serverConfigs.autoBackupEveryX == true)
			{
				int autoBackupEveryXTimerInterval = 0;
				if (Program.serverConfigs.autoBackupEveryXTimeUnit == "minute") autoBackupEveryXTimerInterval = Timing.MinuteToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration);
				else if (Program.serverConfigs.autoBackupEveryXTimeUnit == "hour") autoBackupEveryXTimerInterval = Timing.HourToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration);
				Program.autoBackupEveryXTimer = new Timer(Backup.PerformBackup, Program.serverConfigs, 0, autoBackupEveryXTimerInterval);
			}
		}

		private static void StopServer()
		{

		}

		private static void Set(string key, string value)
		{
			if (key == "autoStartServer")
			{
				if (value == "true" || value == "false")
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoStartServer = Convert.ToBoolean(value);
				}
				else Console.WriteLine($"Error: Available config values for {key} are 'true' and 'false'.");
			}
			else if (key == "autoBackupOnDate")
			{
				if (value == "true" || value == "false")
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoStartServer = Convert.ToBoolean(value);
				}
				else Console.WriteLine($"Error: Available config values for {key} are 'true' and 'false'.");
			}
			else if (key == "autoBackupOnDate_Time")
			{
				if (DateTime.TryParseExact(value, "H:m:s", null, DateTimeStyles.None, out DateTime result))
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoBackupOnDate_Time = value;
				}
				else Console.WriteLine($"Error: Value for {key} must be a time value in h:m:s format.");
			}
			else if (key == "autoBackupEveryX")
			{
				if (value == "true" || value == "false")
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoStartServer = Convert.ToBoolean(value);
				}
				else Console.WriteLine($"Error: Available config values for {key} are 'true' and 'false'.");
			}
			else if (key == "autoBackupEveryXDuration")
			{
				if (int.TryParse(value, out int result))
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoBackupEveryXDuration = result;
				}
				else Console.WriteLine($"Error: Value for {key} must be a positive integer.");
			}
			else if (key == "autoBackupEveryXTimeUnit")
			{
				if (value == "minute")
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoBackupEveryXTimeUnit = value;
					Program.autoBackupEveryXTimer = new Timer(Backup.PerformBackup, Program.serverConfigs, 0, Timing.MinuteToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration));
				}
				else if (value == "hour")
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoBackupEveryXTimeUnit = value;
					Program.autoBackupEveryXTimer = new Timer(Backup.PerformBackup, Program.serverConfigs, 0, Timing.HourToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration));
				}
				else Console.WriteLine($"Error: Available config values for {key} are 'minute' and 'hour'.");
			}
			else if (key == "worldPath")
			{
				if (Directory.Exists(value))
				{
					SaveConfig(key, value);
					Program.serverConfigs.worldPath = value;
				}
				else Console.WriteLine("Error: Path does not exist.");
			}
			else if (key == "backupPath")
			{
				if (Directory.Exists(value))
				{
					SaveConfig(key, value);
					Program.serverConfigs.worldPath = value;
				}
				else Console.WriteLine("Error: Path does not exist.");
			}
			else if (key == "backupLimit")
			{
				if (int.TryParse(value, out int result))
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoBackupEveryXDuration = result;
				}
				else Console.WriteLine($"Error: Value for {key} must be a positive integer.");
			}
			else ShowSyntaxError();
		}

		private static void SaveConfig(string key, string value)
		{
			Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			configuration.Save(ConfigurationSaveMode.Modified);
			configuration.AppSettings.Settings[key].Value = value;
			configuration.Save(ConfigurationSaveMode.Modified);
		}

		private static void ShowSyntaxError()
		{
			Console.WriteLine("Error: Incorrect command syntax.");
		}

		private static void RunExitProcedure()
		{
			if (Program.serverConfigs.serverRunning) StopServer();
			Environment.Exit(0);
		}
	}
}