using System;
using System.Diagnostics;

namespace BedrockServer2000
{
	public class Events
	{
		public static void BedrockServerProcess_Exited(object sender, EventArgs e)
		{
			Console.WriteLine("Server stopped.");
			Program.serverConfigs.serverRunning = false;

			if (Program.serverConfigs.loadRequest)
			{
				Backup.LoadBackup(false);
				Program.serverConfigs.loadRequest = false;
			}
			else if (Program.serverConfigs.exitRequest)
			{
				Console.WriteLine("Server wrapper stopped.");
				Environment.Exit(0);
			}
		}

		public static void BedrockServerProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			Console.WriteLine(e.Data);
		}

		public static void ProgramExited(object sender, EventArgs args)
		{
			if (Program.serverConfigs.serverRunning) Program.bedrockServerProcess.Kill(true);
		}
	}
}