using System;
using System.Net;
using Lidgren.Network;

namespace TankGameTestFramework
{
    public class FakeNetIncomingMessage : FakeNetMessage, INetIncomingMessage
    {
        public double ReceiveTime { get; }
        public INetConnection SenderConnection { get; }
        public NetIncomingMessageType MessageType { get; }

        public FakeNetIncomingMessage(FakeNetOutgoingMessage message, FakeNetConnection senderConnection, NetIncomingMessageType messageType = NetIncomingMessageType.Data)
        {
            Data = message.Data;
            SendTime = message.SendTime;
            ReceiveTime = SendTime + senderConnection.Latency;
            SenderConnection = senderConnection;
            MessageType = messageType;
        }

        #region Not implemented
        public NetDeliveryMethod DeliveryMethod
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
        #endregion
    }
}