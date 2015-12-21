using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DataAccess;
using Entities;
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
        public List<AbstractCreature> Creatures = new List<AbstractCreature>();

        private readonly List<AbstractHero> _selectedHeroes = new List<AbstractHero>();
        private readonly List<AbstractCreature> _selectedCreatures = new List<AbstractCreature>();

        public bool IsInitialize;
        public bool IsGameReady;
        private int _syncedPlayers;
        private Repository repository;
        private readonly CreaturesRepository creaturesRepository;
        private readonly HeroesRepository heroesRepository;
        private readonly IDataCollector _gameInformations = new MockDataCollector();


        public BoardBehavior(int width, int height)
        {
            _width = width;
            _height = height;
            _syncedPlayers = 0;
            _game = new Game(_width, _height);
            _gamePieces = new List<GamePiece>();
            _selectedPiece = new GamePiece(new Point(0, 0));
            repository = new Repository();
            creaturesRepository = repository.Creatures;
            heroesRepository = repository.Heroes;
        }

        public void Initialize()
        {
            IsInitialize = true;
            InstantiateHeroes(_selectedHeroes);
            InstantiateCreature(_selectedCreatures);
        }

        private void InstantiateHeroes(List<AbstractHero> heroes)
        {
            Hero1 = heroes.SingleOrDefault(x => x.HeroTeam == Team.Red);
            Hero2 = heroes.SingleOrDefault(x => x.HeroTeam == Team.Blue);
        }

        private void InstantiateCreature(IEnumerable<AbstractCreature> creatures)
        {
            var creatureGroup = creatures.GroupBy(g => g.Team).ToDictionary(h => h.Key, h => h.ToList());
            FillTable(0, _height - 1, Hero1, creatureGroup[Team.Red]);
            FillTable(_width - 1, _height - 1, Hero2, creatureGroup[Team.Blue]);
            _round = new Round(Hero1, Hero2);
        }

        public RemoteInvokeMethod GetHeroes()
        {
            if (!IsInitialize)
            {
                Initialize();
            }
            var board = _gameInformations.GetBoardInfo();
            IsGameReady = true;
            return new RemoteInvokeMethod("BoardBehaviorMultiplayer", Command.SyncHero, new List<SerializableType> { board, Hero1, Hero2 });
        }

        private void FillTable(int x, int y, AbstractHero hero, IEnumerable<AbstractCreature> creatures)
        {
            foreach (var creatureComponent in creatures)
            {
                var piece = new GamePiece(new Point(x, y));
                _gamePieces.Add(piece);
                _game.BlockOutTiles(x, y);
                creatureComponent.Count = 5;
                creatureComponent.Piece = new Point(x, y);
                creatureComponent.Index = _gamePieces.Count;
                hero.Creatures.Add(creatureComponent);
                Creatures.Add(creatureComponent);
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

        private void Die(Point location)
        {
            var tile = _game.GameBoard[location.X, location.Y];
            var creature1 = Hero1.Creatures.SingleOrDefault(x => x.Piece.X == tile.Location.X && x.Piece.Y == tile.Location.Y);
            var creature2 = Hero2.Creatures.SingleOrDefault(x => x.Piece.X == tile.Location.X && x.Piece.Y == tile.Location.Y);

            tile.CanPass = true;
            tile.CanSelect = true;

            if (creature1 == null)
            {
                creature2.Status = CreatureStatus.Death;
                return;
            }
            creature1.Status = CreatureStatus.Death;
        }

        public void Move(Point location, Point pointStart, Point pointDestination)
        {
            _syncedPlayers = 0;
            _selectedPiece.Location = location;
            var start = _game.AllTiles.Single(o => o.X == pointStart.X && o.Y == pointStart.Y);
            var destination = _game.AllTiles.Single(o => o.X == pointDestination.X && o.Y == pointDestination.Y);
            var path = OnGameStateChanged(location, start, destination);
            var responsePath = path.Select(tile => new Point(tile.Location.X, tile.Location.Y)).Cast<SerializableType>().ToList();
            NetworkActions.SendMessageToClients(Players, Command.Move, responsePath);
        }

        public void FinishAction()
        {
            _syncedPlayers++;
            if (_syncedPlayers != 2) return;
            _syncedPlayers = 0;
            var currentCreature = _round.NextCreature();
            var turns = new List<SerializableType>
            {
                new NextTurn
                {
                    Team = currentCreature.Team.ToString(),
                    CreatureIndex = currentCreature.Index
                }
            };
            NetworkActions.SendMessageToClients(Players, Command.FinishAction, turns);
        }

        public void Attack(AttackModel model)
        {
            var sender = Creatures.SingleOrDefault(x => x.Index == model.SenderCreatureIndex);
            var damage = CalculateDamage(sender);
            var attackModel = new List<SerializableType>()
            {
                new AttackModel
                {
                    TargetCreatureIndex = model.TargetCreatureIndex,
                    SenderCreatureIndex = model.SenderCreatureIndex,
                    Damage = damage
                }
            };
            NetworkActions.SendMessageToClients(Players, Command.Attack, attackModel);
        }

        public void Defend(int index)
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
            NetworkActions.SendMessageToClients(Players, Command.FinishAction, turns);
        }

        private double CalculateDamage(AbstractCreature creature)
        {
            if (creature.Type == CreatureType.Melee)
            {
                return creature.Damage * creature.Count;
            }
            return creature.Damage * (creature.Count / 2);
        }

        public void SelectUnits(Units units)
        {
            var hero = heroesRepository.GetHeroWithName(units.HeroName);
            var abstractHero = Mapper.Map<Hero, AbstractHero>(hero);
            abstractHero.HeroTeam = units.Team;
            _selectedHeroes.Add(abstractHero);

            var creatureNameGroup = new List<string>
            {
                units.Creature1,
                units.Creature2,
                units.Creature3,
                units.Creature4
            };

            var creatures = creaturesRepository.GetCreaturesByName(creatureNameGroup);
            foreach (var creatureName in creatureNameGroup)
            {
                var creatureType = creatures[creatureName];
                var abstractCreature = new AbstractCreature()
                {
                    Damage = creatureType.Damage,
                    Luck = creatureType.Luck,
                    Name = creatureType.Name,
                    Range =  creatureType.Range ?? 0.0,
                    MaxHealth = creatureType.MaxHealth,
                    Speed = creatureType.Speed,
                    Armor = creatureType.Armor,
                    Type = (CreatureType)(creatureType.CombatModeId - 1),
                    Team = units.Team,
                    Status = CreatureStatus.Alive
                };
                _selectedCreatures.Add(abstractCreature);
            }
        }
    }

}
