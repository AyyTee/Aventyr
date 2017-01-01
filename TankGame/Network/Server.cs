﻿using Game;
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
using Game.Rendering;

namespace TankGame.Network
{
    public class Server : INetController
    {
        Dictionary<long, Tank> Tanks = new Dictionary<long, Tank>();
        INetServer _server;
        public INetPeer Peer { get { return _server; } }
        Scene _scene;
        Renderer _renderer;
        public string Name { get { return "Server"; } }
        public int StepCount { get; private set; }
        List<Wall> Walls = new List<Wall>();
        HashSet<long> loading = new HashSet<long>();
        int _idCount;
        public int MessagesSent { get; set; }

        HashSet<ClientInstance> clients = new HashSet<ClientInstance>();

        public class ClientInstance
        {
            public readonly long Id;
            public double LatestTimestamp { get; set; }

            public ClientInstance(long id)
            {
                Id = id;
            }
        }

        public Server(INetServer netServer)
        {
            _server = netServer;
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

            Entity serverMarker = new Entity(_scene);
            serverMarker.AddModel(ModelFactory.CreateCircle(new Vector3(-3, -3, 1), 0.5f, 10));


            Walls.Add(InitNetObject(new Wall(_scene, PolygonFactory.CreateRectangle(3, 2))));
            Walls[0].SetTransform(new Transform2(new Vector2(3, 0)));
            Walls.Add(InitNetObject(new Wall(_scene, PolygonFactory.CreateRectangle(3, 2))));
            Walls[1].SetTransform(new Transform2(new Vector2(1, 3)));

            PortalCommon.UpdateWorldTransform(_scene);
            _renderer?.AddLayer(_scene);
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
        public void SendMessage(ServerMessage data, INetConnection connection = null)
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

                foreach (long clientId in loading.ToArray())
                {
                    ServerMessage message = new ServerMessage
                    {
                        WallsAdded = Walls.Select(wall => new WallAdded(wall)).ToArray()
                    };
                    INetConnection client = _server.Connections.First(item => item.RemoteUniqueIdentifier == clientId);
                    SendMessage(message, client);
                    loading.Remove(clientId);
                }

                List<TankData> tankData = new List<TankData>();
                foreach (long id in Tanks.Keys)
                {
                    tankData.Add(new TankData(id, Tanks[id]));
                }

                List<BulletData> bulletData = new List<BulletData>();
                foreach (Bullet bullet in _scene.GetAll().OfType<Bullet>())
                {
                    bulletData.Add(new BulletData(InitNetObject(bullet)));
                }
                SendMessage(new ServerMessage
                {
                    TankData = tankData.ToArray(),
                    BulletData = bulletData.ToArray(),
                    SceneTime = _scene.Time,
                });
            }
        }

        public void NetworkStep()
        {
            INetIncomingMessage msg;

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

        private void HandleStatusChanged(INetIncomingMessage msg)
        {
            Console.WriteLine(Name + "Status Changed: " + Encoding.Default.GetString(msg.Data));

            if (msg.SenderConnection.Status == NetConnectionStatus.Connected)
            {
                clients.Add(new ClientInstance(msg.SenderConnection.RemoteUniqueIdentifier));
                Tanks.Add(msg.SenderConnection.RemoteUniqueIdentifier, InitNetObject(new Tank(_scene)));
                loading.Add(msg.SenderConnection.RemoteUniqueIdentifier);
            }
            PortalCommon.UpdateWorldTransform(_scene, true);
        }

        private void HandleData(INetIncomingMessage msg)
        {
            ClientMessage data = NetworkHelper.ReadMessage<ClientMessage>(msg);
            ClientInstance client = clients.First(item => item.Id == msg.SenderConnection.RemoteUniqueIdentifier);
            Tank tank = Tanks[msg.SenderConnection.RemoteUniqueIdentifier];

            bool outOfDate = data.LocalSendTime <= client.LatestTimestamp;
            if (!outOfDate)
            {
                client.LatestTimestamp = data.LocalSendTime;
            }

            if (outOfDate && client.LatestTimestamp - data.LocalSendTime < 0.5f)
            {
                //If the message is late then still accept firing input (unless it's really late).
                tank.Input.FireGun |= data.Input.FireGun;
                tank.Input.FirePortal0 |= data.Input.FirePortal0;
                tank.Input.FirePortal1 |= data.Input.FirePortal1;
            }
            else if (!outOfDate)
            {
                tank.SetInput(data.Input);
            }
            
        }
    }
}
