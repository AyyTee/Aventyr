using System;
using Lidgren.Network;

namespace TankGameTests
{
    internal class FakeNetPeerStatistics : INetPeerStatistics
    {
        public int BytesInRecyclePool
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int ReceivedBytes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int ReceivedMessages { get; set; }

        public int ReceivedPackets
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int SentBytes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int SentMessages { get; set; }

        public int SentPackets
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public long StorageBytesAllocated
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}