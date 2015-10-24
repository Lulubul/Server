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
            InstantiateHeroes(HeroType.Magic, HeroRace.Human, HeroType.Might, HeroRace.Orc);
            InstantiateCreature(2, 4);
        }

        private void InstantiateHeroes(HeroType heroType1, HeroRace race1, HeroType heroType2, HeroRace race2)
        {
            Hero1 = new AbstractHero(heroType1, race1, "aurica");
            Hero2 = new AbstractHero(heroType2, race2, "silviu");
        }

        private void InstantiateCreature(int countRed, int countBlue)
        {
            int x = 0, y = _height - 1;
            var creatureTypesHero1 = new List<CreatureType>
            {
                CreatureType.Melee,
                CreatureType.Melee
            };
            for (var i = 0; i < countRed; i++)
            {
                FillTable(x, ref y, Team.Red, Hero1, creatureTypesHero1[i]);
            }

            x = _width - 1;
            y = _height - 1;
            var creatureTypes = new List<CreatureType>
            {
                CreatureType.Melee,
                CreatureType.Range,
                CreatureType.Melee,
                CreatureType.Melee
            };
            for (var i = 0; i < countBlue; i++)
            {
                FillTable(x, ref y, Team.Blue, Hero2, creatureTypes[i]);
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

            //FinishAction();
        }

        private void FillTable(int x, ref int y, Team team, AbstractHero hero, CreatureType creatureType)
        {
            _game.BlockOutTiles(x, y);
            var piece = new GamePiece(new Point(x, y));
            _gamePieces.Add(piece);

            var creatureComponent = new AbstractCreature()
            {
                Type = creatureType,
                Name = "Orc",
                Piece = piece.Location,
                Index = _gamePieces.Count,
                Team = team
            };

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
            var turns = new List<SerializableType>();
            var turn = new NextTurn
            {
                Team = currentCreature.Team.ToString(),
                CreaturePoint = new Point(currentCreature.Piece.X, currentCreature.Piece.Y)
            };
            turns.Add(turn);
            var response = new RemoteInvokeMethod("BoardBehavior", Command.ChangeTurn, turns);
            var bytes = RemoteInvokeMethod.WriteToStream(response);

            foreach (var client in Players.Where(x => x.Team == currentCreature.Team))
            {
                client.Sock.Send(bytes, bytes.Length, 0);
            }
        }

        public void Defend(int index)
        {
            //Hero1.Creatures[index].Armor += 10;
            FinishAction();
        }
    }
}
