using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using NetworkTypes;

namespace Server
{
    /// <summary>
    /// Singleton Network
    /// </summary>
    public sealed class Network
    {
        public Socket Socket;
        public IPEndPoint EndPoint;
        public EndPoint EPoint;
        
        //TODO: Move into config file
        private const int Port = 11000;
        private static NetworkActions _handler;
        private static volatile Network _instance;
        private static readonly object SyncRoot = new object();

        public static Network Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new Network();
                }
                return _instance;
            }
        }

        private Network()
        {
            EndPoint = new IPEndPoint(IPAddress.Any, Port);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _handler = new NetworkActions();
        }

        public void StartListening()
        {
            try
            {
                Console.WriteLine("*** Server Started {0} *** ", DateTime.Now.ToString("G"));
                var host = Dns.GetHostName();
                var heserver = Dns.GetHostEntry(host);
                Console.WriteLine("Listening on : [{0}] {1}:{2}", host, heserver.AddressList[2], Port);

                Socket.Bind(EndPoint);
                Socket.Listen(100);
                Socket.BeginAccept(AcceptCallback, Socket);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress Enter to stop server");
            Console.Read();

            Socket.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            var listener = (Socket)ar.AsyncState;
            var client = listener.EndAccept(ar);
            NewConnection(client);
            Socket.BeginAccept(AcceptCallback, Socket);
        }

        public void NewConnection(Socket sockClient)
        {
            var client = new Player(sockClient);
            Console.WriteLine("Client {0}, joined", client.Sock.RemoteEndPoint);
            var args = new List<SerializableType>();
            var message = new SimpleMessage {Message = "Hello"};
            args.Add(message);
            var remoteMethod = new RemoteInvokeMethod("Handler", Command.Connect.ToString(), args);
            var bytes = RemoteInvokeMethod.WriteToStream(remoteMethod);
            client.Sock.Send(bytes, bytes.Length, SocketFlags.None);
            client.SetupRecieveCallback();
        }

        public static void OnRecievedData(IAsyncResult ar)
        {
            var client = (Player)ar.AsyncState;
            var aryRet = client.GetRecievedData(ar);

            if (aryRet.Length < 1)
            {
                Console.WriteLine("Client {0}, disconnected", client.Sock.RemoteEndPoint);
                client.Sock.Close();
                return;
            }

            var stream = new MemoryStream();
            stream.Write(aryRet, 0, aryRet.Length);
            var remoteInvoke = RemoteInvokeMethod.ReadFromStream(stream);
            Command command;
            Enum.TryParse(remoteInvoke.MethodName, out command);

            var nameClass = remoteInvoke.ServiceClassName;
            _handler.Execute(command, nameClass, remoteInvoke.Parameters, client);
            client.SetupRecieveCallback();
        }
    }
}
