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
		}

		public static void LoadBackup()
		{
			bool serverWasRUnningBefore = Program.serverConfigs.serverRunning;
			if (serverWasRUnningBefore)
			{
				Program.bedrockServerInputStream.WriteLine("say The server is about to close to load a backup.");
				Command.ProcessCommand("stop");
				Thread.Sleep(15000);
			}

			int backupsSaved = Directory.GetDirectories(Program.serverConfigs.backupPath).Length;

			string[] backupLIst = Directory.GetDirectories(Program.serverConfigs.backupPath);

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
			Console.WriteLine("0: Cancel");
			for (int i = 0; i < backupLIst.Length; i += 1)
			{
				Console.WriteLine($"{i + 1}: {backupLIst[i]}");
			}


			if (!int.TryParse(Console.ReadLine(), out int choice))
			{
				Console.WriteLine("Invalid input, load canceled.");
				if (serverWasRUnningBefore) Command.ProcessCommand("start");
				return;
			}

			if (choice > backupsSaved)
			{
				Console.WriteLine("Invalid input, load canceled.");
				if (serverWasRUnningBefore) Command.ProcessCommand("start");
				return;
			}

			if (choice == 0)
			{
				Console.WriteLine("Load canceled.");
				if (serverWasRUnningBefore) Command.ProcessCommand("start");
				return;
			}

			Directory.Delete(Program.serverConfigs.worldPath, true);
			Console.WriteLine("World folder deleted.");
			Console.WriteLine($"Copying \"{Directory.GetDirectories(Program.serverConfigs.backupPath)[choice - 1]}\"");
			CopyFilesRecursively(Directory.GetDirectories(Program.serverConfigs.backupPath)[choice - 1], Program.serverConfigs.worldPath);
			Console.WriteLine($"Backup loaded \"{Directory.GetDirectories(Program.serverConfigs.backupPath)[choice - 1]}\"");

			if (serverWasRUnningBefore) Command.ProcessCommand("start");
		}
	}
}