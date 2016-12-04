using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Lidgren.Network;

namespace TankGameTests
{
    internal class FakeNetClient : INetClient
    {
        public NetPeerConfiguration Configuration
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<INetConnection> Connections
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int ConnectionsCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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

        public INetConnection ServerConnection
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Socket Socket
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NetPeerStatistics Statistics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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

        public long UniqueIdentifier
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NetUPnP UPnP
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public INetConnection Connect(IPEndPoint remoteEndPoint)
        {
            throw new NotImplementedException();
        }

        public INetConnection Connect(string host, int port)
        {
            throw new NotImplementedException();
        }

        public INetConnection Connect(IPEndPoint remoteEndPoint, INetOutgoingMessage hailMessage)
        {
            throw new NotImplementedException();
        }

        public INetConnection Connect(string host, int port, INetOutgoingMessage hailMessage)
        {
            throw new NotImplementedException();
        }

        public INetOutgoingMessage CreateMessage()
        {
            throw new NotImplementedException();
        }

        public INetOutgoingMessage CreateMessage(int initialCapacity)
        {
            throw new NotImplementedException();
        }

        public INetOutgoingMessage CreateMessage(string content)
        {
            throw new NotImplementedException();
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

        public NetIncomingMessage ReadMessage()
        {
            throw new NotImplementedException();
        }

        public bool ReadMessage(out NetIncomingMessage message)
        {
            throw new NotImplementedException();
        }

        public int ReadMessages(IList<NetIncomingMessage> addTo)
        {
            throw new NotImplementedException();
        }

        public void Recycle(IEnumerable<NetIncomingMessage> toRecycle)
        {
            throw new NotImplementedException();
        }

        public void Recycle(NetIncomingMessage msg)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public NetSendResult SendMessage(INetOutgoingMessage msg, INetConnection recipient, NetDeliveryMethod method)
        {
            throw new NotImplementedException();
        }

        public NetSendResult SendMessage(INetOutgoingMessage msg, NetDeliveryMethod method, int sequenceChannel)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(INetOutgoingMessage msg, IList<INetConnection> recipients, NetDeliveryMethod method, int sequenceChannel)
        {
            throw new NotImplementedException();
        }

        public NetSendResult SendMessage(INetOutgoingMessage msg, INetConnection recipient, NetDeliveryMethod method, int sequenceChannel)
        {
            throw new NotImplementedException();
        }

        public void SendUnconnectedMessage(INetOutgoingMessage msg, IList<IPEndPoint> recipients)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void UnregisterReceivedCallback(SendOrPostCallback callback)
        {
            throw new NotImplementedException();
        }

        public NetIncomingMessage WaitMessage(int maxMillis)
        {
            throw new NotImplementedException();
        }
    }
}