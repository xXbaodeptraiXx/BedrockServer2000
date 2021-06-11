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
			if (Program.serverConfigs.backupRunning) return;

			string formattedCommand = command.Trim().ToLower();

			if (formattedCommand == "commands") ShowHelp("");
			else if (formattedCommand == "start") StartServer();
			else if (formattedCommand == "stop")
			{
				if (Program.serverConfigs.serverRunning)
				{
					Thread StopServerThread = new Thread(StopServer);
					StopServerThread.Start();
				}
				else Console.WriteLine("Server is not currently running.");
			}
			else if (formattedCommand == "load")
			{
				// check if the configs are correct, cancel the backup if found any error
				if (!Directory.Exists(Program.serverConfigs.worldPath))
				{
					Console.WriteLine($"World path incorrect");
					return;
				}
				if (!Directory.Exists(Program.serverConfigs.backupPath))
				{
					Console.WriteLine($"Backup path incorrect");
					return;
				}
				if (Directory.Exists(Program.serverConfigs.backupPath) && Directory.GetDirectories(Program.serverConfigs.backupPath).Length < 1)
				{
					Console.WriteLine($"There are no backups to load");
					return;
				}

				Backup.LoadBackup();
			}
			else if (formattedCommand == "backup" && !Program.serverConfigs.backupRunning)
			{
				// check if the configs are correct, cancel the backup if found any error
				if (!Directory.Exists(Program.serverConfigs.worldPath))
				{
					Console.WriteLine($"World path incorrect, can't perform backup.");
					return;
				}
				if (!Directory.Exists(Program.serverConfigs.backupPath))
				{
					Console.WriteLine($"Backup path incorrect, can't perform backup.");
					return;
				}
				if (Program.serverConfigs.backupLimit <= 0)
				{
					Console.WriteLine($"Backup limit can't be smaller than 1, can't perform backup.");
					return;
				}

				Backup.PerformBackup(null);
			}
			else if (formattedCommand == "configs") ShowConfigs("");
			else if (formattedCommand == "reload") Program.LoadConfigs();
			else if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 1)
			{
				if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0] == "commands") ShowHelp(formattedCommand.Remove(0, 9));
				else if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0] == "say")
				{
					if (Program.serverConfigs.serverRunning)
					{
						Program.bedrockServerInputStream.WriteLine("say " + command.Trim().Remove(0, 4));
						Console.WriteLine($"Message sent to chat (\"{command.Trim().Remove(0, 4)}\")");
					}
					else Console.WriteLine("Server is not currently running.");
				}
				else if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 2)
				{
					if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0] == "configs") ShowConfigs(formattedCommand.Remove(0, 8));
					else if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0] == "set") Set(formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1], "");
					else
					{
						if (Program.serverConfigs.serverRunning) Program.bedrockServerInputStream.WriteLine(command);
						else Console.WriteLine("Unknown command.");
					}
				}
				else if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 3)
				{
					if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0] == "set") Set(formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1], formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[2]);
					else
					{
						if (Program.serverConfigs.serverRunning) Program.bedrockServerInputStream.WriteLine(command);
						else Console.WriteLine("Unknown command.");
					}
				}
				else
				{
					if (Program.serverConfigs.serverRunning) Program.bedrockServerInputStream.WriteLine(command);
					else Console.WriteLine("Unknown command.");
				}
			}
			else if (formattedCommand == "clear") Console.Clear();
			else if (formattedCommand == "exit") RunExitProcedure();
			else
			{
				if (Program.serverConfigs.serverRunning) Program.bedrockServerInputStream.WriteLine(command);
				else Console.WriteLine("Unknown command.");
			}
		}

		public static void ShowHelp(string args)
		{
			if (args == "")
			{
				Console.WriteLine(@"Commands:
- commands : show this message
- start : start the server
- stop : stop the server
- backup : backup the world file (available even when the server is not running)
- load : load a saved backup
- ^configs : show server wrapper configs
- reload : reload the configs from the configuration file
- ^set [config_key] [config_value] : change server wrapper configs
- clear : clear the console
- exit : stop the server wrapper* Commands with ^ before their names can be used with 'commands [comand]' to show more information.
  + Example: 'commands set'
Other commands are processed by the bedrock server software if it's running.");
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
			else if (args == "configs")
			{
				Console.WriteLine(@"Commands > configs:
Purpose: show server wrapper configs
Syntax: 
  + 'configs' : show status of all configs
  + 'configs [config_key]' : show status of a specific config key

use 'configs' to know all the config keys

Examples:
  + configs
  + configs autoStartServer
  + configs autoBackupEveryXDuration
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
			Program.bedrockServerProcess.Exited += new EventHandler(Events.BedrockServerProcess_Exited);
			Program.bedrockServerProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(Events.BedrockServerProcess_OutputDataReceived);

			Program.bedrockServerProcess.Start();

			Program.bedrockServerInputStream = Program.bedrockServerProcess.StandardInput;
			Program.bedrockServerProcess.BeginOutputReadLine();
			Program.bedrockServerProcess.BeginErrorReadLine();

			if (Program.serverConfigs.autoBackupEveryX)
			{
				int autoBackupEveryXTimerInterval = 0;
				if (Program.serverConfigs.autoBackupEveryXTimeUnit == "minute") autoBackupEveryXTimerInterval = Timing.MinuteToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration);
				else if (Program.serverConfigs.autoBackupEveryXTimeUnit == "hour") autoBackupEveryXTimerInterval = Timing.HourToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration);
				Program.autoBackupEveryXTimer = new Timer(Backup.PerformBackup, null, autoBackupEveryXTimerInterval, autoBackupEveryXTimerInterval);
			}
		}

		private static void StopServer()
		{
			Program.serverConfigs.serverRunning = false;
			if (Program.serverConfigs.autoBackupEveryX) Program.autoBackupEveryXTimer.Change(Timeout.Infinite, Timeout.Infinite);

			const string stopMessage = "Server closing in 10 seconds";

			Program.bedrockServerInputStream.WriteLine($"say {stopMessage}");
			Console.WriteLine("Server stop message sent.");
			Thread.Sleep(10000);
			Program.bedrockServerInputStream.WriteLine("stop");
		}

		private static void StopServerAndExit()
		{
			StopServer();
			Thread.Sleep(5000);
			Console.WriteLine("Server wrapper stopped.");
			Environment.Exit(0);
		}

		private static void ShowConfigs(string key)
		{
			if (key == "")
			{
				Console.WriteLine($"autoStartServer = {Program.serverConfigs.autoStartServer}");
				Console.WriteLine($"utoBackupOnDate = {Program.serverConfigs.autoBackupOnDate}");
				Console.WriteLine($"autoBackupOnDate_Time = {Program.serverConfigs.autoBackupOnDate_Time}");
				Console.WriteLine($"autoBackupEveryX = {Program.serverConfigs.autoBackupEveryX}");
				Console.WriteLine($"autoBackupEveryXDuration = {Program.serverConfigs.autoBackupEveryXDuration}");
				Console.WriteLine($"autoBackupEveryXTimeUnit = {Program.serverConfigs.autoBackupEveryXTimeUnit}");
				Console.WriteLine($"worldPath = {Program.serverConfigs.worldPath}");
				Console.WriteLine($"backupPath = {Program.serverConfigs.backupPath}");
				Console.WriteLine($"backupLimit = {Program.serverConfigs.backupLimit}");
			}
			else if (key == "autostartserver") Console.WriteLine($"autoStartServer = {Program.serverConfigs.autoStartServer}");
			else if (key == "autobackupondate") Console.WriteLine($"utoBackupOnDate = {Program.serverConfigs.autoBackupOnDate}");
			else if (key == "autobackupondate_time") Console.WriteLine($"autoBackupOnDate_Time = {Program.serverConfigs.autoBackupOnDate_Time}");
			else if (key == "autobackupeveryx") Console.WriteLine($"autoBackupEveryX = {Program.serverConfigs.autoBackupEveryX}");
			else if (key == "autobackupeveryxduration") Console.WriteLine($"autoBackupEveryXDuration = {Program.serverConfigs.autoBackupEveryXDuration}");
			else if (key == "autobackupeveryxtimeunit") Console.WriteLine($"autoBackupEveryXTimeUnit = {Program.serverConfigs.autoBackupEveryXTimeUnit}");
			else if (key == "worldpath") Console.WriteLine($"worldPath = {Program.serverConfigs.worldPath}");
			else if (key == "backuppath") Console.WriteLine($"backupPath = {Program.serverConfigs.backupPath}");
			else if (key == "backuplimit") Console.WriteLine($"backupLimit = {Program.serverConfigs.backupLimit}");
			else Console.WriteLine($"Error: Unknown config key");
		}

		private static void Set(string key, string value)
		{
			if (key == "autostartserver")
			{
				if (value == "true" || value == "false")
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoStartServer = Convert.ToBoolean(value);
				}
				else Console.WriteLine($"Error: Available config values for autoStartServer are 'true' and 'false'.");
			}
			else if (key == "autobackupondate")
			{
				if (value == "true" || value == "false")
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoStartServer = Convert.ToBoolean(value);
				}
				else Console.WriteLine($"Error: Available config values for autoBackupOnDate are 'true' and 'false'.");
			}
			else if (key == "autobackupondate_time")
			{
				if (DateTime.TryParseExact(value, "H:m:s", null, DateTimeStyles.None, out DateTime result))
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoBackupOnDate_Time = value;
				}
				else Console.WriteLine($"Error: Value for autoBackupOnDate_Time must be a time value in h:m:s format.");
			}
			else if (key == "autobackupeveryx")
			{
				if (value == "true" || value == "false")
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoBackupEveryX = Convert.ToBoolean(value);

					if (value == "false") Program.autoBackupEveryXTimer.Change(Timeout.Infinite, Timeout.Infinite);
					if (Program.serverConfigs.serverRunning && value == "true")
					{
						int autoBackupEveryXTimerInterval = 0;
						if (Program.serverConfigs.autoBackupEveryXTimeUnit == "minute") autoBackupEveryXTimerInterval = Timing.MinuteToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration);
						else if (Program.serverConfigs.autoBackupEveryXTimeUnit == "hour") autoBackupEveryXTimerInterval = Timing.HourToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration);
						Program.autoBackupEveryXTimer = new Timer(Backup.PerformBackup, null, autoBackupEveryXTimerInterval, autoBackupEveryXTimerInterval);
					}
				}
				else Console.WriteLine($"Error: Available config values for autoBackupEveryX are 'true' and 'false'.");
			}
			else if (key == "autobackupeveryxduration")
			{
				if (int.TryParse(value, out int result))
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoBackupEveryXDuration = result;
				}
				else Console.WriteLine($"Error: Value for autoBackupEveryXDuration must be a positive integer.");
			}
			else if (key == "autobackupeveryxtimeunit")
			{
				if (value == "minute")
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoBackupEveryXTimeUnit = value;
					if (Program.serverConfigs.serverRunning && Program.serverConfigs.autoBackupEveryX) Program.autoBackupEveryXTimer = new Timer(Backup.PerformBackup, Program.serverConfigs, Timing.MinuteToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration), Timing.MinuteToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration));
				}
				else if (value == "hour")
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoBackupEveryXTimeUnit = value;
					if (Program.serverConfigs.serverRunning && Program.serverConfigs.autoBackupEveryX) Program.autoBackupEveryXTimer = new Timer(Backup.PerformBackup, Program.serverConfigs, Timing.HourToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration), Timing.HourToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration));
				}
				else Console.WriteLine($"Error: Available config values for autoBackupEveryXTimeUnit are 'minute' and 'hour'.");
			}
			else if (key == "worldpath")
			{
				if (Directory.Exists(value))
				{
					SaveConfig(key, value);
					Program.serverConfigs.worldPath = value;
				}
				else Console.WriteLine("Error: Path does not exist.");
			}
			else if (key == "backuppath")
			{
				if (Directory.Exists(value))
				{
					SaveConfig(key, value);
					Program.serverConfigs.backupPath = value;
				}
				else Console.WriteLine("Error: Path does not exist.");
			}
			else if (key == "backuplimit")
			{
				if (int.TryParse(value, out int result))
				{
					SaveConfig(key, value);
					Program.serverConfigs.autoBackupEveryXDuration = result;
				}
				else Console.WriteLine($"Error: Value for backupLimit must be a positive integer.");
			}
			else ShowSyntaxError();
		}

		private static void SaveConfig(string key, string value)
		{
			Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			configuration.Save(ConfigurationSaveMode.Modified);
			configuration.AppSettings.Settings[key].Value = value;
			configuration.Save(ConfigurationSaveMode.Modified);
			Console.WriteLine($"{key} was set to {value}");
		}

		private static void ShowSyntaxError()
		{
			Console.WriteLine("Error: Incorrect command syntax.");
		}

		private static void RunExitProcedure()
		{
			if (Program.serverConfigs.serverRunning)
			{
				Thread StopServerThread = new Thread(StopServerAndExit);
				StopServerThread.Start();
			}
			else
			{
				Console.WriteLine("Server wrapper stopped.");
				Environment.Exit(0);
			}
		}
	}
}