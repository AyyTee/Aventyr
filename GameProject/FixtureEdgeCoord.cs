using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;
using OpenTK;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Game
{
    /// <summary>
    /// A coordinate defined it's position on a edge in a Fixture
    /// </summary>
    [DataContract]
    public class FixtureEdgeCoord : IShallowClone<FixtureEdgeCoord>
    {
        public Fixture Fixture { get; private set; }
        /// <summary>
        /// Return the Actor linked to the Body that is linked to the Fixture being intersected.
        /// </summary>
        public IActor Actor { get { return BodyExt.GetUserData(Fixture.Body).Actor; } }
        private int _edgeIndex;
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

        private float _edgeT;
        /// <summary>
        /// Value between [0,1) the represents the position along the edge.
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

        public FixtureEdgeCoord(Fixture fixture, int edgeIndex = 0, float edgeT = 0)
        {
            Fixture = fixture;
            EdgeT = edgeT;
            EdgeIndex = edgeIndex;
        }

        public FixtureEdgeCoord ShallowClone()
        {
            return new FixtureEdgeCoord(Fixture, EdgeIndex, EdgeT);
        }

        public Line GetEdge()
        {
            PolygonShape shape = (PolygonShape)Fixture.Shape;
            Vector2 v0, v1, scaleFactor;
            v0 = Vector2Ext.ConvertTo(shape.Vertices[EdgeIndex]);
            v1 = Vector2Ext.ConvertTo(shape.Vertices[(EdgeIndex + 1) % shape.Vertices.Count]);
            scaleFactor = Actor.GetTransform().Scale;
            /*return new Line(
                new Vector2(v0.X / scaleFactor.X, v0.Y / scaleFactor.Y),
                new Vector2(v1.X / scaleFactor.X, v1.Y / scaleFactor.Y)
                );*/
            return new Line(v0, v1);
        }

        public Line GetWorldEdge()
        {
            Transform2 transform = BodyExt.GetTransform(Fixture.Body);
            return GetEdge().Transform(transform.GetMatrix());
        }

        public Vector2 GetNormal()
        {
            return GetEdge().GetNormal();
        }

        public Vector2 GetWorldNormal()
        {
            return GetWorldEdge().GetNormal();
        }

        public Vector2 GetPosition()
        {
            switch (Fixture.Shape.ShapeType)
            {
                case ShapeType.Polygon:
                    Line line = GetEdge();
                    return line.Lerp(EdgeT);

                default:
                    throw new NotImplementedException();
            }
        }

        public Vector2 GetWorldPosition()
        {
            var transform = new FarseerPhysics.Common.Transform();
            Fixture.Body.GetTransform(out transform);
            Matrix4 matTransform = Matrix4Ext.ConvertTo(transform);
            return Vector2Ext.Transform(GetPosition(), matTransform);
        }

        public Transform2 GetTransform()
        {
            Transform2 transform = new Transform2();
            transform.Position = GetPosition();
            transform.Rotation = -(float)MathExt.AngleVector(GetWorldNormal()) - Actor.GetTransform().Rotation;
            return transform;
        }

        public Transform2 GetWorldTransform()
        {
            return GetTransform().Transform(Actor.GetWorldTransform());
        }
    }
}
