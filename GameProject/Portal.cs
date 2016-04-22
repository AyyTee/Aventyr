using FarseerPhysics.Dynamics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public static class Portal
    {
        /// <summary>
        /// The distance at which an entity enters and exits a portal.  
        /// It is used to avoid situations where an entity can skip over a portal by sitting exactly on top of it.
        /// </summary>
        public const float EnterMinDistance = 0.001f;

        public static bool IsValid(IPortal portal)
        {
            return portal.Linked != null && portal.GetWorldTransform() != null;
        }

        /// <summary>
        /// Converts a Transform2D from one portal's coordinate space to the portal it is linked with.
        /// If it isn't linked then the Transform2D is unchanged.
        /// </summary>
        public static void Enter(IPortal portal, Transform2 position)
        {
            Debug.Assert(IsValid(portal));
            Matrix4 m = GetPortalMatrix(portal);
            Vector2 v0 = Vector2Ext.Transform(position.Position, m);
            Vector2 v1 = Vector2Ext.Transform(position.Position + new Vector2(1, 0), m);
            Vector2 v2 = Vector2Ext.Transform(position.Position + new Vector2(0, 1), m);

            position.Position = new Vector2(v0.X, v0.Y);

            Transform2 tEnter = portal.GetWorldTransform();
            Transform2 tExit = portal.Linked.GetWorldTransform();
            float flipX = 1;
            float flipY = 1;
            if (Math.Sign(tEnter.Scale.X) == Math.Sign(tExit.Scale.X))
            {
                flipX = -1;
            }
            if (Math.Sign(tEnter.Scale.Y) != Math.Sign(tExit.Scale.Y))
            {
                flipY = -1;
            }
            //position.Scale *= new Vector2(flipX * (v1 - v0).Length, flipY * (v2 - v0).Length);
            position.Size *= flipY * (v2 - v0).Length;
            position.IsMirrored = Math.Sign(position.Scale.X * flipX) != Math.Sign(position.Scale.Y * flipY);

            float angle;
            if (flipX != flipY)
            {
                position.Rotation = -position.Rotation;
                position.Rotation += (float)(MathExt.AngleWrap(portal.GetWorldTransform().Rotation) + MathExt.AngleWrap(portal.Linked.GetWorldTransform().Rotation));
            }
            else
            {
                angle = portal.Linked.GetWorldTransform().Rotation - portal.GetWorldTransform().Rotation;
                position.Rotation += angle;
            }
        }

        public static void Enter(IPortal portal, Transform2 position, Transform2 velocity)
        {
            Enter(portal, position);
            EnterVelocity(portal, velocity);
        }

        public static void Enter(IPortal portal, IPortalable portable)
        {
            Transform2 transform = portable.GetTransform();
            Transform2 velocity = portable.GetVelocity();
            Enter(portal, transform, velocity);
            portable.SetTransform(transform);
            portable.SetVelocity(velocity);
        }

        public static void EnterVelocity(IPortal portal, Transform2 velocity)
        {
            Matrix4 matrix = GetPortalMatrix(portal);
            Vector2 origin = Vector2Ext.Transform(new Vector2(), matrix);
            velocity.Position = Vector2Ext.Transform(velocity.Position, matrix);
            velocity.Position -= origin;
            if (portal.IsMirrored == portal.Linked.IsMirrored)
            {
                velocity.Rotation = -velocity.Rotation;
            }
        }

        public static void EnterVelocity(IPortal portal, ref Vector2 velocity)
        {
            Matrix4 matrix = GetPortalMatrix(portal, portal.Linked);
            Vector2 origin = Vector2Ext.Transform(new Vector2(), matrix);
            velocity = Vector2Ext.Transform(velocity, matrix);
            velocity -= origin;
        }

        public static void EnterVelocity(IPortal portal, Vector2[] velocity)
        {
            Matrix4 matrix = GetPortalMatrix(portal, portal.Linked);
            Vector2 origin = Vector2Ext.Transform(new Vector2(), matrix);
            for (int i = 0; i < velocity.Length; i++)
            {
                velocity[i] = Vector2Ext.Transform(velocity[i], matrix) - origin;
            }
        }

        public static void Enter(IPortal portal, Body body)
        {
            Transform2 transform = new Transform2(body.Position, body.Rotation);
            Transform2 velocity = new Transform2(body.LinearVelocity, body.AngularVelocity);
            Enter(portal, transform, velocity);
            body.Position = Vector2Ext.ConvertToXna(transform.Position);
            body.Rotation = transform.Rotation;
            body.LinearVelocity = Vector2Ext.ConvertToXna(velocity.Position);
            body.AngularVelocity = velocity.Rotation;
        }

        public static void Enter(IPortal portal, SceneNodePlaceable placeable)
        {
            Transform2 transform = placeable.GetTransform();
            Transform2 velocity = placeable.GetVelocity();
            Enter(portal, transform, velocity);
            placeable.SetTransform(transform);
            placeable.SetVelocity(velocity);
            placeable.PortalEnterInvoke(portal);
        }

        /*public static void SetLinked(IPortal portal0, IPortal portal1)
        {
            FixturePortal fixturePortal = portal0 as FixturePortal;
            Debug.Assert(portal == null || Scene == fixturePortal.Scene, "Linked portals must be in the same Scene.");
            if (Linked != fixturePortal)
            {
                if (Linked != null)
                {
                    ((Portal)Linked).Linked = null;
                }
                Linked = fixturePortal;
                if (Linked != null)
                {
                    ((Portal)Linked).SetLinked(this);
                }
            }
        }*/

        /// <summary>
        /// Returns an array of two Vectors defining the Portals local location
        /// </summary>
        public static Vector2[] GetVerts(IPortal portal)
        {
            return new Vector2[] { new Vector2(0, 0.5f), new Vector2(0, -0.5f) };
        }

        public static Vector2[] GetWorldVerts(IPortal portal)
        {
            return Vector2Ext.Transform(GetVerts(portal), portal.GetWorldTransform().GetMatrix());
        }

        public static Matrix4 GetPortalMatrix(IPortal portalEnter)
        {
            Debug.Assert(portalEnter.Linked != null, "Portal must be linked to another portal.");
            return GetPortalMatrix(portalEnter, portalEnter.Linked);
        }

        /// <summary>Returns matrix to transform between one portals coordinate space to another.</summary>
        public static Matrix4 GetPortalMatrix(IPortal portalEnter, IPortal portalExit)
        {
            Transform2 transform = portalExit.GetWorldTransform();
            transform.IsMirrored = !transform.IsMirrored;
            Matrix4 m = portalEnter.GetWorldTransform().GetMatrix();
            return m.Inverted() * transform.GetMatrix();
        }

        public static Line[] GetFovLines(IPortal portal, Vector2 origin, float distance)
        {
            return GetFovLines(portal, origin, distance, portal.GetWorldTransform());
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
            return GetFov(portal, origin, distance, detail, portal.GetWorldTransform());
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
            /*const double angleDiffMin = 0.0001f;
            if (Math.Abs(diff) < angleDiffMin)
            {
                return new Vector2[0];
            }*/

            Matrix2 Rot = Matrix2.CreateRotation((float)diff / (detail - 1));
            for (int i = 3; i < verts.Length - 1; i++)
            {
                verts[i] = MathExt.Matrix2Mult(verts[i - 1] - viewPoint, Rot) + viewPoint;
            }
            return verts;
        }
    }
}
