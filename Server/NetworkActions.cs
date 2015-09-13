using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Models;
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

        public NetworkActions()
        {
            Lobbies = new Dictionary<string, Lobby>();
            History = new Dictionary<Player, string>();
            Players = new List<Player>();
            Games = new Dictionary<string, BoardBehavior>();

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
                    MethodInfo theMethod = Games[lobby.Name].GetType().GetMethod(command.ToString());
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

        public RemoteInvokeMethod Connect()
        {
            var now = DateTime.Now;
            var strDateLine = "Welcome " + now.ToString("G");
            var args = new List<SerializableType>();
            var message = new SimpleMessage();
            message.Message = strDateLine;
            args.Add(message);
            var remote = new RemoteInvokeMethod(args);
            return remote;
        }

        public RemoteInvokeMethod Login(Authentication authentification)
        {
            using (var context = new HeroesEntities())
            {
                var name = authentification.Name;
                var pass = authentification.Pass;
                context.Configuration.LazyLoadingEnabled = false;

                context.Gambler.Load();
                Gambler gambler = context.Gambler.Where(x => x.name == name && x.pass == pass).FirstOrDefault();
                if (gambler != null)
                {
                    var args = new List<SerializableType>();
                    var user = new Gambler();
                    user.Name = gambler.name;
                    user.Id = gambler.id;
                    user.Response = Response.Succed.ToString();
                    args.Add(user);

                    var n = Lobbies.Count;
                    for (var i = 0; i < n; i++)
                    {
                        var info = new LobbyInfo();
                        info.Name = Lobbies.ElementAt(i).Value.Name;
                        info.MaxPlayers = Lobbies.ElementAt(i).Value.MaxPlayers;
                        info.GameType = Lobbies.ElementAt(i).Value.GameType;
                        info.CurrentPlayers = Lobbies.ElementAt(i).Value.Players.Count;
                        args.Add(info);
                    }

                    var remote = new RemoteInvokeMethod(args);
                    return remote;
                }
                else
                {
                    var args = new List<SerializableType>();
                    var user = new Gambler();
                    user.Name = name;
                    user.Id = 0;
                    user.Response = Response.Fail.ToString();
                    args.Add(user);
                    var remote = new RemoteInvokeMethod(args);
                    return remote;
                }
            }
        }

        public RemoteInvokeMethod Logout(Authentication authentification)
        {
            var args = new List<SerializableType>();
            var message = new ResponseMessage { Response = Response.Succed.ToString() };
            args.Add(message);
            var remote = new RemoteInvokeMethod(args);
            return remote;
        }

        public RemoteInvokeMethod Register(Authentication authentification)
        {
            using (var context = new HeroesEntities())
            {
                var gambler = new User();
                gambler.Name = authentification.Name;
                gambler.Slot = authentification.Pass;
                context.User.Add(gambler);
                context.SaveChanges();

                var args = new List<SerializableType>();
                var user = new Gambler
                {
                    Name = gambler.name,
                    Id = gambler.id,
                    Response = Response.Succed.ToString()
                };
                args.Add(user);

                var n = Lobbies.Count;
                for (var i = 0; i < n; i++)
                {
                    var info = new LobbyInfo
                    {
                        PlayerId = Lobbies.ElementAt(i).Value.CreatorId,
                        Name = Lobbies.ElementAt(i).Value.Name,
                        MaxPlayers = Lobbies.ElementAt(i).Value.MaxPlayers,
                        GameType = Lobbies.ElementAt(i).Value.GameType,
                        CurrentPlayers = Lobbies.ElementAt(i).Value.Players.Count
                    };
                    args.Add(info);
                }
                var remote = new RemoteInvokeMethod(args);
                return remote;
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
                var player = Players[index];
                player.Team = Team.Red;
                player.Lobby = lobbyName;
                player.State = State.Join;
                player.Slot = 0;
                newLobby.Players.Add(player);
                Lobbies.Add(lobbyName, newLobby);

                var message = new ResponseMessage
                {
                    Response = Response.Succed.ToString(),
                    Message = room.Name
                };
                args.Add(message);
                var remote = new RemoteInvokeMethod(args);

                var lobby = new List<SerializableType> {room};
                var response = new RemoteInvokeMethod(Command.SyncRooms, lobby);
                var bytes = RemoteInvokeMethod.WriteToStream(response);
                var clients = Players.Where(client => client.State == State.Lobby);
                foreach (var client in clients)
                {
                    client.Sock.Send(bytes, bytes.Length, 0);
                }
                return remote;
            }
            else
            {
                var message = new ResponseMessage
                {
                    Response = Response.Fail.ToString(),
                    Message = "Room name already exist."
                };
                args.Add(message);
                var remote = new RemoteInvokeMethod(args);
                return remote;
            }
        }

        public RemoteInvokeMethod Join(LobbyInfo lobby)
        {
            //Find Player
            var playerId = lobby.PlayerId;
            var roomName = lobby.Name;
            var index = Players.FindIndex(x => x.Id == playerId);

            //Fill Player
            var user = new Gambler()
            {
                Id = playerId,
                Name = Players[index].Name,
                Slot = Lobbies[roomName].EmptySlot()
            };

            if (user.Slot >= 0)
            {
                user.Response = Response.Succed.ToString();
            }
            else
            {
                user.Response = Response.Fail.ToString();
                var users = new List<SerializableType> {user};
                var rim = new RemoteInvokeMethod(users);
                return rim;
            }

            //Send SyncLobby
            var newPlayer = new List<SerializableType> {user};
            var response = new RemoteInvokeMethod(Command.SyncLobby, newPlayer);
            var bytes = RemoteInvokeMethod.WriteToStream(response);

            foreach (Player player in Lobbies[roomName].Players)
            {
                player.Sock.Send(bytes, bytes.Length, 0);
            }

            List<Player> playersInLobby = Players.FindAll(x => (x.State == State.Lobby || x.State == State.Connect) && x.Id != user.Id);
            var lobbyUpdate = new RemoteInvokeMethod(Command.UpdateLobby, newPlayer);
            var bytesMessage = RemoteInvokeMethod.WriteToStream(lobbyUpdate);
            foreach (Player player in playersInLobby)
            {
                player.Sock.Send(bytesMessage, bytesMessage.Length, 0);
            }

            //Send Response
            var args = new List<SerializableType>();

            var lobbyInfo = new LobbyInfo(Lobbies[roomName].CreatorId, Lobbies[roomName].Name, Lobbies[roomName].GameType, Lobbies[roomName].MaxPlayers, Lobbies[roomName].Players.Count);
            args.Add(lobbyInfo);

            foreach (Player player in Lobbies[roomName].Players)
            {
                var lobbyUser = new Gambler();
                lobbyUser.Name = player.Name;
                lobbyUser.Id = player.Id;
                lobbyUser.Slot = player.Slot;
                lobbyUser.Response = Response.Succed.ToString();
                args.Add(lobbyUser);
            }

            Lobbies[lobby.Name].Players.Add(Players[index]);
            Players[index].Slot = user.Slot;
            Players[index].Team = Players[index].Slot % 2 == 0 ? Team.Red : Team.Blue;

            Players[index].State = State.Join;
            args.Add(user);
            var remote = new RemoteInvokeMethod(args);
            return remote;
        }

        public RemoteInvokeMethod ChangeTeam(LobbyInfo lobby)
        {
            var playerId = lobby.PlayerId;
            var roomName = lobby.Name;
            var index = Players.FindIndex(x => x.Id == playerId);
            Player player = Players[index];

            player.Team = player.Team == Team.Blue ? Team.Red : Team.Blue;
            player.Slot = Lobbies[lobby.Name].TeamSlot(player.Team);

            var args = new List<SerializableType>();
            foreach (Player user in Lobbies[roomName].Players)
            {
                var lobbyUser = new Gambler
                {
                    Name = user.Name,
                    Id = user.Id,
                    Slot = user.Slot,
                    Response = Response.Succed.ToString()
                };
                args.Add(lobbyUser);
            }

            //Sync Lobby
            var response = new RemoteInvokeMethod(Command.SyncLobby, args);
            var bytes = RemoteInvokeMethod.WriteToStream(response);
            foreach (Player user in Lobbies[roomName].Players)
            {
                user.Sock.Send(bytes, bytes.Length, 0);
            }

            var remote = new RemoteInvokeMethod(args);
            return remote;
        }

        public RemoteInvokeMethod Leave(LobbyInfo lobby)
        {
            var playerId = lobby.PlayerId;
            var player = Players.Find(x => x.Id == playerId);
            player.State = State.Lobby;
            var roomName = lobby.Name;
            Lobbies[player.Lobby].Players.Remove(player);
            if (Lobbies[player.Lobby].Players.Count < 1)
            {
                Lobbies.Remove(lobby.Name);
            }

            var args = new List<SerializableType>();
            foreach (var user in Lobbies[roomName].Players)
            {
                var lobbyUser = new Gambler
                {
                    Name = user.Name,
                    Id = user.Id,
                    Slot = user.Slot,
                    Response = Response.Succed.ToString()
                };
                args.Add(lobbyUser);
            }

            //Sync Lobby
            var response = new RemoteInvokeMethod(Command.SyncLobby, args);
            var bytes = RemoteInvokeMethod.WriteToStream(response);
            foreach (var user in Lobbies[roomName].Players)
            {
                user.Sock.Send(bytes, bytes.Length, 0);
            }

            var remote = new RemoteInvokeMethod(args);
            return remote;
        }

        public void Start(LobbyInfo lobby)
        {
            var roomName = lobby.Name;

            var args = new List<SerializableType>();

            var board = new BoardBehavior(12, 7);
            foreach (var player in Lobbies[roomName].Players)
            {
                board.players.Add(player);
            }
            Games.Add(roomName, board);

            foreach (Player player in Lobbies[roomName].Players)
            {
                var user = new Gambler();
                user.Id = player.Id;
                user.Name = player.Name;
                user.Slot = player.Slot;
                args.Add(user);
            }

            var response = new RemoteInvokeMethod(Command.Start, args);
            var bytes = RemoteInvokeMethod.WriteToStream(response);
            foreach (Player player in Lobbies[roomName].Players)
            {
                player.Sock.Send(bytes, bytes.Length, 0);
            }

            board.Initialize();
        }

        /*public RemoteInvokeMethod Disconnect(string[] fields)
        {
            int creatorID = Int32.Parse(fields[0]);
            Player player = players.Find(x => x.ID == creatorID);
            lobbies[player.Lobby].Players.Remove(player);
            if (lobbies[player.Lobby].Players.Count < 1)
            {
                lobbies.Remove(player.Lobby);
            }
            string[] args = new string[1];
            args[0] = Response.Succed.ToString();
         
            //switch case : state 
            RemoteInvokeMethod remote = new RemoteInvokeMethod(args);
            return remote;
        }*/
    }
}
