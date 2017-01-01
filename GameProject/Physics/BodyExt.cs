using System;
using System.Collections.Generic;
using System.Diagnostics;
using ClipperLib;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Game.Common;
using Game.Portals;
using Vector2 = OpenTK.Vector2;
using Xna = Microsoft.Xna.Framework;

namespace Game.Physics
{
    public static class BodyExt
    {
        public static Body CreateBody(World world)
        {
            Body body = new Body(world);
            world.ProcessChanges();
            return body;
        }

        /// <summary>
        /// Create body and assign it to an Actor instance.
        /// </summary>
        public static Body CreateBody(World world, Actor actor)
        {
            Debug.Assert(actor != null);
            Debug.Assert(actor.Body == null);
            Debug.Assert(world != null);
            Body body = CreateBody(world);
            SetData(body, actor);
            return body;
        }

        public static BodyData SetData(Body body, Actor entity)
        {
            BodyData userData = new BodyData(entity, body);
            body.UserData = userData;
            return userData;
        }

        public static BodyData GetData(Body body)
        {
            Debug.Assert(body != null);
            Debug.Assert(body.UserData != null);
            return (BodyData)body.UserData;
        }

        public static Transform2 GetTransform(Body body)
        {
            return new Transform2((Vector2)body.Position, 1, body.Rotation);
        }

        public static void SetTransform(Body body, Transform2 transform)
        {
            body.SetTransform((Xna.Vector2)transform.Position, transform.Rotation);
        }

        public static Transform2 GetVelocity(Body body)
        {
            return Transform2.CreateVelocity((Vector2)body.LinearVelocity, body.AngularVelocity);
        }

        public static void SetVelocity(Body body, Transform2 velocity)
        {
            body.LinearVelocity = (Xna.Vector2)velocity.Position;
            body.AngularVelocity = velocity.Rotation;
        }

        public static void ScaleFixtures(Body body, Vector2 scale)
        {
            foreach (Fixture f in body.FixtureList)
            {
                FixtureData data = FixtureExt.GetData(f);
                PolygonShape shape = (PolygonShape)f.Shape;
                Debug.Assert(data.DefaultShape.Length == shape.Vertices.Count);

                FarseerPhysics.Common.Vertices vertices = new FarseerPhysics.Common.Vertices();
                for (int i = 0; i < shape.Vertices.Count; i++)
                {
                    vertices.Add((Xna.Vector2)(data.DefaultShape[i] * scale));
                }
                //FPE will ensure the polygon is c. clockwise so we don't need to check that here.
                shape.Vertices = vertices;
            }
        }

        public struct MassData
        {
            public float Mass;
            /// <summary>
            /// Center of mass in world coordinates.
            /// </summary>
            public Vector2 Centroid;

            public MassData(float mass, Vector2 centroid)
            {
                Mass = mass;
                Centroid = centroid;
            }
        }

        /// <summary>
        /// Returns the centroid and mass of a body excluding parts of the body that are in a portal.
        /// </summary>
        public static MassData GetLocalMassData(Body body)
        {
            var clipped = GetClippedFixtures(body);
            float totalMass = 0;
            Vector2 centroid = new Vector2();
            foreach (Tuple<Fixture, Vector2[]> tuple in clipped)
            {
                float mass = tuple.Item1.Shape.Density * (float)MathExt.GetArea(tuple.Item2);
                totalMass += mass;
                centroid += PolygonExt.GetCentroid(tuple.Item2) * mass;
            }
            centroid /= totalMass;

            return new MassData(totalMass, centroid);
        }

