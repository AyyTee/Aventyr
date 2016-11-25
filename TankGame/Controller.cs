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

namespace TankGame
{
    public class Controller : Game.Controller
    {
        INetController _netController;

        public Controller(Window window, string[] args)
            : base(window)
        {
            if (args.Length >= 2)
            {
                if (args[0] == "server")
                {
                    _netController = new Server(int.Parse(args[1]));
                }
                else if (args[0] == "client")
                {
                    _netController = new Client(int.Parse(args[1]), int.Parse(args[2]), this);
                }
            }
            else
            {
                _netController = new Client(111, 112, this);
            }
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
