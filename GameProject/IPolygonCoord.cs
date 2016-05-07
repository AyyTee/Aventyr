using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IPolygonCoord : IShallowClone<IPolygonCoord>
    {
        /// <summary>
        /// Index of the edge within the polygon. 
        /// If the polygon has no vertices (such as is the case with a circle) then zero is returned.
        /// Otherwise this value is within the range [0,x) where x is the number of vertices. 
        /// </summary>
        int EdgeIndex { get; }
        /// <summary>
        /// Value between [0,1) that represents the position along the edge.
        /// </summary>
        float EdgeT { get; }
        /// <summary>
        /// Returns Transform local to polygon. Rotation represents the normal vector.  Scale is always (1,1).
        /// </summary>
        /// <returns></returns>
        //Transform2 GetTransform();
    }
}
