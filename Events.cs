using System;
using System.Diagnostics;
using System.Threading;

namespace BedrockServer2000
{
	public class Events
	{
		public static void BedrockServerProcess_Exited(object sender, EventArgs e)
		{
			Program.serverConfigs.ServerRunning = false;
			Program.serverConfigs.ExitCompleted = true;

			Program.ExitTImeoutTImer.Change(Timeout.Infinite, Timeout.Infinite);

			CustomConsoleColor.SetColor_Success();
			Console.WriteLine($"{Timing.LogDateTime()} Server stopped.");
			Console.ResetColor();

			if (Program.serverConfigs.LoadRequest)
			{
				Backup.LoadBackup();
				Program.serverConfigs.LoadRequest = false;
			}
			else if (Program.serverConfigs.ExitRequest)
			{
				CustomConsoleColor.SetColor_Success();
				Console.WriteLine($"{Timing.LogDateTime()} Server wrapper stopped.");
				Console.ResetColor();
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
		}

		public static void OnExit(object sender, EventArgs e)
		{
			if (Program.serverConfigs.ServerRunning && !Program.serverProcess.HasExited)
			{
				Program.serverProcess.Kill();
			}
		}

		public static void ExitTImeoutTImer_Tick(object args)
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
	}
}