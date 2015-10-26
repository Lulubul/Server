using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;
using NetworkTypes;

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

        private const int BoardSizeWidth = 12;
        private const int BoardSizeHeight = 7;

        public NetworkActions()
        {
            Lobbies = new Dictionary<string, Lobby>();
            History = new Dictionary<Player, string>();
            Players = new List<Player>();
            Games = new Dictionary<string, BoardBehavior>();
            _repository = new Repository();
            _userRepository = _repository.Users;
            _type = GetType();
        }

        public void Execute(Command command, string serviceClassName, List<SerializableType> parameters, Player client)
        {
            try
            {
                if (serviceClassName == "BoardBehavior")
                {
                    var lobby = parameters[0] as LobbyInfo;
                    parameters.RemoveAt(0);
                    var fields = parameters.ToArray();
                    var theMethod = Games[lobby.Name].GetType().GetMethod(command.ToString());
                    theMethod.Invoke(Games[lobby.Name], fields);
                }
                else
                {
                    var theMethod = _type.GetMethod(command.ToString());
                    var fields = parameters.ToArray();
                    if (theMethod.ReturnType != typeof(void))
                    {
                        var response = (RemoteInvokeMethod)theMethod.Invoke(this, fields);
                        response.MethodName = command.ToString();
                        var bytes = RemoteInvokeMethod.WriteToStream(response);
                        client.Sock.Send(bytes, bytes.Length, 0);

                        if (command != Command.Login && command != Command.Register) return;
                        var user = response.Parameters[0] as Gambler;
                        client.Id = user.Id;
                        client.Name = user.Name;
                        client.State = State.Lobby;
                        Players.Add(client);
                    }
                    else
                    {
                        theMethod.Invoke(this, fields);
                    }
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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

        public RemoteInvokeMethod Connect()
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
            AddLobbyRooms(args);
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
            AddLobbyRooms(args);
            return new RemoteInvokeMethod(args);
        }

        public void AddLobbyRooms(ICollection<SerializableType> args)
        {
            var n = Lobbies.Count;
            for (var i = 0; i < n; i++)
            {
                var info = new LobbyInfo
                {
                    Name = Lobbies.ElementAt(i).Value.Name,
                    MaxPlayers = Lobbies.ElementAt(i).Value.MaxPlayers,
                    GameType = Lobbies.ElementAt(i).Value.GameType,
                    CurrentPlayers = Lobbies.ElementAt(i).Value.Players.Count
                };
                args.Add(info);
            }
        }

        public RemoteInvokeMethod Create(LobbyInfo room)
        {
            var creatorId = room.PlayerId;
            var lobbyName = room.Name;
            var maxPlayer = room.MaxPlayers;

            var args = new List<SerializableType>();
            if (!Lobbies.ContainsKey(lobbyName))
            {
                var index = Players.FindIndex(x => x.Id == creatorId);
                var newLobby = new Lobby(lobbyName, creatorId, maxPlayer, room.GameType);
                Players[index].Team = Team.Red;
                Players[index].Lobby = lobbyName;
                Players[index].State = State.Join;
                Players[index].Slot = 0;
                newLobby.Players.Add(Players[index]);
                Lobbies.Add(lobbyName, newLobby);

                var message = new ResponseMessage
                {
                    Response = Response.Succed.ToString(),
                    Message = room.Name
                };
                args.Add(message);
                var lobby = new List<SerializableType> { room };
                var response = new RemoteInvokeMethod(Command.SyncRooms, lobby);
                var bytes = RemoteInvokeMethod.WriteToStream(response);
                var clients = Players.Where(client => client.State == State.Lobby);
                foreach (var client in clients)
                {
                    client.Sock.Send(bytes, bytes.Length, 0);
                }
            }
            else
            {
                var message = new ResponseMessage
                {
                    Response = Response.Fail.ToString(),
                    Message = "Room name already exist."
                };
                args.Add(message);
            }
            return new RemoteInvokeMethod(args);
        }

        public RemoteInvokeMethod Join(LobbyInfo lobby)
        {
            var playerId = lobby.PlayerId;
            var roomName = lobby.Name;
            var index = Players.FindIndex(x => x.Id == playerId);
            var emptySlot = Lobbies[roomName].EmptySlot();
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

            //Send SyncLobby
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

            //Send Response
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
            var remote = new RemoteInvokeMethod(args);
            return remote;
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
            var board = new BoardBehavior(BoardSizeWidth, BoardSizeHeight);
            foreach (var player in Lobbies[roomName].Players)
            {
                board.Players.Add(player);
            }
            Games.Add(roomName, board);

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

        public void InitializeBoard(SimpleMessage message)
        {
            if (!Games[message.Message].IsInitialize)
            {
                Games[message.Message].Initialize();
            }
            Games[message.Message].SendHeroesToClient();
            
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
    }

}
