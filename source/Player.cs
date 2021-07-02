namespace BedrockServer2000
{
	public struct Player
	{
		public string Name { get; private set; }
		public string Xuid { get; private set; }

		public Player(string name, string xuid)
		{
			Name = name;
			Xuid = xuid;
		}
	}
}