namespace Server
{
    public class ServerCore
    {
        public Network Network;
        public ServerCore()
        {
            Network = Network.Instance;
        }

        public void Start()
        {
            Network.StartListening();
        }

    }
}
