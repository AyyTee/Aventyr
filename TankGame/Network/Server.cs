using Game;
using Game.Portals;
using Lidgren.Network;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame.Network
{
    public class Server : INetController
    {
        Dictionary<long, Tank> Tanks = new Dictionary<long, Tank>();
        NetServer _server;
        public NetPeer Peer { get { return _server; } }
        Scene _scene;
        Renderer _renderer;
        public string Name { get { return "Server"; } }
        double _lastTimestamp;
        public int StepCount { get; private set; }

        public Server(int port)
        {
            var config = NetworkHelper.GetDefaultConfig();
            config.Port = port;
            _server = new NetServer(config);
            _server.Start();
        }

        public void Init(Renderer renderer, Size canvasSize)
        {
            _renderer = renderer;
            _scene = new Scene();
            _scene.Gravity = new Vector2();
            _scene.SetActiveCamera(new Camera2(_scene, new Transform2(new Vector2(), 10), canvasSize.Width / (float)canvasSize.Height));

            Entity entity2 = new Entity(_scene);
            entity2.AddModel(ModelFactory.CreatePlane(new Vector2(10, 10)));
            entity2.ModelList[0].SetTexture(_renderer?.Textures["default.png"]);

            PortalCommon.UpdateWorldTransform(_scene);
            _renderer?.AddLayer(_scene);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="connection">Client to send the message to.  
        /// If null then the message will be sent to every client.</param>
        public void SendMessage(ServerMessage data, NetConnection connection = null)
        {
            if (connection != null)
            {
                connection.SendMessage(
                    NetworkHelper.PrepareMessage(this, data),
                    NetworkHelper.DeliveryMethod, 
                    0);
            }
            else
            {
                _server.SendToAll(
                    NetworkHelper.PrepareMessage(this, data),
                    NetworkHelper.DeliveryMethod);
            }
        }

        public void Step()
        {
            NetworkStep();

            if (_scene != null)
            {
                _scene.Step();
                //FakeStep();

                List<TankData> tankData = new List<TankData>();
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

        public void NetworkStep()
        {
            NetIncomingMessage msg;

            while ((msg = _server.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        Console.WriteLine(Name + msg.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        Console.WriteLine(Name + "Status Changed: " + Encoding.Default.GetString(msg.Data));

                        if (msg.SenderConnection.Status == NetConnectionStatus.Connected)
                        {
                            Tanks.Add(msg.SenderConnection.RemoteUniqueIdentifier, new Tank(_scene));
                        }
                        PortalCommon.UpdateWorldTransform(_scene, true);
                        break;
                    case NetIncomingMessageType.Data:
                        ClientMessage data = NetworkHelper.ReadMessage<ClientMessage>(msg);
                        if (data.LocalSendTime <= _lastTimestamp)
                        {
                            continue;
                        }
                        _lastTimestamp = data.LocalSendTime;

                        Tanks[msg.SenderConnection.RemoteUniqueIdentifier].SetInput(data.Input);
                        break;
                    default:
                        Console.WriteLine(Name + "Unhandled type: " + msg.MessageType);
                        break;
                }
                _server.Recycle(msg);
            }
        }
    }
}
