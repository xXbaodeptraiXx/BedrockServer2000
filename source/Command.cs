using System;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.IO;

namespace BedrockServer2000
{
	public static class Command
	{
		public static void ProcessCommand(string command)
		{
			if (Program.serverConfigs.BackupRunning) return;

			string formattedCommand = command.Trim().ToLower();

			if (formattedCommand == "commands") ShowHelp("");
			else if (formattedCommand == "start") StartServer();
			else if (formattedCommand == "stop")
			{
				if (Program.serverConfigs.ServerRunning)
				{
					Thread StopServerThread = new Thread(StopServer);
					StopServerThread.Start();
				}
				else
				{
					Console.WriteLine("Server is not currently running.");
				}
			}
			else if (formattedCommand == "load")
			{
				// check if the configs are correct, cancel load if found any error
				if (!Directory.Exists(Program.serverConfigs.WorldPath))
				{
					Console.WriteLine($"World path incorrect");
					return;
				}
				if (!Directory.Exists(Program.serverConfigs.BackupPath))
				{
					Console.WriteLine($"Backup path incorrect");
					return;
				}
				if (Directory.Exists(Program.serverConfigs.BackupPath) && Directory.GetDirectories(Program.serverConfigs.BackupPath).Length < 1)
				{
					Console.WriteLine($"There are no backups to load");
					return;
				}

				Program.serverConfigs.ServerWasRunningBefore = Program.serverConfigs.ServerRunning;
				Program.serverConfigs.LoadRequest = Program.serverConfigs.ServerRunning;
				if (Program.serverConfigs.ServerRunning)
				{
					Program.serverInput.WriteLine("say The server is about to restart to load a backup.");
					StopServer();
				}
				else Backup.LoadBackup();
			}
			else if (formattedCommand == "backup" && !Program.serverConfigs.BackupRunning)
			{
				// check if the configs are correct, cancel the backup if found any error
				if (!Directory.Exists(Program.serverConfigs.WorldPath))
				{
					Console.WriteLine($"World path incorrect, can't perform backup.");
					return;
				}
				if (!Directory.Exists(Program.serverConfigs.BackupPath))
				{
					Console.WriteLine($"Backup path incorrect, can't perform backup.");
					return;
				}
				if (Program.serverConfigs.BackupLimit <= 0)
				{
					Console.WriteLine($"Backup limit can't be smaller than 1, can't perform backup.");
					return;
				}

				if (Program.serverConfigs.ServerRunning) Backup.SendOnlineBackupRequest();
				else Backup.PerformOfflineBackup();
			}
			else if (formattedCommand == "configs") ShowConfigs("");
			else if (formattedCommand == "reload") Program.serverConfigs.LoadConfigs();
			else if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 1)
			{
				if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0] == "commands") ShowHelp(formattedCommand.Remove(0, 9));
				else if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0] == "say")
				{
					if (Program.serverConfigs.ServerRunning)
					{
						Program.serverInput.WriteLine("say " + command.Trim().Remove(0, 4));
						Console.WriteLine($"Message sent to chat (\"{command.Trim().Remove(0, 4)}\")");
					}
					else
					{
						Console.WriteLine("Server is not currently running.");
					}
				}
				else if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 2)
				{
					if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0] == "configs") ShowConfigs(formattedCommand.Remove(0, 8));
					else if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0] == "set") Set(formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1], "");
					else
					{
						if (Program.serverConfigs.ServerRunning) Program.serverInput.WriteLine(command);
						else
						{
							Console.WriteLine("Unknown command.");
						}
					}
				}
				else if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 3)
				{
					if (formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0] == "set") Set(formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1], formattedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[2]);
					else
					{
						if (Program.serverConfigs.ServerRunning) Program.serverInput.WriteLine(command);
						else
						{
							Console.WriteLine("Unknown command.");
						}
					}
				}
				else
				{
					if (Program.serverConfigs.ServerRunning) Program.serverInput.WriteLine(command);
					else
					{
						Console.WriteLine("Unknown command.");
					}
				}
			}
			else if (formattedCommand == "clear") Console.Clear();
			else if (formattedCommand == "exit") RunExitProcedure();
			else
			{
				if (Program.serverConfigs.ServerRunning) Program.serverInput.WriteLine(command);
				else
				{
					Console.WriteLine("Unknown command.");
				}
			}
		}

		public static void ShowHelp(string args)
		{
			if (args == "")
			{
				//TODO: Insert embedded content (commands)
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
Other commands are processed by the bedrock server software if it's running.
");
			}
			else if (args == "set")
			{
				//TODO: Insert embedded content (commands_set)
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
  + set worldPath worlds/Abc
  + set autoStartServer true
  + set backupLimit 32
  + set autoBackupEveryXTimeUnit hour
");
			}
			else if (args == "configs")
			{
				//TODO: Insert embedded content (commands_configs)
				Console.WriteLine(@"Commands > configs:
Purpose: show server wrapper configs
Syntax: 
  + 'configs' : show status of all configs
  + 'configs [config_key]' : show status of a specific config key

use 'configs' to know all the config keys.
use 'configs banlist' to show banned players.

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
			if (!Program.serverConfigs.ServerExecutableExists)
			{
				Console.WriteLine("Server executable not found, can't start server.");
				return;
			}

			Program.serverConfigs.ServerRunning = true;

			Console.WriteLine($"{Timing.LogDateTime()} Starting server.");

			Program.serverProcess = new Process();
			Program.serverProcess.StartInfo.FileName = "bedrock_server";
			Program.serverProcess.StartInfo.UseShellExecute = false;
			Program.serverProcess.StartInfo.CreateNoWindow = true;
			Program.serverProcess.StartInfo.RedirectStandardInput = true;
			Program.serverProcess.StartInfo.RedirectStandardOutput = true;
			Program.serverProcess.StartInfo.RedirectStandardError = true;
			Program.serverProcess.EnableRaisingEvents = true;
			Program.serverProcess.OutputDataReceived += new DataReceivedEventHandler(Events.BedrockServerProcess_OutputDataReceived);
			Program.serverProcess.Exited += new EventHandler(Events.BedrockServerProcess_Exited);
			Program.serverProcess.Start();

			Console.WriteLine($"{Timing.LogDateTime()} Using this terminal: " + Program.serverProcess.StartInfo.FileName);
			Program.serverProcess.BeginOutputReadLine();
			Program.serverProcess.BeginErrorReadLine();
			Program.serverInput = Program.serverProcess.StandardInput;

			if (Program.serverConfigs.AutoBackupEveryX)
			{
				int autoBackupEveryXTimerInterval = 0;
				if (Program.serverConfigs.AutoBackupEveryXTimeUnit == "minute") autoBackupEveryXTimerInterval = Conversions.MinuteToMilliseconds(Program.serverConfigs.AutoBackupEveryXDuration);
				else if (Program.serverConfigs.AutoBackupEveryXTimeUnit == "hour") autoBackupEveryXTimerInterval = Conversions.HourToMilliseconds(Program.serverConfigs.AutoBackupEveryXDuration);
				Program.autoBackupEveryXTimer.Change(autoBackupEveryXTimerInterval, autoBackupEveryXTimerInterval);
			}
		}

		private static void StopServer()
		{
			Program.serverConfigs.ServerRunning = false;
			if (Program.serverConfigs.AutoBackupEveryX) Program.autoBackupEveryXTimer.Change(Timeout.Infinite, Timeout.Infinite);

			const string stopMessage = "Server closing in 10 seconds";

			Program.serverInput.WriteLine($"say {stopMessage}");
			Console.WriteLine($"{Timing.LogDateTime()} Server stop message sent.");
			Thread.Sleep(10000);
			Program.serverInput.WriteLine("stop");

			Program.serverConfigs.ExitCompleted = false;
			Program.exitTImeoutTImer.Change(30000, 30000);
		}

		private static void ShowConfigs(string key)
		{
			if (key == "")
			{
				Console.WriteLine($"autoStartServer = {Program.serverConfigs.AutoStartServer}");
				Console.WriteLine($"utoBackupOnDate = {Program.serverConfigs.AutoBackupOnDate}");
				Console.WriteLine($"autoBackupOnDate_Time = {Program.serverConfigs.AutoBackupOnDate_Time}");
				Console.WriteLine($"autoBackupEveryX = {Program.serverConfigs.AutoBackupEveryX}");
				Console.WriteLine($"autoBackupEveryXDuration = {Program.serverConfigs.AutoBackupEveryXDuration}");
				Console.WriteLine($"autoBackupEveryXTimeUnit = {Program.serverConfigs.AutoBackupEveryXTimeUnit}");
				Console.WriteLine($"worldPath = {Program.serverConfigs.WorldPath}");
				Console.WriteLine($"backupPath = {Program.serverConfigs.BackupPath}");
				Console.WriteLine($"backupLimit = {Program.serverConfigs.BackupLimit}");
				if (Program.serverConfigs.BanList.Length >= 1)
				{
					Console.Write("Banned players: {");
					for (int i = 0; i < Program.serverConfigs.BanList.Length; i += 1)
					{
						Console.Write(Program.serverConfigs.BanList[i]);
						if (i != Program.serverConfigs.BanList.Length - 1) Console.Write(", ");
					}
					Console.WriteLine("}");

					Events.BanlistScanTimer_Tick(null);
				}
				else Console.WriteLine("Ban list is empty.");
			}
			else if (key == "autostartserver") Console.WriteLine($"autoStartServer = {Program.serverConfigs.AutoStartServer}");
			else if (key == "autobackupondate") Console.WriteLine($"utoBackupOnDate = {Program.serverConfigs.AutoBackupOnDate}");
			else if (key == "autobackupondate_time") Console.WriteLine($"autoBackupOnDate_Time = {Program.serverConfigs.AutoBackupOnDate_Time}");
			else if (key == "autobackupeveryx") Console.WriteLine($"autoBackupEveryX = {Program.serverConfigs.AutoBackupEveryX}");
			else if (key == "autobackupeveryxduration") Console.WriteLine($"autoBackupEveryXDuration = {Program.serverConfigs.AutoBackupEveryXDuration}");
			else if (key == "autobackupeveryxtimeunit") Console.WriteLine($"autoBackupEveryXTimeUnit = {Program.serverConfigs.AutoBackupEveryXTimeUnit}");
			else if (key == "worldpath") Console.WriteLine($"worldPath = {Program.serverConfigs.WorldPath}");
			else if (key == "backuppath") Console.WriteLine($"backupPath = {Program.serverConfigs.BackupPath}");
			else if (key == "backuplimit") Console.WriteLine($"backupLimit = {Program.serverConfigs.BackupLimit}");
			else if (key == "banlist")
			{
				if (Program.serverConfigs.BanList.Length >= 1)
				{
					Console.Write("Banned players: {");
					for (int i = 0; i < Program.serverConfigs.BanList.Length; i += 1)
					{
						Console.Write(Program.serverConfigs.BanList[i]);
						if (i != Program.serverConfigs.BanList.Length - 1) Console.Write(", ");
					}
					Console.WriteLine("}");

					Events.BanlistScanTimer_Tick(null);
				}
				else Console.WriteLine("Ban list is empty.");
			}
			else Console.WriteLine($"Error: Unknown config key");
		}

		private static void Set(string key, string value)
		{
			if (key == "autostartserver")
			{
				if (value == "true" || value == "false")
				{
					SaveConfig(key, value);
					Program.serverConfigs.AutoStartServer = Convert.ToBoolean(value);
				}
				else Console.WriteLine($"Error: Available config values for autoStartServer are 'true' and 'false'.");
			}
			else if (key == "autobackupondate")
			{
				if (value == "true" || value == "false")
				{
					SaveConfig(key, value);
					Program.serverConfigs.AutoBackupOnDate = Convert.ToBoolean(value);
				}
				else Console.WriteLine($"Error: Available config values for autoBackupOnDate are 'true' and 'false'.");
			}
			else if (key == "autobackupondate_time")
			{
				if (DateTime.TryParseExact(value, "H:m:s", null, DateTimeStyles.None, out DateTime result))
				{
					SaveConfig(key, value);
					Program.serverConfigs.AutoBackupOnDate_Time = value;
				}
				else Console.WriteLine($"Error: Value for autoBackupOnDate_Time must be a time value in h:m:s format.");
			}
			else if (key == "autobackupeveryx")
			{
				if (value == "true" || value == "false")
				{
					SaveConfig(key, value);
					Program.serverConfigs.AutoBackupEveryX = Convert.ToBoolean(value);

					if (value == "false") Program.autoBackupEveryXTimer.Change(Timeout.Infinite, Timeout.Infinite);
					if (Program.serverConfigs.ServerRunning && value == "true")
					{
						int autoBackupEveryXTimerInterval = 0;
						if (Program.serverConfigs.AutoBackupEveryXTimeUnit == "minute") autoBackupEveryXTimerInterval = Conversions.MinuteToMilliseconds(Program.serverConfigs.AutoBackupEveryXDuration);
						else if (Program.serverConfigs.AutoBackupEveryXTimeUnit == "hour") autoBackupEveryXTimerInterval = Conversions.HourToMilliseconds(Program.serverConfigs.AutoBackupEveryXDuration);
						Program.autoBackupEveryXTimer.Change(autoBackupEveryXTimerInterval, autoBackupEveryXTimerInterval);
					}
				}
				else Console.WriteLine($"Error: Available config values for autoBackupEveryX are 'true' and 'false'.");
			}
			else if (key == "autobackupeveryxduration")
			{
				if (int.TryParse(value, out int result))
				{
					SaveConfig(key, value);
					Program.serverConfigs.AutoBackupEveryXDuration = result;

					if (Program.serverConfigs.ServerRunning && Program.serverConfigs.AutoBackupEveryX)
					{
						if (Program.serverConfigs.AutoBackupEveryXTimeUnit == "minute")
							Program.autoBackupEveryXTimer.Change(Conversions.MinuteToMilliseconds(Program.serverConfigs.AutoBackupEveryXDuration), Conversions.MinuteToMilliseconds(Program.serverConfigs.AutoBackupEveryXDuration));
						else if (Program.serverConfigs.AutoBackupEveryXTimeUnit == "hour")
							Program.autoBackupEveryXTimer.Change(Conversions.HourToMilliseconds(Program.serverConfigs.AutoBackupEveryXDuration), Conversions.HourToMilliseconds(Program.serverConfigs.AutoBackupEveryXDuration));
					}
				}
				else Console.WriteLine($"Error: Value for autoBackupEveryXDuration must be a positive integer.");
			}
			else if (key == "autobackupeveryxtimeunit")
			{
				if (value == "minute")
				{
					SaveConfig(key, value);
					Program.serverConfigs.AutoBackupEveryXTimeUnit = value;
					if (Program.serverConfigs.ServerRunning && Program.serverConfigs.AutoBackupEveryX) Program.autoBackupEveryXTimer.Change(Conversions.MinuteToMilliseconds(Program.serverConfigs.AutoBackupEveryXDuration), Conversions.MinuteToMilliseconds(Program.serverConfigs.AutoBackupEveryXDuration));
				}
				else if (value == "hour")
				{
					SaveConfig(key, value);
					Program.serverConfigs.AutoBackupEveryXTimeUnit = value;
					if (Program.serverConfigs.ServerRunning && Program.serverConfigs.AutoBackupEveryX) Program.autoBackupEveryXTimer.Change(Conversions.HourToMilliseconds(Program.serverConfigs.AutoBackupEveryXDuration), Conversions.HourToMilliseconds(Program.serverConfigs.AutoBackupEveryXDuration));
				}
				else Console.WriteLine($"Error: Available config values for autoBackupEveryXTimeUnit are 'minute' and 'hour'.");
			}
			else if (key == "worldpath")
			{
				if (Directory.Exists(value))
				{
					SaveConfig(key, value);
					Program.serverConfigs.WorldPath = value;
				}
				else Console.WriteLine("Error: Path does not exist.");
			}
			else if (key == "backuppath")
			{
				if (Directory.Exists(value))
				{
					SaveConfig(key, value);
					Program.serverConfigs.BackupPath = value;
				}
				else Console.WriteLine("Error: Path does not exist.");
			}
			else if (key == "backuplimit")
			{
				if (int.TryParse(value, out int result))
				{
					SaveConfig(key, value);
					Program.serverConfigs.BackupLimit = result;
				}
				else Console.WriteLine($"Error: Value for backupLimit must be a positive integer.");
			}
			else ShowSyntaxError();
		}

		private static void SaveConfig(string key, string value)
		{
			Configs.SetValue(key, value);
			Console.WriteLine($"{Timing.LogDateTime()} {key} was set to {value}");
		}

		private static void ShowSyntaxError()
		{
			Console.WriteLine("Error: Incorrect command syntax.");
		}

		private static void RunExitProcedure()
		{
			if (Program.serverConfigs.ServerRunning)
			{
				Program.serverConfigs.ExitRequest = true;
				Thread StopServerThread = new Thread(StopServer);
				StopServerThread.Start();
			}
			else
			{
				Console.WriteLine($"{Timing.LogDateTime()} Server wrapper stopped.");
				Environment.Exit(0);
			}
		}
	}
}