using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;

namespace Game
{
    public interface IGetVelocity
    {
        /// <summary>Returns a copy of the local velocity.</summary>
        Transform2 GetVelocity();
    }
}
