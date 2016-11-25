using System;
using System.Collections.Generic;
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

namespace TankGame.Network
{
    public class Client : INetController
    {
        /// <summary>
        /// This client's id.
        /// </summary>
        public long RemoteId { get { return _client.UniqueIdentifier; } }
        Dictionary<long, Tank> Tanks = new Dictionary<long, Tank>();
        NetClient _client;
        public NetPeer Peer { get { return _client; } }
        public bool IsConnected { get { return _client.ServerConnection != null; } }
        Queue<InputTime> _inputQueue = new Queue<InputTime>();
        Scene _scene;
        Controller _controller;
        Renderer _renderer;
        public string Name { get { return "Client"; } }
        double _lastTimestamp;
        bool _sceneUpdated = false;
        public int StepCount { get; private set; }
        TankCamera _tankCamera;

        struct InputTime
        {
            public TankInput Input;
            public double Timestamp;
        }

        public Client(int port, int serverPort, Controller controller)
        {
            _controller = controller;

            var config = NetworkHelper.GetDefaultConfig();
            config.Port = port;

            _client = new NetClient(config);
            _client.Start();

            _client.Connect(
                new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), serverPort)
                );

            _scene = new Scene();
            _scene.Gravity = new Vector2();
        }

        public void Init(Renderer renderer, Size canvasSize)
        {
            _renderer = renderer;

            Camera2 camera = new Camera2(
                _scene,
                new Transform2(new Vector2(), 10),
                canvasSize.Width / (float)canvasSize.Height);

            _scene.SetActiveCamera(camera);

            _tankCamera = new TankCamera(camera, null, _controller);

            Entity entity2 = new Entity(_scene);
            entity2.AddModel(ModelFactory.CreatePlane(new Vector2(10, 10)));
            entity2.ModelList[0].SetTexture(_renderer?.Textures["default.png"]);

            /*Actor actor = new Actor(_scene, PolygonFactory.CreateRectangle(0.8f, 1));
            Entity entity = new Entity(_scene);
            entity.SetParent(actor);
            entity.AddModel(ModelFactory.CreateCube(new Vector3(0.8f, 1, 1)));*/

            PortalCommon.UpdateWorldTransform(_scene);
            _renderer?.AddLayer(_scene);
        }

        public void Step()
        {
            NetworkStep();

            Tank tank = GetTank();
            TankInput input = new TankInput
            {
                MoveFoward = _controller.InputExt.KeyDown(Key.W),
                MoveBackward = _controller.InputExt.KeyDown(Key.S),
                TurnLeft = _controller.InputExt.KeyDown(Key.A),
                TurnRight = _controller.InputExt.KeyDown(Key.D),
                ReticlePos = new Vector2(),
            };
            
            if (IsConnected)
            {
                _inputQueue.Enqueue(new InputTime
                {
                    Input = input,
                    Timestamp = NetTime.Now
                });

                SendMessage(new ClientMessage { Input = input });
            }

            /*if (IsConnected)
                Console.Out.WriteLine(_client.ServerConnection.AverageRoundtripTime + " " + (NetTime.Now - _client.ServerConnection.GetRemoteTime(NetTime.Now)));
                */
            if (_sceneUpdated)
            {
                while (_inputQueue.Count > 0 && _client.ServerConnection.GetRemoteTime(_inputQueue.Peek().Timestamp) < _lastTimestamp - _client.ServerConnection.AverageRoundtripTime/2)
                {
                    _inputQueue.Dequeue();
                }
                //Debug.Assert(_inputQueue.Count < 20);
                var inputArray = _inputQueue.ToArray();
                //Console.WriteLine(_inputQueue.Count);
                for (int i = 0; i < inputArray.Length; i++)
                {
                    tank?.SetInput(inputArray[i].Input);
                    _scene.Step();
                }
                _sceneUpdated = false;
            }
            else
            {
                tank?.SetInput(input);
                _scene.Step();
            }
            StepCount++;
        }

        public Tank GetTank()
        {
            Tank tank;
            Tanks.TryGetValue(RemoteId, out tank);
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
            NetIncomingMessage msg;

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
                        ServerMessage data = NetworkHelper.ReadMessage<ServerMessage>(msg);
                        if (data.LocalSendTime <= _lastTimestamp)
                        {
                            continue;
                        }
                        _lastTimestamp = data.LocalSendTime;

                        foreach (TankData tankData in data.TankData)
                        {
                            Tank tank;
                            Tanks.TryGetValue(tankData.ClientId, out tank);

                            if (tank == null)
                            {
                                tank = tank ?? new Tank(_scene);
                                Tanks.Add(tankData.ClientId, tank);

                                if (tankData.ClientId == RemoteId)
                                {
                                    _tankCamera.SetTank(tank);
                                }
                            }

                            tank.Actor.SetTransform(tankData.Transform);
                            tank.Actor.SetVelocity(tankData.Velocity);
                            tank.Actor.WorldTransform = tankData.Transform;
                            tank.Actor.WorldVelocity = tankData.Velocity;
                            tank.Actor.Children[0].WorldTransform = tankData.Transform;
                            tank.Actor.Children[0].WorldVelocity = tankData.Velocity;

                            _sceneUpdated = true;
                        }
                        break;
                    default:
                        Console.WriteLine(Name + "Unhandled type: " + msg.MessageType);
                        break;
                }
                _client.Recycle(msg);
            }
        }
    }
}
