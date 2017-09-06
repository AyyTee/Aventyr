namespace Lidgren.Network
{
    public interface INetPeerStatistics
    {
        int BytesInRecyclePool { get; }
        int ReceivedBytes { get; }
        int ReceivedMessages { get; }
        int ReceivedPackets { get; }
        int SentBytes { get; }
        int SentMessages { get; }
        int SentPackets { get; }
        long StorageBytesAllocated { get; }

        string ToString();
    }
}