using System.Net;

namespace Lidgren.Network
{
    public interface INetConnection
    {
        float AverageRoundtripTime { get; }
        int CurrentMTU { get; }
        INetOutgoingMessage LocalHailMessage { get; }
        NetPeer Peer { get; }
        IPEndPoint RemoteEndPoint { get; }
        NetIncomingMessage RemoteHailMessage { get; }
        float RemoteTimeOffset { get; }
        long RemoteUniqueIdentifier { get; }
        NetConnectionStatistics Statistics { get; }
        NetConnectionStatus Status { get; }
        object Tag { get; set; }

        void Approve();
        void Approve(INetOutgoingMessage localHail);
        bool CanSendImmediately(NetDeliveryMethod method, int sequenceChannel);
        void Deny();
        void Deny(string reason);
        void Disconnect(string byeMessage);
        double GetLocalTime(double remoteTimestamp);
        double GetRemoteTime(double localTimestamp);
        void GetSendQueueInfo(NetDeliveryMethod method, int sequenceChannel, out int windowSize, out int freeWindowSlots);
        NetSendResult SendMessage(INetOutgoingMessage msg, NetDeliveryMethod method, int sequenceChannel);
        string ToString();
    }
}