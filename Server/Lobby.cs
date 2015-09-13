using System.Collections.Generic;
using NetworkTypes;

namespace Server
{
    public class Lobby 
    {
        public List<Player> Players = new List<Player>();
        public string Name;
        public string GameType;
        public int Id, CreatorId, MaxPlayers;

        public Lobby(string name, int creatorId, int maxPlayers, string gameType)
        {
            Name = name;
            CreatorId = creatorId;
            MaxPlayers = maxPlayers;
            GameType = gameType;
        }

        public int EmptySlot()
        {
            var slot = -1;
            for (var i = 0; i < MaxPlayers; i++ )
            {
                var index = Players.FindIndex(x => x.Slot == i);
                if (index >= 0) continue;
                slot = i;
                break;
            }
            return slot;
        }

        public int TeamSlot(Team team)
        {
            var slot = 0;
            for (var i = 0; i < MaxPlayers; i++)
            {
                var index = Players.FindIndex(x => x.Slot == i && x.Team == team);
                if (index >= 0) continue;
                slot = i;
                break;
            }
            return slot;
        }
    }

}
