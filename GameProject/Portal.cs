using FarseerPhysics.Dynamics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public abstract class Portal : Placeable2D
    {
        //public Scene Scene { get; private set; }
        public Portal Linked { get; private set; }
        /// <summary>
        /// The local size of the portal.
        /// </summary>
        public float Size { get { return GetTransform().Scale.X; } }
        /// <summary>
        /// True if entities can travel through this portal.  Does not affect portal clipping.
        /// </summary>
        //public bool IsPortalable { get; set; }
        /// <summary>
        /// The distance at which an entity enters and exits a portal.  
        /// It is used to avoid situations where an entity can skip over a portal by sitting exactly on top of it.
        /// </summary>
        public const float EnterMinDistance = 0.001f;
        /// <summary>
        /// If OneSided is true then the portal can only be viewed through it's front side.
        /// Entities can still travel though the portal in both directions however.
        /// </summary>
        public bool OneSided { get; set; }
        public bool IsMirrored { get; set; }
        private Exception _nullScene = new Exception("Portal must be assigned to a scene.");
        public Portal(Scene scene)
            : base(scene)
        {
            if (scene == null)
            {
                throw _nullScene;
            }
            OneSided = false;//true;
            Scene.PortalList.Add(this);
        }

        public virtual void Dispose()
        {
            SetLinked(null);
        }

        public virtual void Step()
        {
        }

        public abstract Transform2D GetVelocity();

        /// <summary>
        /// Whether a portal can be entered, rendered, and clip models.
        /// </summary>
        public bool IsValid()
        {
            return _isValid() && (Linked != null && Linked._isValid());
        }

        protected virtual bool _isValid()
        {
            return Linked != null;
        }

        /// <summary>
        /// Converts a Transform2D from one portal's coordinate space to the portal it is linked with.  If it isn't linked then the Transform2D is unchanged
        /// </summary>
        /// <param name="position"></param>
        public void Enter(Transform2D position)
        {
            Debug.Assert(IsValid());
            Matrix4 m = GetPortalMatrix();
            Vector2 v0 = Vector2Ext.Transform(position.Position, m);
            Vector2 v1 = Vector2Ext.Transform(position.Position + new Vector2(1, 0), m);
            Vector2 v2 = Vector2Ext.Transform(position.Position + new Vector2(0, 1), m);

            position.Position = new Vector2(v0.X, v0.Y);

            Transform2D tEnter = GetWorldTransform();
            Transform2D tExit = Linked.GetWorldTransform();
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
            position.Scale *= new Vector2(flipX * (v1 - v0).Length, flipY * (v2 - v0).Length);

            float angle;
            if (flipX != flipY)
            {
                position.Rotation = -position.Rotation;
                position.Rotation += (float)(MathExt.AngleWrap(GetWorldTransform().Rotation) + MathExt.AngleWrap(Linked.GetWorldTransform().Rotation));
            }
            else
            {
                angle = Linked.GetWorldTransform().Rotation - GetWorldTransform().Rotation;
                position.Rotation += angle;
            }
        }

        public void Enter(Transform position)
        {
            Transform2D entity2D = position.GetTransform2D();
            Enter(entity2D);
            position.Rotation = new Quaternion(position.Rotation.X, position.Rotation.Y, position.Rotation.Z, entity2D.Rotation);
            position.Position = new Vector3(entity2D.Position.X, entity2D.Position.Y, position.Position.Z);
            position.Scale = new Vector3(entity2D.Scale.X, entity2D.Scale.Y, position.Scale.Z);
        }

        public void Enter(Transform2D position, Transform2D velocity)
        {
            float rotationPrev = position.Rotation;
            Enter(position);
            velocity.Position = Vector2Ext.Transform(velocity.Position, Matrix4.CreateRotationZ(position.Rotation - rotationPrev + (float)Math.PI));
            
            if (GetWorldTransform().IsMirrored() == Linked.GetWorldTransform().IsMirrored())
            {
                velocity.Rotation = -velocity.Rotation;
            }
        }

        public void Enter(Body body)
        {
            Transform2D transform = new Transform2D(body.Position, body.Rotation);
            Transform2D velocity = new Transform2D(body.LinearVelocity, body.AngularVelocity);
            Enter(transform, velocity);
            body.Position = Vector2Ext.ConvertToXna(transform.Position);
            body.Rotation = transform.Rotation;
            body.LinearVelocity = Vector2Ext.ConvertToXna(velocity.Position);
            body.AngularVelocity = velocity.Rotation;
        }

        public void Enter(Entity entity)
        {
            Transform2D transform = entity.GetTransform();
            Transform2D velocity = entity.Velocity;
            this.Enter(transform, velocity);
            entity.SetTransform(transform);
            entity.SetVelocity(velocity);
        }

        public static void SetLinked(Portal portal0, Portal portal1)
        {
            portal0.SetLinked(portal1);
        }

        private void SetLinked(Portal portal)
        {
            if (Linked != portal)
            {
                if (Linked != null)
                {
                    Linked.Linked = null;
                }
                Linked = portal;
                if (Linked != null)
                {
                    Linked.SetLinked(this);
                }
            }
        }

        /// <summary>
        /// Returns an array of two Vectors defining the Portals local location
        /// </summary>
        public Vector2[] GetVerts()
        {
            return new Vector2[] { new Vector2(0, 0.5f), new Vector2(0, -0.5f) };
        }

        public Vector2[] GetWorldVerts()
        {
            return Vector2Ext.Transform(GetVerts(), GetWorldTransform().GetMatrix());
        }

        public Matrix4 GetPortalMatrix()
        {
            Debug.Assert(Linked != null, "Portal must be linked to another portal.");
            return GetPortalMatrix(this, Linked);
        }

        /// <summary>
        /// Returns matrix to transform between one portals coordinate space to another
        /// </summary>
        public static Matrix4 GetPortalMatrix(Portal portalEnter, Portal portalExit)
        {
            //The portalExit is temporarily mirrored before getting the transformation matrix
            Transform2D transform = portalExit.GetWorldTransform();
            Vector2 v = transform.Scale;
            //if (portalEnter.IsMirrored != portalExit.IsMirrored)
            {
                transform.Scale = new Vector2(-v.X, v.Y);
            }
            Matrix4 m = portalEnter.GetWorldTransform().GetMatrix().Inverted() * transform.GetMatrix();
            transform.Scale = v;
            return m;
        }

        /// <summary>
        /// Returns a polygon in world space representing the 2D FOV through the portal.  
        /// Polygon is not guaranteed to be non-degenerate which can occur if the viewPoint is edge-on to the portal.
        /// </summary>
        public Vector2[] GetFov(Vector2 origin, float distance)
        {
            return GetFov(origin, distance, 10);
        }

        public Line[] GetFovLines(Vector2 origin, float distance)
        {
            Vector2[] vertices = GetFov(origin, distance);
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
        public Vector2[] GetFov(Vector2 viewPoint, float distance, int detail)
        {
            Matrix4 a = GetWorldTransform().GetMatrix();
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
