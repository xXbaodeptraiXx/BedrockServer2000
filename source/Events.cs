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
			if (Program.ServerRunning && !Program.serverProcess.HasExited)
			{
				Program.serverProcess.Kill();
			}
		}
		#endregion

		#region Bedrock Server Process Events		
		public static void BedrockServerProcess_Exited(object sender, EventArgs e)
		{
			Program.ServerRunning = false;
			Program.ExitCompleted = true;

			Program.exitTImeoutTImer.Change(Timeout.Infinite, Timeout.Infinite);

			Console.WriteLine($"{Timing.LogDateTime()} Server stopped.");

			if (Program.LoadRequest)
			{
				Backup.LoadBackup();
			}
			else if (Program.ExitRequest)
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

			if (Program.BackupFileListRequest)
			{
				if ((e.Data.Contains(Path.DirectorySeparatorChar)
				|| e.Data.Contains(Path.AltDirectorySeparatorChar))
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

			string outputData = e.Data;
			// remove "NO LOG FILE - " from the server's output data
			if (outputData.StartsWith("NO LOG FILE! - ")) outputData = outputData.Remove(0, 15);
			Console.WriteLine($"{Timing.LogDateTime()} {outputData}");

			if (e.Data.Contains("[INFO] Player connected: "))
			{
				string playerName = e.Data.Remove(0, e.Data.IndexOf("[INFO] Player connected: ") + 25).Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
				string playerXuid = e.Data.Split(" ", StringSplitOptions.RemoveEmptyEntries)[^1];
				OnPlayerJoin(new Player(playerName, playerXuid));
			}
			else if (e.Data.Contains("[INFO] Player disconnected: "))
			{
				string playerName = e.Data.Remove(0, e.Data.IndexOf("[INFO] Player disconnected: ") + 28).Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
				string playerXuid = e.Data.Split(" ", StringSplitOptions.RemoveEmptyEntries)[^1];
				OnPlayerLeave(new Player(playerName, playerXuid));
			}
		}
		#endregion

		#region Timer Ticks
		public static void AutoBackupEveryXTimer_TIck(object timerAargs)
		{
			if (!Program.PlayerActivitySinceLastBackup) return;

			// check if the configs are correct, cancel the backup if found any error
			if (!Directory.Exists((string)Program.ServerConfigs["worldPath"]))
			{
				Console.WriteLine($"World path incorrect, can't perform backup.");
				return;
			}
			if (!Directory.Exists((string)Program.ServerConfigs["backupPath"]))
			{
				Console.WriteLine($"Backup path incorrect, can't perform backup.");
				return;
			}
			if ((int)Program.ServerConfigs["backupLimit"] <= 0)
			{
				Console.WriteLine($"Backup limit can't be smaller than 1, can't perform backup.");
				return;
			}

			if (Program.ServerRunning) Backup.SendOnlineBackupRequest();
			else Backup.PerformOfflineBackup();

			if (Program.Players.Count == 0) Program.PlayerActivitySinceLastBackup = false;
		}

		public static void BanlistScanTimer_Tick(object timerArgs)
		{
			if (Program.BackupRunning || !Program.ServerRunning) return;
			foreach (Player player in Program.Players)
			{
				for (int i = 0; i < ((string[])Program.ServerConfigs["banList"]).Length; i += 1)
				{
					if (player.Name == ((string[])Program.ServerConfigs["banList"])[i])
					{
						Console.WriteLine($"{Timing.LogDateTime()} Player name \"{player.Name}\" found in ban list.");
						Program.serverInput.WriteLine($"kick {player.Name}");
					}
				}
			}
		}

		public static void ExitTImeoutTImer_Tick(object timerArgs)
		{
			if (!Program.ExitCompleted)
			{
				Console.WriteLine($"{Timing.LogDateTime()} Exit timed out.");
				Program.serverProcess.Kill();
				Console.WriteLine($"{Timing.LogDateTime()} Force killed server process.");
				Program.ExitCompleted = true;

				Program.exitTImeoutTImer.Change(Timeout.Infinite, Timeout.Infinite);
			}
		}
		#endregion

		#region Player connection activity
		public static void OnPlayerJoin(Player player)
		{
			if ((bool)Program.ServerConfigs["autoBackupEveryX"] && !Program.PlayerActivitySinceLastBackup) Program.PlayerActivitySinceLastBackup = true;
			if (!Program.Players.Exists(x => x.Name == player.Name))
			{
				Program.Players.Add(player);
			}
			foreach (string playerName in (string[])Program.ServerConfigs["banList"])
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
			if ((bool)Program.ServerConfigs["autoBackupEveryX"] && !Program.PlayerActivitySinceLastBackup) Program.PlayerActivitySinceLastBackup = true;
			if (Program.Players.Exists(x => x.Name == player.Name))
			{
				Program.Players.Remove(Program.Players.Find(x => x.Name == player.Name));
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
