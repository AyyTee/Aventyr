using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Lidgren.Network
{
    public interface INetPeer
    {
        NetPeerConfiguration Configuration { get; }
        List<INetConnection> Connections { get; }
        int ConnectionsCount { get; }
        AutoResetEvent MessageReceivedEvent { get; }
        int Port { get; }
        Socket Socket { get; }
        NetPeerStatistics Statistics { get; }
        NetPeerStatus Status { get; }
        object Tag { get; set; }
        long UniqueIdentifier { get; }
        NetUPnP UPnP { get; }

        INetConnection Connect(IPEndPoint remoteEndPoint);
        INetConnection Connect(string host, int port);
        INetConnection Connect(IPEndPoint remoteEndPoint, INetOutgoingMessage hailMessage);
        INetConnection Connect(string host, int port, INetOutgoingMessage hailMessage);
        INetOutgoingMessage CreateMessage();
        INetOutgoingMessage CreateMessage(string content);
        INetOutgoingMessage CreateMessage(int initialCapacity);
        void DiscoverKnownPeer(IPEndPoint endPoint);
        bool DiscoverKnownPeer(string host, int serverPort);
        void DiscoverLocalPeers(int serverPort);
        void FlushSendQueue();
        INetConnection GetConnection(IPEndPoint ep);
        void Introduce(IPEndPoint hostInternal, IPEndPoint hostExternal, IPEndPoint clientInternal, IPEndPoint clientExternal, string token);
        void RawSend(byte[] arr, int offset, int length, IPEndPoint destination);
        NetIncomingMessage ReadMessage();
        bool ReadMessage(out NetIncomingMessage message);
        int ReadMessages(IList<NetIncomingMessage> addTo);
        void Recycle(NetIncomingMessage msg);
        void Recycle(IEnumerable<NetIncomingMessage> toRecycle);
        void RegisterReceivedCallback(SendOrPostCallback callback, SynchronizationContext syncContext = null);
        void SendDiscoveryResponse(INetOutgoingMessage msg, IPEndPoint recipient);
        NetSendResult SendMessage(INetOutgoingMessage msg, INetConnection recipient, NetDeliveryMethod method);
        NetSendResult SendMessage(INetOutgoingMessage msg, INetConnection recipient, NetDeliveryMethod method, int sequenceChannel);
        void SendMessage(INetOutgoingMessage msg, IList<INetConnection> recipients, NetDeliveryMethod method, int sequenceChannel);
        void SendUnconnectedMessage(INetOutgoingMessage msg, IPEndPoint recipient);
        void SendUnconnectedMessage(INetOutgoingMessage msg, IList<IPEndPoint> recipients);
        void SendUnconnectedMessage(INetOutgoingMessage msg, string host, int port);
        void SendUnconnectedToSelf(INetOutgoingMessage om);
        void Shutdown(string bye);
        void Start();
        void UnregisterReceivedCallback(SendOrPostCallback callback);
        NetIncomingMessage WaitMessage(int maxMillis);
    }
}