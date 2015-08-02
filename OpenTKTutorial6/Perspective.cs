using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class Perspective
    {
        public Orientation Orient;
        public Vector2d POV;
        public Perspective(Vector2d Position)
        {
            Orient = new Orientation(Position);
            this.POV = Position;
        }
        public Perspective(Vector2d Position, Vector2d POV)
        {
            Orient = new Orientation(Position);
            this.POV = POV;
        }
        public Perspective(Vector2d Position, double Scale)
        {
            Orient = new Orientation(Position, 0, Scale);
        }
        public void SetOrientation(Vector2d Position, double Rotation, double Scale, bool Mirrored) 
        {
            Orient.Position = Position;
            Orient.Rotation = Rotation;
            Orient.Scale = Scale;
            Orient.Mirrored = Mirrored;
        }
        public Matrix4d GetTransform()
        {
            Matrix4d M = Matrix4d.Identity;
            M *= Matrix4d.CreateTranslation(new Vector3d(Orient.Position));
            M *= Matrix4d.CreateRotationZ(Orient.Rotation);
            if (Orient.Mirrored == false)
            {
                M *= Matrix4d.Scale(Orient.Scale);
            }
            else
            {
                M *= Matrix4d.Scale(-Orient.Scale, Orient.Scale, Orient.Scale);
            }
            
            return M;
        }
        public Matrix4d GetTransformInverse()
        {
            return GetTransform().Inverted();
        }
    }
}
