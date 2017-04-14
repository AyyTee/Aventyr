using Game.Portals;
using Game.Rendering;
using System.Collections.Generic;

namespace Game
{
    public interface IScene : ITime
    {
        /// <summary>Perform one simulation step.</summary>
        /// <param name="stepSize">Size of simulation step in seconds.</param>
        /// <returns>Current time in the simulation.</returns>
        void Step(float stepSize);
        List<IPortal> GetPortalList();
        List<IPortalable> GetPortalableList();
        /// <summary>Returns a list of all the objects that exist within an IScene.</summary>
        List<ISceneObject> GetAll();
        ICamera2 GetCamera();
    }
}
