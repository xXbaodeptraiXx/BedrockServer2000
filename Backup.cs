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

		public static void PerformBackup(bool manualCall)
		{
			Program.serverConfigs.PlayerJoinSinceLastBackup = false;
			if (!manualCall && !Program.serverConfigs.PlayerJoinSinceLastBackup) return;

			Program.serverConfigs.BackupRunning = true;
			try
			{
				if (Program.serverConfigs.ServerRunning)
				{
					CustomConsoleColor.SetColor_Success();
					Console.WriteLine($"{Timing.LogDateTime()} Server backup message sent.");
					Console.ResetColor();
					Program.serverInputStream.WriteLine("save hold");
					Thread.Sleep(5000);
				}

				CustomConsoleColor.SetColor_WorkStart();
				Console.WriteLine($"{Timing.LogDateTime()} Starting backup");

				CustomConsoleColor.SetColor_Work();
				// Remove oldest backups if the number of backups existing is over backupLimit
				// Keep deleting oldest backups until the number of existing backups is smaller than backupLimit
				int currentNumberOfBackups = Directory.GetDirectories(Program.serverConfigs.BackupPath).Length;
				while (currentNumberOfBackups >= Program.serverConfigs.BackupLimit)
				{
					string[] backups = Directory.GetDirectories(Program.serverConfigs.BackupPath);
					Directory.Delete(backups[0], true);
					Console.WriteLine($"{Timing.LogDateTime()} Backup deleted: {backups[0]}");
					currentNumberOfBackups = Directory.GetDirectories(Program.serverConfigs.BackupPath).Length;
				}
				DateTime now = DateTime.Now;
				string newBackupName = $"{now.Day}_{now.Month}_{now.Year}-{now.Hour}_{now.Minute}_{now.Second}";
				Console.WriteLine($"{Timing.LogDateTime()} Copying backup...");
				CopyFilesRecursively(Program.serverConfigs.WorldPath, Program.serverConfigs.BackupPath + "/" + newBackupName);

				if (Program.serverConfigs.ServerRunning) Program.serverInputStream.WriteLine("save resume");
				CustomConsoleColor.SetColor_Success();
				Console.WriteLine($"{Timing.LogDateTime()} Backup saved: {Program.serverConfigs.BackupPath + "/" + newBackupName}");
				Console.ResetColor();
			}
			catch (Exception e)
			{
				CustomConsoleColor.SetColor_Error();
				Console.WriteLine($"EXCEPTION THROWN: {e.Message}");
				Console.WriteLine($"Data: {e.Data}");
				Console.WriteLine($"Source: {e.Source}");
				Console.WriteLine($"StackTrace: {e.StackTrace}");
				Console.ResetColor();
				if (Program.serverConfigs.ServerRunning) Program.serverInputStream.WriteLine($"say Error ocurred while running backup. Exception was thrown ({e.Message}), data:\"{e.Data}\", stackTRace:\"{e.StackTrace}\". PLease contact server admin.");
			}
			Program.serverConfigs.BackupRunning = false;
		}

		public static void LoadBackup()
		{
			int backupsSaved = Directory.GetDirectories(Program.serverConfigs.BackupPath).Length;

			string[] backupLIst = Directory.GetDirectories(Program.serverConfigs.BackupPath);

			// sort backup list by date
			while (true)
			{
				bool sortPerformed = false;
				for (int i = 0, i2 = i + 1; i < backupLIst.Length - 1 && i2 < backupLIst.Length; i += 1, i2 += 1)
				{
					if (DateTime.Compare(Directory.GetCreationTime(backupLIst[i]), Directory.GetCreationTime(backupLIst[i2])) > 0)
					{
						string cache = backupLIst[i2];
						backupLIst[i2] = backupLIst[i];
						backupLIst[i] = cache;
						sortPerformed = true;
					}
				}
				if (!sortPerformed) break;
			}

			Console.WriteLine($"There are {backupsSaved} backups saved, which one would you like to load? (By continuing, you agree to overwrite the existing world and replace it with a chosen backup)");
			for (int i = 0; i < backupLIst.Length; i += 1)
			{
				Console.WriteLine($"{i + 1}: {backupLIst[i]}");
			}
			Console.WriteLine("r: Most recent backup");
			Console.WriteLine("c: Cancel");

			string input = Console.ReadLine();
			int choice;

			if (input.Trim().ToLower() == "c")
			{
				CustomConsoleColor.SetColor_Error();
				Console.WriteLine($"{Timing.LogDateTime()} Load canceled.");
				Console.ResetColor();
				if (Program.serverConfigs.ServerWasRunningBefore) Command.ProcessCommand("start");
				return;
			}
			else if (input.Trim().ToLower() == "r")
			{
				choice = backupLIst.Length;
			}
			else if (!int.TryParse(input, out choice))
			{
				CustomConsoleColor.SetColor_Error();
				Console.WriteLine($"{Timing.LogDateTime()} Invalid input, oad canceled.");
				Console.ResetColor();
				if (Program.serverConfigs.ServerWasRunningBefore) Command.ProcessCommand("start");
				return;
			}
			else if (choice > backupsSaved || choice < 1)
			{
				CustomConsoleColor.SetColor_Error();
				Console.WriteLine($"{Timing.LogDateTime()} Invalid input, oad canceled.");
				Console.ResetColor();
				if (Program.serverConfigs.ServerWasRunningBefore) Command.ProcessCommand("start");
				return;
			}

			try
			{
				Directory.Delete(Program.serverConfigs.WorldPath, true);
			}
			catch (Exception e)
			{
				CustomConsoleColor.SetColor_Error();
				Console.WriteLine($"{Timing.LogDateTime()} Exception thrown: {e.Message}");
				Console.WriteLine($"{Timing.LogDateTime()} Load failed.");
				Console.ResetColor();
				return;
			}
			CustomConsoleColor.SetColor_Work();
			Console.WriteLine($"{Timing.LogDateTime()} World folder deleted.");
			Console.WriteLine($"{Timing.LogDateTime()} Copying \"{backupLIst[choice - 1]}\"");
			CopyFilesRecursively(backupLIst[choice - 1], Program.serverConfigs.WorldPath);
			CustomConsoleColor.SetColor_Success();
			Console.WriteLine($"{Timing.LogDateTime()} Backup loaded \"{backupLIst[choice - 1]}\"");
			Console.ResetColor();

			if (Program.serverConfigs.ServerWasRunningBefore) Command.ProcessCommand("start");
		}
	}
}