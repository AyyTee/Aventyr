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
            List<Vector2> verts = new List<Vector2>();
            foreach (Vector2 v in GetVerts())
            {
                Vector4 b = Vector4.Transform(new Vector4(v.X, v.Y, 0, 1), a);
                verts.Add(new Vector2(b.X, b.Y));
            }
            return verts.ToArray();
        }
    }
}
