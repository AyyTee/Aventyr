using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Reflection;
using System.Diagnostics;
using Game.Common;

namespace TankGame.Network
{
    public static class NetworkHelper
    {
        public const NetDeliveryMethod DeliveryMethod = NetDeliveryMethod.ReliableUnordered;

        public static void LogMessage(string name, byte[] messageData, bool IsOutgoing)
        {
            string isOutgoing = IsOutgoing ? " Out " : " In  ";
            Console.WriteLine(name + isOutgoing + Encoding.Default.GetString(messageData));
        }

        public static NetPeerConfiguration GetDefaultConfig()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("Portal Tank Game");
            config.ConnectionTimeout = 10000;
            config.SimulatedMinimumLatency = 0.1f;
            config.SimulatedRandomLatency = 0.02f;
            config.SimulatedDuplicatesChance = 0.005f;
            config.SimulatedLoss = 0.005f;
            //Prevent sent and recieved messages from overwriting eachother in the buffer.
            config.UseMessageRecycling = false;
            return config;
        }

        public static INetOutgoingMessage PrepareMessage<T>(INetController sender, T data) where T : Message
        {
            data.MessageId = sender.MessagesSent;
            sender.MessagesSent++;
            data.LocalSendTime = NetTime.Now;

            var message = sender.Peer.CreateMessage();
            byte[] serializedData = NetworkSerializer.Serialize(data).ToArray();
            message.Write(serializedData.Length);
            message.Write(serializedData);
            //LogMessage(sender.Name, serializedData, true);
            return message;
        }

        public static T ReadMessage<T>(INetIncomingMessage message)
        {
            int length = message.ReadInt32();
            byte[] serializedData = new byte[length];
            serializedData = message.ReadBytes(length);
            return NetworkSerializer.Deserialize<T>(serializedData);
        }

        public static void SetServerId(INetObject netObject, int serverId)
        {
            DebugEx.Assert(netObject.ServerId == null || netObject.ServerId == serverId);
            netObject.ServerId = serverId;
        }
    }
}
