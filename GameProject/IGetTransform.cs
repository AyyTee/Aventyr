using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;

namespace Game
{
    public interface IGetTransform
    {
        /// <summary>Returns a copy of the local transform.</summary>
        Transform2 GetTransform();
    }
}
