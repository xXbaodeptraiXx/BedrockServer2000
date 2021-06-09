namespace BedrockServer2000
{
	public class ServerConfigs
	{
		public bool serverExecutableExists { get; set; }
		public bool backupRunning { get; set; }
		public bool serverRunning { get; set; }

		public bool autoStartServer { get; set; }

		public bool autoBackupOnDate { get; set; }
		public string autoBackupOnDate_Time { get; set; }

		public bool autoBackupEveryX { get; set; }
		public int autoBackupEveryXDuration { get; set; }
		public string autoBackupEveryXTimeUnit { get; set; }

		public string worldPath { get; set; }
		public string backupPath { get; set; }
		public int backupLimit { get; set; }
	}
}