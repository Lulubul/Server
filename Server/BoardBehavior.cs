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
        public bool IsInitialize;

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
            IsInitialize = true;
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
            var y = _height - 1;
            var creatureGroup = creatures.GroupBy(g => g.Team).ToDictionary(h => h.Key, h => h.ToList());
            FillTable(0, y, Hero1, creatureGroup[Team.Red]);
            FillTable(_width - 1, _height - 1, Hero2, creatureGroup[Team.Blue]);
            _round = new Round(Hero1, Hero2);
        }

        public void SendHeroesToClient()
        {
            var board = _gameInformations.GetBoardInfo();
            NetworkActions.SendMessageToClients(Players, Command.SyncHero, new List<SerializableType> { board, Hero1, Hero2 });
        }

        private void FillTable(int x, int y, AbstractHero hero, IEnumerable<AbstractCreature> creatures)
        {
            foreach (var creatureComponent in creatures)
            {
                var piece = new GamePiece(new Point(x, y));
                _gamePieces.Add(piece);
                _game.BlockOutTiles(x, y);
                creatureComponent.Piece = piece.Location;
                creatureComponent.Index = _gamePieces.Count;
                hero.Creatures.Add(creatureComponent);
                y -= 2;
            }
        }

        private IEnumerable<Tile> OnGameStateChanged(Point location, Tile start, Tile destination)
        {
            if (_selectedPiece == null) return new List<Tile>();
            Func<Tile, Tile, double> distance = (node1, node2) => 1;
            Func<Tile, double> estimate = t => Math.Sqrt(Math.Pow(t.X - destination.X, 2) + Math.Pow(t.Y - destination.Y, 2));
            _game.GameBoard[location.X, location.Y].CanPass = true;
            _game.GameBoard[_selectedPiece.X, _selectedPiece.Y].CanPass = true;
            return PathFind.FindPath(start, destination, distance, estimate).ToList();
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
            NetworkActions.SendMessageToClients(Players, Command.Move, responsePath);
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
            NetworkActions.SendMessageToClients(Players, Command.ChangeTurn, turns);
        }

        public void Defend(int index)
        {
            FinishAction();
        }
    }

}
