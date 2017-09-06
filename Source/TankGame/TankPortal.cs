using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Common;
using Game.Portals;
using TankGame.Network;

namespace TankGame
{
    public class TankPortal : FixturePortal, INetObject
    {
        public int? ServerId { get; set; }

        public TankPortal(Scene scene) : base(scene)
        {
        }

        public TankPortal(Scene scene, IWall parent, IPolygonCoord position) : base(scene, parent, position)
        {
        }
    }
}
