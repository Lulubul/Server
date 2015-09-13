using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServerCore
    {
        public Network Network;
        public void Start()
        {
            Network = Network.Instance;
        }
    }
}
