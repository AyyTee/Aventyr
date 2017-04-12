using System;
using System.Net;
using System.Reflection;
using Lidgren.Network;
using System.Collections.Generic;
using System.Linq;

namespace TankGameTestFramework
{
    public class FakeNetOutgoingMessage : FakeNetMessage, INetOutgoingMessage
    {
        public FakeNetIncomingMessage ToIncomingMessage(FakeNetConnection senderConnection)
        {
            return new FakeNetIncomingMessage
            {
                Data = Data,
                SendTime = SendTime,
                ReceiveTime = SendTime + senderConnection.Latency,
                SenderConnection = senderConnection,
            };
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