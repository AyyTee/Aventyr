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

namespace TankGameTests
{
    [TestClass]
    public class UnitTest1
    {
        Client _client;
        FakeNetClient _netClient;
        Server _server;
        FakeNetServer _netServer;

        [TestInitialize]
        public void Initialize()
        {
            NetTime.AutomaticTimeKeeping = false;
            _netClient = new FakeNetClient();
            FakeController controller = new FakeController();
            _client = new Client(null, controller, _netClient);
            _client.Init(null, controller.CanvasSize);

            _netServer = new FakeNetServer();
            FakeController controllerServer = new FakeController();
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
        }

        [TestMethod]
        public void ClientTankFiresOnce()
        {
            for (int i = 0; i < 60; i++)
            {
                NetTime.SetTime(i / 60f);

                ServerMessage data = new ServerMessage();
                data.TankData = new TankData[]
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
                        TurretWorldTransform = new Transform2(),
                    },
                };
                var message = ((FakeNetOutgoingMessage)NetworkHelper.PrepareMessage(_client, data)).ToIncomingMessage();
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
        public void FakeNetTest0()
        {
            _client.SendMessage(new ClientMessage { Input = new TankInput { FireGun = true } });
            ClientMessage clientMessage = NetworkHelper.ReadMessage<ClientMessage>(_netServer.ReadMessage());
            Assert.IsTrue(clientMessage.Input.FireGun);

            _server.SendMessage(new ServerMessage { SceneTime = 1 });
            ServerMessage serverMessage = NetworkHelper.ReadMessage<ServerMessage>(_netClient.ReadMessage());
            Assert.IsTrue(serverMessage.SceneTime == 1);
        }
    }
}
