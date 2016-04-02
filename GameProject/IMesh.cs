using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IMesh
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
