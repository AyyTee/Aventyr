using System.Net;

namespace Lidgren.Network
{
    public interface INetClient : INetPeer
    {
        NetConnectionStatus ConnectionStatus { get; }
        INetConnection ServerConnection { get; }

        INetConnection Connect(IPEndPoint remoteEndPoint, INetOutgoingMessage hailMessage);
        void Disconnect(string byeMessage);
        NetSendResult SendMessage(INetOutgoingMessage msg, NetDeliveryMethod method);
        NetSendResult SendMessage(INetOutgoingMessage msg, NetDeliveryMethod method, int sequenceChannel);
        string ToString();
    }
}