using System;
using System.Net;
using Lidgren.Network;
using System.Collections.Generic;
using System.Linq;

namespace TankGameTestFramework
{
    public class FakeNetConnection : INetConnection
    {
        public FakeNetConnection(FakeNetPeer start, FakeNetPeer end)
        {
            StartPoint = start;
            EndPoint = end;
        }

        public FakeNetPeer StartPoint { get; private set; }

        public FakeNetPeer EndPoint { get; private set; }

        public List<FakeNetIncomingMessage> MessagesInTransit { get; private set; } = new List<FakeNetIncomingMessage>();

        public float AverageRoundtripTime => (float)Latency * 2;

        public long RemoteUniqueIdentifier => EndPoint.UniqueIdentifier;

        public FakeNetConnection ConnectionPair => EndPoint.Connections.First(item => item.RemoteUniqueIdentifier == StartPoint.UniqueIdentifier);

        #region Not Implemented
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

        public NetConnectionStatistics Statistics
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
        public void GetSendQueueInfo(NetDeliveryMethod method, int sequenceChannel, out int windowSize, out int freeWindowSlots)
        {
            throw new NotImplementedException();
        }
        #endregion

        public double GetLocalTime(double remoteTimestamp)
        {
            return remoteTimestamp;
        }

        public double GetRemoteTime(double localTimestamp)
        {
            return localTimestamp;
        }

        public NetSendResult SendMessage(INetOutgoingMessage msg, NetDeliveryMethod method, int sequenceChannel)
        {
            var _msg = (FakeNetOutgoingMessage)msg;
            _msg.SendTime = NetTime.Now;
            if (Latency > 0)
            {
                MessagesInTransit.Add(new FakeNetIncomingMessage(_msg, ConnectionPair));
            }
            else
            {
                EndPoint.EnqueueArrivedMessage(new FakeNetIncomingMessage(_msg, ConnectionPair));
            }
            return NetSendResult.Sent;
        }

        public void SetTime(double time)
        {
            var arrivals = MessagesInTransit
                .FindAll(item => item.ReceiveTime <= time)
                .OrderBy(item => item.ReceiveTime)
                .ToList();
            MessagesInTransit = MessagesInTransit.Except(arrivals).ToList();
            arrivals.ForEach(EndPoint.EnqueueArrivedMessage);
        }

        public double Latency { get; set; }

        public NetConnectionStatus Status => NetConnectionStatus.Connected;
    }
}