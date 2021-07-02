using System;

namespace BedrockServer2000
{
	// This is not the kind of "backup path" that is specified in the configuration file
	// this kind of backup path looks like this: "backups/30_6_2021-13_42_7"
	public struct BackupPath : IComparable<BackupPath>
	{
		public string Path { get; private set; }
		public DateTime CreationDate { get; private set; }

		public int CompareTo(BackupPath other) => this.CreationDate.CompareTo(other.CreationDate);

		public BackupPath(string path)
		{
			this.Path = path;

			string directoryName = System.IO.Path.GetFileName(Path);
			string date = directoryName.Split("-", StringSplitOptions.RemoveEmptyEntries)[0];
			string time = directoryName.Split("-", StringSplitOptions.RemoveEmptyEntries)[1];

			int year = Convert.ToInt32(date.Split("_", StringSplitOptions.RemoveEmptyEntries)[2]);
			int month = Convert.ToInt32(date.Split("_", StringSplitOptions.RemoveEmptyEntries)[1]);
			int day = Convert.ToInt32(date.Split("_", StringSplitOptions.RemoveEmptyEntries)[0]);

			int hour = Convert.ToInt32(time.Split("_", StringSplitOptions.RemoveEmptyEntries)[0]);
			int minute = Convert.ToInt32(time.Split("_", StringSplitOptions.RemoveEmptyEntries)[1]);
			int second = Convert.ToInt32(time.Split("_", StringSplitOptions.RemoveEmptyEntries)[2]);

			this.CreationDate = new DateTime(year, month, day, hour, minute, second);
		}
	}
}