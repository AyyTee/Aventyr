using ClipperLib;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class ActorExt
    {
        /// <summary>
        /// Returns polygon that is the local polygon with only the local transforms Scale component applied. 
        /// This is useful because the vertices should match up with vertices in the physics fixtures for this Actor's body (within rounding errors).
        /// </summary>
        public static List<Vector2> GetFixtureContour(IActor actor)
        {
            return GetFixtureContour(actor.Vertices, actor.GetTransform().Scale);
        }

        public static List<Vector2> GetFixtureContour(IList<Vector2> vertices, Vector2 scale)
        {
            Debug.Assert(scale.X != 0 && scale.Y != 0);
            Matrix4 scaleMat = Matrix4.CreateScale(new Vector3(scale));
            List<Vector2> contour = Vector2Ext.Transform(vertices, scaleMat);
            if (Math.Sign(scale.X) != Math.Sign(scale.Y))
            {
                contour.Reverse();
            }
            return contour;
        }

        public static List<Vector2> GetGravity(IActor actor, IList<IPortal> portals, Vector2 gravity)
        {
            Debug.Assert(actor != null);
            Debug.Assert(portals != null);
            List<Vector2> impulses = new List<Vector2>();
            if (gravity == Vector2.Zero)
            {
                return impulses;
            }
            Vector2 impulse = Vector2.Zero;
            foreach (Fixture f in actor.Body.FixtureList)
            {
                PolygonShape polygon = f.Shape as PolygonShape;
                if (polygon != null)
                {
                    Clipper clipper = new Clipper();
                    clipper.AddPath(ClipperConvert.ToIntPoint(Vector2Ext.ToOtk(polygon.Vertices)), PolyType.ptSubject, true);
                    foreach (IPortal p in portals)
                    {
                        if (!Portal.IsValid(p))
                        {
                            continue;
                        }
                        Transform2 t = FixtureExt.GetData(f).Actor.GetWorldTransform();
                        t.SetScale(Vector2.One);
                        Vector2[] vertices = Vector2Ext.Transform(Portal.GetWorldVerts(p, 10000), t.GetMatrix().Inverted());
                        Vector2[] mask = new Vector2[]
                        {
                                vertices[0],
                                vertices[1],
                                vertices[0] + vertices[1]
                        };
                        clipper.AddPath(ClipperConvert.ToIntPoint(mask), PolyType.ptClip, true);
                    }
                }
            }
            return impulses;
        }

        /// <summary>
        /// Verifies the BodyType for Actor bodies is correct.
        /// </summary>
        /// <returns></returns>
        public static void AssertBodyType(IActor actor)
        {
            if (actor.Body.BodyType != actor.BodyType)
            {
                Debug.Fail("");
            }
            foreach (BodyData data in BodyExt.GetData(actor.Body).Children)
            {
                _assertBodyType(data);
            }
        }

        private static void _assertBodyType(BodyData bodyData)
        {
            Debug.Assert(
                (bodyData.Body.BodyType == BodyType.Dynamic && bodyData.Actor.BodyType == BodyType.Dynamic) ||
                (bodyData.Body.BodyType == BodyType.Kinematic && bodyData.Actor.BodyType != BodyType.Dynamic));
            foreach (BodyData data in bodyData.Children)
            {
                _assertBodyType(data);
            }
        }
    }
}
