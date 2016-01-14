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
        public bool IsInitialize;
        public bool IsGameReady;
        public NetworkActions NetworkHandler;

        private readonly List<AbstractHero> _selectedHeroes = new List<AbstractHero>();
        private readonly List<AbstractCreature> _selectedCreatures = new List<AbstractCreature>();
        private int _syncedPlayers;
        private readonly CreaturesRepository _creaturesRepository;
        private readonly HeroesRepository _heroesRepository;
        private readonly IDataCollector _gameInformations = new DataCollector();
        private bool _gameIsOver;
        private Team _winner;
        private string _boardName;

        public BoardBehavior(int width, int height, string boardName)
        {
            _width = width;
            _height = height;
            _boardName = boardName;
            _syncedPlayers = 0;
            _game = new Game(_width, _height);
            _gamePieces = new List<GamePiece>();
            _selectedPiece = new GamePiece(new Point(0, 0));
            var repository = new Repository();
            _creaturesRepository = repository.Creatures;
            _heroesRepository = repository.Heroes;
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
            var path = PathFind.FindPath(start, destination, distance, estimate).ToList();
            _game.GameBoard[destination.X, destination.Y].CanPass = false;
            return path;
        }

        #region Creature Action
        public void FinishAction()
        {
            _syncedPlayers++;
            if (_syncedPlayers != 2) return;

            if (_gameIsOver)
            {
                var winner = new List<SerializableType>
                {
                    new NextTurn
                    {
                        Team = _winner.ToString(),
                        CreatureIndex = 1
                    }
                };

                foreach (var player in Players)
                {
                    player.State = State.Lobby;
                    player.Lobby = "";
                    player.Slot = -1;
                }
                NetworkHandler.RemoveLobby(_boardName);
                NetworkActions.SendMessageToClients(Players, Command.EndGame, winner);
                return;
            }

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
            _syncedPlayers = 0;
            var sender = Creatures.SingleOrDefault(x => x.Index == model.SenderCreatureIndex);
            var target = Creatures.SingleOrDefault(x => x.Index == model.TargetCreatureIndex);
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
            var totalHealth = (target.Count - 1) * target.MaxHealth + target.Health - damage;
            target.Health = totalHealth <= 0 ? 0 : totalHealth % (target.MaxHealth + 1);
            target.Count = totalHealth <= 0 ? 0 : (int)totalHealth / target.MaxHealth + 1;
            NetworkActions.SendMessageToClients(Players, Command.Attack, attackModel);
        }

        public void Defend(int index)
        {
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

        public void Die(Point point)
        {
            _syncedPlayers = 0;
            var creature = Creatures.SingleOrDefault(x => x.Piece.X == point.X && x.Piece.Y == point.Y && x.Status == CreatureStatus.Alive);
            if (creature == null || creature.Status == CreatureStatus.Death)
            {
                return;
            }
            var tile = _game.GameBoard[creature.Piece.X, creature.Piece.Y];
            tile.CanPass = true;
            tile.CanSelect = true;
            creature.Status = CreatureStatus.Death;

            if (Creatures.Where(x => x.Team == creature.Team).All(x => x.Status == CreatureStatus.Death))
            {
                _gameIsOver = true;
                _winner = creature.Team;
            }

            NetworkActions.SendMessageToClients(Players, Command.Die, new List<SerializableType>() { point });
        }

        public void Move(Point pointStart, Point pointDestination)
        {
            _syncedPlayers = 0;
            _selectedPiece.Location = pointStart;
            var start = _game.AllTiles.Single(o => o.X == pointStart.X && o.Y == pointStart.Y);
            var destination = _game.AllTiles.Single(o => o.X == pointDestination.X && o.Y == pointDestination.Y);

            var creature = Creatures.SingleOrDefault(x => x.Piece.X == pointStart.X && x.Piece.Y == pointStart.Y && x.Status == CreatureStatus.Alive);
            creature.Piece = pointDestination;

            var path = OnGameStateChanged(pointStart, start, destination);
            var responsePath = path.Select(tile => new Point(tile.Location.X, tile.Location.Y))
                                .Cast<SerializableType>()
                                .ToList();
            NetworkActions.SendMessageToClients(Players, Command.Move, responsePath);
        }
        #endregion

        private static double CalculateDamage(AbstractCreature creature)
        {
            if (creature.Type == CreatureType.Melee)
            {
                return creature.Damage * creature.Count * 3;
            }
            return creature.Damage * creature.Count * 2;
        }

        public void SelectUnits(Units units)
        {
            var hero = _heroesRepository.GetHeroWithName(units.HeroName);
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

            var creatures = _creaturesRepository.GetCreaturesByName(creatureNameGroup);
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
