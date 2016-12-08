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
        FakeController Controller;
        FakeNetClient NetClient;

        [TestInitialize]
        public void Initialize()
        {
            NetTime.AutomaticTimeKeeping = false;
            NetClient = new FakeNetClient();
            Controller = new FakeController();
            Client = new Client(null, Controller, NetClient);
            Client.Init(null, Controller.CanvasSize);

            var connections = new List<INetConnection>();
            var c = new FakeNetConnection();
            c.AverageRoundtripTime = 0.2f;
            connections.Add(c);
            NetClient.Connections = connections;
        }

        [TestCleanup]
        public void Cleanup()
        {
            Client = null;
            Controller = null;
            NetClient = null;
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
                        ClientId = Client.RemoteId,
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

                Controller.Input.KeyCurrent.Add(Key.Space);
                Client.Step();

                Assert.IsTrue(Client.Scene.GetAll().OfType<Bullet>().Count() <= 1);
            }
            

            Client.Step();
        }
    }
}
