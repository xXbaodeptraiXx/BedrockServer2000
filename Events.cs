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
		}

		public static void BedrockServerProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			Console.WriteLine(e.Data);
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