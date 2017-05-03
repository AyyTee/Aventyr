using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Game.Common;
using Game.Portals;
using OpenTK;
using Xna = Microsoft.Xna;

namespace Game.Physics
{
    public class FixtureData
    {
        public readonly Fixture Fixture;

        /// <summary>
        /// All FixturePortals that this fixture is colliding with.
        /// </summary>
        public HashSet<IPortal> PortalCollisions = new HashSet<IPortal>();
        public HashSet<IPortal> PortalCollisionsPrevious = new HashSet<IPortal>();
        FixturePortal[] _portalParents = new FixturePortal[2];
        public Vector2[] DefaultShape;
        /// <summary>
        /// The portals this fixture is a collision edge for (a maximum of 2). 
        /// Both array indices are null if this fixture does not belong to a portal.
        /// </summary>
        public FixturePortal[] PortalParents 
        { 
            get
            {
                return _portalParents;
            }
            private set
            {
                Debug.Assert(GetChildPortals().Count == 0, "This fixture cannot be assigned to a portal.");
                _portalParents = value;
            }
        }

        List<Fixture> _fixtureChildren = new List<Fixture>();
        public List<Fixture> FixtureChildren => new List<Fixture>(_fixtureChildren);

        public Actor Actor
        {
            get
            {
                Debug.Assert(Fixture.Body.UserData != null, "Body UserData does not exist.");
                BodyData userData = BodyExt.GetData(Fixture.Body);
                return userData.Actor;
            }
        }

        #region Constructors
        public FixtureData()
        {
        }

        public FixtureData(Fixture fixture)
        {
            Fixture = fixture;
            DefaultShape = Vector2Ext.ToOtk(((PolygonShape)fixture.Shape).Vertices);
        }
        #endregion

        public bool PartOfPortal(FixturePortal portal)
        {
            return PortalParents[0] == portal || PortalParents[1] == portal;
        }

        public bool IsPortalParentless()
        {
            return PortalParents[0] == null && PortalParents[1] == null;
        }

        public List<FixturePortal> GetPortalChildren()
        {
            return Actor.Children.OfType<FixturePortal>().Where(
                item => FixtureExt.GetFixtureAttached(item) == Fixture).ToList();
        }

        /// <summary>
        /// Updates the Fixtures used for FixturePortal collisions.
        /// </summary>
        public void ProcessChanges()
        {
            foreach (Fixture f in _fixtureChildren)
            {
                Fixture.Body.DestroyFixture(f);
            }
            //FixtureExt.GetUserData(Fixture).Entity.Scene.World.ProcessChanges();
            _fixtureChildren.Clear();
            var sortedPortals = GetChildPortals().ToArray().OrderBy(item => PolygonExt.EdgeIndexT(item.Position)).ToList();
            sortedPortals.RemoveAll(item => !item.IsValid());
            for (int i = 0; i < sortedPortals.Count(); i++)
            {
                if (i == 0 || (i > 0 && sortedPortals[i].Position.EdgeIndex != sortedPortals[i - 1].Position.EdgeIndex))
                {
                    Fixture fixture = FixtureExt.CreateFixture(Fixture.Body, CreatePortalShape(sortedPortals[i], true));
                    _fixtureChildren.Add(fixture);
                    FixtureExt.GetData(fixture).PortalParents = new[] {
                        sortedPortals[i],
                        null
                    };
                }
                if (i < sortedPortals.Count() - 1 && sortedPortals[i].Position.EdgeIndex == sortedPortals[i + 1].Position.EdgeIndex)
                {
                    Fixture fixture = FixtureExt.CreateFixture(Fixture.Body, CreatePortalShape(sortedPortals[i], sortedPortals[i + 1]));
                    _fixtureChildren.Add(fixture);
                    FixtureExt.GetData(fixture).PortalParents = new[] {
                        sortedPortals[i],
                        sortedPortals[i+1]
                    };
                }
                else
                {
                    Fixture fixture = FixtureExt.CreateFixture(Fixture.Body, CreatePortalShape(sortedPortals[i], false));
                    _fixtureChildren.Add(fixture);
                    FixtureExt.GetData(fixture).PortalParents = new[] {
                        sortedPortals[i],
                        null
                    };
                }
            }
        }

        PolygonShape CreatePortalShape(FixturePortal portal, FixturePortal portalNext)
        {
            Debug.Assert(portal.Position.EdgeIndex == portalNext.Position.EdgeIndex);
            Debug.Assert(portal.Position.EdgeT < portalNext.Position.EdgeT);
            
            Transform2 t0 = portal.GetTransform();
            t0.MirrorX = false;
            t0.Size = Math.Abs(t0.Size);

            Transform2 t1 = portalNext.GetTransform();
            t1.MirrorX = false;
            t1.Size = Math.Abs(t1.Size);

            Vector2[] verts = 
            {
                Vector2Ext.Transform(Portal.Vertices[0], t0.GetMatrix()),
                Vector2Ext.Transform(Portal.Vertices[0] + new Vector2(-FixturePortal.EdgeMargin, 0), t0.GetMatrix()),
                Vector2Ext.Transform(Portal.Vertices[1] + new Vector2(-FixturePortal.EdgeMargin, 0), t1.GetMatrix()),
                Vector2Ext.Transform(Portal.Vertices[1], t1.GetMatrix())
            };
            
            verts = MathExt.SetWinding(verts, false);
            return new PolygonShape(new FarseerPhysics.Common.Vertices(verts.Select(v => (Xna.Framework.Vector2)v)), 0);
        }

        PolygonShape CreatePortalShape(FixturePortal portal, bool previousVertex)
        {
            int i = 1;
            if (previousVertex)
            {
                i = 0;
            }
            int iNext = (i + 1) % 2;

            Transform2 t = portal.GetTransform();
            t.MirrorX = false;
            t.Size = Math.Abs(t.Size);

            int index = (portal.Position.EdgeIndex + i) % Actor.Vertices.Count;

            Vector2[] verts = 
            {
                Vector2Ext.Transform(Portal.Vertices[iNext], t.GetMatrix()),
                Actor.GetFixtureContour(Actor)[index],
                Vector2Ext.Transform(Portal.Vertices[iNext] + new Vector2(-FixturePortal.EdgeMargin, 0), t.GetMatrix())
            };
            
            verts = MathExt.SetWinding(verts, false);

            return new PolygonShape(new FarseerPhysics.Common.Vertices(verts.Select(v => (Xna.Framework.Vector2)v)), 0);
        }

        /// <summary>
        /// A list of FixturePortals that are parented to this fixture.
        /// </summary>
        List<FixturePortal> GetChildPortals()
        {
            List<FixturePortal> portals = Actor.Children.OfType<FixturePortal>().ToList();
            return portals.FindAll(item => FixtureExt.GetFixtureAttached(item) == Fixture);
        }
    }
}
