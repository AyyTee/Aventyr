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
    class Portal : Entity
    {
        bool FacingUp { get; set; }
        double _size = 1;
        public double Size { get { return _size; } set { _size = value; } }

        public Portal(bool FacingUp) : base (new Vector3())
        {
            this.FacingUp = FacingUp;

            Plane p = new Plane(Controller.Shaders["default"]);
            p.Transform.Scale = new Vector3(0.05f, 1, 1);
            Models.Add(p);
        }

        /// <summary>
        /// Returns a polygon representing the 2D FOV through the portal
        /// </summary>
        public PolygonOld GetVertices(Vector2 origin, float distance)
        {

        }

    }
}
