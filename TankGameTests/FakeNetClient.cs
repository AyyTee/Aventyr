using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Lidgren.Network;
using System.Linq;
using Game;

namespace TankGameTests
{
    internal class FakeNetClient : INetClient
    {
        public Queue<FakeNetIncomingMessage> Messages = new Queue<FakeNetIncomingMessage>();

        public NetPeerConfiguration Configuration
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<INetConnection> Connections { get; set; } = new List<INetConnection>();

        public int ConnectionsCount { get { return Connections.Count; } }

        public NetConnectionStatus ConnectionStatus
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public AutoResetEvent MessageReceivedEvent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Port
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public INetConnection ServerConnection { get { return Connections.FirstOrDefault(); } }

        public Socket Socket
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public INetPeerStatistics Statistics { get; set; } = new FakeNetPeerStatistics();

        public NetPeerStatus Status
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

        public long UniqueIdentifier { get; set; } = new Random().NextLong();

        public NetUPnP UPnP
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public INetConnection Connect(IPEndPoint remoteEndPoint)
        {
            return new FakeNetConnection();
        }

        public INetConnection Connect(string host, int port)
        {
            return new FakeNetConnection();
        }

        public INetConnection Connect(IPEndPoint remoteEndPoint, INetOutgoingMessage hailMessage)
        {
            return new FakeNetConnection();
        }

        public INetConnection Connect(string host, int port, INetOutgoingMessage hailMessage)
        {
            return new FakeNetConnection();
        }

        public INetOutgoingMessage CreateMessage()
        {
            return new FakeNetOutgoingMessage();
        }

        public INetOutgoingMessage CreateMessage(int initialCapacity)
        {
            return new FakeNetOutgoingMessage();
        }

        public INetOutgoingMessage CreateMessage(string content)
        {
            return new FakeNetOutgoingMessage();
        }

        public void Disconnect(string byeMessage)
        {
            throw new NotImplementedException();
        }

        public void DiscoverKnownPeer(IPEndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public bool DiscoverKnownPeer(string host, int serverPort)
        {
            throw new NotImplementedException();
        }

        public void DiscoverLocalPeers(int serverPort)
        {
            throw new NotImplementedException();
        }

        public void FlushSendQueue()
        {
            throw new NotImplementedException();
        }

        public INetConnection GetConnection(IPEndPoint ep)
        {
            throw new NotImplementedException();
        }

        public void Introduce(IPEndPoint hostInternal, IPEndPoint hostExternal, IPEndPoint clientInternal, IPEndPoint clientExternal, string token)
        {
            throw new NotImplementedException();
        }

        public void RawSend(byte[] arr, int offset, int length, IPEndPoint destination)
        {
            throw new NotImplementedException();
        }

        public INetIncomingMessage ReadMessage()
        {
            if (Messages.Count == 0)
            {
                return null;
            }
            return Messages.Dequeue();
        }

        public bool ReadMessage(out INetIncomingMessage message)
        {
            message = Messages.Dequeue();
            return Messages.Count <= 0;
        }

        public int ReadMessages(IList<INetIncomingMessage> addTo)
        {
            throw new NotImplementedException();
        }

        public void Recycle(IEnumerable<INetIncomingMessage> toRecycle)
        {
        }

        public void Recycle(INetIncomingMessage msg)
        {
        }

        public void RegisterReceivedCallback(SendOrPostCallback callback, SynchronizationContext syncContext = null)
        {
            throw new NotImplementedException();
        }

        public void SendDiscoveryResponse(INetOutgoingMessage msg, IPEndPoint recipient)
        {
            throw new NotImplementedException();
        }

        public NetSendResult SendMessage(INetOutgoingMessage msg, NetDeliveryMethod method)
        {
            return NetSendResult.Sent;
        }

        public NetSendResult SendMessage(INetOutgoingMessage msg, INetConnection recipient, NetDeliveryMethod method)
        {
            return NetSendResult.Sent;
        }

        public NetSendResult SendMessage(INetOutgoingMessage msg, NetDeliveryMethod method, int sequenceChannel)
        {
            return NetSendResult.Sent;
        }

        public void SendMessage(INetOutgoingMessage msg, IList<INetConnection> recipients, NetDeliveryMethod method, int sequenceChannel)
        {
        }

        public NetSendResult SendMessage(INetOutgoingMessage msg, INetConnection recipient, NetDeliveryMethod method, int sequenceChannel)
        {
            return NetSendResult.Sent;
        }

        public void SendUnconnectedMessage(INetOutgoingMessage msg, IList<IPEndPoint> recipients)
        {
        }

        public void SendUnconnectedMessage(INetOutgoingMessage msg, IPEndPoint recipient)
        {
            throw new NotImplementedException();
        }

        public void SendUnconnectedMessage(INetOutgoingMessage msg, string host, int port)
        {
            throw new NotImplementedException();
        }

        public void SendUnconnectedToSelf(INetOutgoingMessage om)
        {
            throw new NotImplementedException();
        }

        public void Shutdown(string bye)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
        }

        public void UnregisterReceivedCallback(SendOrPostCallback callback)
        {
            throw new NotImplementedException();
        }

        public INetIncomingMessage WaitMessage(int maxMillis)
        {
            throw new NotImplementedException();
        }
    }
}