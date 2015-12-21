namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            AutoMapperConfig.Initialize();
            var server = new ServerCore();
            server.Start();
        }
    }
}
