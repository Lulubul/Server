using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DataAccess;
using Entities;
using NetworkTypes;
using Type = System.Type;

namespace Server
{
    public class NetworkActions : INetworkActions
    {
        public Dictionary<string, Lobby> Lobbies;
        public Dictionary<Player, string> History;
        public List<Player> Players;
        public Dictionary<string, BoardBehavior> Games;

        private readonly Type _type;
        private readonly Repository _repository;
        private readonly UserRepository _userRepository;
        private readonly HeroesRepository _heroRepository;
        private readonly CreaturesRepository _creaturesRepository;
        private const int BoardSizeWidth = 12;
        private const int BoardSizeHeight = 7;

        private readonly List<SerializableType> _heroes;
        private readonly List<SerializableType> _creatures;

        public NetworkActions()
        {
            Lobbies = new Dictionary<string, Lobby>();
            History = new Dictionary<Player, string>();
            Players = new List<Player>();
            Games = new Dictionary<string, BoardBehavior>();
            _repository = new Repository();
            _userRepository = _repository.Users;
            _heroRepository = _repository.Heroes;
            _creaturesRepository = _repository.Creatures;
            _type = GetType();

            _heroes = Mapper.Map<List<Hero>, List<HeroInfo>>(_heroRepository.GetHeroes())
                            .Cast<SerializableType>()
                            .ToList();
            _creatures = Mapper.Map<List<Creature>, List<CreatureInfo>>(_creaturesRepository.GetCreatures())
                               .Cast<SerializableType>()
                               .ToList();
        }

