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
    public class FixtureIntersection
    {
        private Fixture _fixture;

        public Fixture Fixture
        {
            get { return _fixture; }
        }
        /// <summary>
        /// Return the Entity linked to the Body that is linked to the Fixture being intersected.
        /// </summary>
        public Entity Entity
        {
            get 
            {
                BodyUserData userData = BodyExt.GetUserData(Fixture.Body);//(BodyUserData)Fixture.Body.UserData;
                return userData.LinkedEntity;
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
                        {
                            PolygonShape shape = (PolygonShape)_fixture.Shape;
                            Debug.Assert(value >= 0 && value < shape.Vertices.Count, "EdgeIndex must have a value between [0, vertex count).");
                            break;
                        }
                    case ShapeType.Circle:
                        {
                            Debug.Assert(value == 0, "EdgeIndex cannot be assigned a value other than 0 for a circle fixture.");
                            break;
                        }
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

        public FixtureIntersection(Fixture fixture, int edgeIndex = 0, float edgeT = 0)
        {
            _fixture = fixture;
            EdgeT = edgeT;
            EdgeIndex = edgeIndex;
        }

        public Line GetEdge()
        {
            PolygonShape shape = (PolygonShape)_fixture.Shape;
            Line line = new Line(
                VectorExt2.ConvertTo(shape.Vertices[EdgeIndex]),
                VectorExt2.ConvertTo(shape.Vertices[(EdgeIndex + 1) % shape.Vertices.Count])
                );
            return line;
        }

        public Line GetWorldEdge()
        {
            Line line = GetEdge();
            var transform = new FarseerPhysics.Common.Transform();
            _fixture.Body.GetTransform(out transform);
            Matrix4 matTransform = MatrixExt4.ConvertTo(transform);
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
            switch (_fixture.ShapeType)
            {
                case ShapeType.Polygon:
                    {
                        Line line = GetWorldEdge();
                        return line.Lerp(EdgeT);
                    }
                default:
                    {
                        return new Vector2();
                    }
                /*case ShapeType.Circle:
                    {
                        return 
                    }*/
            }
        }

        public Transform2D GetTransform()
        {
            Transform2D transform = new Transform2D();

            transform.Position = Entity.Transform.WorldToLocal(GetPosition());
            transform.Parent = Entity.Transform;
            transform.Rotation = -(float)MathExt.AngleVector(GetWorldNormal()) - Entity.Transform.Rotation;// +(float)Math.PI;
            return transform;
        }
    }
}
