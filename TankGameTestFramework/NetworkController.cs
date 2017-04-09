using Game;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGameTestFramework
{
    public class NetworkController : IUpdateable
    {
        public List<FakeNetConnection> Connections { get; private set; } = new List<FakeNetConnection>();

        public NetworkController()
        {
        }

        public void Update()
        {
            NetTime.SetTime(NetTime.Now + 1 / 60.0);
            foreach (var connection in Connections)
            {
                connection.SetTime(NetTime.Now);
            }
        }
    }
}
