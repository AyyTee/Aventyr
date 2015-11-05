using FarseerPhysics.Collision;
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
    public class FixturePortal : Portal, IVertices2D
    {
        public override Entity EntityParent
        {
            get
            {
                if (Position != null)
                {
                    return FixtureExt.GetUserData(Position.Fixture).Entity;
                }
                return null;
            }
        }
        public FixtureEdgeCoord Position { get; private set; }
        private List<int> _fixtureIds = new List<int>();
        public bool IsMirrored { get; set; }
        public Body EntityBody
        {
            get
            {
                return EntityParent.Body;
            }
        }
        public Fixture FixtureParent {
            get
            {
                if (Position != null)
                {
                    return Position.Fixture;
                }
                return null;
            }
        }
        
        public const float EdgeMargin = 0.02f;
        public const float CollisionMargin = 0.02f;

        public FixturePortal(Scene scene, FixtureEdgeCoord position)
            : base(scene)
        {
            if (position != null)
            {
                SetFixtureParent(position);
            }
        }

        /// <summary>
        /// Returns a copy of the Transform local to the Body this is attached to.
        /// </summary>
        /// <returns></returns>
        public override Transform2D GetTransform()
        {
            Transform2D transform = new Transform2D();
            if (Position == null)
            {
                return transform;
            }
            Line edge = Position.GetEdge();
            transform.Position = edge.Lerp(Position.EdgeT);
            transform.Rotation = -edge.Angle() + (float)Math.PI/2;
            if (IsMirrored)
            {
                transform.Scale = new Vector2(1, -1);
            }
            
            transform.Parent = FixtureExt.GetUserData(Position.Fixture).Entity.Transform;
            return transform;
        }

        public void Remove()
        {
            Fixture[] fixtures = GetFixtures();
            foreach (Fixture f in fixtures)
            {
                EntityBody.DestroyFixture(f);
            }
            _fixtureIds.Clear();
        }

        public void SetFixtureParent(FixtureEdgeCoord position)
        {
            Remove();
            
            Position = position;
            if (Position != null)
            {
                Vector2[][] verts = new Vector2[2][];
                Fixture fixture = Position.Fixture;
                switch (fixture.ShapeType)
                {
                    case ShapeType.Polygon:
                        {
                            verts = GetEdgeVertices(Position);
                            break;
                        }
                    default:
                        {
                            Debug.Assert(false, "Invalid shape type for fixture.");
                            break;
                        }
                }

                //wake up all the bodies so that they will fall if there is now a portal entrance below them
                foreach (Body b in Scene.PhysWorld.BodyList)
                {
                    b.Awake = true;
                }
                /*Entity entity = Scene.CreateEntity();
                entity.Transform.Parent = GetTransform().Parent;*/
                for (int i = 0; i < verts.Length; i++)
                {
                    verts[i] = MathExt.SetHandedness(verts[i], false);
                    FarseerPhysics.Common.Vertices fixtureVerts = new FarseerPhysics.Common.Vertices();
                    fixtureVerts.AddRange(Vector2Ext.ConvertToXna(verts[i]));
                    Fixture fixtureTemp = FixtureExt.CreatePortalFixture(EntityParent.Body, new PolygonShape(fixtureVerts, 0), this);
                    _fixtureIds.Add(fixtureTemp.FixtureId);

                    //entity.Models.Add(Model.CreatePolygon(verts[i]));
                }
            }
        }

        private Vector2[][] GetEdgeVertices(FixtureEdgeCoord position)
        {
            Vector2[][] edgeFixtures = new Vector2[2][];
            var tempVerts = Vector2Ext.Transform(GetVerts(), GetTransform().GetMatrix());
            Line edge = Position.GetEdge();
            PolygonShape shape = (PolygonShape)position.Fixture.Shape;
            for (int i = 0; i < 2; i++)
            {
                int i0 = i;
                if (!IsMirrored)
                {
                    i0 = (i + 1) % 2;
                }
                Vector2[] verts = new Vector2[3];
                edgeFixtures[i] = verts;
                int index = (position.EdgeIndex + i) % shape.Vertices.Count;
                verts[0] = tempVerts[i0];
                verts[1] = Vector2Ext.ConvertTo(shape.Vertices[index]);
                verts[2] = Vector2Ext.Transform(GetVerts()[i0] + new Vector2(-EdgeMargin, 0), GetTransform().GetMatrix());
            }
            return edgeFixtures;
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

        public Vector2[] GetBounds(float margin)
        {
            float width, height;
            width = (float)Math.Abs(Math.Cos(GetTransform().Rotation) * GetTransform().WorldScale.X) + margin * 2;
            height = (float)Math.Abs(Math.Sin(GetTransform().Rotation) * GetTransform().WorldScale.X) + margin * 2;
            return new Vector2[] {
                GetTransform().WorldPosition + new Vector2(-width/2f, -height/2f),
                GetTransform().WorldPosition + new Vector2(-width/2f, height/2f),
                GetTransform().WorldPosition + new Vector2(width/2f, height/2f),
                GetTransform().WorldPosition + new Vector2(width/2f, -height/2f)
            };
        }

        public Vector2[] GetBounds()
        {
            return GetBounds(0);
        }
    }
}
