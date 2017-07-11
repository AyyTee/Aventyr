using System;
using NUnit.Framework;
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
using OpenTK;

namespace TankGameTests
{
    [TestFixture]
    public class TankGameTests
    {
        Client _client;
        FakeNetClient _netClient;
        Server _server;
        FakeNetServer _netServer;
        FakeVirtualWindow _clientWindow;
        FakeVirtualWindow _serverWindow;

        FakeNetPeer[] _netPeers => new FakeNetPeer[] { _netClient, _netServer };

        [SetUp]
        public void Initialize()
        {
            NetTime.AutomaticTimeKeeping = false;
            NetTime.SetTime(0);

            _netClient = new FakeNetClient(TankGameTestFramework.Program.ClientUniqueId);
            _clientWindow = new FakeVirtualWindow();
            _client = new Client(_clientWindow, null, _netClient);
            _client.Init();

            _netServer = new FakeNetServer(TankGameTestFramework.Program.ServerUniqueId);
            _serverWindow = new FakeVirtualWindow();
            _server = new Server(_serverWindow, _netServer);
            _server.Init();

            _netServer.Connections.Add(new FakeNetConnection(_netServer, _netClient)
            {
                Latency = 0.1f
            });
            _netClient.Connections.Add(new FakeNetConnection(_netClient, _netServer)
            {
                Latency = 0.1f
            });
        }

        [TearDown]
        public void Cleanup()
        {
            NetTime.SetTime(0);
        }

        [Test]
        public void ClientTankFiresOnce()
        {
            for (int i = 0; i < 60; i++)
            {
                NetTime.SetTime(i / 60f);

                MessageToClient data = new MessageToClient
                {
                    TankData = new TankData[]
                    {
                        new TankData
                        {
                            OwnerId = _client.ServerSideId,
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
                var message = new FakeNetIncomingMessage(
                    ((FakeNetOutgoingMessage)NetworkHelper.PrepareMessage(_client, data)),
                    _netServer.Connections[0]);
                _netServer.EnqueueArrivedMessage(message);

                _clientWindow.Update(new HashSet<Key>() { Key.Space }, new HashSet<MouseButton>(), new Vector2());
                _client.Update(1 / 60.0);

                Assert.IsTrue(_client.Scene.GetAll().OfType<Bullet>().Count() <= 1);
            }
        }

        /// <summary>
        /// See if the fake net implementation can pass messages.
        /// </summary>
        [Test]
        public void ServerRecieveClient()
        {
            _client.SendMessage(new MessageToServer { Input = new TankInput { FireGun = true } });

            AdvanceTime(1);

            MessageToServer clientMessage = NetworkHelper.ReadMessage<MessageToServer>(_netServer.ReadMessage());
            Assert.IsTrue(clientMessage.Input.FireGun);
        }

        [Test]
        public void ClientRecieveServer()
        {
            _server.SendMessage(new MessageToClient { SceneTime = 1 });
            AdvanceTime(2);
            MessageToClient serverMessage = NetworkHelper.ReadMessage<MessageToClient>(_netClient.ReadMessage());
            Assert.IsTrue(serverMessage.SceneTime == 1);
        }

        [Test]
        public void ServerRecieveClientWithLatency()
        {
            FakeNetPeer reader = _netClient;

            _netServer.Connections[0].Latency = 1;
            _netClient.ServerConnection.Latency = 1;
            _server.SendMessage(new MessageToClient());

            Assert.IsTrue(reader.ReadMessage() == null);

            AdvanceTime(0.5);
            Assert.IsTrue(reader.ReadMessage() == null);

            AdvanceTime(0.5);
            Assert.IsTrue(reader.ReadMessage() != null);
        }

        [Test]
        public void ClientRecieveServerWithLatency()
        {
            FakeNetPeer reader = _netServer;

            _netServer.Connections[0].Latency = 1;
            _netClient.ServerConnection.Latency = 1;
            _client.SendMessage(new MessageToServer());

            Assert.IsTrue(reader.ReadMessage() == null);

            AdvanceTime(0.5);
            Assert.IsTrue(reader.ReadMessage() == null);

            AdvanceTime(0.5);
            Assert.IsTrue(reader.ReadMessage() != null);
        }

        [Test]
        public void ClientTankDoesNotJitter()
        {
            double timeDelta = 1 / 60.0;

            _netServer.Connections.ForEach(item => item.Latency = 0.5);
            _netClient.Connections.ForEach(item => item.Latency = 0.5);

            var client = new Client(_clientWindow, null, _netClient);
            var server = new Server(_serverWindow, _netServer);

            _netServer.EnqueueArrivedMessage(new FakeNetIncomingMessage(new FakeNetOutgoingMessage(), _netServer.Connections[0], NetIncomingMessageType.StatusChanged));
            AdvanceTime(1);

            var prevPosition = new Vector2();
            var prevVelocity = new Vector2();
            for (int i = 0; i < 200; i++)
            {
                if (i < 100)
                {
                    _clientWindow.Update(new HashSet<Key>() { Key.W }, new HashSet<MouseButton>(), new Vector2());
                }

                server.Update(timeDelta);
                client.Update(timeDelta);

                if (prevPosition != new Vector2())
                {
                    Assert.IsTrue((prevPosition + prevVelocity / 60 - client.OwnedTank.WorldTransform.Position).Length < 0.0001f);
                }
                prevPosition = client.OwnedTank?.WorldTransform.Position ?? new Vector2();
                prevVelocity = client.OwnedTank?.WorldVelocity.Position ?? new Vector2();

                AdvanceTime(timeDelta);
            }
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
