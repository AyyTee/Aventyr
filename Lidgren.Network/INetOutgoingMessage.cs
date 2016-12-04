namespace Lidgren.Network
{
    public interface INetOutgoingMessage : INetBuffer
    {
        bool Encrypt(NetEncryption encryption);
        string ToString();
    }
}