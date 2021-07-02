using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace BedrockServer2000
{
	public static class Backup
	{
		private static void CopyFilesRecursively(string sourcePath, string targetPath)
		{
			// Now Create all of the directories
			foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
			{
				Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
			}

			// Copy all the files & Replaces any files with the same name
			foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
			{
				File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
			}
		}

		public static void SendOnlineBackupRequest()
		{
			Program.serverConfigs.BackupRunning = true;

			Console.WriteLine($"{Timing.LogDateTime()} Starting backup.");
			Program.serverInput.WriteLine("save hold");
			Thread.Sleep(5000);
			Program.serverInput.WriteLine("save query");
			Program.serverConfigs.BackupFileListRequest = true;
		}

		public static void PerformOfflineBackup()
		{
			Program.serverConfigs.BackupRunning = true;
			Console.WriteLine($"{Timing.LogDateTime()} Starting backup.");

			// Remove oldest backups if the number of backups existing is over backupLimit
			// Keep deleting oldest backups until the number of existing backups is smaller than backupLimit
			int currentNumberOfBackups = Directory.GetDirectories(Program.serverConfigs.BackupPath).Length;
			while (currentNumberOfBackups >= Program.serverConfigs.BackupLimit)
			{
				List<BackupPath> backupList = new List<BackupPath>();
				foreach (string path in Directory.GetDirectories(Program.serverConfigs.BackupPath)) backupList.Add(new BackupPath(path));
				backupList.Sort();

				Directory.Delete(backupList[0].Path, true);
				Console.WriteLine($"{Timing.LogDateTime()} Backup deleted: {backupList[0]}");
				currentNumberOfBackups = Directory.GetDirectories(Program.serverConfigs.BackupPath).Length;
			}

			// copy world folder to backup directory
			DateTime now = DateTime.Now;
			string backupDate = $"{now.Day}_{now.Month}_{now.Year}-{now.Hour}_{now.Minute}_{now.Second}";
			Console.WriteLine($"{Timing.LogDateTime()} Copying world...");
			CopyFilesRecursively(Program.serverConfigs.WorldPath, Program.serverConfigs.BackupPath + "/" + backupDate);

			Console.WriteLine($"{Timing.LogDateTime()} Backup saved: {Program.serverConfigs.BackupPath + "/" + backupDate}");
			Program.serverConfigs.BackupRunning = false;
		}

		public static async void PerformOnlineBackup(string rawFileList)
		{
			// example rawFileList:"MyWorl/db/076668.ldb:2063267, MyWorl/db/076172.ldb:2112934, MyWorl/db/075035.ldb:2121417, MyWorl/db/075601.ldb:2128066, MyWorl/db/074313.ldb:2149801, MyWorl/db/076709.ldb:507, MyWorl/db/075034.ldb:2154209, MyWorl/db/070142.ldb:2140762, MyWorl/db/066522.ldb:2112848, MyWorl/db/076678.ldb:2112754, MyWorl/db/076680.ldb:2112708, MyWorl/db/075033.ldb:2122601, MyWorl/db/075596.ldb:2117282, MyWorl/db/066383.ldb:2125019, MyWorl/db/075554.ldb:2071121, MyWorl/db/075553.ldb:2125041, MyWorl/db/066503.ldb:2132426, MyWorl/db/066523.ldb:2117128, MyWorl/db/075036.ldb:2120232, MyWorl/db/075414.ldb:2113422, MyWorl/db/CURRENT:16, MyWorl/db/066498.ldb:2113848, MyWorl/db/075031.ldb:2133023, MyWorl/db/066502.ldb:2137043, MyWorl/db/074935.ldb:2139185, MyWorl/db/066525.ldb:2124410, MyWorl/db/072632.ldb:59792, MyWorl/db/066505.ldb:2124889, MyWorl/db/075552.ldb:2133091, MyWorl/db/075599.ldb:2129055, MyWorl/db/076679.ldb:2127845, MyWorl/db/074941.ldb:2140179, MyWorl/db/066500.ldb:2122266, MyWorl/db/066521.ldb:2138651, MyWorl/db/066378.ldb:2111977, MyWorl/db/075226.ldb:40484, MyWorl/db/075411.ldb:2136644, MyWorl/db/075409.ldb:2115715, MyWorl/db/073897.ldb:2138662, MyWorl/db/066504.ldb:2150317, MyWorl/db/074317.ldb:256637, MyWorl/db/073898.ldb:2140515, MyWorl/db/075030.ldb:2144795, MyWorl/db/075603.ldb:2134094, MyWorl/db/075605.ldb:2069903, MyWorl/db/072543.ldb:82936, MyWorl/db/070050.ldb:17890, MyWorl/db/074311.ldb:2117328, MyWorl/db/066497.ldb:2116838, MyWorl/db/076692.ldb:2085, MyWorl/db/070079.ldb:2124, MyWorl/db/066382.ldb:2114528, MyWorl/db/075472.ldb:2150110, MyWorl/db/075037.ldb:2117574, MyWorl/db/074940.ldb:2137587, MyWorl/db/074312.ldb:2121583, MyWorl/db/066379.ldb:2149251, MyWorl/db/074308.ldb:2121005, MyWorl/db/076209.ldb:2120505, MyWorl/db/076652.ldb:2121663, MyWorl/db/075285.ldb:2126485, MyWorl/db/074307.ldb:2155091, MyWorl/db/074314.ldb:2121459, MyWorl/db/076174.ldb:2130346, MyWorl/db/066517.ldb:2137016, MyWorl/db/075236.ldb:2132397, MyWorl/db/076682.ldb:2125521, MyWorl/db/066524.ldb:2125377, MyWorl/db/074968.ldb:208698, MyWorl/db/074316.ldb:2122258, MyWorl/db/MANIFEST-076708:6562, MyWorl/db/066518.ldb:2131989, MyWorl/db/074360.ldb:2113234, MyWorl/db/076653.ldb:2131, MyWorl/db/066270.ldb:2133555, MyWorl/db/076676.ldb:2135139, MyWorl/db/076693.ldb:258311, MyWorl/db/075038.ldb:97290, MyWorl/db/066506.ldb:2037025, MyWorl/db/076706.ldb:75084, MyWorl/db/066519.ldb:2136538, MyWorl/db/075032.ldb:2127473, MyWorl/db/066271.ldb:247981, MyWorl/db/076677.ldb:2116064, MyWorl/db/076689.ldb:74467, MyWorl/db/074121.ldb:2128699, MyWorl/db/074310.ldb:1031228, MyWorl/db/075598.ldb:2134214, MyWorl/db/075597.ldb:2132783, MyWorl/db/076690.ldb:4120, MyWorl/db/066541.ldb:370537, MyWorl/db/076684.ldb:2126436, MyWorl/db/076691.ldb:1096, MyWorl/db/074942.ldb:49783, MyWorl/db/075413.ldb:2122447, MyWorl/db/075412.ldb:2135156, MyWorl/db/075602.ldb:2127544, MyWorl/db/066377.ldb:2132234, MyWorl/db/076175.ldb:62830, MyWorl/db/076685.ldb:2141856, MyWorl/db/076707.ldb:14193, MyWorl/db/066499.ldb:2145890, MyWorl/db/074315.ldb:2122366, MyWorl/db/075233.ldb:2135740, MyWorl/db/075410.ldb:2110643, MyWorl/db/074938.ldb:2138780, MyWorl/db/066380.ldb:2125759, MyWorl/db/074469.ldb:2117876, MyWorl/db/075234.ldb:2121466, MyWorl/db/066520.ldb:2125759, MyWorl/db/076173.ldb:2118561, MyWorl/db/070099.ldb:2127797, MyWorl/db/066384.ldb:845475, MyWorl/db/066501.ldb:2123349, MyWorl/db/076686.ldb:3800, MyWorl/db/075600.ldb:2115653, MyWorl/db/070143.ldb:2122323, MyWorl/db/076710.log:0, MyWorl/db/074934.ldb:2134942, MyWorl/db/075284.ldb:2141875, MyWorl/db/074936.ldb:2137060, MyWorl/db/074297.ldb:536939, MyWorl/db/066381.ldb:2141687, MyWorl/db/076683.ldb:2143294, MyWorl/db/075235.ldb:2117770, MyWorl/db/076258.ldb:70715, MyWorl/db/074939.ldb:2134834, MyWorl/db/075604.ldb:2118876, MyWorl/db/066376.ldb:2127715, MyWorl/db/074937.ldb:2138470, MyWorl/db/074933.ldb:2129061, MyWorl/db/074309.ldb:2113895, MyWorl/db/076681.ldb:2127646, MyWorl/level.dat:2237, MyWorl/level.dat_old:2237, MyWorl/levelname.txt:7"

			Program.serverConfigs.BackupFileListRequest = false;

			// Remove oldest backups if the number of backups existing is over backupLimit
			// Keep deleting oldest backups until the number of existing backups is smaller than backupLimit
			int currentNumberOfBackups = Directory.GetDirectories(Program.serverConfigs.BackupPath).Length;
			while (currentNumberOfBackups >= Program.serverConfigs.BackupLimit)
			{
				List<BackupPath> backupList = new List<BackupPath>();
				foreach (string path in Directory.GetDirectories(Program.serverConfigs.BackupPath)) backupList.Add(new BackupPath(path));
				backupList.Sort();

				Directory.Delete(backupList[0].Path, true);
				Console.WriteLine($"{Timing.LogDateTime()} Backup deleted: {backupList[0]}");
				currentNumberOfBackups = Directory.GetDirectories(Program.serverConfigs.BackupPath).Length;
			}

			try
			{
				List<string> filesToCopy = new List<string>();
				Console.WriteLine($"{Timing.LogDateTime()} Parsing raw file list.");
				foreach (string file in rawFileList.Split(",", StringSplitOptions.RemoveEmptyEntries)) filesToCopy.Add(file.Split(":", StringSplitOptions.RemoveEmptyEntries)[0].Trim());
				// converts rawFileList to a list of paths
				// examples:
				// "MyWorl/db/076668.ldb"
				// "MyWorl/db/076172.ldb"
				// "MyWorl/db/075035.ldb"

				List<string> destinations = new List<string>();
				DateTime now = DateTime.Now;
				string backupDate = $"{now.Day}_{now.Month}_{now.Year}-{now.Hour}_{now.Minute}_{now.Second}";
				// example backupDate: "30_6_2021-13_42_7"

				Console.WriteLine($"{Timing.LogDateTime()} Creating destination paths.");
				foreach (string file in filesToCopy)
				{
					destinations.Add($"{Program.serverConfigs.BackupPath}/{backupDate}/{file.Remove(0, file.IndexOf("/") + 1)}");
					// converts paths in filesToCopy to destination path that points to the backup path
					// examples:
					// "backups/30_6_2021-13_42_7/MyWorl/db/076668.ldb"
					// "backups/30_6_2021-13_42_7/MyWorl/db/076172.ldb"
					// "backups/30_6_2021-13_42_7/MyWorl/db/075035.ldb"
				}

				Console.WriteLine($"{Timing.LogDateTime()} Creating directories...");
				foreach (string path in destinations)
				{
					string createDirectoryPath = path.Remove(path.LastIndexOf("/"));
					// converts destination paths to paths without file names at the end
					// examples:
					// "backups/30_6_2021-13_42_7/MyWorl/db"
					// "backups/30_6_2021-13_42_7/MyWorl"
					Directory.CreateDirectory(createDirectoryPath);
				}

				Console.WriteLine($"{Timing.LogDateTime()} Copying files...");
				for (int i = 0; i < destinations.Count; i += 1)
				{
					string sourcePath = $"{Program.serverConfigs.WorldPath.Remove(Program.serverConfigs.WorldPath.LastIndexOf("/"))}/{filesToCopy[i]}";
					// joints the world path with the paths in filesToCopy to generate source paths for the files
					// examples:
					// "worlds//MyWorl/db/076668.ldb"
					// "worlds/MyWorl/db/076172.ldb"
					// "worlds/MyWorl/db/075035.ldb"

					File.Copy(sourcePath, destinations[i], true);
				}

				if (Program.serverConfigs.ServerRunning) Program.serverInput.WriteLine("save resume");
				Console.WriteLine($"{Timing.LogDateTime()} Backup saved: {Program.serverConfigs.BackupPath + "/" + backupDate}");
			}
			catch (Exception e)
			{
				Program.serverConfigs.BackupRunning = false;

				Console.WriteLine($"EXCEPTION THROWN: {e.Message}");
				Console.WriteLine($"Data: {e.Data}");
				Console.WriteLine($"Source: {e.Source}");
				Console.WriteLine($"StackTrace: {e.StackTrace}");

				// Send error message to in-game chat if server is running
				if (Program.serverConfigs.ServerRunning) Program.serverInput.WriteLine($"say Error ocurred while running backup. Exception was thrown ({e.Message}), data:\"{e.Data}\", stackTRace:\"{e.StackTrace}\". PLease contact server admin.");
			}

			Program.serverConfigs.BackupRunning = false;
		}

		public static void LoadBackup()
		{
			int backupsSaved = Directory.GetDirectories(Program.serverConfigs.BackupPath).Length;

			List<BackupPath> backupList = new List<BackupPath>();
			foreach (string path in Directory.GetDirectories(Program.serverConfigs.BackupPath)) backupList.Add(new BackupPath(path));
			backupList.Sort();

			for (int i = 0; i < backupList.Count; i += 1)
			{
				Console.WriteLine($"{i + 1}: {backupList[i]}");
			}
			Console.WriteLine($"There are {backupsSaved}/{Program.serverConfigs.BackupLimit} backups saved, which one would you like to load?");
			Console.WriteLine("By continuing, you agree to overwrite the existing world and replace it with a chosen backup.");
			Console.WriteLine("r: Most recent backup");
			Console.WriteLine("c: Cancel");

			string input = Console.ReadLine();
			int choice;

			if (input.Trim().ToLower() == "c")
			{
				Console.WriteLine($"{Timing.LogDateTime()} Load canceled.");
				if (Program.serverConfigs.ServerWasRunningBefore) Command.ProcessCommand("start");
				return;
			}
			else if (input.Trim().ToLower() == "r")
			{
				choice = backupList.Count;
			}
			else if (!int.TryParse(input, out choice))
			{
				Console.WriteLine($"{Timing.LogDateTime()} Invalid input, oad canceled.");
				if (Program.serverConfigs.ServerWasRunningBefore) Command.ProcessCommand("start");
				return;
			}
			else if (choice > backupsSaved || choice < 1)
			{
				Console.WriteLine($"{Timing.LogDateTime()} Invalid input, oad canceled.");
				if (Program.serverConfigs.ServerWasRunningBefore) Command.ProcessCommand("start");
				return;
			}

			Console.WriteLine($"{Timing.LogDateTime()} Copying \"{backupList[choice - 1]}\"");
			CopyFilesRecursively(backupList[choice - 1].Path, Program.serverConfigs.WorldPath);
			Console.WriteLine($"{Timing.LogDateTime()} Backup loaded \"{backupList[choice - 1]}\"");

			if (Program.serverConfigs.ServerWasRunningBefore) Command.ProcessCommand("start");
		}
	}
}
