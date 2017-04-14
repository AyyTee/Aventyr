using Game;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Rendering;

namespace TankGame.Network
{
    public interface INetController : IUpdateable
    {
        void Init();
        string Name { get; }
        INetPeer Peer { get; }
        int StepCount { get; }
        int MessagesSent { get; set; }
    }
}
