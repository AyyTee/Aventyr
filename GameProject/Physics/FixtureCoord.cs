using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Game.Common;
using Game.Serialization;
using OpenTK;

namespace Game.Physics
{
    /// <summary>
    /// A coordinate defined by its position on a edge in a Fixture
    /// </summary>
    [DataContract]
    public class FixtureCoord : IShallowClone<FixtureCoord>, IPolygonCoord
    {
        public Fixture Fixture { get; private set; }
        /// <summary>
        /// Return the Actor linked to the Body that is linked to the Fixture being intersected.
        /// </summary>
        public Actor Actor { get { return BodyExt.GetData(Fixture.Body).Actor; } }

        int _edgeIndex;
        /// <summary>Index value of edge in Fixture's Shape.</summary>
        public int EdgeIndex
        {
            get { return _edgeIndex; }
            set
            {
                switch (Fixture.Shape.ShapeType)
                {
                    case ShapeType.Polygon:
                        PolygonShape shape = (PolygonShape)Fixture.Shape;
                        Debug.Assert(value >= 0 && value < shape.Vertices.Count, "EdgeIndex must have a value between [0, vertex count).");
                        break;

                    case ShapeType.Circle:
                        Debug.Assert(value == 0, "EdgeIndex cannot be assigned a value other than 0 for a circle fixture.");
                        break;
                }
                _edgeIndex = value;
            }
        }

        float _edgeT;
        /// <summary>
        /// Value between [0,1) that represents the position along the edge.
        /// </summary>
        public float EdgeT
        {
            get { return _edgeT; }
            set 
            {
                Debug.Assert(value >= 0 && value <= 1, "EdgeT must have a value between [0, 1].");
                _edgeT = value;
            }
        }

        /// <summary>
        /// EdgeIndex + EdgeT
        /// </summary>
        public float EdgeIndexT { get { return (float)EdgeIndex + EdgeT; } }

        public FixtureCoord(Fixture fixture, int edgeIndex = 0, float edgeT = 0)
        {
            Fixture = fixture;
            EdgeT = edgeT;
            EdgeIndex = edgeIndex;
        }

        public FixtureCoord ShallowClone()
        {
            return new FixtureCoord(Fixture, EdgeIndex, EdgeT);
        }

        IPolygonCoord IShallowClone<IPolygonCoord>.ShallowClone()
        {
            return ShallowClone();
        }

        public LineF GetEdge()
        {
            PolygonShape shape = (PolygonShape)Fixture.Shape;
            Vector2 v0, v1, scaleFactor;
            v0 = (Vector2)shape.Vertices[EdgeIndex];
            v1 = (Vector2)shape.Vertices[(EdgeIndex + 1) % shape.Vertices.Count];
            scaleFactor = Actor.GetTransform().Scale;
            return new LineF(v0, v1);
        }

        public Vector2 GetPosition()
        {
            switch (Fixture.Shape.ShapeType)
            {
                case ShapeType.Polygon:
                    LineF line = GetEdge();
                    return line.Lerp(EdgeT);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
