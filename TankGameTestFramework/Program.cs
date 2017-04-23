using Game;
using Game.Common;
using Game.Rendering;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TankGame;
using TankGame.Network;

namespace TankGameTestFramework
{
    public class Program
    {
        public const long ServerUniqueId = 11111;
        public const long ClientUniqueId = 22222;

        static void Main(string[] args)
        {
            bool splitScreen = true;
            //splitScreen = false;
            if (splitScreen)
            {
                RunSplitScreen();
            }
            else
            {
                Run();
            }
        }

        static void RunSplitScreen()
        {
            var controller = new ResourceController(new Vector2i(1000, 800));

            var center = new Vector2i(controller.ClientSize.X / 2, controller.ClientSize.Y / 2);

            //var random = new Random(123);
            var server = new FakeNetServer(ServerUniqueId);
            var client = new FakeNetClient(ClientUniqueId);
            server.Connections.Add(new FakeNetConnection(server, client) { Latency = 0.5 });
            client.Connections.Add(new FakeNetConnection(client, server) { Latency = 0.5 });

            server.EnqueueArrivedMessage(new FakeNetIncomingMessage(new FakeNetOutgoingMessage(), server.Connections[0], NetIncomingMessageType.StatusChanged));

            var windowClient = new VirtualWindow(controller)
            {
                CanvasSize = center,
                CanvasPosition = new Vector2i(0, center.Y)
            };
            controller.AddController(new Server(windowClient, server));

            var windowServer = new VirtualWindow(controller)
            {
                CanvasSize = center,
                CanvasPosition = center
            };
            controller.AddController(new Client(windowServer, null, client));

            var netController = new NetworkController();
            netController.Connections.AddRange(server.Connections);
            netController.Connections.AddRange(client.Connections);
            controller.AddController(netController);

            controller.Run();
        }

        static void Run()
        {
            const int serverPort = 45619;
            const int clientCount = 1;

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var server = new Process();
            server.StartInfo.FileName = "TankGame.exe";
            server.StartInfo.WorkingDirectory = path;
            server.StartInfo.Arguments = $"server {serverPort}";
            server.Start();

            var clients = new Process[clientCount];
            for (int i = 0; i < clientCount; i++)
            {
                Thread.Sleep(1000);
                int clientPort = serverPort + 1 + i;
                clients[i] = new Process();
                clients[i].StartInfo.FileName = "TankGame.exe";
                clients[i].StartInfo.WorkingDirectory = path;
                clients[i].StartInfo.Arguments = $"client {serverPort} {clientPort}";
                clients[i].Start();
            }

            server.WaitForExit();
            foreach (Process process in clients)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
        }
    }
}
