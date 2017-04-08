using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TankGame.Network;
using System.Collections.Generic;
using Lidgren.Network;
using TankGame;
using Game;
using OpenTK.Input;
using System.Linq;
using Game.Common;
using Game.Portals;
using TankGameTestFramework;

namespace TankGameTests
{
    [TestClass]
    public class UnitTest1
    {
        Client _client;
        FakeNetClient _netClient;
        Server _server;
        FakeNetServer _netServer;

        FakeNetPeer[] _netPeers => new FakeNetPeer[] { _netClient, _netServer };

        [TestInitialize]
        public void Initialize()
        {
            NetTime.AutomaticTimeKeeping = false;
            NetTime.SetTime(0);

            _netClient = new FakeNetClient();
            var controller = new FakeController();
            _client = new Client(null, controller, _netClient);
            _client.Init(null, controller.CanvasSize);

            _netServer = new FakeNetServer();
            var controllerServer = new FakeController();
            _server = new Server(_netServer);
            _server.Init(null, controllerServer.CanvasSize);

            _netServer.Connections.Add(new FakeNetConnection
            {
                EndPoint = _netClient,
                AverageRoundtripTime = 0.2f
            });
            _netClient.Connections.Add(new FakeNetConnection
            {
                EndPoint = _netServer,
                AverageRoundtripTime = 0.2f
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client = null;
            _netClient = null;
            _server = null;
            _netServer = null;

            NetTime.SetTime(0);
        }

        [TestMethod]
        public void ClientTankFiresOnce()
        {
            for (int i = 0; i < 60; i++)
            {
                NetTime.SetTime(i / 60f);

                ServerMessage data = new ServerMessage
                {
                    TankData = new TankData[]
                    {
                        new TankData
                        {
                            OwnerId = _client.ServerId,
                            GunFiredTime = -1,
                            Transform = new Transform2(),
                            WorldTransform = new Transform2(),
                            Velocity = Transform2.CreateVelocity(),
                            WorldVelocity = Transform2.CreateVelocity(),
                            TurretTransform = new Transform2(),
                            TurretWorldTransform = new Transform2()
                        }
                    }
                };
                var message = ((FakeNetOutgoingMessage)NetworkHelper.PrepareMessage(_client, data)).ToIncomingMessage(0);
                message.MessageType = NetIncomingMessageType.Data;
                _netClient.Messages.Enqueue(message);

                ((FakeController)_client.Controller).Input.KeyCurrent.Add(Key.Space);
                _client.Step();

                Assert.IsTrue(_client.Scene.GetAll().OfType<Bullet>().Count() <= 1);
            }
        }

        /// <summary>
        /// See if the fake net implementation can pass messages.
        /// </summary>
        [TestMethod]
        public void ServerRecieveClient()
        {
            _client.SendMessage(new ClientMessage { Input = new TankInput { FireGun = true } });

            NetTime.SetTime(NetTime.Now + 1);
            _netClient.Connections.ForEach(item => item.SetTime(NetTime.Now));

            ClientMessage clientMessage = NetworkHelper.ReadMessage<ClientMessage>(_netServer.ReadMessage());
            Assert.IsTrue(clientMessage.Input.FireGun);
        }

        [TestMethod]
        public void ClientRecieveServer()
        {
            _server.SendMessage(new ServerMessage { SceneTime = 1 });
            ServerMessage serverMessage = NetworkHelper.ReadMessage<ServerMessage>(_netClient.ReadMessage());
            Assert.IsTrue(serverMessage.SceneTime == 1);
        }

        [TestMethod]
        public void ServerRecieveClientWithLatency()
        {
            FakeNetPeer reader = _netClient;

            _netServer.Connections[0].Latency = 1;
            _server.SendMessage(new ServerMessage());

            Assert.IsTrue(reader.ReadMessage() == null);

            AdvanceTime(0.5);
            Assert.IsTrue(reader.ReadMessage() == null);

            AdvanceTime(0.5);
            Assert.IsTrue(reader.ReadMessage() != null);
        }

        [TestMethod]
        public void ClientRecieveServerWithLatency()
        {
            FakeNetPeer reader = _netServer;

            _netClient.Connections[0].Latency = 1;
            _client.SendMessage(new ClientMessage());

            Assert.IsTrue(reader.ReadMessage() == null);

            AdvanceTime(0.5);
            Assert.IsTrue(reader.ReadMessage() == null);

            AdvanceTime(0.5);
            Assert.IsTrue(reader.ReadMessage() != null);
        }

        public void AdvanceTime(double amount)
        {
            NetTime.SetTime(NetTime.Now + amount);
            foreach (var netPeer in _netPeers)
            {
                netPeer.Connections.ForEach(item => item.SetTime(NetTime.Now));
            }
        }
    }
}
