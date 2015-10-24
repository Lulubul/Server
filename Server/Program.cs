using System;
using DataAccess;
using Entities;
using NetworkTypes;
using System.Data.Entity;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new ServerCore();
            server.Start();
        }
    }
}
