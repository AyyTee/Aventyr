using OpenTK;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// A closed polygon that acts as a wall.
    /// </summary>
    public interface IWall
    {
        IList<Vector2> Vertices { get; }
        /// <summary>
        /// World coordinates for vertices.
        /// </summary>
        IList<Vector2> GetWorldVertices();
    }
}
