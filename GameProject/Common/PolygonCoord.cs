using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public class PolygonCoord : IPolygonCoord
    {
        /// <summary>
        /// Index of the edge within the polygon.
        /// </summary>
        [DataMember]
        public int EdgeIndex { get; set; }
        float _edgeT;
        /// <summary>
        /// Value between [0,1] that represents the position along the edge.
        /// </summary>
        [DataMember]
        public float EdgeT
        {
            get { return _edgeT; }
            set
            {
                Debug.Assert(_edgeT >= 0 && _edgeT <= 1);
                _edgeT = value;
            }
        }

        public PolygonCoord()
            : this(0, 0)
        {
        }

        public PolygonCoord(int edgeIndex, float edgeT)
        {
            EdgeIndex = edgeIndex;
            EdgeT = edgeT;
        }

        public IPolygonCoord ShallowClone()
        {
            return new PolygonCoord(EdgeIndex, EdgeT);
        }
    }
}
