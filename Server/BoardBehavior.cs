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
        public List<Player> Players  = new List<Player>();

        public Hero Hero1;
        public Hero Hero2;

        public BoardBehavior(int w, int h)
        {
            Hero1 = new Hero("Hero1");
            Hero2 = new Hero("Hero2");
            _width = w;
            _height = h;
            _game = new Game(_width, _height);
            _gamePieces = new List<GamePiece>();
            _selectedPiece = new GamePiece(new Point(0, 0));

            var meleeCreature = new AbstractCreature
            {
                Type = CreatureType.Melee,
                Name = "Orc"
            };
            var rangeCreature = new AbstractCreature
            {
                Type = CreatureType.Range,
                Name = "Orchy"
            };

            Hero1.Creatures.Add(rangeCreature);
            Hero1.Creatures.Add(meleeCreature);

            Hero2.Creatures.Add(rangeCreature);
            Hero2.Creatures.Add(meleeCreature);
        }

        public void Initialize()
        {
            InstantiateCreature(Hero1.Creatures.Count, Hero2.Creatures.Count);
        }

        private void InstantiateCreature(int countRed, int countBlue)
        {
            int x = 0, y = _height - 1;
            
            for (var i = 0; i < countRed; i++)
            {
                FillTable(x, ref y, Team.Red, Hero1);
            }

            x = _width - 1;
            y = _height - 1;
            
            for (var i = 0; i < countBlue; i++)
            {
                FillTable(x, ref y, Team.Blue, Hero2);
            }

            _round = new Round(Hero1, Hero2);

            var heroes = new List<SerializableType> {Hero1};
            heroes.AddRange(Hero1.Creatures);
            heroes.Add(Hero2);
            heroes.AddRange(Hero2.Creatures);

            var response = new RemoteInvokeMethod("BoardBehavior", Command.SyncHero, heroes);
            var bytes = RemoteInvokeMethod.WriteToStream(response);
            foreach (var client in Players)
            {
                client.Sock.Send(bytes, bytes.Length, 0);
            }

            FinishAction();
        }   

        private void FillTable(int x, ref int y, Team team, Hero hero)
        {
            _game.BlockOutTiles(x, y);
            var piece = new GamePiece(new Point(x, y));
            _gamePieces.Add(piece);
            var creatureComponent = new Creature
            {
                Piece = piece,
                Index = _gamePieces.Count,
                Team = team
            };
            hero.InstanceCreatures.Add(creatureComponent);
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

            var responsePath = new List<SerializableType>();
            foreach (var tile in path)
            {
                Point spacial = new Point(tile.Location.X, tile.Location.Y);
                responsePath.Add(spacial);
            }

            var response = new RemoteInvokeMethod("BoardBehavior", Command.Move, responsePath);
            var bytes = RemoteInvokeMethod.WriteToStream(response);
            foreach (var client in Players)
            {
                client.Sock.Send(bytes, bytes.Length, 0);
            }
        }

        public void FinishAction()
        {
            var _currentCreature = _round.NextCreature();
            var turns = new List<SerializableType>();
            var turn = new NextTurn
            {
                Team = _currentCreature.Team.ToString(),
                CreaturePoint = new Point(_currentCreature.Piece.Location.X, _currentCreature.Piece.Location.Y)
            };
            turns.Add(turn);
            var response = new RemoteInvokeMethod("BoardBehavior", Command.ChangeTurn, turns);
            var bytes = RemoteInvokeMethod.WriteToStream(response);

            foreach (var client in Players.Where(x => x.Team == _currentCreature.Team))
            {
                client.Sock.Send(bytes, bytes.Length, 0);
            }
        }

        public void Defend(int index)
        {
            Hero1.InstanceCreatures[index].Armor += 10;
            FinishAction();
        }
    }
}
