﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTypes;

namespace Server
{
    interface INetworkActions
    {
        RemoteInvokeMethod Login(Authentication authentification);
        RemoteInvokeMethod Logout(Authentication authentification);
        RemoteInvokeMethod Register(Authentication authentification);
        /*RemoteInvokeMethod Join(string[] args);
         RemoteInvokeMethod Create(string[] args);
         RemoteInvokeMethod ChangeTeam(string[] args);
         RemoteInvokeMethod Leave(string[] args);
         RemoteInvokeMethod Disconnect(string[] args);*/
    }
}
