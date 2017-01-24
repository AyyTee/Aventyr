using System.Runtime.Serialization;
using Game.Common;

namespace Game
{
    [DataContract]
    public class WallCoord : IPolygonCoord
    {
        [DataMember]
        public IWall Wall { get; private set; }
        [DataMember]
        public int EdgeIndex { get; private set; }
        [DataMember]
        public float EdgeT { get; private set; }

        public WallCoord(IWall wall, int edgeIndex, float edgeT)
        {
            EdgeIndex = edgeIndex;
            EdgeT = edgeT;
            Wall = wall;
        }

        public WallCoord(IWall wall, IPolygonCoord coord)
            : this(wall, coord.EdgeIndex, coord.EdgeT)
        {
        }

        public IPolygonCoord ShallowClone()
        {
            return new WallCoord(Wall, EdgeIndex, EdgeT);
        }
    }
}
