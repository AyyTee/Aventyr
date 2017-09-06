using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Net;
using Game;
using OpenTK;
using OpenTK.Input;
using Game.Portals;
using System.Diagnostics;
using Game.Common;
using Game.Rendering;
using OpenTK.Graphics;

namespace TankGame.Network
{
    public class Client : INetController
    {
        /// <summary>
        /// This client's id.
        /// </summary>
        public long ServerSideId => _client.UniqueIdentifier;
        public Tank OwnedTank => _tanks.GetOrDefault(ServerSideId);
        readonly Dictionary<long, Tank> _tanks = new Dictionary<long, Tank>();
        readonly INetClient _client;
        public INetPeer Peer => _client;
        public bool IsConnected => _client.ServerConnection != null;
        readonly Queue<InputTime> _inputQueue = new Queue<InputTime>();
        public Scene Scene { get; private set; }
        public string Name => "Client";
        double _lastTimestamp;
        bool _sceneUpdated;
        public int StepCount { get; private set; }
        TankCamera _tankCamera;
        readonly IVirtualWindow _window;
        RollingAverage _fpsCounter = new RollingAverage(60, 0);

        public int MessagesSent { get; set; }

        struct InputTime
        {
            public TankInput Input;
            public double Timestamp;
        }

        public Client(IVirtualWindow window, IPEndPoint serverAddress, INetClient client)
        {
            _window = window;

            _client = client;
            _client.Start();

            _client.Connect(serverAddress);

            Scene = new Scene();
            Scene.Gravity = new Vector2();

            Init();
        }

        public void Init()
        {
            Camera2 camera = new Camera2(
                Scene,
                new Transform2(new Vector2(), size: 10),
                _window.CanvasSize.X / (float)_window.CanvasSize.Y);

            _tankCamera = new TankCamera(camera, null);

            Entity entity2 = new Entity(Scene);
            entity2.AddModel(ModelFactory.CreatePlane(new Vector2(10, 10), Color4.White, new Vector3(-5, -5, 0)));
            entity2.ModelList[0].SetTexture(_window.Textures?.@Default());

            PortalCommon.UpdateWorldTransform(Scene);
        }

        public void Render(double timeDelta)
        {
            _window.Layers.Clear();
            _window.Layers.Add(LayerEx.FromScene(Scene));

            var gui = new Layer();
            gui.Camera = new HudCamera2(_window.CanvasSize);
            _fpsCounter.Enqueue((float)timeDelta);
            gui.Renderables.Add(Draw.Text(
                _window.Fonts?.LatoRegular(), 
                new Vector2(),
                $"Client\nId {_client.UniqueIdentifier}\n\nFPS\nAvg {(1 / _fpsCounter.GetAverage()).ToString("00.00")}\nMin {(1 / _fpsCounter.Queue.Max()).ToString("00.00")}\n{_window.MousePosition}"));
            _window.Layers.Add(gui);
        }

        public void Update(double timeDelta)
        {
            NetworkStep();

            var input = TankInput.CreateInput(_window, _tankCamera.Camera);
            
            if (IsConnected)
            {
                _inputQueue.Enqueue(new InputTime
                {
                    Input = input,
                    Timestamp = NetTime.Now
                });
                if (_inputQueue.Count > 100)
                {
                    DebugEx.Fail("Input queue is full.");
                    _inputQueue.Dequeue();
                }

                SendMessage(new MessageToServer { Input = input });
            }

            if (_sceneUpdated)
            {
                foreach (Bullet b in Scene.GetAll().OfType<Bullet>().Where(item => item.ServerId == null))
                {
                    Scene.MarkForRemoval(b);
                }
                while (_inputQueue.Count > 0 && _client.ServerConnection.GetRemoteTime(_inputQueue.Peek().Timestamp) < _lastTimestamp - _client.ServerConnection.AverageRoundtripTime/2)
                {
                    _inputQueue.Dequeue();
                }
                InputTime[] inputArray = _inputQueue.ToArray();
                for (int i = 0; i < inputArray.Length; i++)
                {
                    OwnedTank?.SetInput(inputArray[i].Input);
                    Scene.Step(1 / _window.UpdatesPerSecond);
                }
                _sceneUpdated = false;
            }
            else
            {
                OwnedTank?.SetInput(input);
                Scene.Step(1 / _window.UpdatesPerSecond);
            }
            StepCount++;
        }

        public void SendMessage(MessageToServer data)
        {
            _client.ServerConnection.SendMessage(
                NetworkHelper.PrepareMessage(this, data), 
                NetworkHelper.DeliveryMethod, 
                0);
        }

        public void NetworkStep()
        {
            INetIncomingMessage msg;

            while ((msg = _client.ReadMessage()) != null)
            {
                DebugEx.Assert(msg.SenderConnection?.RemoteUniqueIdentifier != _client.UniqueIdentifier,
                    "Unique identifier should not be the same as this client.");
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
                        break;
                    case NetIncomingMessageType.Data:
                        HandleData(msg);
                        break;
                    default:
                        Console.WriteLine(Name + "Unhandled type: " + msg.MessageType);
                        break;
                }
                _client.Recycle(msg);
            }
        }

        void HandleData(INetIncomingMessage msg)
        {
            MessageToClient data = NetworkHelper.ReadMessage<MessageToClient>(msg);
            bool outOfDate = data.LocalSendTime <= _lastTimestamp;
            if (!outOfDate)
            {
                _lastTimestamp = data.LocalSendTime;
            }
            
            foreach (WallAdded added in data.WallsAdded)
            {
                added.WallCreate(Scene);
            }

            if (!outOfDate)
            {
                Scene.Time = data.SceneTime;

                foreach (TankData tankData in data.TankData)
                {
                    Tank tank = _tanks.GetOrDefault(tankData.OwnerId);

                    if (tank == null)
                    {
                        tank = new Tank(Scene);
                        _tanks.Add(tankData.OwnerId, tank);

                        if (tankData.OwnerId == ServerSideId)
                        {
                            _tankCamera.SetTank(tank);
                        }
                    }

                    tankData.UpdateTank(tank, Scene);

                    _sceneUpdated = true;
                }

                foreach (BulletData bulletData in data.BulletData)
                {
                    var bullet = (Bullet)Scene.GetAll().OfType<INetObject>().FirstOrDefault(item => item.ServerId == bulletData.ServerId);
                    bullet = bullet ?? new Bullet(Scene, new Vector2(), new Vector2());
                    bulletData.UpdateBullet(bullet);
                }

                /*foreach (PortalData portalData in data.PortalData)
                {
                    var portal = (TankPortal)Scene.GetAll().OfType<INetObject>().FirstOrDefault(item => item.ServerId == portalData.ServerId);
                    portal = portal ?? new TankPortal(Scene, new Vector2(), new Vector2());
                    portalData.Update(portal);
                }*/
            }

            PortalCommon.UpdateWorldTransform(Scene, true);
        }
    }
}
