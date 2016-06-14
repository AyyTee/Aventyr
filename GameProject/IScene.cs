using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IScene
    {
        float Time { get; }
        /// <summary>Perform one simulation step.</summary>
        /// <param name="stepSize">Size of simulation step in seconds.</param>
        /// <returns>Current time in the simulation.</returns>
        void Step(float stepSize);
        List<IPortal> GetPortalList();
        /// <summary>Returns a list of all the objects that exist within an IScene.</summary>
        List<ISceneObject> GetAll();
    }
}
