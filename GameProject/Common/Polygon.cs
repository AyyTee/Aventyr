using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    /// <summary>
    /// Simple implementation of IPolygon.
    /// </summary>
    public class Polygon : IPolygon
    {
        public IList<Vector2> Vertices { get; set; }
        public Vector2 this[int index]
        {
            get { return Vertices[index]; }
            set
            {
                Debug.Assert(Vector2Ext.IsReal(value));
                Vertices[index] = value;
            }
        }

        public Polygon()
            : this(new List<Vector2>())
        {
        }

        public Polygon(IList<Vector2> vertices)
        {
            Vertices = vertices;
        }
    }
}
