using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xna = Microsoft.Xna.Framework;

namespace Game
{
    [Serializable]
    public class Portal : Placeable2D, IVertices2D
    {
        private Portal _linked = null;
        private bool _oneSided = true;
        public Entity EntityParent { get; private set; }
        private List<int> _fixtureIds = new List<int>();
        public Body EntityBody
        {
            get
            {
                return EntityParent.Body;
            }
        }
        public Fixture SensorFixture { get; private set; }
        private Exception _nullScene = new Exception("Portal must be assigned to a scene.");
        /// <summary>
        /// If OneSided is true then the portal can only be viewed through it's front side.
        /// Entities can still travel though the portal in both directions however.
        /// </summary>
        public bool OneSided
        {
            get { return _oneSided; }
            set { _oneSided = value; }
        }
        /// <summary>
        /// The distance at which an entity enters and exits a portal.  
        /// It is used to avoid situations where an entity can skip over a portal by sitting exactly on top of it.
        /// </summary>
        public const float EntityMinDistance = 0.001f;
        public const float PortalMargin = 0.02f;

        public Portal Linked
        {
            get { return _linked; }
        }

        private Portal()
        {
        }

        public Portal(Scene scene, Transform2D transform, Fixture fixture, int edgeIndex, float edgeT)
            : base(scene)
        {
            if (scene == null)
            {
                throw _nullScene;
            }
            if (transform != null)
            {
                Transform.SetLocal(transform);
            }
            Transform.UniformScale = true;

            if (fixture != null)
            {
                SetEntityParent(fixture, edgeIndex, edgeT);
            }
        }

        public Portal(Scene scene) 
            : this(scene, null, null, 0, 0)
        {
        }

        public Portal(Scene scene, bool leftHanded)
            : this(scene, null, null, 0, 0)
        {
            SetFacing(leftHanded);   
        }

        public Portal(Scene scene, Vector2 position)
            : this(scene, null, null, 0, 0)
        {
            Transform.Position = position;
        }

        public Portal(Scene scene, Transform2D transform)
            : this(scene)
        {
            Transform.SetLocal(transform);
        }

        public void Remove()
        {
            Fixture[] fixtures = GetFixtures();
            foreach (Fixture f in fixtures)
            {
                EntityBody.DestroyFixture(f);
            }
            _fixtureIds.Clear();
            Transform.Parent = null;
        }

        public void SetEntityParent(FixtureIntersection fixturePoint)
        {
            SetEntityParent(fixturePoint.Fixture, fixturePoint.EdgeIndex, fixturePoint.EdgeT);
        }

        /// <summary>
        /// Sets whichs Entity this Portal is parented too.  Fixtures are added to this Entity's Body for handling portal collisions.
        /// If this Portal was previously parented to an Entity, the previously added Fixtures will be removed.
        /// </summary>
        /// <param name="parentEntity">Entity that Portal is parented to.  If null then this Portal is not parented to any Entity.</param>
        /// <param name="transform">Transform relative to the parent Entity.  Values are copied so references are not preserved.</param>
        public void SetEntityParent(Fixture fixture, int edgeIndex, float edgeT)
        {
            Remove();
            //Transform.SetLocal(transform);
            
            EntityParent = null;
            if (fixture != null)
            {
                Vector2[][] verts = new Vector2[2][];

                switch (fixture.ShapeType)
                {
                    case ShapeType.Polygon:
                        {
                            PolygonShape shape = (PolygonShape)fixture.Shape;
                            Vector2 v0 = new Vector2(shape.Vertices[edgeIndex].X, shape.Vertices[edgeIndex].Y);
                            int index = (edgeIndex + 1) % shape.Vertices.Count;
                            Vector2 v1 = new Vector2(shape.Vertices[index].X, shape.Vertices[index].Y);
                            Line line = new Line(v0, v1);
                            Transform.Position = line.Lerp(edgeT);
                            Transform.Rotation = -line.Angle() + (float)Math.PI/2;

                            var tempVerts = VectorExt2.Transform(GetVerts(), Transform.GetMatrix());
                            verts[0] = new Vector2[3];
                            verts[0][0] = tempVerts[0];
                            verts[0][1] = VectorExt2.ConvertTo(shape.Vertices[edgeIndex]);
                            verts[0][2] = VectorExt2.Transform(GetVerts()[0] + new Vector2(-PortalMargin, 0), Transform.GetMatrix());
                            verts[1] = new Vector2[3];
                            verts[1][0] = tempVerts[1];
                            verts[1][1] = VectorExt2.ConvertTo(shape.Vertices[index]);
                            verts[1][2] = VectorExt2.Transform(GetVerts()[1] + new Vector2(-PortalMargin, 0), Transform.GetMatrix());
                            break;
                        }
                    default:
                        {
                            Debug.Assert(false, "Invalid shape type for fixture.");
                            break;
                        }
                }

                EntityParent = FixtureExt.GetUserData(fixture).Entity;
                Transform.Parent = EntityParent.Transform;

                List<Fixture> fixtures = new List<Fixture>();
                

                /*Entity entity = Scene.CreateEntity();
                entity.Transform.Parent = EntityParent.Transform;*/

                /*verts[0] = VectorExt2.Transform(GetFixtureLeftVerts(), Transform.GetMatrix());
                verts[1] = VectorExt2.Transform(GetFixtureRightVerts(), Transform.GetMatrix());*/
                
                for (int i = 0; i < verts.Length; i++)
                {
                    verts[i] = MathExt.SetHandedness(verts[i], false);
                    FarseerPhysics.Common.Vertices fixtureVerts = new FarseerPhysics.Common.Vertices();
                    fixtureVerts.AddRange(VectorExt2.ConvertToXna(verts[i]));
                    fixtures.Add(FixtureExt.CreatePortalFixture(EntityParent.Body, new PolygonShape(fixtureVerts, 0), this));

                    //entity.Models.Add(Model.CreatePolygon(VectorExt2.ConvertTo(fixtureVerts)));
                }

                verts[0] = VectorExt2.Transform(GetVerts(), Transform.GetMatrix());
                Xna.Vector2[] vertsXna = VectorExt2.ConvertToXna(verts[0]);
                SensorFixture = FixtureExt.CreatePortalFixture(EntityParent.Body, new EdgeShape(vertsXna[0], vertsXna[1]), this);
                SensorFixture.IsSensor = true;
                fixtures.Add(SensorFixture);
                
                foreach (Fixture f in fixtures)
                {
                    _fixtureIds.Add(f.FixtureId);
                }
            }
        }

