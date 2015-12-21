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
                    RaceEnum = RaceEnum.Human,
                    HeroTeam = Team.Blue,
                    Name = "Orrin"
                },
                new AbstractHero
                {
                    Type = HeroType.Might,
                    RaceEnum = RaceEnum.Orc,
                    HeroTeam = Team.Red,
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
                    Health = 20,
                    Range = 20,
                    Count = 5,
                    Name = "Aurel2",
                    Armor = 10,
                    Team = Team.Red,
                    Type = CreatureType.Melee
                },
                new AbstractCreature
                {
                    Health = 20,
                    Range = 20,
                     Count = 5,
                    Name = "Aurel",
                    Armor = 10,
                    Team = Team.Red,
                    Type = CreatureType.Melee
                },
                new AbstractCreature
                {
                    Health = 20,
                    Range = 20,
                    Count = 5,
                    Name = "Aurel",
                    Armor = 10,
                    Team = Team.Red,
                    Type = CreatureType.Melee
                },
                new AbstractCreature
                {
                    Health = 20,
                    Range = 20,
                    Count = 5,
                    Name = "Aurel",
                    Armor = 10,
                    Team = Team.Blue,
                    Type = CreatureType.Range
                },
                new AbstractCreature
                {
                    Health = 20,
                    Range = 20,
                    Count = 5,
                    Name = "Aurel",
                    Armor = 10,
                    Team = Team.Blue,
                    Type = CreatureType.Range
                },
                new AbstractCreature
                {
                    Health = 20,
                    Range = 20,
                    Count = 5,
                    Name = "Aurel",
                    Armor = 10,
                    Team = Team.Blue,
                    Type = CreatureType.Range
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
