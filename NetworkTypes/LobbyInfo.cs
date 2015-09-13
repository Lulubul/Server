namespace NetworkTypes
{
    public sealed class LobbyInfo : SerializableType
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }
        public string GameType { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }

        public LobbyInfo()
        {

        }

        public LobbyInfo(int playerId, string name, string gameType, int maxPlayers, int currentPlayers)
        {
            PlayerId = playerId;
            Name = name;
            GameType = gameType;
            MaxPlayers = maxPlayers;
            CurrentPlayers = currentPlayers;
        }


    }
}