        public void SetSize(float size)
        {
            Transform.Scale = new Vector2(Transform.Scale.X, size);
        }

        public void SetFacing(bool leftHanded)
        {
            if (leftHanded)
            {
                Transform.Scale = new Vector2(1, Transform.Scale.Y);
            }
            else
            {
                Transform.Scale = new Vector2(-1, Transform.Scale.Y);
            }
        }

        public Fixture[] GetFixtures()
        {
            Fixture[] fixture = new Fixture[_fixtureIds.Count];
            for (int i = 0; i < _fixtureIds.Count; i++)
            {
                int fixtureId = _fixtureIds[i];
                fixture[i] = EntityBody.FixtureList.Find(item => item.FixtureId == fixtureId);
                Debug.Assert(fixture[i] != null, "Fixture could not be found.");
            }
            return fixture;
        }

        public static void ConnectPortals(Portal portal0, Portal portal1)
        {
            portal0._linked = portal1;
            portal1._linked = portal0;
        }

        private void SetPortal(Portal portal)
        {
            if (_linked != portal)
            {
                if (_linked != null)
                {
                    _linked.SetPortal(null);
                }
                _linked = portal;
                if (_linked != null)
                {
                    _linked.SetPortal(this);
                }
            }
        }

        /// <summary>
        /// Returns an array of two Vectors defining the Portals local location
        /// </summary>
        public Vector2[] GetVerts()
        {
            return new Vector2[] { new Vector2(0, 0.5f), new Vector2(0, -0.5f)};
        }

        private Vector2[] GetFixtureLeftVerts()
        {
            return new Vector2[] { 
                new Vector2(0, 0.5f),
                new Vector2(0, 0.5f + PortalMargin),
                new Vector2(-PortalMargin, 0.5f),
            };
        }

        private Vector2[] GetFixtureRightVerts()
        {
            return new Vector2[] { 
                new Vector2(0, -0.5f),
                new Vector2(-PortalMargin, -0.5f),
                new Vector2(0, -(0.5f + PortalMargin)), 
            };
        }

        public Vector2[] GetWorldVerts()
        {
            return VectorExt2.Transform(GetVerts(), Transform.GetWorldMatrix());
        }

        public Vector2[] GetFOV(Vector2 origin, float distance)
        {
            return GetFOV(origin, distance, 10);
        }

