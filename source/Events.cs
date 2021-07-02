using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace BedrockServer2000
{
	public static class Events
	{
		#region Program Events
		public static void OnProgramExit(object sender, EventArgs e)
		{
			if (Program.serverConfigs.ServerRunning && !Program.serverProcess.HasExited)
			{
				Program.serverProcess.Kill();
			}
		}
		#endregion

		#region Bedrock Server Process Events		
		public static void BedrockServerProcess_Exited(object sender, EventArgs e)
		{
			Program.serverConfigs.ServerRunning = false;
			Program.serverConfigs.ExitCompleted = true;

			Program.exitTImeoutTImer.Change(Timeout.Infinite, Timeout.Infinite);

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

			if (e.Data == "Data saved. Files are now ready to be copied." || e.Data == "Saving...")
			{
				Console.WriteLine($"{Timing.LogDateTime()} {e.Data}");
				return;
			}

			if (Program.serverConfigs.BackupFileListRequest)
			{
				if (e.Data.Contains("/")
				&& e.Data.Contains(".ldb")
				&& e.Data.Contains(":")
				&& e.Data.Contains(", "))
				{
					Console.WriteLine($"{Timing.LogDateTime()} Raw file list received.");
					Backup.PerformOnlineBackup(e.Data);
					return;
				}
				else
				{
					Program.serverInput.WriteLine("save query");
				}
			}

			// remove "NO LOG FILE - " from the server's output data
			if (e.Data.StartsWith("NO LOG FILE! - ")) Console.WriteLine($"{Timing.LogDateTime()} {e.Data.Remove(0, 15)}");

			if (e.Data.Contains("[INFO] Player connected: "))
			{
				string playerName = e.Data.Remove(0, e.Data.IndexOf("[INFO] PLayer connected: ") + 25).Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
				string playerXuid = e.Data.Split(" ", StringSplitOptions.RemoveEmptyEntries)[^1];
				OnPlayerJoin(new Player(playerName, playerXuid));
			}
			else if (e.Data.Contains("[INFO] Player disconnected: "))
			{
				string playerName = e.Data.Remove(0, e.Data.IndexOf("[INFO] PLayer disconnected: ") + 28).Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
				string playerXuid = e.Data.Split(" ", StringSplitOptions.RemoveEmptyEntries)[^1];
				OnPlayerLeave(new Player(playerName, playerXuid));
			}
		}
		#endregion

		#region Timer Ticks
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

			if (Program.serverConfigs.ServerRunning) Backup.SendOnlineBackupRequest();
			else Backup.PerformOfflineBackup();

			if (Program.serverConfigs.Players.Count == 0) Program.serverConfigs.PlayerActivitySinceLastBackup = false;
		}

		public static void BanlistScanTimer_Tick(object timerArgs)
		{
			if (Program.serverConfigs.BackupRunning || !Program.serverConfigs.ServerRunning) return;
			foreach (Player player in Program.serverConfigs.Players)
			{
				for (int i = 0; i < Program.serverConfigs.BanList.Length; i += 1)
				{
					if (player.Name == Program.serverConfigs.BanList[i])
					{
						Console.WriteLine($"{Timing.LogDateTime()} Player name \"{player.Name}\" found in ban list.");
						Program.serverInput.WriteLine($"kick {player.Name}");
					}
				}
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

				Program.exitTImeoutTImer.Change(Timeout.Infinite, Timeout.Infinite);
			}
		}
		#endregion

		#region Player connection activity
		public static void OnPlayerJoin(Player player)
		{
			if (Program.serverConfigs.AutoBackupEveryX && !Program.serverConfigs.PlayerActivitySinceLastBackup) Program.serverConfigs.PlayerActivitySinceLastBackup = true;
			if (!Program.serverConfigs.Players.Exists(x => x.Name == player.Name))
			{
				Program.serverConfigs.Players.Add(player);
			}
			foreach (string playerName in Program.serverConfigs.BanList)
			{
				if (playerName == player.Name)
				{
					Console.WriteLine($"{Timing.LogDateTime()} Player name \"{playerName}\" found in ban list.");
					AutoKick(playerName, 5000);
					break;
				}
			}
		}

		public static void OnPlayerLeave(Player player)
		{
			if (Program.serverConfigs.AutoBackupEveryX && !Program.serverConfigs.PlayerActivitySinceLastBackup) Program.serverConfigs.PlayerActivitySinceLastBackup = true;
			if (Program.serverConfigs.Players.Exists(x => x.Name == player.Name))
			{
				Program.serverConfigs.Players.Remove(Program.serverConfigs.Players.Find(x => x.Name == player.Name));
			}
		}
		#endregion

		#region Actions
		private static async void AutoKick(string name, int delay)
		{
			Thread.Sleep(delay);
			Program.serverInput.WriteLine($"kick {name} Banned player detected");
		}
		#endregion
	}
}