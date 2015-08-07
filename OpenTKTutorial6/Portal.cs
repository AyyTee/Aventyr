using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
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
        }

        public Portal(bool leftHanded)
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

        /// <summary>
        /// Returns a polygon representing the 2D FOV through the portal
        /// </summary>
        public Vector2[] GetFOV(Vector2 origin, float distance)
        {
            Matrix4 a = Transform.GetMatrix();
            int detail = 10;
            Vector2[] verts = new Vector2[detail + 2];
            Vector2[] portal = GetVerts();
            for (int i = 0; i < portal.Length; i++)
            {
                Vector4 b = Vector4.Transform(new Vector4(portal[i].X, portal[i].Y, 0, 1), a);
                verts[i] = new Vector2(b.X, b.Y);
            }
            //minumum distance in order to prevent self intersections
            float distanceMin = Math.Max((verts[0] - origin).Length, (verts[1] - origin).Length) + 0.01f;
            distance = Math.Max(distance, distanceMin);

            
            verts[verts.Length - 1] = (verts[0] - origin).Normalized() * distance + origin;
            verts[2] = (verts[1] - origin).Normalized() * distance + origin;
            //find the angle between the edges of the FOV
            double angle0 = MathExt.AngleLine(verts[verts.Length - 1], origin);
            double angle1 = MathExt.AngleLine(verts[2], origin);
            float diff = (float)MathExt.AngleDiff(angle0, angle1);
            Matrix2 Rot = Matrix2.CreateRotation(diff / (detail - 1));
            for (int i = 3; i < verts.Length - 1; i++)
            {
                verts[i] = MathExt.Matrix2Mult(verts[i - 1] - origin, Rot) + origin;
            }
            return verts.ToArray();
        }
    }
}
