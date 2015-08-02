using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class Orientation
    {
        public double Rotation, Scale;
        public Vector2d Position;
        public bool Mirrored;
        public Orientation(Vector2d Position, double Rotation, double Scale, bool Mirrored)
        {
            _SetEverything(Position, Rotation, Scale, Mirrored);
        }
        public Orientation(Vector2d Position, double Rotation, double Scale)
        {
            _SetEverything(Position, Rotation, Scale, false);
        }
        public Orientation(Vector2d Position, double Rotation)
        {
            _SetEverything(Position, Rotation, 1, false);
        }
        public Orientation(Vector2d Position)
        {
            _SetEverything(Position, 0, 1, false);
        }
        private void _SetEverything(Vector2d Position, double Rotation, double Scale, bool Mirrored)
        {
            this.Position = Position;
            this.Rotation = Rotation;
            this.Scale = Scale;
            this.Mirrored = Mirrored;
        }
        public Matrix4d GetTransform()
        {
            Matrix4d M = Matrix4d.Identity;
            M *= Matrix4d.CreateRotationZ(Rotation);
            if (Mirrored)
            {
                M *= Matrix4d.Scale(-Scale, Scale, Scale);
            }
            else
            {
                M *= Matrix4d.Scale(Scale);
            }
            M *= Matrix4d.CreateTranslation(new Vector3d(Position));
            return M;
        }
    }
}
