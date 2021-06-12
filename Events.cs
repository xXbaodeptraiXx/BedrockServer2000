using System;
using System.Diagnostics;

namespace BedrockServer2000
{
	public class Events
	{
		public static void BedrockServerProcess_Exited(object sender, EventArgs e)
		{
			Program.serverConfigs.serverRunning = false;
			Console.WriteLine($"{Timing.LogDateTime()} Server stopped.");

			if (Program.serverConfigs.loadRequest)
			{
				Backup.LoadBackup();
				Program.serverConfigs.loadRequest = false;
			}
			else if (Program.serverConfigs.exitRequest)
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
		}

		public static void ProgramExited(object sender, EventArgs args)
		{
			if (Program.serverConfigs.serverRunning) Program.bedrockServerProcess.Kill(true);
		}
	}
}