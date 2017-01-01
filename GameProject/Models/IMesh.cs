using System.Collections.Generic;
using Game.Serialization;

namespace Game.Models
{
    public interface IMesh : IShallowClone<IMesh>
    {
        /// <summary>
        /// Get a shallow copy of list containing vertices in the mesh.
        /// </summary>
        List<Vertex> GetVertices();
        /// <summary>
        /// Get a shallow copy of list containing indices in the mesh.  Every 3 indices defines a triangle.
        /// </summary>
        List<int> GetIndices();
    }
}
