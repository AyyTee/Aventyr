namespace Lidgren.Network
{
    public interface INetServer : INetPeer
    {
        void SendToAll(INetOutgoingMessage msg, NetDeliveryMethod method);
        void SendToAll(INetOutgoingMessage msg, INetConnection except, NetDeliveryMethod method, int sequenceChannel);
    }
}