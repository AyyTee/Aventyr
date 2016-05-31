using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
