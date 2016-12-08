using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame.Network
{
    public interface INetObject
    {
        int ServerId { get; set; }
        int LocalId { get; set; }
    }
}
