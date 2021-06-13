using System;
using System.IO;
using System.Collections.Generic;

namespace BedrockServer2000
{
	public class Configs
	{
		public static int KeyCount()
		{
			if (!File.Exists("BedrockServer2000.scf")) return 0;

			string[] rawLines = File.ReadAllLines("BedrockServer2000.scf");
			List<string> configLInes = new List<string>();
			foreach (string line in rawLines)
				if (!line.StartsWith("#") && line != "") configLInes.Add(line);

			return configLInes.Count;
		}

		public static string GetValue(string key)
		{
			if (!File.Exists("BedrockServer2000.scf")) return "";

			string[] rawLines = File.ReadAllLines("BedrockServer2000.scf");
			List<string> configLines = new List<string>();
			foreach (string line in rawLines)
				if (!line.StartsWith("#") && line != "") configLines.Add(line);

			foreach (string line in configLines)
			{
				string _key = line.Split('=', StringSplitOptions.RemoveEmptyEntries)[0];
				string value;
				if (line.Split('=', StringSplitOptions.RemoveEmptyEntries).Length == 1) value = "";
				else value = line.Split('=', StringSplitOptions.RemoveEmptyEntries)[1];

				if (_key == key) return value;
			}
			return "";
		}

		public static void SetValue(string key, string value)
		{
			if (!File.Exists("BedrockServer2000.scf")) throw new FileNotFoundException();

			string[] rawLines = File.ReadAllLines("BedrockServer2000.scf");

			int lineIndexToChange = 0;

			for (int i = 0; i < rawLines.Length; i += 1)
			{
				if (!rawLines[i].StartsWith("#") && rawLines[i] != "")
				{
					string _key = rawLines[i].Split('=', StringSplitOptions.RemoveEmptyEntries)[0];
					if (_key.ToLower() == key.ToLower())
					{
						lineIndexToChange = i + 1;
						break;
					}
				}
			}

			if (lineIndexToChange == 0) throw new KeyNotFoundException();
			else
			{
				rawLines[lineIndexToChange - 1] = $"{rawLines[lineIndexToChange - 1].Split('=', StringSplitOptions.RemoveEmptyEntries)[0]}={value}";
				File.WriteAllLines("BedrockServer2000.scf", rawLines);
			}
		}
	}
}