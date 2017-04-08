using Game;
using Game.Rendering;
using Lidgren.Network;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TankGame.Network;

namespace TankGameTestFramework
{
    public class FrameworkController : IGameController
    {
        INetController _netController;

        public FrameworkController(IVirtualWindow window, FakeNetServer connectTo)
        {
            Client client = new Client(window, new IPEndPoint(0, 0), this, new FakeNetClient());
        }

        public FrameworkController(IVirtualWindow window, ICollection<FakeNetClient> connectTo)
        {
            Server server = new Server(window, new FakeNetServer());

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
