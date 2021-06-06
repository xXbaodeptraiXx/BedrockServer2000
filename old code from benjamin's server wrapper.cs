using System;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Configuration;

namespace Minecraft_Bedrock_Server_Manager
{
	public partial class MainForm : Form
	{
		Process minecraftProcess;
		StreamWriter mcInputStream;

		readonly bool serverExecutableExists = File.Exists("bedrock_server.exe");

		string players = "Loading Player List.....";
		string players2 = "";

		string worldPath = "";
		string backupPath = "";
		int backupLimit;

		bool backupRunning;
		bool serverRunning;

		private System.Threading.Timer autoBackupOnDateTimer;

		public delegate void fpTextBoxCallback_t(string strText);
		public fpTextBoxCallback_t fpTextBoxCallback;

		string autoStartServer = "";
		string autoBackupOnDate = "";
		string autoBackupOnDate_Time = "";

		string autoBackupEveryX = "";
		int autoBackupEveryXDuration = 0;
		string autoBackupEveryXTimeUnit = "";

		int restartTryLimit = 0;

		public MainForm()
		{
			// load default configs and set them to the configs file if keys are empty, if not, load the configs from configs file
			if (ConfigurationManager.AppSettings["autoStartServer"].ToString() != "true" && ConfigurationManager.AppSettings["autoStartServer"].ToString() != "false")
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoStartServer"].Value = "false";
				configuration.Save(ConfigurationSaveMode.Modified);
				autoStartServer = "false";
			}
			else
				autoStartServer = ConfigurationManager.AppSettings["autoStartServer"].ToString();

			if (ConfigurationManager.AppSettings["worldPath"].ToString() == "" && Directory.Exists("worlds"))
			{
				if (Directory.GetDirectories("worlds").Length >= 1)
				{
					Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					configuration.Save(ConfigurationSaveMode.Modified);
					configuration.AppSettings.Settings["worldPath"].Value = Directory.GetDirectories("worlds")[0];
					configuration.Save(ConfigurationSaveMode.Modified);
					worldPath = Directory.GetDirectories("worlds")[0];
				}
			}
			else
				worldPath = ConfigurationManager.AppSettings["worldPath"].ToString();

			if (!int.TryParse(ConfigurationManager.AppSettings["backupLimit"].ToString(), out int importVal))
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["backupLimit"].Value = Convert.ToString(32);
				configuration.Save(ConfigurationSaveMode.Modified);
				backupLimit = 32;
			}
			else
				backupLimit = Convert.ToInt32(ConfigurationManager.AppSettings["backupLimit"]);

