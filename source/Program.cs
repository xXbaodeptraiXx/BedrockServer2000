using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace BedrockServer2000
{
	public static class Program
	{
		// configs
		public const string appName = "bs2k";
		public static ServerConfig serverConfigs;

		// server process
		public static Process serverProcess;
		public static StreamWriter serverInput;

		// timers
		public static Timer autoBackupEveryXTimer;
		public static Timer exitTImeoutTImer;
		public static Timer banlistScanTImer;

		private static void InitializeComponents()
		{
			// timers
			autoBackupEveryXTimer = new Timer(Events.AutoBackupEveryXTimer_TIck);
			exitTImeoutTImer = new Timer(Events.ExitTImeoutTImer_Tick);
			banlistScanTImer = new Timer(Events.BanlistScanTimer_Tick);

			// server process events
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Events.OnProgramExit);
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(Events.OnProgramExit);

			// serverConfigs
			serverConfigs = new ServerConfig();
			serverConfigs.LoadConfigs();
		}

		static void Main()
		{
			if (!File.Exists($"{appName}.conf"))
			{
				Console.WriteLine("Configuration file not found.");
				return;
			}

			InitializeComponents();

			Console.WriteLine($"{Timing.LogDateTime()} Server wrapper started.");

			if (serverConfigs.AutoStartServer) Command.ProcessCommand("start");

			// start the banlist scan timer
			//TODO: add "banlistScanInterval" key in configuration file to specify the interval between each scan in seconds
			banlistScanTImer.Change(15000, 15000);

			// console input loop
			while (true)
			{
				if (serverConfigs.LoadRequest) continue;
				Command.ProcessCommand(Console.ReadLine());
			}
		}
	}
}
