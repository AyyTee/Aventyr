using Game;
using Game.Portals;
using Lidgren.Network;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using OpenTK.Input;
using System.Threading;
using TankGame.Network;
using System.Diagnostics;

namespace TankGame
{
    public class Controller : Game.Controller
    {
        NetServer server;
        NetClient client;
        public NetPeer peer { get { return server != null ? (NetPeer)server : client; } }
        public bool IsServer { get { return server != null; } }
        public bool IsConnected { get { return peer.ConnectionsCount > 0; } }
        int _messagesSent = 0;
        public long ClientId { get { return peer.UniqueIdentifier; } }
        Dictionary<long, Tank> Tanks = new Dictionary<long, Tank>();

        Scene scene;

        public Controller(Window window, string[] args)
            : base(window)
        {
            if (args.Length >= 2)
            {
                NetPeerConfiguration config = new NetPeerConfiguration("Portal Tank Game");
                config.Port = int.Parse(args[1]);
                config.ConnectionTimeout = 10000;
                //Prevent sent and recieved messages from overwriting eachother in the buffer.
                config.UseMessageRecycling = false;
                if (args[0] == "server")
                {
                    server = new NetServer(config);
                    server.Start();
                }
                else if (args[0] == "client")
                {
                    client = new NetClient(config);
                    client.Start();

                    client.Connect(
                        new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), int.Parse(args[2]))
                        );
                }
            }
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            scene = new Scene();
            scene.Gravity = new Vector2();
            scene.SetActiveCamera(new Camera2(scene, new Transform2(new Vector2(), 10), CanvasSize.Width / (float)CanvasSize.Height));

            Entity entity2 = new Entity(scene);
            entity2.AddModel(ModelFactory.CreatePlane(new Vector2(10, 10)));
            entity2.ModelList[0].SetTexture(Renderer?.Textures["default.png"]);

            PortalCommon.UpdateWorldTransform(scene);
            Renderer?.AddLayer(scene);
        }

        public override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            NetworkStep();

            if (!IsServer)
            {
                Tank tank = GetTank();
                if (tank != null)
                {
                    TankInput input = new TankInput
                    {
                        MoveFoward = InputExt.KeyDown(Key.W),
                        MoveBackward = InputExt.KeyDown(Key.S),
                        TurnLeft = InputExt.KeyDown(Key.A),
                        TurnRight = InputExt.KeyDown(Key.D),
                        ReticlePos = new Vector2(),
                    };
                    tank.SetInput(input);

                    if (IsConnected)
                    {
                        ClientSendMessage(new ClientMessage { Input = input });
                    }
                }
            }

            scene?.Step();
        }

        public Tank GetTank()
        {
            Tank tank;
            Tanks.TryGetValue(ClientId, out tank);
            return tank;
        }

        public void ClientSendMessage(ClientMessage data)
        {
            client.ServerConnection.SendMessage(SendMessage(data), NetDeliveryMethod.ReliableUnordered, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="connection">Client to send the message to.  
        /// If null then the message will be sent to every client.</param>
        public void ServerSendMessage(ServerMessage data, NetConnection connection = null)
        {
            if (connection != null)
            {
                connection.SendMessage(SendMessage(data), NetDeliveryMethod.ReliableUnordered, 0);
            }
            else
            {
                server.SendToAll(SendMessage(data), NetDeliveryMethod.ReliableUnordered);
            }
        }

        private NetOutgoingMessage SendMessage<T>(T data) where T : Message
        {
            data.MessageId = _messagesSent;
            _messagesSent++;

            var message = peer.CreateMessage();
            message.Write(NetworkSerializer.Serialize(data).ToArray());
            LogMessage(message.Data, true);
            return message;
        }

        public void NetworkStep()
        {
            NetIncomingMessage msg;
            
            while ((msg = peer.ReadMessage()) != null)
            {
                string prefix = IsServer ? "Server " : "Client ";
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        Console.WriteLine(prefix + msg.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        Console.WriteLine(prefix + "Status Changed: " + Encoding.Default.GetString(msg.Data));
                        if (IsServer)
                        {
                            if (msg.SenderConnection.Status == NetConnectionStatus.Connected)
                            {
                                List<TankData> tankData = new List<TankData>();

                                Tanks.Add(msg.SenderConnection.RemoteUniqueIdentifier, new Tank(scene));

                                foreach (long id in Tanks.Keys)
                                {
                                    tankData.Add(new TankData
                                    {
                                        ClientId = id,
                                        Transform = Tanks[id].Actor.GetTransform(),
                                        Velocity = Tanks[id].Actor.GetVelocity(),
                                    });
                                }
                                SendMessage(new ServerMessage
                                {
                                    TankData = tankData.ToArray()
                                });
                            }
                        }
                        break;
                    case NetIncomingMessageType.Data:
                        int messageId;
                        if (IsServer)
                        {
                            //Server side message handling.
                            ClientMessage data = NetworkSerializer.Deserialize<ClientMessage>(msg.Data);
                            messageId = data.MessageId;

                            Tanks[msg.SenderConnection.RemoteUniqueIdentifier].SetInput(data.Input);
                        }
                        else
                        {
                            //Client side message handling.
                            ServerMessage data = NetworkSerializer.Deserialize<ServerMessage>(msg.Data);
                            messageId = data.MessageId;

                            foreach (TankData tankData in data.TankData)
                            {
                                Tank tank;
                                Tanks.TryGetValue(tankData.ClientId, out tank);

                                if (tank == null)
                                {
                                    tank = tank ?? new Tank(scene);
                                    Tanks.Add(tankData.ClientId, tank);
                                }

                                tank.Actor.SetTransform(tankData.Transform);
                                tank.Actor.SetVelocity(tankData.Velocity);
                            }
                        }
                        LogMessage(msg.Data, false);
                        break;
                    default:
                        Console.WriteLine(prefix + "Unhandled type: " + msg.MessageType);
                        break;
                }
                peer.Recycle(msg);
            }
        }

        public void LogMessage(byte[] messageData, bool IsOutgoing)
        {
            string prefix = IsServer ? "Server " : "Client ";
            string isOutgoing = IsOutgoing ? "Out " : "In  ";
            Console.WriteLine(prefix + isOutgoing + Encoding.Default.GetString(messageData));
        }
    }
}
