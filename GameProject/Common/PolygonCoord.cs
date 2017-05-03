using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Game.Serialization;

namespace Game.Common
{
    [DataContract]
    public class PolygonCoord : IPolygonCoord
    {
        /// <summary>
        /// Index of the edge within the polygon.
        /// </summary>
        [DataMember]
        public int EdgeIndex { get; private set; }

        /// <summary>
        /// Value between [0,1] that represents the position along the edge.
        /// </summary>
        [DataMember]
        public float EdgeT { get; private set; }

        public PolygonCoord(int edgeIndex, float edgeT)
        {
            Debug.Assert(edgeT >= 0 && edgeT <= 1);
            EdgeIndex = edgeIndex;
            EdgeT = edgeT;
        }

        public IPolygonCoord ShallowClone() => (PolygonCoord)MemberwiseClone();
    }
}