        public static Vector2 GetLocalOrigin(Body body)
        {
            BodyData data = GetData(body);
            Vector2 center;
            //If this isn't the root body then the center point will be just outside of the parent portal.
            if (data.IsChild)
            {
                LineF portalLine = new LineF(Portal.GetWorldVerts(data.BodyParent.Portal.Linked));
                Vector2 offset = portalLine.Delta.PerpendicularLeft.Normalized() * 0.01f;
                center = portalLine.Center + offset;
                if (portalLine.GetSideOf(center + offset) == portalLine.GetSideOf(body.Position))
                {
                    center = portalLine.Center - offset;
                }
            }
            else
            {
                center = (Vector2)body.Position;
            }
            
            return center;
        }

        /// <summary>
        /// Returns a list of fixtures and their shapes with regions inside portals clipped away.  
        /// This excludes portal fixtures and the shapes are in world coordinates.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        static List<Tuple<Fixture, Vector2[]>> GetClippedFixtures(Body body)
        {
            BodyData data = GetData(body);

            /* If this body isn't colliding with any portals then we just return a list of
             * fixtures and vertices.*/
            if (data.PortalCollisions().Count <= 0)
            {
                List<Tuple<Fixture, Vector2[]>> fixtures = new List<Tuple<Fixture, Vector2[]>>();
                foreach (Fixture f in body.FixtureList)
                {
                    fixtures.Add(new Tuple<Fixture, Vector2[]>(f, FixtureExt.GetWorldPoints(f)));
                }
                return fixtures;
            }

            Vector2 center = GetLocalOrigin(body);

            List<List<IntPoint>> clipPaths = new List<List<IntPoint>>();
            foreach (IPortal p in data.PortalCollisions())
            {
                Vector2[] verts = Portal.GetWorldVerts(p);
                float scale = 100;

                Vector2 v0 = verts[0] + (verts[1] - verts[0]).Normalized() * scale;
                Vector2 v1 = verts[1] - (verts[1] - verts[0]).Normalized() * scale;
                Vector2 depth = (verts[1] - verts[0]).PerpendicularLeft.Normalized() * scale;
                if (new LineF(v0, v1).GetSideOf(v1 + depth) == new LineF(v0, v1).GetSideOf(center))
                {
                    depth *= -1;
                }

                Vector2[] box = new Vector2[]
                {
                    v0, v1, v1 + depth, v0 + depth
                };
                box = MathExt.SetWinding(box, true);

                clipPaths.Add(ClipperConvert.ToIntPoint(box));
            }

            List<Tuple<Fixture, Vector2[]>> clippedFixtures = new List<Tuple<Fixture, Vector2[]>>();

            Clipper clipper = new Clipper();
            foreach (Fixture f in body.FixtureList)
            {
                if (!FixtureExt.GetData(f).IsPortalParentless())
                {
                    continue;
                }

                clipper.Clear();
                clipper.AddPaths(clipPaths, PolyType.ptClip, true);

                clipper.AddPath(
                    ClipperConvert.ToIntPoint(FixtureExt.GetWorldPoints(f)),
                    PolyType.ptSubject,
                    true);

                List<List<IntPoint>> result = new List<List<IntPoint>>();
                clipper.Execute(ClipType.ctDifference, result, PolyFillType.pftEvenOdd, PolyFillType.pftNonZero);
                Debug.Assert(
                    result.Count <= 1,
                    "This fixture is too large for the portal masking or something has gone wrong with the clipper.");

                if (result.Count > 0)
                {
                    clippedFixtures.Add(new Tuple<Fixture, Vector2[]>(f, ClipperConvert.ToVector2(result[0])));
                }
            }

            Debug.Assert(clippedFixtures.Count > 0);
            return clippedFixtures;
        }

        public static void Remove(Body body)
        {
            BodyData data = GetData(body);
            _remove(data);
            int removed = data.Parent.BodyChildren.RemoveAll(item => GetData(item.Body) == data);
            Debug.Assert(removed == 1);
        }

        static void _remove(BodyData bodyData)
        {
            foreach (BodyData data in bodyData.Children)
            {
                _remove(data);
            }
            bodyData.Actor.Scene.World.RemoveBody(bodyData.Body);
        }
    }
}
