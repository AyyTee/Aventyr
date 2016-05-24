using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class WallCoord : IPolygonCoord
    {
        public IWall Wall { get; set; }
        public int EdgeIndex { get; set; }
        public float EdgeT { get; set; }

        public WallCoord(IWall wall, int edgeIndex, float edgeT)
        {
            EdgeIndex = edgeIndex;
            EdgeT = edgeT;
            Wall = wall;
        }

        public WallCoord(IWall wall, PolygonCoord coord)
            : this(wall, coord.EdgeIndex, coord.EdgeT)
        {
        }

        public IPolygonCoord ShallowClone()
        {
            return new WallCoord(Wall, EdgeIndex, EdgeT);
        }
    }
}
