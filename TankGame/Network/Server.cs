using Game;
using Game.Portals;
using Lidgren.Network;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Rendering;

namespace TankGame.Network
{
    public class Server : INetController
    {
        Dictionary<long, Tank> _tanks = new Dictionary<long, Tank>();
        readonly INetServer _server;
        public INetPeer Peer => _server;
        Scene _scene;
        public string Name => "Server";
        public int StepCount { get; private set; }
        List<Wall> _walls = new List<Wall>();
        HashSet<long> _loading = new HashSet<long>();
        int _idCount;
        public int MessagesSent { get; set; }
        public Scene Hud { get; private set; }

        HashSet<ClientInstance> _clients = new HashSet<ClientInstance>();
        readonly IVirtualWindow _window;

        public class ClientInstance
        {
            public readonly long Id;
            public double LatestTimestamp { get; set; }

            public ClientInstance(long id)
            {
                Id = id;
            }
        }

        public Server(IVirtualWindow window, INetServer netServer)
        {
            _window = window;
            _server = netServer;
            _server.Start();

            Init();
        }

        public void Init()
        {
            _scene = new Scene();
            _scene.Gravity = new Vector2();
            new Camera2(_scene, new Transform2(new Vector2(), 10), _window.CanvasSize.Width / (float)_window.CanvasSize.Height);

            Entity entity2 = new Entity(_scene);
            entity2.AddModel(ModelFactory.CreatePlane(new Vector2(10, 10)));
            entity2.ModelList[0].SetTexture(_window.Textures?.Default);

            Entity serverMarker = new Entity(_scene);
            serverMarker.AddModel(ModelFactory.CreateCircle(new Vector3(-3, -3, 1), 0.5f, 10));


            _walls.Add(InitNetObject(new Wall(_scene, PolygonFactory.CreateRectangle(3, 2))));
            _walls[0].SetTransform(new Transform2(new Vector2(3, 0)));
            _walls.Add(InitNetObject(new Wall(_scene, PolygonFactory.CreateRectangle(3, 2))));
            _walls[1].SetTransform(new Transform2(new Vector2(1, 3)));

            PortalCommon.UpdateWorldTransform(_scene);

            Hud = new Scene();
            Camera2 camera = new Camera2(
                Hud,
                new Transform2(new Vector2(), _window.CanvasSize.Height),
                _window.CanvasSize.Width / (float)_window.CanvasSize.Height);
        }

        public T InitNetObject<T>(T netObject) where T : INetObject
        {
            if (netObject.ServerId == null)
            {
                NetworkHelper.SetServerId(netObject, _idCount);
                _idCount++;
            }
            return netObject;
        }

        /// <param name="data"></param>
        /// <param name="connection">Client to send the message to.  
        /// If null then the message will be sent to every client.</param>
        public void SendMessage(MessageToClient data, INetConnection connection = null)
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

        public void Render(double timeDelta)
        {
            _window.Layers.Clear();
            _window.Layers.Add(new Layer(_scene));

            var gui = new Layer(Hud);
            gui.DrawText(
                _window.Fonts?.Inconsolata,
                new Vector2(-_window.CanvasSize.Width / 2, _window.CanvasSize.Height / 2),
                $"Client\nId {_server.UniqueIdentifier}\n\nFPS\nAvg { (1 / timeDelta).ToString("00.00") }\nMax { (1 / timeDelta).ToString("00.00") }\nMin { (1 / timeDelta).ToString("00.00") }");
        }

        public void Update(double timeDelta)
        {
            NetworkStep();

            if (_scene != null)
            {
                _scene.Step(1 / _window.UpdatesPerSecond);

                foreach (long clientId in _loading.ToArray())
                {
                    var message = new MessageToClient
                    {
                        WallsAdded = _walls.Select(wall => new WallAdded(wall)).ToArray()
                    };
                    INetConnection client = _server.Connections.First(item => item.RemoteUniqueIdentifier == clientId);
                    SendMessage(message, client);
                    _loading.Remove(clientId);
                }

                foreach (INetObject netObject in _scene.GetAll().OfType<INetObject>())
                {
                    InitNetObject(netObject);
                }

                SendMessage(new MessageToClient
                {
                    TankData = _tanks.Keys.Select(id => new TankData(id, _tanks[id])).ToArray(),
                    BulletData = _scene.GetAll().OfType<Bullet>().Select(item => new BulletData(item)).ToArray(),
                    //PortalData = _scene.GetAll().OfType<TankPortal>().Select(item => new PortalData(InitNetObject(item))).ToArray(),
                    SceneTime = _scene.Time,
                });
            }

            Render(timeDelta);
        }

        public void NetworkStep()
        {
            INetIncomingMessage msg;

            while ((msg = _server.ReadMessage()) != null)
            {
                //Debug.Assert(msg.SenderConnection?.RemoteUniqueIdentifier != _server.UniqueIdentifier, 
                //    "Unique identifier should not be the same as the servers.");
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        Console.WriteLine(Name + msg.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        HandleStatusChanged(msg);
                        break;
                    case NetIncomingMessageType.Data:
                        HandleData(msg);
                        break;
                    default:
                        Console.WriteLine(Name + "Unhandled type: " + msg.MessageType);
                        break;
                }
                _server.Recycle(msg);
            }
        }

        void HandleStatusChanged(INetIncomingMessage msg)
        {
            Console.WriteLine(Name + "Status Changed: " + Encoding.Default.GetString(msg.Data));

            if (msg.SenderConnection.Status == NetConnectionStatus.Connected)
            {
                _clients.Add(new ClientInstance(msg.SenderConnection.RemoteUniqueIdentifier));
                _tanks.Add(msg.SenderConnection.RemoteUniqueIdentifier, InitNetObject(new Tank(_scene)));
                _loading.Add(msg.SenderConnection.RemoteUniqueIdentifier);
            }
            PortalCommon.UpdateWorldTransform(_scene, true);
        }

        void HandleData(INetIncomingMessage msg)
        {
            MessageToServer data = NetworkHelper.ReadMessage<MessageToServer>(msg);
            ClientInstance client = _clients.First(item => item.Id == msg.SenderConnection.RemoteUniqueIdentifier);
            Tank tank = _tanks[msg.SenderConnection.RemoteUniqueIdentifier];

            bool outOfDate = data.LocalSendTime <= client.LatestTimestamp;
            if (!outOfDate)
            {
                client.LatestTimestamp = data.LocalSendTime;
            }

            if (outOfDate && client.LatestTimestamp - data.LocalSendTime < 0.5f)
            {
                //If the message is late then still accept firing input (unless it's really late).
                tank.Input.FireGun |= data.Input.FireGun;
                tank.Input.FirePortal[0] |= data.Input.FirePortal[0];
                tank.Input.FirePortal[1] |= data.Input.FirePortal[1];
            }
            else if (!outOfDate)
            {
                tank.SetInput(data.Input);
            }
            
        }
    }
}
