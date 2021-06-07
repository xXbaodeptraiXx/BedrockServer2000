using System;
using System.IO;
using System.Threading;

namespace BedrockServer2000
{
	public class Backup
	{
		private static void CopyFilesRecursively(string sourcePath, string targetPath)
		{
			//Now Create all of the directories
			foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
			{
				Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
			}

			//Copy all the files & Replaces any files with the same name
			foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
			{
				File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
			}
		}

		public static void PerformBackup(object args)
		{
			Program.serverConfigs.backupRunning = true;
			Program.ConsoleInputThread.Suspend();

			// check if the configs are correct, cancel the backup if found any error
			if (!Directory.Exists(((ServerConfigs)args).worldPath))
			{
				Console.WriteLine($"World path incorrect, can't perform backup");
				return;
			}
			if (!Directory.Exists(((ServerConfigs)args).backupPath))
			{
				Console.WriteLine($"Backup path incorrect, can't perform backup");
				return;
			}
			if (((ServerConfigs)args).backupLimit <= 0)
			{
				Console.WriteLine($"Number of backups to keep can't be smaller than 1, can't perform backup");
				return;
			}

			if (Program.serverConfigs.serverRunning)
			{
				Program.bedrockServerInputStream.WriteLine("say Performing backup");
				Console.WriteLine("Telling players that the server is running a backup");
				File.AppendAllText(@"ServerLog.csv", "\r\n" + DateTime.Now.ToString() + " " + "Telling players that the server is running a backup\r\n");
			}

			if (Program.serverConfigs.serverRunning)
			{
				Program.bedrockServerInputStream.WriteLine("save hold");
				Thread.Sleep(5000);
			}

			Console.WriteLine("Starting Backup");
			File.AppendAllText(@"ServerLog.csv", DateTime.Now.ToString() + " " + "Starting Backup\r\n");

			// Remove oldest backups if the number of backups existing is over the limit (backupLimit)
			// Keep deleting oldest backups until the number of existing backups is smaller than backupLimit
			int currentNumberOfBackups = Directory.GetDirectories(((ServerConfigs)args).backupPath).Length;
			while (currentNumberOfBackups >= ((ServerConfigs)args).backupLimit)
			{
				string[] backups = Directory.GetDirectories(((ServerConfigs)args).backupPath);
				Directory.Delete(backups[0], true);
				Console.WriteLine($"Backup deleted: {backups[0]}");
				File.AppendAllText(@"ServerLog.csv", DateTime.Now.ToString() + " " + $"Backup deleted: {backups[0]}\r\n");
				currentNumberOfBackups = Directory.GetDirectories(((ServerConfigs)args).backupPath).Length;
			}
			DateTime now = DateTime.Now;
			string newBackupName = $"{now.Day}_{now.Month}_{now.Year}-{now.Hour}_{now.Minute}_{now.Second}";
			CopyFilesRecursively(((ServerConfigs)args).worldPath, ((ServerConfigs)args).backupPath + "\\" + newBackupName);
			Thread.Sleep(10000);

			if (Program.serverConfigs.serverRunning) Program.bedrockServerInputStream.WriteLine("save resume");

			Console.WriteLine($"Backup saved: {((ServerConfigs)args).backupPath + "\\" + newBackupName}");
			File.AppendAllText(@"ServerLog.csv", DateTime.Now.ToString() + " " + $"Backup saved: {((ServerConfigs)args).backupPath + "\\" + newBackupName}");

			if (Program.serverConfigs.serverRunning) Program.bedrockServerInputStream.WriteLine("say Backup complete");

			Program.serverConfigs.backupRunning = false;
			Program.consoleInputThread.Resume();
		}
	}
}