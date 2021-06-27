using System;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace BedrockServer2000
{
	public class Events
	{
		public static void AutoBackupEveryXTimer_TIck(object timerAargs)
		{
			if (!Program.serverConfigs.PlayerActivitySinceLastBackup) return;

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

			Backup.PerformBackup(false);

			if (Program.serverConfigs.PlayerList.Count == 0) Program.serverConfigs.PlayerActivitySinceLastBackup = false;
		}

		public static void BedrockServerProcess_Exited(object sender, EventArgs e)
		{
			Program.serverConfigs.ServerRunning = false;
			Program.serverConfigs.ExitCompleted = true;

			Program.ExitTImeoutTImer.Change(Timeout.Infinite, Timeout.Infinite);

			Console.WriteLine($"{Timing.LogDateTime()} Server stopped.");

			if (Program.serverConfigs.LoadRequest)
			{
				Backup.LoadBackup();
				Program.serverConfigs.LoadRequest = false;
			}
			else if (Program.serverConfigs.ExitRequest)
			{
				Console.WriteLine($"{Timing.LogDateTime()} Server wrapper stopped.");
				Environment.Exit(0);
			}
		}

		public static void BedrockServerProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data == null) return;
			else
			{
				string outputData = e.Data;
				if (outputData.StartsWith("NO LOG FILE! - ")) outputData = outputData.Remove(0, 15);
				Console.WriteLine($"{Timing.LogDateTime()} {outputData}");
			}

			if (e.Data.Contains("[INFO] Player connected: "))
			{
				if (Program.serverConfigs.AutoBackupEveryX && !Program.serverConfigs.PlayerActivitySinceLastBackup) Program.serverConfigs.PlayerActivitySinceLastBackup = true;

				string playerName = e.Data.Remove(0, e.Data.IndexOf("[INFO] PLayer connected: ") + 25).Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
				if (!Program.serverConfigs.PlayerList.Exists(x => x == playerName))
				{
					Program.serverConfigs.PlayerList.Add(playerName);
				}
				foreach (string name in Program.serverConfigs.BanList)
				{
					if (name == playerName)
					{
						Console.WriteLine($"{Timing.LogDateTime()} Player name \"{playerName}\" found in ban list.");
						AutoKick(playerName, 5000);
						break;
					}
				}
			}
			else if (e.Data.Contains("[INFO] Player disconnected: "))
			{
				if (Program.serverConfigs.AutoBackupEveryX && !Program.serverConfigs.PlayerActivitySinceLastBackup) Program.serverConfigs.PlayerActivitySinceLastBackup = true;

				string playerName = e.Data.Remove(0, e.Data.IndexOf("[INFO] PLayer disconnected: ") + 28).Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
				if (Program.serverConfigs.PlayerList.Exists(x => x == playerName))
				{
					Program.serverConfigs.PlayerList.Remove(playerName);
				}
			}
		}

		private static async void AutoKick(string name, int delay)
		{
			Thread.Sleep(delay);
			Program.serverInput.WriteLine($"kick {name} Banned player detected");
		}

		public static void OnExit(object sender, EventArgs e)
		{
			if (Program.serverConfigs.ServerRunning && !Program.serverProcess.HasExited)
			{
				Program.serverProcess.Kill();
			}
		}

		public static void ExitTImeoutTImer_Tick(object timerArgs)
		{
			if (!Program.serverConfigs.ExitCompleted)
			{
				Console.WriteLine($"{Timing.LogDateTime()} Exit timed out.");
				Program.serverProcess.Kill();
				Console.WriteLine($"{Timing.LogDateTime()} Force killed server process.");
				Program.serverConfigs.ExitCompleted = true;

				Program.ExitTImeoutTImer.Change(Timeout.Infinite, Timeout.Infinite);
			}
		}

		public static void BanlistScanTimer_Tick(object timerArgs)
		{
			if (Program.serverConfigs.BackupRunning || !Program.serverConfigs.ServerRunning) return;
			foreach (string name in Program.serverConfigs.PlayerList)
			{
				for (int i = 0; i < Program.serverConfigs.BanList.Length; i += 1)
				{
					if (name == Program.serverConfigs.BanList[i])
					{
						Console.WriteLine($"{Timing.LogDateTime()} Player name \"{name}\" found in ban list.");
						Program.serverInput.WriteLine($"kick {name}");
					}
				}
			}
		}
	}
}