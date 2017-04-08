using Game;
using Game.Portals;
using Lidgren.Network;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using OpenTK.Input;
using System.Threading;
using TankGame.Network;
using System.Diagnostics;
using Game.Rendering;

namespace TankGame
{
    public class TankGameController : IGameController
    {
        readonly INetController _netController;
        readonly VirtualWindow _window;

        public TankGameController(VirtualWindow window)
        {
            _window = window;
            //_netController = netController;


            var config = NetworkHelper.GetDefaultConfig();
            config.Port = 1001;
            _netController = new Server(_window, new NetServer(config));


            //var config = NetworkHelper.GetDefaultConfig();
            //config.Port = int.Parse(args[1]);

            //NetClient client = new NetClient(config);

            //var serverAddress = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), int.Parse(args[2]));

            //_netController = new Client(serverAddress, this, client);





            _netController.Init();
        }

        public void Update()
        {
            _netController.Step();
        }

        public void Render()
        {

        }
    }
}
