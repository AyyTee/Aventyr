using System;
using System.Net;
using System.Reflection;
using Lidgren.Network;

namespace TankGameTests
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

        public double ReceiveTime
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NetConnection SenderConnection
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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

        public NetIncomingMessageType MessageType { get; set; }

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