        /// <summary>
        /// Converts a Transform2D from one portal's coordinate space to the portal it is linked with.  If it isn't linked then the Transform2D is unchanged
        /// </summary>
        /// <param name="entityPos"></param>
        public void Enter(Transform2D entityPos)
        {
            Matrix4 m = GetPortalMatrix();
            Vector2 v0 = VectorExt2.Transform(entityPos.Position, m);
            Vector2 v1 = VectorExt2.Transform(entityPos.Position + new Vector2(1, 0), m);
            Vector2 v2 = VectorExt2.Transform(entityPos.Position + new Vector2(0, 1), m);

            entityPos.Position = new Vector2(v0.X, v0.Y);

            Transform2D tEnter = Transform;
            Transform2D tExit = Linked.Transform;
            float flipX = 1;
            float flipY = 1;
            if (Math.Sign(tEnter.WorldScale.X) == Math.Sign(tExit.WorldScale.X))
            {
                flipX = -1;
            }
            if (Math.Sign(tEnter.WorldScale.Y) != Math.Sign(tExit.WorldScale.Y))
            {
                flipY = -1;
            }
            entityPos.Scale *= new Vector2(flipX * (v1 - v0).Length, flipY * (v2 - v0).Length);

            float angle;
            if (flipX != flipY)
            {
                entityPos.Rotation = -entityPos.Rotation;
                entityPos.Rotation += (float)(MathExt.AngleWrap(Transform.WorldRotation) + MathExt.AngleWrap(Linked.Transform.WorldRotation));
            }
            else
            {
                angle = Linked.Transform.WorldRotation - Transform.WorldRotation;
                entityPos.Rotation += angle;
            }
        }

        public void Enter(Transform entity)
        {
            Transform2D entity2D = entity.GetTransform2D();
            Enter(entity2D);
            entity.Rotation = new Quaternion(entity.Rotation.X, entity.Rotation.Y, entity.Rotation.Z, entity2D.Rotation);
            entity.Position = new Vector3(entity2D.Position.X, entity2D.Position.Y, entity.Position.Z);
            entity.Scale = new Vector3(entity2D.Scale.X, entity2D.Scale.Y, entity.Scale.Z);
        }

        /// <summary>
        /// Returns matrix to transform between one portals coordinate space to another
        /// </summary>
        public static Matrix4 GetPortalMatrix(Portal portalEnter, Portal portalExit)
        {
            //The portalExit is temporarily mirrored before getting the transformation matrix
            Vector2 v = portalExit.Transform.Scale;
            portalExit.Transform.Scale = new Vector2(-v.X, v.Y);
            Matrix4 m = portalEnter.Transform.GetWorldMatrix().Inverted() * portalExit.Transform.GetWorldMatrix();
            portalExit.Transform.Scale = v;
            return m;
        }

        public Matrix4 GetPortalMatrix()
        {
            Debug.Assert(Linked != null, "Portal must be linked to another portal.");
            return GetPortalMatrix(this, Linked);
        }

        /// <summary>
        /// Returns a polygon representing the 2D FOV through the portal.  If the polygon is degenerate then an array of length 0 will be returned.
        /// </summary>
        public Vector2[] GetFOV(Vector2 viewPoint, float distance, int detail)
        {
            Matrix4 a = Transform.GetWorldMatrix();
            Vector2[] verts = new Vector2[detail + 2];
            Vector2[] portal = GetVerts();
            for (int i = 0; i < portal.Length; i++)
            {
                Vector4 b = Vector4.Transform(new Vector4(portal[i].X, portal[i].Y, 0, 1), a);
                verts[i] = new Vector2(b.X, b.Y);
            }
            //minumum distance in order to prevent self intersections
            const float errorMargin = 0.01f;
            float distanceMin = Math.Max((verts[0] - viewPoint).Length, (verts[1] - viewPoint).Length) + errorMargin;
            distance = Math.Max(distance, distanceMin);
            //get the leftmost and rightmost edges of the FOV
            verts[verts.Length - 1] = (verts[0] - viewPoint).Normalized() * distance + viewPoint;
            verts[2] = (verts[1] - viewPoint).Normalized() * distance + viewPoint;
            //find the angle between the edges of the FOV
            double angle0 = MathExt.AngleLine(verts[verts.Length - 1], viewPoint);
            double angle1 = MathExt.AngleLine(verts[2], viewPoint);
            double diff = MathExt.AngleDiff(angle0, angle1);
            Debug.Assert(diff <= Math.PI + double.Epsilon && diff >= -Math.PI);
            //handle case where lines overlap eachother
            const double angleDiffMin = 0.0001f;
            if (Math.Abs(diff) < angleDiffMin)
            {
                return new Vector2[0];
            }

            Matrix2 Rot = Matrix2.CreateRotation((float)diff / (detail - 1));
            for (int i = 3; i < verts.Length - 1; i++)
            {
                verts[i] = MathExt.Matrix2Mult(verts[i - 1] - viewPoint, Rot) + viewPoint;
            }
            return verts;
        }
    }
}
