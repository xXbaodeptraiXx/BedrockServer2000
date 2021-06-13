using System;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace BedrockServer2000
{
	class Program
	{
		public static ServerConfig serverConfigs = new ServerConfig();
		public static Process serverProcess;
		public static StreamWriter serverInputStream;
		public static Thread consoleInputThread;
		public static Timer autoBackupEveryXTimer = new Timer(Backup.PerformBackup);
		public static string appName = "BedrockServer2000";

		static void Main()
		{
			// debug line
			Directory.SetCurrentDirectory("/home/bao/bedrock_server");

			// Process
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Events.OnExit);
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(Events.OnExit);

			serverProcess = new Process();
			serverProcess.StartInfo.FileName = "bedrock_server";
			serverProcess.StartInfo.UseShellExecute = false;
			serverProcess.StartInfo.CreateNoWindow = true;
			serverProcess.StartInfo.RedirectStandardInput = true;
			serverProcess.StartInfo.RedirectStandardOutput = true;
			serverProcess.StartInfo.RedirectStandardError = true;
			serverProcess.EnableRaisingEvents = true;
			serverProcess.OutputDataReceived += new DataReceivedEventHandler(Events.BedrockServerProcess_OutputDataReceived);
			serverProcess.Exited += new EventHandler(Events.BedrockServerProcess_Exited);

			serverConfigs.LoadConfigs();

			Console.WriteLine($"{Timing.LogDateTime()} Server wrapper started.");

			if (serverConfigs.autoStartServer) Command.ProcessCommand("start");

			consoleInputThread = new Thread(ConsoleInput);
			consoleInputThread.Start();
		}

		static void ConsoleInput()
		{
			while (true)
			{
				if (serverConfigs.loadRequest) continue;
				Command.ProcessCommand(Console.ReadLine());
			}
		}
	}
}
