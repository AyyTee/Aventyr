using System.Collections.Generic;
using Game.Serialization;
using System.Linq;

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

    public static class IMeshEx
    {
        public static bool IsValid(this IMesh mesh)
        {
            var vertices = mesh.GetVertices();
            var indices = mesh.GetIndices();
            return indices.Count % 3 == 0 &&
                (indices.Count == 0 ||
                (indices.Max() < vertices.Count() && indices.Min() >= 0));
        }
    }
}
