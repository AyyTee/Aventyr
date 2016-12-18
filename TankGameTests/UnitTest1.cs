using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TankGame.Network;
using System.Collections.Generic;
using Lidgren.Network;
using TankGame;
using Game;
using OpenTK.Input;
using System.Linq;

namespace TankGameTests
{
    [TestClass]
    public class UnitTest1
    {
        Client Client;
        FakeNetClient NetClient;
        Server Server;
        FakeNetServer NetServer;

        [TestInitialize]
        public void Initialize()
        {
            NetTime.AutomaticTimeKeeping = false;
            NetClient = new FakeNetClient();
            FakeController controller = new FakeController();
            Client = new Client(null, controller, NetClient);
            Client.Init(null, controller.CanvasSize);

            NetServer = new FakeNetServer();
            FakeController controllerServer = new FakeController();
            Server = new Server(NetServer);
            Server.Init(null, controllerServer.CanvasSize);

            NetServer.Connections.Add(new FakeNetConnection
            {
                EndPoint = NetClient,
                AverageRoundtripTime = 0.2f
            });
            NetClient.Connections.Add(new FakeNetConnection
            {
                EndPoint = NetServer,
                AverageRoundtripTime = 0.2f
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            Client = null;
            NetClient = null;
            Server = null;
            NetServer = null;
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
                        OwnerId = Client.ServerId,
                        GunFiredTime = -1,
                        Transform = new Transform2(),
                        WorldTransform = new Transform2(),
                        Velocity = Transform2.CreateVelocity(),
                        WorldVelocity = Transform2.CreateVelocity(),
                        TurretTransform = new Transform2(),
                        TurretWorldTransform = new Transform2(),
                    },
                };
                var message = ((FakeNetOutgoingMessage)NetworkHelper.PrepareMessage(Client, data)).ToIncomingMessage();
                message.MessageType = NetIncomingMessageType.Data;
                NetClient.Messages.Enqueue(message);

                ((FakeController)Client.Controller).Input.KeyCurrent.Add(Key.Space);
                Client.Step();

                Assert.IsTrue(Client.Scene.GetAll().OfType<Bullet>().Count() <= 1);
            }
        }

        /// <summary>
        /// See if the fake net implementation can pass messages.
        /// </summary>
        [TestMethod]
        public void FakeNetTest0()
        {
            Client.SendMessage(new ClientMessage { Input = new TankInput { FireGun = true } });
            ClientMessage clientMessage = NetworkHelper.ReadMessage<ClientMessage>(NetServer.ReadMessage());
            Assert.IsTrue(clientMessage.Input.FireGun);

            Server.SendMessage(new ServerMessage { SceneTime = 1 });
            ServerMessage serverMessage = NetworkHelper.ReadMessage<ServerMessage>(NetClient.ReadMessage());
            Assert.IsTrue(serverMessage.SceneTime == 1);
        }
    }
}
