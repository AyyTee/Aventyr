using FarseerPhysics.Dynamics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game.Portals
{
    public static class Portal
    {
        /// <summary>
        /// The minimum distance something can be to a portal.
        /// It is used to avoid situations where an entity can skip over a portal by sitting exactly on top of it.
        /// </summary>
        public const float EnterMinDistance = 0.001f;

        /// <summary>
        /// Returns whether a portal can be entered.
        /// </summary>
        public static bool IsValid(IPortal portal)
        {
            return portal.Linked != null &&
                portal.WorldTransform != null &&
                portal.WorldVelocity != null &&
                portal.Linked.WorldTransform != null &&
                portal.Linked.WorldVelocity != null;
        }

        public static void SetLinked(IPortal p0, IPortal p1)
        {
            if (p0.Linked != null)
            {
                p0.Linked.Linked = null;
            }
            if (p1.Linked != null)
            {
                p1.Linked.Linked = null;
            }
            p0.Linked = p1;
            p1.Linked = p0;
        }

        public static Transform2 Enter(IPortal portal, Transform2 transform)
        {
            Debug.Assert(IsValid(portal));
            return transform.Transform(GetLinkedTransform(portal, portal.Linked));
        }

        /// <summary>
        /// Returns new velocity from entering portal.
        /// </summary>
        /// <param name="portal">Portal being entered.</param>
        /// <param name="intersectT">Intersection point on the portal.</param>
        /// <param name="velocity">Velocity before entering.</param>
        public static Transform2 EnterVelocity(IPortal portal, float intersectT, Transform2 velocity, bool ignorePortalVelocity = false)
        {
            Debug.Assert(IsValid(portal));
            Matrix4 matrix = GetLinkedMatrix(portal);
            Vector2 origin = Vector2Ext.Transform(new Vector2(), matrix);
            Transform2 velocityClone = velocity.ShallowClone();

            if (!ignorePortalVelocity)
            {
                velocityClone.Position -= portal.WorldVelocity.Position;
                velocityClone.Rotation -= portal.WorldVelocity.Rotation;
                velocityClone.Position -= GetAngularVelocity(portal, intersectT);
            }
            velocityClone.Position = Vector2Ext.Transform(velocityClone.Position, matrix);
            velocityClone.Position -= origin;

            if (portal.WorldTransform.MirrorX == portal.Linked.WorldTransform.MirrorX)
            {
                velocityClone.Rotation = -velocityClone.Rotation;
            }
            if (!ignorePortalVelocity)
            {
                velocityClone.Position += portal.Linked.WorldVelocity.Position;
                velocityClone.Rotation += portal.Linked.WorldVelocity.Rotation;
                velocityClone.Position += GetAngularVelocity(portal.Linked, intersectT);
            }
            return velocityClone;
        }

        private static Vector2 GetAngularVelocity(IPortal portal, float intersectT)
        {
            Vector2 intersect = new Line(GetWorldVerts(portal)).Lerp(intersectT);
            return MathExt.AngularVelocity(intersect, portal.WorldTransform.Position, portal.WorldVelocity.Rotation);
        }

        public static void Enter(IPortal portal, IPortalCommon portalable, float intersectT, bool ignorePortalVelocity = false, bool worldOnly = false)
        {
            Transform2 transform = portalable.GetTransform();
            Transform2 velocity = portalable.GetVelocity();

            //Copies are made just for debug purposes.  The originals should not change before the EnterPortal callback.
            Transform2 transformCopy = transform.ShallowClone();
            Transform2 velocityCopy = velocity.ShallowClone();

            IPortalable cast = portalable as IPortalable;
            if (cast != null)
            {
                if (!worldOnly)
                {
                    cast.SetTransform(Enter(portal, transform));
                    cast.SetVelocity(EnterVelocity(portal, intersectT, velocity, ignorePortalVelocity));
                }
            }

            if (portalable.WorldTransform != null)
            {
                portalable.WorldTransform = Enter(portal, portalable.WorldTransform);
                portalable.WorldVelocity = EnterVelocity(portal, intersectT, portalable.WorldVelocity, ignorePortalVelocity);
            }

            //If a static actor enters a portal then it's no longer static.
            IActor actorCast = portalable as IActor;
            if (actorCast != null && actorCast.BodyType == BodyType.Static)
            {
                actorCast.SetBodyType(BodyType.Kinematic);
            }

            foreach (IPortalCommon p in portalable.Children)
            {
                p.Path.Enter(portal.Linked);
            }

            Debug.Assert(transform == transformCopy && velocity == velocityCopy);

            if (cast != null)
            {
                cast.EnterPortal?.Invoke(new EnterCallbackData(portal, cast, intersectT), transform, velocity);
            }
        }

        public static void Enter(IPortal portal, Body body, bool ignorePortalVelocity = false)
        {
            Transform2 transform = new Transform2(body.Position, 1, body.Rotation);
            Transform2 velocity = Transform2.CreateVelocity(Vector2Ext.ToOtk(body.LinearVelocity), body.AngularVelocity);
            velocity = EnterVelocity(portal, 0.5f, velocity, ignorePortalVelocity);
            transform = Enter(portal, transform);
            body.Position = Vector2Ext.ToXna(transform.Position);
            body.Rotation = transform.Rotation;
            body.LinearVelocity = Vector2Ext.ToXna(velocity.Position);
            body.AngularVelocity = velocity.Rotation;

            BodyExt.ScaleFixtures(body, transform.Scale);
        }

        /// <summary>
        /// Returns an array of two Vectors defining the Portals local location
        /// </summary>
        public static Vector2[] GetVerts(IPortal portal)
        {
            return new Vector2[] { new Vector2(0, 0.5f), new Vector2(0, -0.5f) };
        }

        /// <summary>
        /// Get the portal's vertices in world coordinates.
        /// </summary>
        public static Vector2[] GetWorldVerts(IPortal portal)
        {
            return GetWorldVerts(portal, portal.WorldTransform);
        }

        /// <summary>
        /// Get the portal's vertices in world coordinates.
        /// </summary>
        public static Vector2[] GetWorldVerts(IPortal portal, Transform2 worldTransform)
        {
            return Vector2Ext.Transform(GetVerts(portal), worldTransform.GetMatrix());
        }

        /// <summary>
        /// Get the portal's vertices in world coordinates after being scaled.
        /// </summary>
        public static Vector2[] GetWorldVerts(IPortal portal, float scalar)
        {
            return Vector2Ext.Transform(Vector2Ext.Scale(GetVerts(portal), scalar), portal.WorldTransform.GetMatrix());
        }

        public static Matrix4 GetLinkedMatrix(IPortal portalEnter)
        {
            Debug.Assert(portalEnter.Linked != null, "Portal must be linked to another portal.");
            return GetLinkedMatrix(portalEnter, portalEnter.Linked);
        }

        /// <summary>Returns matrix to transform between one portals coordinate space to another.</summary>
        public static Matrix4 GetLinkedMatrix(IPortal portalEnter, IPortal portalExit)
        {
            Transform2 transform = portalExit.WorldTransform;
            transform.MirrorX = !transform.MirrorX;
            Matrix4 m = portalEnter.WorldTransform.GetMatrix();
            return m.Inverted() * transform.GetMatrix();
        }

        public static Transform2 GetLinkedTransform(IPortal portalEnter)
        {
            Debug.Assert(portalEnter.Linked != null, "Portal must be linked to another portal.");
            return GetLinkedTransform(portalEnter, portalEnter.Linked);
        }

        public static Transform2 GetLinkedTransform(IPortal portalEnter, IPortal portalExit)
        {
            Transform2 tExit = portalExit.WorldTransform;
            tExit.MirrorX = !tExit.MirrorX;
            Transform2 tEnter = portalEnter.WorldTransform;
            return tEnter.Inverted().Transform(tExit);
        }

        public static Line[] GetFovLines(IPortal portal, Vector2 origin, float distance)
        {
            return GetFovLines(portal, origin, distance, portal.WorldTransform);
        }

        public static Line[] GetFovLines(IPortal portal, Vector2 origin, float distance, Transform2 transform)
        {
            Vector2[] vertices = GetFov(portal, origin, distance);
            Line[] lines = new Line[] {
                new Line(vertices[1], vertices[2]),
                new Line(vertices[0], vertices[vertices.Length-1])
            };
            return lines;
        }

        /// <summary>
        /// Returns a polygon in world space representing the 2D FOV through the portal.  
        /// Polygon is not guaranteed to be non-degenerate which can occur if the viewPoint is edge-on to the portal.
        /// </summary>
        public static Vector2[] GetFov(IPortal portal, Vector2 origin, float distance)
        {
            return GetFov(portal, origin, distance, 10);
        }

        public static Vector2[] GetFov(IPortal portal, Vector2 origin, float distance, int detail)
        {
            return GetFov(portal, origin, distance, detail, portal.WorldTransform);
        }

        /// <summary>
        /// Returns a polygon in world space representing the 2D FOV through the portal.  
        /// Polygon is not guaranteed to be non-degenerate which can occur if the viewPoint is edge-on to the portal.
        /// </summary>
        public static Vector2[] GetFov(IPortal portal, Vector2 viewPoint, float distance, int detail, Transform2 transform)
        {
            Matrix4 a = transform.GetMatrix();
            Vector2[] verts = new Vector2[detail + 2];
            Vector2[] portalVerts = Portal.GetVerts(portal);
            for (int i = 0; i < portalVerts.Length; i++)
            {
                Vector4 b = Vector4.Transform(new Vector4(portalVerts[i].X, portalVerts[i].Y, 0, 1), a);
                verts[i] = new Vector2(b.X, b.Y);
            }
            //Minumum distance in order to prevent self intersections.
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
            /*const double angleDiffMin = 0.0001f;
            if (Math.Abs(diff) < angleDiffMin)
            {
                return new Vector2[0];
            }*/

            Matrix2 Rot = Matrix2.CreateRotation((float)diff / (detail - 1));
            for (int i = 3; i < verts.Length - 1; i++)
            {
                verts[i] = Vector2Ext.Transform(verts[i - 1] - viewPoint, Rot) + viewPoint;
            }
            return verts;
        }

        /// <summary>
        /// Returns all the portal intersections for a line in this path.  
        /// TFirst is the t value for the line intersection.  
        /// TLast is the t value for the portal intersection.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static IntersectCoord[] PathIntersections(PortalPath path, Line line)
        {
            IntersectCoord[] intersections = new IntersectCoord[path.Portals.Count];
            line = line.ShallowClone();

            Line[] portalLines = new Line[path.Portals.Count];
            for (int i = 0; i < path.Portals.Count; i++)
            {
                portalLines[i] = new Line(GetWorldVerts(path.Portals[i]));
            }

            for (int i = path.Portals.Count - 1; i >= 0; i--)
            {
                Matrix4 mat = GetLinkedMatrix(path.Portals[i].Linked);
                line[1] = line.Transform(mat)[1];
                for (int j = i + 1; j < path.Portals.Count; j++)
                {
                    portalLines[j] = portalLines[j].Transform(mat);
                }
            }

            for (int i = 0; i < path.Portals.Count; i++)
            {
                intersections[i] = MathExt.LineLineIntersect(portalLines[i], line, true);
            }

            return intersections;
        }

        /// <summary>
        /// Get all valid portals that collide with a polygon.  Portals can occlude eachother.
        /// </summary>
        /// <param name="margin">Minimum distance inside of polygon for a portal collision to count.  
        /// Useful for avoiding round off errors.</param>
        public static List<IPortal> GetCollisions(Vector2 center, IList<Vector2> polygon, IList<IPortal> portals, double margin = 0)
        {
            List<IPortal> collisions = new List<IPortal>();
            foreach (IPortal p in portals.Where(item => IsValid(item)))
            {
                Line portalLine = new Line(GetWorldVerts(p));
                if (MathExt.LineInPolygon(portalLine, polygon) && 
                    (margin == 0 || 
                    MathExt.PointPolygonDistance(portalLine[0], polygon) > margin ||
                    MathExt.PointPolygonDistance(portalLine[1], polygon) > margin))
                {
                    collisions.Add(p);
                }
            }

            var ordered = collisions.OrderBy(item => (item.WorldTransform.Position - center).Length).ToList();
            for (int i = 0; i < ordered.Count; i++)
            {
                IPortal portal = ordered[i];
                for (int j = ordered.Count - 1; j > i; j--)
                {
                    Line currentLine = new Line(GetWorldVerts(ordered[i]));
                    Line checkLine = new Line(GetWorldVerts(ordered[j]));
                    Side checkSide = currentLine.GetSideOf(checkLine);
                    if (checkSide != currentLine.GetSideOf(center))
                    {
                        ordered.RemoveAt(j);
                    }
                }
            }
            return ordered.ToList();
        }
    }
}