			if (ConfigurationManager.AppSettings["autoBackupOnDate"].ToString() != "true" && ConfigurationManager.AppSettings["autoBackupOnDate"].ToString() != "false")
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupOnDate"].Value = "false";
				configuration.Save(ConfigurationSaveMode.Modified);
				autoBackupOnDate = "false";
			}
			else
				autoBackupOnDate = ConfigurationManager.AppSettings["autoBackupOndate"].ToString();

			if (ConfigurationManager.AppSettings["autoBackupOnDate_Time"].ToString() == "")
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupOnDate_Time"].Value = "00:00:00";
				configuration.Save(ConfigurationSaveMode.Modified);
				autoBackupOnDate_Time = "00:00:00";
			}
			else
				autoBackupOnDate_Time = ConfigurationManager.AppSettings["autoBackupOnDate_Time"].ToString();

			if (ConfigurationManager.AppSettings["autoBackupEveryX"].ToString() != "true" && ConfigurationManager.AppSettings["autoBackupEveryX"].ToString() != "false")
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupEveryX"].Value = "false";
				configuration.Save(ConfigurationSaveMode.Modified);
				autoBackupEveryX = "false";
			}
			else
				autoBackupEveryX = ConfigurationManager.AppSettings["autoBackupEveryX"].ToString();

			if (!int.TryParse(ConfigurationManager.AppSettings["autoBackupEveryXDuration"].ToString(), out importVal))
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupEveryXDuration"].Value = "1";
				configuration.Save(ConfigurationSaveMode.Modified);
				autoBackupEveryXDuration = 1;
			}
			else
				autoBackupEveryXDuration = Convert.ToInt32(ConfigurationManager.AppSettings["autoBackupEveryXDuration"].ToString());

			if (ConfigurationManager.AppSettings["autoBackupEveryXTimeUnit"].ToString() != "minute" && ConfigurationManager.AppSettings["autoBackupEveryXTimeUnit"].ToString() != "hour")
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupEveryXTimeUnit"].Value = "hour";
				configuration.Save(ConfigurationSaveMode.Modified);
				autoBackupEveryXTimeUnit = "hour";
			}
			else
				autoBackupEveryXTimeUnit = ConfigurationManager.AppSettings["autoBackupEveryXTimeUnit"].ToString();

			backupPath = ConfigurationManager.AppSettings["backupPath"].ToString();

			//DateTime dt = Convert.ToDateTime(DateTime.Now, System.Globalization.CultureInfo.CreateSpecificCulture("en-us").DateTimeFormat);

			CheckForIllegalCrossThreadCalls = false;
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
			fpTextBoxCallback = new fpTextBoxCallback_t(AddTextToOutputTextBox);
			InitializeComponent();


			if (autoBackupEveryX == "true")
			{
				autoBackupEveryXCheckBox.Checked = true;
			}
			else
			{
				autoBackupEveryXCheckBox.Checked = false;
			}

			autoBackupEveryXDurationNumericUpDown.Value = autoBackupEveryXDuration;
			autoBackupEveryXTimeUnitCombo.Text = autoBackupEveryXTimeUnit;

			autoBackupEveryXTimer.Interval = autoBackupEveryXTimerIntervalCalc(autoBackupEveryXDuration, autoBackupEveryXTimeUnit);

			if (autoBackupOnDateCheckBox.Checked)
				autoBackupOnDate_DateTimePicker.Enabled = true;
			else
				autoBackupOnDate_DateTimePicker.Enabled = false;

			if (autoBackupEveryXCheckBox.Checked)
			{
				autoBackupEveryXDurationNumericUpDown.Enabled = true;
				autoBackupEveryXTimeUnitCombo.Enabled = true;
				autoBackupEveryXTimer.Start();
			}
			else
			{
				autoBackupEveryXDurationNumericUpDown.Enabled = false;
				autoBackupEveryXTimeUnitCombo.Enabled = false;
			}

			worldPathTxtBox.Text = worldPath;
			backupPathTxtBox.Text = backupPath;
			backupLimitNumericUpDown.Value = backupLimit;


			autoBackupOnDate_DateTimePicker.Text = autoBackupOnDate_Time;
			List<string> weatherList = new List<string>();
			weatherList.Add("clear");
			weatherList.Add("rain");
			weatherList.Add("thunder");

			List<string> gameRuleList = new List<string>();
			gameRuleList.Add("commandblockoutput");
			gameRuleList.Add("commandblocksenabled");
			gameRuleList.Add("dodaylightcycle");
			gameRuleList.Add("doentitydrops");
			gameRuleList.Add("dofiretick");
			gameRuleList.Add("doimmediaterespawn");
			gameRuleList.Add("doinsomnia");
			gameRuleList.Add("domobloot");
			gameRuleList.Add("domobspawning");
			gameRuleList.Add("dotiledrops");
			gameRuleList.Add("doweathercycle");
			gameRuleList.Add("drowningdamage");
			gameRuleList.Add("falldamage");
			gameRuleList.Add("firedamage");
			//gameRuleList.Add("functioncommandlimit");
			gameRuleList.Add("keepinventory");
			//gameRuleList.Add("maxcommandchainlength");
			gameRuleList.Add("mobgriefing");
			gameRuleList.Add("naturalregeneration");
			gameRuleList.Add("pvp");
			//gameRuleList.Add("randomtickspeed");
			gameRuleList.Add("sendcommandfeedback");
			gameRuleList.Add("showcoordinates");
			gameRuleList.Add("showdeathmessages");
			gameRuleList.Add("tntexplodes");

			stopServerButton.Enabled = false;
			backupButton.Enabled = false;

			setWeatherButton.Enabled = false;
			opPlayerButton.Enabled = false;
			deOpPlayerButton.Enabled = false;
			setGameruleButton.Enabled = false;

			trueGRRadioButton.Enabled = false;
			falseGRRadioButton2.Enabled = false;

			txtInputCommand.Enabled = false;
			btnExecute.Enabled = false;

			if (autoStartServer == "true")
			{
				startServerCheckbox.Checked = true;
				startServerButton_Click(null, EventArgs.Empty);
			}
			if (autoBackupOnDate == "true")
			{
				autoBackupOnDateCheckBox.Checked = true;
				autoBackupOnDateTimerInitialize();
			}

			weatherComboBox.DataSource = weatherList;
			gameRuleComboBox.DataSource = gameRuleList;
		}

		#region Output Processing
		private void ConsoleOutputHandler(object sendingProcess, System.Diagnostics.DataReceivedEventArgs outLine)
		{
			if (!string.IsNullOrEmpty(outLine.Data))
			{
				if (InvokeRequired)
					Invoke(fpTextBoxCallback, Environment.NewLine + outLine.Data);
				else
					fpTextBoxCallback(Environment.NewLine + outLine.Data);
			}
		}

		public void AddTextToOutputTextBox(string strText)
		{
			string blah = strText.Replace("\r\n", "");
			try
			{
				if (blah.Contains("CrashReporter") && restartTryLimit < 3)
				{
					Thread.Sleep(5000);
					startServerButton_Click(null, EventArgs.Empty);
					restartTryLimit++;
					return;
				}
				if (blah.Contains("Server Started."))
				{
					restartTryLimit = 0;
				}
				if (blah.Contains("Network port occupied, can't start server."))
				{
					this.txtOutput.AppendText(strText);
					txtOutput.ScrollToCaret();
					File.AppendAllText(@"ServerLog.csv", strText);
					return;
				}
				if (blah.Contains("commandblock") && blah.Contains("="))
				{
					string removeComma = blah.Replace(", ", "\r\n");
					gameRulesTxtOutput.Text = removeComma;
					return;
				}
				if (strText.Contains("players online"))
				{

					strText = strText.Replace("\r\n", "");
				}
				if (blah.Contains(", xuid") || blah.Contains(", port"))
				{
					this.txtOutput.AppendText(strText);
					txtOutput.ScrollToCaret();
					File.AppendAllText(@"ServerLog.csv", strText);
				}
				else if (strText.Contains("players online"))
				{
					players = players + strText + "\r\n";
				}
				else if (strText.Contains("players online") || (!blah.Contains(" ") && strText.Length > 0) || blah.Contains(", "))
				{
					string removeCR = strText.Replace("\r\n", "");
					removeCR = removeCR.Replace(" ", "");
					string[] names = removeCR.Split(',');
					Array.Sort(names);
					string result = string.Join("\r\n", names);
					players = players + result;
				}
				else
				{
					this.txtOutput.AppendText(strText);
					txtOutput.ScrollToCaret();
					File.AppendAllText(@"ServerLog.csv", strText);
				}
			}
			catch
			{
			}
		}
		#endregion

		#region Events
		#region Main Server Administraion Button Events
		private void startServerButton_Click(object sender, EventArgs e)
		{
			if (!serverExecutableExists)
			{
				txtOutput.Text += "Server executable not found, can't start server.\r\n";
				return;
			}
			serverRunning = true;

			startServerButton.Enabled = false;
			stopServerButton.Enabled = true;
			backupButton.Enabled = true;

			setWeatherButton.Enabled = true;
			opPlayerButton.Enabled = true;
			deOpPlayerButton.Enabled = true;
			setGameruleButton.Enabled = true;

			trueGRRadioButton.Enabled = true;
			falseGRRadioButton2.Enabled = true;

			txtInputCommand.Enabled = true;
			btnExecute.Enabled = true;

			string processFileName = "";

			processFileName = @"bedrock_server.exe";
			minecraftProcess = new System.Diagnostics.Process();

			minecraftProcess.StartInfo.FileName = processFileName;
			AddTextToOutputTextBox("Using this terminal: " + minecraftProcess.StartInfo.FileName);

			minecraftProcess.StartInfo.UseShellExecute = false;
			minecraftProcess.StartInfo.CreateNoWindow = true;
			minecraftProcess.StartInfo.RedirectStandardInput = true;
			minecraftProcess.StartInfo.RedirectStandardOutput = true;
			minecraftProcess.StartInfo.RedirectStandardError = true;

			minecraftProcess.EnableRaisingEvents = true;
			minecraftProcess.Exited += new EventHandler(ProcessExited);
			minecraftProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(ConsoleOutputHandler);
			minecraftProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(ConsoleOutputHandler);

			minecraftProcess.Start();

			mcInputStream = minecraftProcess.StandardInput;
			minecraftProcess.BeginOutputReadLine();
			minecraftProcess.BeginErrorReadLine();

			mcInputStream.WriteLine("gamerule");

			playersOnlineUpdateTimer.Start();
		}

		private void stopServerButton_Click(object sender, EventArgs e)
		{
			backgroundStopServer.RunWorkerAsync();
		}

		private void backupButton_Click(object sender, EventArgs e)
		{
			if (!backupRunning)
				backgroundBackup.RunWorkerAsync();
		}
		#endregion
		#region Advanced Server Administration Button Events
		private void btnExecute_Click(object sender, EventArgs e)
		{
			try
			{
				if (this.minecraftProcess.HasExited)
				{
					txtOutput.AppendText("\r\n\r\nThe server has been shutdown.\r\n");
					File.AppendAllText(@"ServerLog.csv", "\r\n" + DateTime.Now.ToString() + " " + "The server has been shutdown.\r\n");
					return;
				}
				mcInputStream.WriteLine(txtInputCommand.Text);
			}
			catch
			{
			}
		}

		private void setWeatherButton_Click(object sender, EventArgs e)
		{
			try
			{
				mcInputStream.WriteLine("weather " + weatherComboBox.SelectedItem);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void opPlayerButton_Click(object sender, EventArgs e)
		{
			try
			{
				mcInputStream.WriteLine("op " + opPlayerTextBox1.Text.ToString());
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void deOpPlayerButton_Click(object sender, EventArgs e)
		{
			try
			{
				mcInputStream.WriteLine("deop " + deOpTextBox1.Text.ToString());
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			string trueFalse = " false";
			if (trueGRRadioButton.Checked)
				trueFalse = " true";
			if (falseGRRadioButton2.Checked)
				trueFalse = " false";
			try
			{
				mcInputStream.WriteLine("gamerule " + gameRuleComboBox.SelectedItem + trueFalse);
				mcInputStream.WriteLine("gamerule ");
			}
			catch
			{
			}
		}
		#endregion
		#region Advanced Interactive Server Administraion Events
		#region Set Weather Combo Box
		private void weatherComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				setWeatherButton_Click(sender, e);
		}
		#endregion
		#region OP Player Text Box
		private void opPlayerTextBox1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				opPlayerButton_Click(sender, e);
		}
		#endregion
		#region DeOP Player Text Box
		private void deOpTextBox1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				deOpPlayerButton_Click(sender, e);
		}
		#endregion
		#region Set Game Rule Combo Box
		private void gameRuleComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				button1_Click(sender, e);
		}
		#endregion
		#region Set Game Rule TrueFalse Radio Button
		private void trueGRRadioButton_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				button1_Click(sender, e);
		}

		private void falseGRRadioButton2_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				button1_Click(sender, e);
		}
		#endregion
		#region Command Input Text Box
		private void txtInputCommand_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				btnExecute_Click(sender, e);
		}
		#endregion
		#region Configs
		#region Auto Start Server
		private void autoStartServerCheckVBox_checkChanged(object sender, EventArgs e)
		{
			Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			configuration.Save(ConfigurationSaveMode.Modified);
			configuration.AppSettings.Settings["autoStartServer"].Value = startServerCheckbox.Checked.ToString().ToLower();
			configuration.Save(ConfigurationSaveMode.Modified);
		}
		#endregion
		#region Auto Backup Check Boxes
		private void autoBackupOnDateCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			autoBackupOnDate_DateTimePicker.Enabled = autoBackupOnDateCheckBox.Checked;
			if (autoBackupOnDateCheckBox.Checked)
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupOnDate"].Value = "true";
				configuration.Save(ConfigurationSaveMode.Modified);

				autoBackupOnDate = "true";

				autoBackupOnDateTimerInitialize();
			}
			else
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupOnDate"].Value = "false";
				configuration.Save(ConfigurationSaveMode.Modified);

				autoBackupOnDate = "false";

				autoBackupOnDateTimer = null;
			}
		}

		private void autoBackupEveryXCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (autoBackupEveryXCheckBox.Checked)
			{
				if (!autoBackupEveryXTimer.Enabled)
					autoBackupEveryXTimer.Start();

				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupEveryX"].Value = "true";
				configuration.Save(ConfigurationSaveMode.Modified);
				autoBackupEveryX = "true";

				autoBackupEveryXDurationNumericUpDown.Enabled = true;
				autoBackupEveryXTimeUnitCombo.Enabled = true;
			}
			else
			{
				if (autoBackupEveryXTimer.Enabled)
					autoBackupEveryXTimer.Stop();

				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupEveryX"].Value = "false";
				configuration.Save(ConfigurationSaveMode.Modified);
				autoBackupEveryX = "false";

				autoBackupEveryXDurationNumericUpDown.Enabled = false;
				autoBackupEveryXTimeUnitCombo.Enabled = false;
			}
		}
		#endregion
		#region Auto Backup Interactive Elements
		#region Auto Backup On Date
		private void autoBackupOnDate_DateTimePicker_ValueChanged(object sender, EventArgs e)
		{
			autoBackupOnDateTimerInitialize();
			Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			configuration.Save(ConfigurationSaveMode.Modified);
			configuration.AppSettings.Settings["autoBackupOnDate_Time"].Value = autoBackupOnDate_DateTimePicker.Value.TimeOfDay.ToString();
			configuration.Save(ConfigurationSaveMode.Modified);
		}
		#endregion
		#region Auto Backup Every X
		private void autoBackupEveryXDurationNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (autoBackupEveryXDurationNumericUpDown.Value < 1)
				autoBackupEveryXDurationNumericUpDown.Value = 1;

			Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			configuration.Save(ConfigurationSaveMode.Modified);
			configuration.AppSettings.Settings["autoBackupEveryXDuration"].Value = Convert.ToString(autoBackupEveryXDurationNumericUpDown.Value);
			configuration.Save(ConfigurationSaveMode.Modified);
			autoBackupEveryXDuration = (int)autoBackupEveryXDurationNumericUpDown.Value;

			autoBackupEveryXTimer.Interval = autoBackupEveryXTimerIntervalCalc(autoBackupEveryXDuration, autoBackupEveryXTimeUnit);
			autoBackupEveryXTimer.Stop();
			autoBackupEveryXTimer.Start();
		}

		private void autoBackupEveryXTimeUnitCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (autoBackupEveryXTimeUnitCombo.Text == "minute" || autoBackupEveryXTimeUnitCombo.Text == "hour")
			{
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configuration.Save(ConfigurationSaveMode.Modified);
				configuration.AppSettings.Settings["autoBackupEveryXTimeUnit"].Value = autoBackupEveryXTimeUnitCombo.Text;
				configuration.Save(ConfigurationSaveMode.Modified);
				autoBackupEveryXTimeUnit = autoBackupEveryXTimeUnitCombo.Text;
			}
			autoBackupEveryXTimer.Interval = autoBackupEveryXTimerIntervalCalc(autoBackupEveryXDuration, autoBackupEveryXTimeUnit);
			autoBackupEveryXTimer.Stop();
			autoBackupEveryXTimer.Start();
		}
		#endregion
		#region World Path, Backup Path and Backup Limit
		private void worldPathChangeButton_Click(object sender, EventArgs e)
		{
			Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			configuration.Save(ConfigurationSaveMode.Modified);
			configuration.AppSettings.Settings["worldPath"].Value = worldPathTxtBox.Text;
			configuration.Save(ConfigurationSaveMode.Modified);
			worldPath = worldPathTxtBox.Text;
		}

		private void backupPathChangeButton_Click(object sender, EventArgs e)
		{
			Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			configuration.Save(ConfigurationSaveMode.Modified);
			configuration.AppSettings.Settings["backupPath"].Value = backupPathTxtBox.Text;
			configuration.Save(ConfigurationSaveMode.Modified);
			backupPath = backupPathTxtBox.Text;
		}

		private void backupsLimitNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			configuration.Save(ConfigurationSaveMode.Modified);
			configuration.AppSettings.Settings["backupLimit"].Value = Convert.ToString((int)backupLimitNumericUpDown.Value);
			configuration.Save(ConfigurationSaveMode.Modified);
			backupLimit = (int)backupLimitNumericUpDown.Value;
		}
		#endregion
		#endregion
		#endregion
		#endregion
		#region Other Events
		private void backgroundStopServer_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			try
			{
				serverRunning = false;

				stopServerButton.Enabled = false;
				backupButton.Enabled = false;

				setWeatherButton.Enabled = false;
				opPlayerButton.Enabled = false;
				deOpPlayerButton.Enabled = false;
				setGameruleButton.Enabled = false;

				trueGRRadioButton.Enabled = false;
				falseGRRadioButton2.Enabled = false;

				txtInputCommand.Enabled = false;
				btnExecute.Enabled = false;

				txtOutput.AppendText("\r\nTelling players that the server is going down in 10 seconds\n");
				File.AppendAllText(@"ServerLog.csv", "\r\n" + DateTime.Now.ToString() + " " + "Telling players that the server is going down in 10 seconds\r\n");
				mcInputStream.WriteLine("say Server closing in 10 seconds");
				Thread.Sleep(10000);

				playersOnlineUpdateTimer.Stop();
				mcInputStream.WriteLine("stop");
				playerOnlineOutput.Clear();
				gameRulesTxtOutput.Clear();

				startServerButton.Enabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		public void ProcessExited(object sender, EventArgs e)
		{
			txtOutput.AppendText("\r\n\r\nThe server has been shutdown.\r\n");
			File.AppendAllText(@"ServerLog.csv", "\r\n" + DateTime.Now.ToString() + " " + "The server has been shutdown.\r\n");
		}

		private void OnProcessExit(object sender, EventArgs e)
		{
			if (serverExecutableExists)
			{
				mcInputStream.WriteLine("stop");
				Thread.Sleep(5000);
			}
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (minecraftProcess != null)
				minecraftProcess.Close();
		}
		#endregion
		#endregion

		#region Timers
		private void autoBackupOnDateTimerInitialize()
		{
			TimeSpan alertTime = autoBackupOnDate_DateTimePicker.Value.TimeOfDay;
			DateTime current = DateTime.Now;
			TimeSpan timeToGo = alertTime - current.TimeOfDay;
			if (timeToGo < TimeSpan.Zero)
			{
				return; // time already passed
			}
			autoBackupOnDateTimer = new System.Threading.Timer(x =>
			{
				backgroundBackup.RunWorkerAsync();
			}, null, timeToGo, Timeout.InfiniteTimeSpan);
		}

		void playersOnlineUpdateTimer_Tick(object sender, EventArgs e)
		{
			if (!backupRunning && serverExecutableExists)
			{
				try
				{

					if (players != players2)
					{
						playerOnlineOutput.Clear();
						playerOnlineOutput.Text = players;
					}
					mcInputStream.WriteLine("list");
					players = players2;
				}
				catch
				{

				}
			}
		}

		#region Auto Backup Every X Timer
		private void autoBackupEveryXTimer_Tick(object sender, EventArgs e)
		{
			backupButton.PerformClick();
		}

		private int autoBackupEveryXTimerIntervalCalc(int timeDuration, string timeUnit)
		{
			switch (timeUnit)
			{
				case "minute":
					return timeDuration * 60000;
				case "hour":
					return timeDuration * 3600000;
			}
			return timeDuration;
		}
		#endregion
		#endregion

		#region Backup
		private void CopyFilesRecursively(string sourcePath, string targetPath)
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

		private void backgroundBackup_doWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			if (!serverRunning)
				return;

			backupRunning = true;

			// check if the configs are correct, cancel the backup if found any error
			if (!Directory.Exists(worldPath))
			{
				// txtOutput.AppendText($"\r\nWorld path incorrect, can't perform backup\r\n\r\n");
				return;
			}
			if (!Directory.Exists(backupPath))
			{
				// txtOutput.AppendText($"\r\nBackup path incorrect, can't perform backup\r\n\r\n");
				return;
			}
			if (backupLimit <= 0)
			{
				// txtOutput.AppendText($"\r\nNumber of backups to keep can't be smaller than 1, can't perform backup\r\n\r\n");
				return;
			}

			stopServerButton.Enabled = false;
			backupButton.Enabled = false;

			setWeatherButton.Enabled = false;
			opPlayerButton.Enabled = false;
			deOpPlayerButton.Enabled = false;
			setGameruleButton.Enabled = false;

			trueGRRadioButton.Enabled = false;
			falseGRRadioButton2.Enabled = false;

			worldPathChangeButton.Enabled = false;
			backupPathChangeButton.Enabled = false;
			backupLimitNumericUpDown.Enabled = false;

			autoBackupOnDate_DateTimePicker.Enabled = false;
			autoBackupEveryXDurationNumericUpDown.Enabled = false;
			autoBackupEveryXTimeUnitCombo.Enabled = false;

			txtInputCommand.Enabled = false;
			btnExecute.Enabled = false;

			mcInputStream.WriteLine("say Performing backup");
			txtOutput.AppendText("\r\nTelling players that the server is running a backup\n");
			File.AppendAllText(@"ServerLog.csv", "\r\n" + DateTime.Now.ToString() + " " + "Telling players that the server is running a backup\r\n");

			mcInputStream.WriteLine("save hold");
			Thread.Sleep(5000);

			txtOutput.AppendText("\r\nStarting Backup\n");
			File.AppendAllText(@"ServerLog.csv", DateTime.Now.ToString() + " " + "Starting Backup\r\n");

			// Remove oldest backups if the number of backups existing is over the limit (backupLimit)
			// Keep deleting oldest backups until the number of existing backups is smaller than backupLimit
			int currentNumberOfBackups = Directory.GetDirectories(backupPath).Length;
			while (currentNumberOfBackups >= backupLimit)
			{
				string[] backups = Directory.GetDirectories(backupPath);
				Directory.Delete(backups[0], true);
				txtOutput.AppendText($"\r\nBackup deleted: {backups[0]}\n");
				File.AppendAllText(@"ServerLog.csv", DateTime.Now.ToString() + " " + $"Backup deleted: {backups[0]}\r\n");
				currentNumberOfBackups = Directory.GetDirectories(backupPath).Length;
			}
			DateTime now = DateTime.Now;
			string newBackupName = $"{now.Day}_{now.Month}_{now.Year}-{now.Hour}_{now.Minute}_{now.Second}";
			CopyFilesRecursively(worldPath, backupPath + "\\" + newBackupName);
			Thread.Sleep(10000);

			mcInputStream.WriteLine("save resume");

			txtOutput.AppendText($"\r\nBackup saved: {backupPath + "\\" + newBackupName}\n");
			File.AppendAllText(@"ServerLog.csv", DateTime.Now.ToString() + " " + $"Backup saved: {backupPath + "\\" + newBackupName}");
			mcInputStream.WriteLine("say Backup complete");

			stopServerButton.Enabled = true;
			backupButton.Enabled = true;

			setWeatherButton.Enabled = true;
			opPlayerButton.Enabled = true;
			deOpPlayerButton.Enabled = true;
			setGameruleButton.Enabled = true;

			trueGRRadioButton.Enabled = true;
			falseGRRadioButton2.Enabled = true;

			worldPathChangeButton.Enabled = true;
			backupPathChangeButton.Enabled = true;
			backupLimitNumericUpDown.Enabled = true;

			autoBackupOnDate_DateTimePicker.Enabled = true;
			autoBackupEveryXDurationNumericUpDown.Enabled = true;
			autoBackupEveryXTimeUnitCombo.Enabled = true;

			txtInputCommand.Enabled = true;
			btnExecute.Enabled = true;

			backupRunning = false;
			#endregion
		}
	}
}