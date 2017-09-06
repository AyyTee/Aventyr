using System.Net;

namespace Lidgren.Network
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public interface INetIncomingMessage : INetBuffer
    {
        NetDeliveryMethod DeliveryMethod { get; }
        NetIncomingMessageType MessageType { get; }
        double ReceiveTime { get; }
        INetConnection SenderConnection { get; }
        IPEndPoint SenderEndPoint { get; }
        int SequenceChannel { get; }

        bool Decrypt(NetEncryption encryption);
        double ReadTime(bool highPrecision);
        string ToString();
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}