using System;
using System.Net;
using System.Reflection;
using Lidgren.Network;

namespace TankGameTestFramework
{
    public class FakeNetIncomingMessage : FakeNetMessage, INetIncomingMessage
    {
        public FakeNetIncomingMessage()
        {
        }

        public NetDeliveryMethod DeliveryMethod
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double ReceiveTime { get; set; }

        public INetConnection SenderConnection { get; set; }

        public IPEndPoint SenderEndPoint
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int SequenceChannel
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NetIncomingMessageType MessageType { get; set; } = NetIncomingMessageType.Data;

        public bool Decrypt(NetEncryption encryption)
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