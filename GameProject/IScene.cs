using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IScene
    {
        /// <summary>
        /// Perform one simulation step.
        /// </summary>
        /// <param name="stepSize">Size of simulation step in seconds.</param>
        void Step(float stepSize);
    }
}
