using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface ITime
    {
        /// <summary>
        /// Time elapsed in seconds.
        /// </summary>
        double Time { get; }
    }
}
