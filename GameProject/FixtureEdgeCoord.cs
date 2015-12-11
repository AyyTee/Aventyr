using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;
using OpenTK;
using System.Diagnostics;

namespace Game
{
    /// <summary>
    /// A coordinate defined it's position on a edge in a Fixture
    /// </summary>
    public class FixtureEdgeCoord
    {
        public Fixture Fixture { get; private set; }
        /// <summary>
        /// Return the Entity linked to the Body that is linked to the Fixture being intersected.
        /// </summary>
        public Entity Entity
        {
            get 
            {
                return BodyExt.GetUserData(Fixture.Body).LinkedEntity;
            }
        }
        private int _edgeIndex;
        /// <summary>
        /// Index value of edge in Fixture's Shape.
        /// </summary>
        public int EdgeIndex
        {
            get { return _edgeIndex; }
            set
            {
                switch (Fixture.ShapeType)
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
        public float EdgeIndexT { get {return (float)EdgeIndex + EdgeT;} }

        public FixtureEdgeCoord(Fixture fixture, int edgeIndex = 0, float edgeT = 0)
        {
            Fixture = fixture;
            EdgeT = edgeT;
            EdgeIndex = edgeIndex;
        }

        public Line GetEdge()
        {
            PolygonShape shape = (PolygonShape)Fixture.Shape;
            Line line = new Line(
                Vector2Ext.ConvertTo(shape.Vertices[EdgeIndex]),
                Vector2Ext.ConvertTo(shape.Vertices[(EdgeIndex + 1) % shape.Vertices.Count])
                );
            return line;
        }

        public Line GetWorldEdge()
        {
            Line line = GetEdge();
            var transform = new FarseerPhysics.Common.Transform();
            Fixture.Body.GetTransform(out transform);
            Matrix4 matTransform = Matrix4Ext.ConvertTo(transform);
            line.Transform(matTransform);
            return line;
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
            switch (Fixture.ShapeType)
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

        public Transform2D GetTransform()
        {
            Transform2D transform = new Transform2D();

            transform.Position = GetPosition();//Entity.Transform.WorldToLocal(GetPosition());
            transform.Parent = Entity.Transform;
            transform.Rotation = -(float)MathExt.AngleVector(GetWorldNormal()) - Entity.Transform.Rotation;
            return transform;
        }

        public Transform2D GetWorldTransform()
        {
            Transform2D transform = new Transform2D();

            transform.Position = GetWorldPosition();//Entity.Transform.WorldToLocal(GetPosition());
            transform.Rotation = -(float)MathExt.AngleVector(GetWorldNormal()) - Entity.Transform.Rotation;
            return transform;
        }
    }
}
