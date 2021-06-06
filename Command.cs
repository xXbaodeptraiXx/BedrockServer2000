using System;
using System.Threading;

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
			Program.bedrockServerProcess.Exited += new EventHandler(Events.bedrockServerProcess_Exited);
			Program.bedrockServerProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(Events.bedrockServerProcess_ErrorDataReceived);
			Program.bedrockServerProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(Events.bedrockServerProcess_OutputDataReceived);

			Program.bedrockServerProcess.Start();

			Program.bedrockServerInputStream = Program.bedrockServerProcess.StandardInput;
			Program.bedrockServerProcess.BeginOutputReadLine();
			Program.bedrockServerProcess.BeginErrorReadLine();

			if (Program.serverConfigs.autoBackupEveryX == true)
			{
				int autoBackupEveryXTimerInterval = 0;
				if (Program.serverConfigs.autoBackupEveryXTimeUnit == "minute") autoBackupEveryXTimerInterval = Timing.MinuteToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration);
				else if (Program.serverConfigs.autoBackupEveryXTimeUnit == "hour") autoBackupEveryXTimerInterval = Timing.HourToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration);
				Program.autoBackupEveryXTimer = new Timer(Backup.PerformBackup, Program.serverConfigs, 0, autoBackupEveryXTimerInterval);
			}
		}
	}
}