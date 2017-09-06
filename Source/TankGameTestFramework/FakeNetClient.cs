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
        INetConnection INetClient.ServerConnection => Connections.FirstOrDefault();

        public FakeNetConnection ServerConnection => Connections.FirstOrDefault();

        public FakeNetClient(long uniqueIdentifier) : base(uniqueIdentifier)
        {
        }
    }
}