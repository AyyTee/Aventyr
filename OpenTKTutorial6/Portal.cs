using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Portal : Entity
    {
        public Portal()
        {
            Models.Add(Model.CreatePlane());
            Models[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            Models[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            Models.Add(Model.CreatePlane());
            Models[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);
        }

        public Portal(bool leftHanded) : this()
        {
            SetFacing(leftHanded);
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

        /// <summary>
        /// Returns an array of two Vectors defining the Portals local location
        /// </summary>
        public Vector2[] GetVerts()
        {
            return new Vector2[] { new Vector2(0, 0.5f), new Vector2(0, -0.5f)};
        }

        public Vector2[] GetFOV(Vector2 origin, float distance)
        {
            return GetFOV(origin, distance, 10);
        }

        /*public static Transform2D GetTransform(Portal portalEnter, Portal portalExit)
        {
            Transform2D t = new Transform2D();
            Transform2D tEnter = portalEnter.Transform;
            Transform2D tExit = portalExit.Transform;
            t.Rotation = tExit.Rotation - tEnter.Rotation;
            t.Scale = Vector2.Divide(tExit.Scale, tEnter.Scale);
            Matrix2 m = Matrix2.CreateRotation(t.Rotation) * Matrix2.CreateScale(t.Scale);
            t.Position = tExit.Position - MathExt.Matrix2Mult(tEnter.Position, m);
            return t;
        }*/

        /// <summary>
        /// Returns matrix to transform between one portals coordinate space to another
        /// </summary>
        public static Matrix4 GetMatrix(Portal portalEnter, Portal portalExit)
        {
            return portalEnter.Transform.GetMatrix().Inverted() * portalExit.Transform.GetMatrix();
        }

        /// <summary>
        /// Returns a polygon representing the 2D FOV through the portal.  If the polygon is degenerate then an array of length 0 will be returned.
        /// </summary>
        public Vector2[] GetFOV(Vector2 origin, float distance, int detail)
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
            float distanceMin = Math.Max((verts[0] - origin).Length, (verts[1] - origin).Length) + errorMargin;
            distance = Math.Max(distance, distanceMin);
            //get the leftmost and rightmost edges of the FOV
            verts[verts.Length - 1] = (verts[0] - origin).Normalized() * distance + origin;
            verts[2] = (verts[1] - origin).Normalized() * distance + origin;
            //find the angle between the edges of the FOV
            double angle0 = MathExt.AngleLine(verts[verts.Length - 1], origin);
            double angle1 = MathExt.AngleLine(verts[2], origin);
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
                verts[i] = MathExt.Matrix2Mult(verts[i - 1] - origin, Rot) + origin;
            }
            return verts;
        }
    }
}