        public void Execute(Command command, string serviceClassName, List<SerializableType> parameters, Player player)
        {
            try
            {
                var objectOfInvokation = GetObjectOfInvokation(serviceClassName, player);
                var theMethod = objectOfInvokation.GetType().GetMethod(command.ToString());
                if (theMethod == null)
                {
                    return;
                }
                if (theMethod.ReturnType == typeof (void))
                {
                    theMethod.Invoke(objectOfInvokation, parameters.ToArray());
                    return;
                }

                var remoteInvokeMethod = theMethod.Invoke(objectOfInvokation, parameters.ToArray()) as RemoteInvokeMethod;
                if (remoteInvokeMethod != null)
                {
                    remoteInvokeMethod.MethodName = remoteInvokeMethod.MethodName ?? command.ToString();
                    var bytes = RemoteInvokeMethod.WriteToStream(remoteInvokeMethod);
                    player.Sock.Send(bytes, bytes.Length, 0);
                    AddNewPlayer(player, remoteInvokeMethod);
                }

                if (command != Command.InitializeBoard) return;
                player.State = State.Play;
                var gameName = ((SimpleMessage)parameters[0]).Message;
                if (IsGameReady(gameName))
                {
                    SendGameIsReadyCallback(gameName);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private object GetObjectOfInvokation(string serviceClassName, Player player)
        {
            if (serviceClassName == "BoardBehavior" && player.Lobby != null && Games.ContainsKey(player.Lobby))
            {
                return Games[player.Lobby];
            }
            return this;
        }

        private void AddNewPlayer(Player player, RemoteInvokeMethod remoteInvokeMethod)
        {
            if (Players.Any(p => p.Id == player.Id)) return;
            var user = remoteInvokeMethod.Parameters.FirstOrDefault() as Gambler;
            player.Id = user.Id;
            player.Name = user.Name;
            player.State = State.Lobby;
            Players.Add(player);
        }

        public static RemoteInvokeMethod Connect()
        {
            return new RemoteInvokeMethod(new List<SerializableType>()
            {
                new SimpleMessage { Message = "Welcome" }
            });
        }

        public RemoteInvokeMethod Login(Authentication authentification)
        {
            var name = authentification.Name;
            var pass = authentification.Pass;
            var user = _userRepository.Get(name, pass);
            var args = new List<SerializableType>();
            var gambler = new Gambler();
            args.Add(gambler);
            if (user == null)
            {
                gambler.Response = Response.Fail.ToString();
                return new RemoteInvokeMethod(args);
            }
            gambler.Name = user.Username;
            gambler.Id = user.Id;
            gambler.Response = Response.Succed.ToString();
            args.AddRange(Lobbies.Values.Select(lobby => new LobbyInfo()
            {
                GameType = lobby.GameType,
                CurrentPlayers = lobby.Players.Count,
                MaxPlayers = lobby.MaxPlayers,
                Name = lobby.Name,
                PlayerId = lobby.CreatorId
            }));
            return new RemoteInvokeMethod(args);
        }

        public RemoteInvokeMethod Logout(Authentication authentification)
        {
            return new RemoteInvokeMethod(new List<SerializableType>()
            {
                new ResponseMessage { Response = Response.Succed.ToString() }
            });
        }

        public RemoteInvokeMethod Register(Authentication authentification)
        {
            var username = authentification.Name;
            var password = authentification.Pass;
            var id = _userRepository.Add(username, password).Id;
            var args = new List<SerializableType>();
            var gambler = new Gambler
            {
                Name = username,
                Id = id,
                Response = Response.Succed.ToString()
            };
            args.Add(gambler);
            args.AddRange(Lobbies.Values.Cast<SerializableType>());
            return new RemoteInvokeMethod(args);
        }

        public RemoteInvokeMethod Create(LobbyInfo room)
        {
            var creatorId = room.PlayerId;
            var lobbyName = room.Name;
            var maxPlayer = room.MaxPlayers;
            var args = new List<SerializableType>();
            if (!Lobbies.ContainsKey(lobbyName))
            {
                var player = Players.SingleOrDefault(x => x.Id == creatorId);
                var newLobby = new Lobby(lobbyName, creatorId, maxPlayer, room.GameType);
                player.Team = Team.Red;
                player.Lobby = lobbyName;
                player.State = State.Join;
                player.Slot = 0;
                newLobby.Players.Add(player);
                Lobbies.Add(lobbyName, newLobby);

                var board = new BoardBehavior(BoardSizeWidth, BoardSizeHeight);
                Games.Add(lobbyName, board);

                var message = new ResponseMessage
                {
                    Response = Response.Succed.ToString(),
                    Message = room.Name
                };
                SendUnits(player);
                args.Add(message);
                var lobby = new List<SerializableType> { room };
                var response = new RemoteInvokeMethod(Command.SyncRooms, lobby);
                var bytes = RemoteInvokeMethod.WriteToStream(response);
                var clients = Players.Where(client => client.State == State.Lobby);
                foreach (var client in clients)
                {
                    client.Sock.Send(bytes, bytes.Length, 0);
                }
                return new RemoteInvokeMethod(args);
            }
            args.Add(new ResponseMessage
            {
                Response = Response.Fail.ToString(),
                Message = "Room name already exist."
            });
            return new RemoteInvokeMethod(args);
        }

        public RemoteInvokeMethod Join(LobbyInfo lobby)
        {
            var playerId = lobby.PlayerId;
            var roomName = lobby.Name;
            var index = Players.FindIndex(x => x.Id == playerId);
            var emptySlot = Lobbies[roomName].EmptySlot();
            Players[index].Lobby = lobby.Name;
            var gambler = new Gambler()
            {
                Id = playerId,
                Name = Players[index].Name,
                Slot = emptySlot
            };

            if (emptySlot >= 0)
            {
                gambler.Response = Response.Succed.ToString();
            }
            else
            {
                gambler.Response = Response.Fail.ToString();
                var user = new List<SerializableType> { gambler };
                return new RemoteInvokeMethod(user);
            }

            SendSyncLobby(roomName, gambler);
            SendUnits(Players[index]);

            var args = new List<SerializableType>();
            var lobbyInfo = new LobbyInfo(Lobbies[roomName].CreatorId, Lobbies[roomName].Name, Lobbies[roomName].GameType, Lobbies[roomName].MaxPlayers, Lobbies[roomName].Players.Count);
            args.Add(lobbyInfo);

            args.AddRange(Lobbies[roomName].Players.Select(player => new Gambler
            {
                Name = player.Name,
                Id = player.Id,
                Slot = player.Slot,
                Response = Response.Succed.ToString()
            }));

            Lobbies[lobby.Name].Players.Add(Players[index]);
            Players[index].Slot = gambler.Slot;
            Players[index].Team = Players[index].Slot % 2 == 0 ? Team.Red : Team.Blue;

            Players[index].State = State.Join;
            args.Add(gambler);
            return new RemoteInvokeMethod(args);
        }

        public RemoteInvokeMethod ChangeTeam(LobbyInfo lobby)
        {
            var playerId = lobby.PlayerId;
            var roomName = lobby.Name;
            var index = Players.FindIndex(x => x.Id == playerId);
            var player = Players[index];
            player.Team = player.Team == Team.Blue ? Team.Red : Team.Blue;
            player.Slot = Lobbies[lobby.Name].TeamSlot(player.Team);

            var args = Lobbies[roomName].Players.Select(user => new Gambler
            {
                Name = user.Name,
                Id = user.Id,
                Slot = user.Slot,
                Response = Response.Succed.ToString()
            }).Cast<SerializableType>().ToList();

            var response = new RemoteInvokeMethod(Command.SyncLobby, args);
            var bytes = RemoteInvokeMethod.WriteToStream(response);
            var clients = Players.Where(client => client.Lobby == roomName);
            foreach (var client in clients)
            {
                client.Sock.Send(bytes, bytes.Length, 0);
            }
            return new RemoteInvokeMethod(args);
        }

        public RemoteInvokeMethod Leave(LobbyInfo lobby)
        {
            var playerId = lobby.PlayerId;
            var player = Players.Find(x => x.Id == playerId);
            player.State = State.Lobby;
            Lobbies[lobby.Name].Players.Remove(player);
            if (Lobbies[lobby.Name].Players.Count < 1)
            {
                Lobbies.Remove(lobby.Name);
                var remainsLobbies = Lobbies.Values.Select(l => new LobbyInfo()
                {
                    PlayerId = l.CreatorId,
                    Name = l.Name,
                    GameType = l.GameType,
                    MaxPlayers = l.MaxPlayers,
                    CurrentPlayers = 2
                }).Cast<SerializableType>().ToList();
                return new RemoteInvokeMethod(Command.SyncRooms, remainsLobbies);
            }

            var args = Lobbies[lobby.Name].Players.Select(user => new Gambler
            {
                Name = user.Name,
                Id = user.Id,
                Slot = user.Slot,
                Response = Response.Succed.ToString()
            }).Cast<SerializableType>().ToList();

            //Sync Lobby
            var response = new RemoteInvokeMethod(Command.SyncLobby, args);
            var bytes = RemoteInvokeMethod.WriteToStream(response);
            foreach (var user in Lobbies[lobby.Name].Players)
            {
                user.Sock.Send(bytes, bytes.Length, 0);
            }
            return new RemoteInvokeMethod(args);
        }

        public void Start(LobbyInfo lobby)
        {
            var roomName = lobby.Name;
            var board = Games[roomName];
            foreach (var player in Lobbies[roomName].Players)
            {
                board.Players.Add(player);
            }

            var args = Lobbies[roomName].Players.Select(player => new Gambler
            {
                Id = player.Id,
                Name = player.Name,
                Slot = player.Slot,
                Response = Response.Succed.ToString()
            }).Cast<SerializableType>().ToList();

            var bytes = RemoteInvokeMethod.WriteToStream(new RemoteInvokeMethod(Command.Start, args));

            foreach (var player in Lobbies[roomName].Players)
            {
                player.Sock.Send(bytes, bytes.Length, 0);
            }
        }

        public RemoteInvokeMethod InitializeBoard(SimpleMessage message)
        {
            return Games[message.Message].GetHeroes();
        }

        private bool IsGameReady(string gameName)
        {
            return Games[gameName].Players.All(x => x.State == State.Play);
        }

        private void SendGameIsReadyCallback(string gameName)
        {
            var remoteInvokeMethod = new RemoteInvokeMethod("BoardBehaviorMultiplayer", Command.GameIsReady, new List<SerializableType>());
            var bytes = RemoteInvokeMethod.WriteToStream(remoteInvokeMethod);
            foreach (var player in Games[gameName].Players)
            {
                player.Sock.Send(bytes, bytes.Length, 0);
            }
        }

        public RemoteInvokeMethod Disconnect(LobbyInfo lobby)
        {
            var creatorId = lobby.PlayerId;
            var player = Players.Find(x => x.Id == creatorId);
            Lobbies[player.Lobby].Players.Remove(player);
            if (Lobbies[player.Lobby].Players.Count < 1)
            {
                Lobbies.Remove(player.Lobby);
            }
            return new RemoteInvokeMethod(new List<SerializableType>
            {
                new SimpleMessage()
                {
                    Message = Response.Succed.ToString()
                }
            });
        }

        public static void SendMessageToClients(IEnumerable<Player> players, Command command, List<SerializableType> parameters)
        {
            var response = new RemoteInvokeMethod("BoardBehaviorMultiplayer", command, parameters);
            var bytes = RemoteInvokeMethod.WriteToStream(response);
            foreach (var client in players)
            {
                client.Sock.Send(bytes, bytes.Length, 0);
            }
        }

        private void SendSyncLobby(string roomName, Gambler gambler)
        {
            var newPlayer = new List<SerializableType> { gambler };
            var response = new RemoteInvokeMethod(Command.SyncLobby, newPlayer);
            var bytes = RemoteInvokeMethod.WriteToStream(response);

            foreach (var player in Lobbies[roomName].Players)
            {
                player.Sock.Send(bytes, bytes.Length, 0);
            }

            var playersInLobby = Players.FindAll(x => (x.State == State.Lobby || x.State == State.Connect) && x.Id != gambler.Id);
            var lobbyUpdate = new RemoteInvokeMethod(Command.UpdateLobby, newPlayer);
            var bytesMessage = RemoteInvokeMethod.WriteToStream(lobbyUpdate);
            foreach (var player in playersInLobby)
            {
                player.Sock.Send(bytesMessage, bytesMessage.Length, 0);
            }
        }

        private void SendUnits(Player player)
        {
            var units = new List<SerializableType>();
            units.AddRange(_creatures);
            units.AddRange(_heroes);
            var remoteInvokeMethod = new RemoteInvokeMethod(Command.SendUnits, units);
            var bytesMessage = RemoteInvokeMethod.WriteToStream(remoteInvokeMethod);
            player.Sock.Send(bytesMessage, bytesMessage.Length, 0);
        }

    }

}
