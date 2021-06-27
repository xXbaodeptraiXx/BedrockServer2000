﻿using System;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace BedrockServer2000
{
	class Program
	{
		// main configs
		public const string appName = "bs2k";
		public static ServerConfig serverConfigs = new ServerConfig();

		// server process
		public static Process serverProcess;
		public static StreamWriter serverInput;

		// timers
		public static Timer autoBackupEveryXTimer = new Timer(Events.AutoBackupEveryXTimer_TIck);
		public static Timer ExitTImeoutTImer = new Timer(Events.ExitTImeoutTImer_Tick);
		public static Timer BanlistScanTImer = new Timer(Events.BanlistScanTimer_Tick);

		static void Main()
		{
			if (!File.Exists($"{appName}.conf"))
			{
				Console.WriteLine("Configuration file not found.");
				return;
			}

			//  server process events
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Events.OnExit);
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(Events.OnExit);

			serverConfigs.LoadConfigs();

			Console.WriteLine($"{Timing.LogDateTime()} Server wrapper started.");

			if (serverConfigs.AutoStartServer) Command.ProcessCommand("start");

			//TODO: add "banlistScanInterval" key in configuration file to specify the interval between each scan in seconds
			BanlistScanTImer.Change(15000, 15000);

			// console input loop
			while (true)
			{
				if (serverConfigs.LoadRequest) continue;
				Command.ProcessCommand(Console.ReadLine());
			}
		}
	}
}
