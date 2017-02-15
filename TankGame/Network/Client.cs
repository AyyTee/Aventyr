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
using System.Drawing;
using Game.Portals;
using System.Diagnostics;
using Game.Common;
using Game.Rendering;

namespace TankGame.Network
{
    public class Client : INetController
    {
        /// <summary>
        /// This client's id.
        /// </summary>
        public long ServerId => _client.UniqueIdentifier;

        readonly Dictionary<long, Tank> _tanks = new Dictionary<long, Tank>();
        readonly INetClient _client;
        public INetPeer Peer => _client;
        public bool IsConnected => _client.ServerConnection != null;
        readonly Queue<InputTime> _inputQueue = new Queue<InputTime>();
        public Scene Scene { get; private set; }
        public readonly IController Controller;
        Renderer _renderer;
        public string Name => "Client";
        double _lastTimestamp;
        bool _sceneUpdated;
        public int StepCount { get; private set; }
        TankCamera _tankCamera;
        public int MessagesSent { get; set; }

        struct InputTime
        {
            public TankInput Input;
            public double Timestamp;
        }

        public Client(IPEndPoint serverAddress, IController controller, INetClient client)
        {
            Controller = controller;

            _client = client;
            _client.Start();

            _client.Connect(serverAddress);

            Scene = new Scene();
            Scene.Gravity = new Vector2();
        }

        public void Init(Renderer renderer, Size canvasSize)
        {
            _renderer = renderer;

            Camera2 camera = new Camera2(
                Scene,
                new Transform2(new Vector2(), 10),
                canvasSize.Width / (float)canvasSize.Height);

            Scene.SetActiveCamera(camera);

            _tankCamera = new TankCamera(camera, null, Controller);

            Entity entity2 = new Entity(Scene);
            entity2.AddModel(ModelFactory.CreatePlane(new Vector2(10, 10)));
            entity2.ModelList[0].SetTexture(_renderer?.Textures["default.png"]);

            PortalCommon.UpdateWorldTransform(Scene);
            _renderer?.AddLayer(Scene);
        }

        public void Step()
        {
            NetworkStep();

            Tank tank = GetTank();
            TankInput input = new TankInput
            {
                MoveFoward = Controller.Input.KeyDown(Key.W),
                MoveBackward = Controller.Input.KeyDown(Key.S),
                TurnLeft = Controller.Input.KeyDown(Key.A),
                TurnRight = Controller.Input.KeyDown(Key.D),
                ReticlePos = Controller.Input.GetMouseWorldPos(_tankCamera.Camera, Vector2Ext.ToOtk(Controller.CanvasSize)),
                FireGun = Controller.Input.KeyPress(Key.Space),
                FirePortalLeft = Controller.Input.MousePress(MouseButton.Left),
                FirePortalRight = Controller.Input.MousePress(MouseButton.Right)
            };
            
            if (IsConnected)
            {
                _inputQueue.Enqueue(new InputTime
                {
                    Input = input,
                    Timestamp = NetTime.Now
                });
                if (_inputQueue.Count > 60)
                {
                    _inputQueue.Dequeue();
                }

                SendMessage(new ClientMessage { Input = input });
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
                    tank?.SetInput(inputArray[i].Input);
                    Scene.Step();
                }
                _sceneUpdated = false;
            }
            else
            {
                tank?.SetInput(input);
                Scene.Step();
            }
            StepCount++;
        }

        Tank GetTank()
        {
            Tank tank;
            _tanks.TryGetValue(ServerId, out tank);
            return tank;
        }

        public void SendMessage(ClientMessage data)
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
            ServerMessage data = NetworkHelper.ReadMessage<ServerMessage>(msg);
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
                    Tank tank;
                    _tanks.TryGetValue(tankData.OwnerId, out tank);

                    if (tank == null)
                    {
                        tank = new Tank(Scene);
                        _tanks.Add(tankData.OwnerId, tank);

                        if (tankData.OwnerId == ServerId)
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
