using OpenTK;
using System;
using System.Diagnostics;

namespace Game
{
    public class Portal
    {
        private Portal _linked;
        private Transform2D _transform = new Transform2D();
        private bool _oneSided = false;

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
        /// It is nessesary to avoid situations where an entity can skip over a portal by sitting exactly on top of it.
        /// </summary>
        public const float EntityMinDistance = 0.001f;

        public Transform2D Transform
        {
          get { return _transform; }
          set { _transform = value; }
        }

        public Portal Linked
        {
            get { return _linked; }
        }

        public Portal()
        {
            Transform.FixedScale = true;
        }

        public Portal(bool leftHanded)
            : this()
        {
            SetFacing(leftHanded);
        }

        public Portal(Vector2 position)
            : this()
        {
            Transform.Position = position;
        }

        public Portal(Transform2D transform)
            : this()
        {
            Transform = transform;
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

        public static void Link(Portal portal0, Portal portal1)
        {
            portal0._linked = portal1;
            portal1._linked = portal0;
        }

        private void SetLink(Portal portal)
        {
            if (_linked != portal)
            {
                if (_linked != null)
                {
                    _linked.SetLink(null);
                }
                _linked = portal;
                if (_linked != null)
                {
                    _linked.SetLink(this);
                }
            }
        }

        public void Unlink(Portal portal)
        {
            SetLink(null);
        }

        /// <summary>
        /// Returns an array of two Vectors defining the Portals local location
        /// </summary>
        public Vector2[] GetVerts()
        {
            return new Vector2[] { new Vector2(0, 0.5f), new Vector2(0, -0.5f)};
        }

        public Vector2[] GetWorldVerts()
        {
            return VectorExt2.Transform(GetVerts(), Transform.GetMatrix());
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
            Matrix4 m = GetMatrix();
            Vector2 v0 = entityPos.Position;
            Vector2 v1 = v0 + new Vector2(1, 0);
            Vector2 v2 = v0 + new Vector2(0, 1);
            Vector2 v3 = VectorExt2.Transform(v0, m);
            Vector2 v4 = VectorExt2.Transform(v1, m);
            Vector2 v5 = VectorExt2.Transform(v2, m);

            entityPos.Position = new Vector2(v3.X, v3.Y);
            //position the entity slightly outside of the exit portal to avoid precision issues with portal collision checking
            /*Line exitLine = new Line(Linked.GetWorldVerts());
            float distanceToPortal = exitLine.PointDistance(v3, false);
            Debug.Assert(distanceToPortal > 0);
            if (distanceToPortal < EntityMinDistance)
            {
                Vector2 exitNormal = Linked.Transform.GetNormal();
                if (exitLine.GetSideOf(v3) != exitLine.GetSideOf(exitNormal))
                {
                    exitNormal = -exitNormal;
                }
                entityPos.Position += exitNormal * (EntityMinDistance - distanceToPortal);
            }*/

            Transform2D tEnter = Transform;
            Transform2D tExit = Linked.Transform;
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
            entityPos.Scale *= new Vector2(flipX * (v4 - v3).Length, flipY * (v5 - v3).Length);

            float angle;
            if (flipX != flipY)
            {
                entityPos.Rotation = -entityPos.Rotation;
                entityPos.Rotation += (float)(MathExt.AngleWrap(Transform.Rotation) + MathExt.AngleWrap(Linked.Transform.Rotation));
            }
            else
            {
                angle = Linked.Transform.Rotation - Transform.Rotation;
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
        public static Matrix4 GetMatrix(Portal portalEnter, Portal portalExit)
        {
            Vector2 v = portalExit.Transform.Scale;
            portalExit.Transform.Scale = new Vector2(-v.X, v.Y);
            Matrix4 m = portalEnter.Transform.GetMatrix().Inverted() * portalExit.Transform.GetMatrix();
            portalExit.Transform.Scale = v;
            return m;
        }

        public Matrix4 GetMatrix()
        {
            Debug.Assert(Linked != null, "Portal must be linked to another portal.");
            return GetMatrix(this, Linked);
        }

        /// <summary>
        /// Returns a polygon representing the 2D FOV through the portal.  If the polygon is degenerate then an array of length 0 will be returned.
        /// </summary>
        public Vector2[] GetFOV(Vector2 viewPoint, float distance, int detail)
        {
            Matrix4 a = Transform.GetMatrix();
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
