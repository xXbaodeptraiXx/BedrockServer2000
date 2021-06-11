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

			if (Program.serverConfigs.serverRunning)
			{
				Program.bedrockServerInputStream.WriteLine("say Performing backup");
				Console.WriteLine("Telling players that the server is running a backup.");
				Program.bedrockServerInputStream.WriteLine("save hold");
				Thread.Sleep(5000);
			}

			Console.WriteLine("Starting backup");

			// Remove oldest backups if the number of backups existing is over backupLimit
			// Keep deleting oldest backups until the number of existing backups is smaller than backupLimit
			int currentNumberOfBackups = Directory.GetDirectories(Program.serverConfigs.backupPath).Length;
			while (currentNumberOfBackups >= Program.serverConfigs.backupLimit)
			{
				string[] backups = Directory.GetDirectories(Program.serverConfigs.backupPath);
				Directory.Delete(backups[0], true);
				Console.WriteLine($"Backup deleted: {backups[0]}");
				currentNumberOfBackups = Directory.GetDirectories(Program.serverConfigs.backupPath).Length;
			}
			DateTime now = DateTime.Now;
			string newBackupName = $"{now.Day}_{now.Month}_{now.Year}-{now.Hour}_{now.Minute}_{now.Second}";
			Console.WriteLine("Copying backup...");
			CopyFilesRecursively(Program.serverConfigs.worldPath, Program.serverConfigs.backupPath + "/" + newBackupName);

			if (Program.serverConfigs.serverRunning) Program.bedrockServerInputStream.WriteLine("save resume");

			Console.WriteLine($"Backup saved: {Program.serverConfigs.backupPath + "/" + newBackupName}");

			if (Program.serverConfigs.serverRunning) Program.bedrockServerInputStream.WriteLine("say Backup complete");

			Program.serverConfigs.backupRunning = false;

			if (Program.serverConfigs.autoBackupEveryX == true)
			{
				int autoBackupEveryXTimerInterval = 0;
				if (Program.serverConfigs.autoBackupEveryXTimeUnit == "minute") autoBackupEveryXTimerInterval = Timing.MinuteToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration);
				else if (Program.serverConfigs.autoBackupEveryXTimeUnit == "hour") autoBackupEveryXTimerInterval = Timing.HourToMilliseconds(Program.serverConfigs.autoBackupEveryXDuration);
				Program.autoBackupEveryXTimer = new Timer(Backup.PerformBackup, null, autoBackupEveryXTimerInterval, autoBackupEveryXTimerInterval);
			}
		}
	}
}