using System.Collections.Generic;
using NetworkTypes;

namespace Server
{

    public interface IDataCollector
    {
        List<AbstractHero> GetHeroes();
        List<AbstractCreature> GetCreatures();
        BoardInfo GetBoardInfo();
    }

    public class MockDataCollector : IDataCollector
    {
        public List<AbstractHero> GetHeroes()
        {
            return new List<AbstractHero>()
            {
                new AbstractHero
                {
                    Type = HeroType.Magic,
                    Race = HeroRace.Human,
                    Name = "Orrin"
                },
                new AbstractHero
                {
                    Type = HeroType.Might,
                    Race = HeroRace.Orc,
                    Name = "Sir Christian"
                }
            };
        }

        public List<AbstractCreature> GetCreatures()
        {
            return new List<AbstractCreature>()
            {
                new AbstractCreature
                {
                    Name = HeroRace.Orc.ToString(),
                    Type = CreatureType.Melee,
                    Team = Team.Red
                },
                new AbstractCreature
                {
                    Name = HeroRace.Orc.ToString(),
                    Type = CreatureType.Melee,
                    Team = Team.Red
                },
                new AbstractCreature
                {
                    Name = HeroRace.Orc.ToString(),
                    Type = CreatureType.Melee,
                    Team = Team.Red
                },
                new AbstractCreature
                {
                    Name = HeroRace.Orc.ToString(),
                    Type = CreatureType.Melee,
                    Team = Team.Red
                },
                new AbstractCreature
                {
                    Name = HeroRace.Orc.ToString(),
                    Type = CreatureType.Melee,
                    Team = Team.Blue
                },
                new AbstractCreature
                {
                    Name = HeroRace.Orc.ToString(),
                    Type = CreatureType.Range,
                    Team = Team.Blue
                },
                new AbstractCreature
                {
                    Name = HeroRace.Orc.ToString(),
                    Type = CreatureType.Melee,
                    Team = Team.Blue
                },
                new AbstractCreature
                {
                    Name = HeroRace.Orc.ToString(),
                    Type = CreatureType.Range,
                    Team = Team.Blue
                }
            };
        }

        public BoardInfo GetBoardInfo()
        {
            return new BoardInfo
            {
                Height = 7,
                Width = 12,
                HexWidth = 4f,
                Spacing = 3.46f
            };
        }
    }
}
