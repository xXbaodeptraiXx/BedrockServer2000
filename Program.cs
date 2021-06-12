using System;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace BedrockServer2000
{
	class Program
	{
		public static ServerConfig serverConfigs = new ServerConfig();

		public static Process bedrockServerProcess;
		public static StreamWriter bedrockServerInputStream;

		public static Thread consoleInputThread;

		public static Timer autoBackupEveryXTimer;

		static void Main()
		{
			// debug line
			//Directory.SetCurrentDirectory("/home/bao/bedrock_server");

			serverConfigs.serverRunning = false;
			serverConfigs.backupRunning = false;
			serverConfigs.exitRequest = false;
			serverConfigs.loadRequest = false;
			serverConfigs.serverWasRunningBefore = false;

			Process.GetCurrentProcess().Exited += new EventHandler(Events.ProgramExited);

			serverConfigs.LoadConfigs();

			Console.WriteLine($"{Timing.LogDateTime()} Server wrapper started.");

			if (serverConfigs.autoStartServer) Command.ProcessCommand("start");

			consoleInputThread = new Thread(ConsoleInput);
			consoleInputThread.Start();
		}

		public static void ConsoleInput()
		{
			while (true)
			{
				if (serverConfigs.loadRequest) continue;
				Command.ProcessCommand(Console.ReadLine());
			}
		}
	}
}
