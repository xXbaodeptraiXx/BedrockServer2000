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
		public static Timer autoBackupEveryXTimer = new Timer(Events.AutoBackupEveryXTimer_TIck);
		public static Timer ExitTImeoutTImer = new Timer(Events.ExitTImeoutTImer_Tick);
		public static Timer BanlistScanTImer = new Timer(Events.BanlistScanTimer_Tick);
		public const string appName = "BedrockServer2000";

		static void Main()
		{
			// debug code
			// Directory.SetCurrentDirectory("/home/bao/bedrock_server");

			if (!File.Exists($"{appName}.conf"))
			{
				CustomConsoleColor.SetColor_Error();
				Console.WriteLine("Configuration file not found.");
				Console.ResetColor();
				return;
			}

			//  Process events
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Events.OnExit);
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(Events.OnExit);

			serverConfigs.LoadConfigs();

			CustomConsoleColor.SetColor_Success();
			Console.WriteLine($"{Timing.LogDateTime()} Server wrapper started.");
			Console.ResetColor();

			if (serverConfigs.AutoStartServer) Command.ProcessCommand("start");

			//TODO: add "banlistScanInterval" key in configuration file to specify the interval between each scan in seconds
			BanlistScanTImer.Change(15000, 15000);

			// console input
			while (true)
			{
				if (serverConfigs.LoadRequest) continue;
				Command.ProcessCommand(Console.ReadLine());
			}
		}
	}
}
