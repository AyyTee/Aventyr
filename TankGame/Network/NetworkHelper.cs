using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Reflection;

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
            config.SimulatedRandomLatency = 0.05f;
            config.SimulatedDuplicatesChance = 0.005f;
            config.SimulatedLoss = 0.005f;
            //Prevent sent and recieved messages from overwriting eachother in the buffer.
            config.UseMessageRecycling = false;
            return config;
        }

        public static NetOutgoingMessage PrepareMessage<T>(INetController sender, T data) where T : Message
        {
            data.MessageId = sender.Peer.Statistics.SentMessages;
            data.StepCount = sender.StepCount;
            data.LocalSendTime = NetTime.Now;

            var message = sender.Peer.CreateMessage();
            //message.WriteTime(true);
            byte[] serializedData = NetworkSerializer.Serialize(data).ToArray();
            message.Write(serializedData.Length);
            message.Write(serializedData);
            //LogMessage(sender.Name, serializedData, true);
            return message;
        }

        public static T ReadMessage<T>(NetIncomingMessage message)
        {
            //sendTime = message.ReadTime(true);
            int length = message.ReadInt32();
            byte[] serializedData = new byte[length];
            message.ReadBytes(serializedData, 0, length);
            return NetworkSerializer.Deserialize<T>(serializedData);
        }
    }
}
