using System;
using System.Collections.Generic;
using System.Linq;
using NetworkTypes;
using PathFinding;

namespace Server
{
    public class BoardBehavior
    {
        private Round _round;
        private readonly Game _game;
        private readonly GamePiece _selectedPiece;
        private readonly int _width;
        private readonly int _height;
        private readonly List<GamePiece> _gamePieces;
        
        public AbstractHero Hero1;
        public AbstractHero Hero2;
        public List<Player> Players = new List<Player>();

        private readonly IDataCollector _gameInformations = new MockDataCollector();

        public BoardBehavior(int w, int h)
        {
            _width = w;
            _height = h;
            _game = new Game(_width, _height);
            _gamePieces = new List<GamePiece>();
            _selectedPiece = new GamePiece(new Point(0, 0));
        }

        public void Initialize()
        {
            InstantiateHeroes(_gameInformations.GetHeroes());
            InstantiateCreature(_gameInformations.GetCreatures());
        }

        private void InstantiateHeroes(List<AbstractHero> heroes)
        {
            Hero1 = heroes.First();
            Hero2 = heroes.Last();
        }

        private void InstantiateCreature(IEnumerable<AbstractCreature> creatures)
        {
            int x = 0, y = _height - 1;
            var creatureGroup = creatures.GroupBy(g => g.Team).ToDictionary(h => h.Key, h => h.ToList());

            foreach (var creature in creatureGroup[Team.Red])
            {
                FillTable(x, ref y, Hero1, creature);
            }

            x = _width - 1;
            y = _height - 1;

            foreach (var creature in creatureGroup[Team.Blue])
            {
                FillTable(x, ref y, Hero2, creature);
            }

            _round = new Round(Hero1, Hero2);
            var board = new BoardInfo
            {
                Height = 7,
                Width = 12,
                HexWidth = 4f,
                Spacing = 3.46f
            };

            var heroes = new List<SerializableType> {board, Hero1, Hero2 };
            var response = new RemoteInvokeMethod("BoardBehaviorMultiplayer", Command.SyncHero, heroes);
            var bytes = RemoteInvokeMethod.WriteToStream(response);
            foreach (var client in Players)
            {
                client.Sock.Send(bytes, bytes.Length, 0);
            }
        }

        private void FillTable(int x, ref int y, AbstractHero hero, AbstractCreature creatureComponent)
        {
            _game.BlockOutTiles(x, y);
            var piece = new GamePiece(new Point(x, y));
            _gamePieces.Add(piece);
            creatureComponent.Piece = piece.Location;
            creatureComponent.Index = _gamePieces.Count;
            hero.Creatures.Add(creatureComponent);
            y -= 2;
        }

        private IEnumerable<Tile> OnGameStateChanged(Point location, Tile start, Tile destination)
        {
            if (_selectedPiece == null) return new List<Tile>();
            var dp = _selectedPiece;
            Func<Tile, Tile, double> distance = (node1, node2) => 1;
            Func<Tile, double> estimate = t => Math.Sqrt(Math.Pow(t.X - destination.X, 2) + Math.Pow(t.Y - destination.Y, 2));
            _game.GameBoard[location.X, location.Y].CanPass = true;
            _game.GameBoard[dp.X, dp.Y].CanPass = true;
            var path = PathFind.FindPath(start, destination, distance, estimate).ToList();
            return path;
        }

        private void DieCreature(Point location)
        {
            var tile = _game.GameBoard[location.X, location.Y];
            tile.CanPass = true;
            tile.CanSelect = true;
        }

        public void Move(Point location, Point pointStart, Point pointDestination)
        {
            _selectedPiece.Location = location;

            var start = _game.AllTiles.Single(o => o.X == pointStart.X && o.Y == pointStart.Y);
            var destination = _game.AllTiles.Single(o => o.X == pointDestination.X && o.Y == pointDestination.Y);
            var path = OnGameStateChanged(location, start, destination);
            var responsePath = path.Select(tile => new Point(tile.Location.X, tile.Location.Y)).Cast<SerializableType>().ToList();
            var response = new RemoteInvokeMethod("BoardBehavior", Command.Move, responsePath);
            var bytes = RemoteInvokeMethod.WriteToStream(response);
            foreach (var client in Players)
            {
                client.Sock.Send(bytes, bytes.Length, 0);
            }
        }

        public void FinishAction()
        {
            var currentCreature = _round.NextCreature();
            var turns = new List<SerializableType>
            {
                new NextTurn
                {
                    Team = currentCreature.Team.ToString(),
                    CreatureIndex = currentCreature.Index
                }
            };

            var response = new RemoteInvokeMethod("BoardBehavior", Command.ChangeTurn, turns);
            var bytes = RemoteInvokeMethod.WriteToStream(response);
            foreach (var client in Players)
            {
                client.Sock.Send(bytes, bytes.Length, 0);
            }
        }

        public void Defend(int index)
        {
            FinishAction();
        }
    }


    public interface IDataCollector
    {
        List<AbstractHero> GetHeroes();
        List<AbstractCreature> GetCreatures();
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
    }
}
