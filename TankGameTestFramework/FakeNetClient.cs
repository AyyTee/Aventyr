using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Lidgren.Network;
using System.Linq;
using Game;

namespace TankGameTestFramework
{
    public class FakeNetClient : FakeNetPeer, INetClient
    {
        public INetConnection ServerConnection => Connections.FirstOrDefault();
    }
}