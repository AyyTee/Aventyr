using System;
using System.Net;
using System.Reflection;
using Lidgren.Network;
using System.Collections.Generic;
using System.Linq;

namespace TankGameTests
{
    internal class FakeNetOutgoingMessage : FakeNetMessage, INetOutgoingMessage
    {
        public FakeNetIncomingMessage ToIncomingMessage()
        {
            var message = new FakeNetIncomingMessage {Data = Data};
            return message;
        }

        public bool Encrypt(NetEncryption encryption)
        {
            throw new NotImplementedException();
        }

        public void EnsureBufferSize(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public IPEndPoint ReadIPEndPoint()
        {
            throw new NotImplementedException();
        }
    }
}