using System;
using System.Net.Sockets;
using NetworkTypes;

namespace Server
{
    public class Player
    {
        private readonly Socket _mSock;	
        private readonly byte[] _mByBuff;

        public string Lobby;
        public string Name { get; set; }
        public int Flag { get; set; }
        public Team Team { get; set; }
        public int Id { get; set; }
        public int Slot { get; set; }
        public State State;

        public Player(Socket sock)
        {
            _mSock = sock;
            _mByBuff = new byte[256];
            State = State.Connect;
        }
        public Socket Sock
        {
            get { return _mSock; }
        }

        /// <summary>
        /// Setup the callback for recieved data and loss of conneciton
        /// </summary>
        /// <param name="app"></param>
        public void SetupRecieveCallback()
        {
            try
            {
                var recieveData = new AsyncCallback(Network.OnRecievedData);
                _mSock.BeginReceive(_mByBuff, 0, _mByBuff.Length, SocketFlags.None, recieveData, this);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Recieve callback setup failed! {0}", ex.Message);
            }
        }

        /// <summary>
        /// Data has been recieved so we shall put it in an array and
        /// return it.
        /// </summary>
        /// <param name="ar"></param>
        /// <returns>Array of bytes containing the received data</returns>
        public byte[] GetRecievedData(IAsyncResult ar)
        {
            var nBytesRec = 0;
            try
            {
                nBytesRec = _mSock.EndReceive(ar);
            }
            catch
            {
                // TODO: Treat expection
            }
            var byReturn = new byte[nBytesRec];
            Array.Copy(_mByBuff, byReturn, nBytesRec);
            return byReturn;
        }
    }
}
