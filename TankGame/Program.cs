using Game;
using Game.Common;
using Game.Rendering;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TankGame.Network;

namespace TankGame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new[] { "server", "45619" };
            }
            
            var resourceController = new ResourceController(new Size(1000, 800), args[0]);
            var window = new VirtualWindow(resourceController);
            window.CanvasSize = resourceController.ClientSize;


            var config = NetworkHelper.GetDefaultConfig();

            int serverPort;
            int.TryParse(args[1], out serverPort);

            if (args[0] == "client")
            {
                int clientPort;
                int.TryParse(args[2], out clientPort);
                config.Port = clientPort;

                var serverAddress = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), serverPort);
                resourceController.AddController(new Client(window, serverAddress, new NetClient(config)));
            }
            else if (args[0] == "server")
            {
                config.Port = serverPort;
                resourceController.AddController(new Server(window, new NetServer(config)));
            }

            resourceController.Run();
        }
    }
}
