using Game;
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
    public class Controller : Game.Controller
    {
        INetController _netController;

        public Controller(Window window, FakeNetServer connectTo)
            : base(window)
        {
            Client client = new Client(new IPEndPoint(0, 0), this, new FakeNetClient());
        }

        public Controller(Window window, ICollection<FakeNetClient> connectTo)
            : base(window)
        {
            Server server = new Server(new FakeNetServer());
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _netController.Init(Renderer, CanvasSize);
        }

        public override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            _netController.Step();
        }
    }
}
