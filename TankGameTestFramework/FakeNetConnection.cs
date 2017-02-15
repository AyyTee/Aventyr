using System;
using System.Net;
using Lidgren.Network;

namespace TankGameTestFramework
{
    public class FakeNetConnection : INetConnection
    {
        public FakeNetPeer EndPoint { get; set; }

        public float AverageRoundtripTime { get; set; }

        public int CurrentMTU
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NetOutgoingMessage LocalHailMessage
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NetPeer Peer
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NetIncomingMessage RemoteHailMessage
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public float RemoteTimeOffset
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public long RemoteUniqueIdentifier
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NetConnectionStatistics Statistics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NetConnectionStatus Status
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object Tag
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        INetOutgoingMessage INetConnection.LocalHailMessage
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Approve()
        {
            throw new NotImplementedException();
        }

        public void Approve(INetOutgoingMessage localHail)
        {
            throw new NotImplementedException();
        }

        public void Approve(NetOutgoingMessage localHail)
        {
            throw new NotImplementedException();
        }

        public bool CanSendImmediately(NetDeliveryMethod method, int sequenceChannel)
        {
            throw new NotImplementedException();
        }

        public void Deny()
        {
            throw new NotImplementedException();
        }

        public void Deny(string reason)
        {
            throw new NotImplementedException();
        }

        public void Disconnect(string byeMessage)
        {
            throw new NotImplementedException();
        }

        public double GetLocalTime(double remoteTimestamp)
        {
            return remoteTimestamp;
        }

        public double GetRemoteTime(double localTimestamp)
        {
            return localTimestamp;
        }

        public void GetSendQueueInfo(NetDeliveryMethod method, int sequenceChannel, out int windowSize, out int freeWindowSlots)
        {
            throw new NotImplementedException();
        }

        public NetSendResult SendMessage(INetOutgoingMessage msg, NetDeliveryMethod method, int sequenceChannel)
        {
            var _msg = (FakeNetOutgoingMessage) msg;
            _msg.SendTime = NetTime.Now;
            EndPoint.Messages.Enqueue(_msg.ToIncomingMessage(EndPoint.Latency));
            return NetSendResult.Sent;
        }
    }
}