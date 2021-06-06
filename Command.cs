using System;

namespace BedrockServer2000
{
	public class Command
	{
		public static void ProcessCommand(string command)
		{
			string formattedCommand = command.Trim().ToLower();
			if (formattedCommand == "start") StartServer();
			else if (formattedCommand == "backup") Backup.PerformBackup(Program.serverConfigs);
			else Program.bedrockServerInputStream.WriteLine(formattedCommand);
		}

		private static void StartServer()
		{
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
		}
	}
}