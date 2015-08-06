using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Transform
    {
        private Vector3 _position = new Vector3();
        private Quaternion _rotation = new Quaternion(0, 0, 1, 0);
        private Vector3 _scale = new Vector3(1, 1, 1);

        public Quaternion Rotation { get { return _rotation; } set { _rotation = value; } }
        public Vector3 Scale { get { return _scale; } set { _scale = value; } }
        public Vector3 Position { get { return _position; } set { _position = value; } }

        public Transform()
        {
        }

        public Transform(Vector3 position)
        {
            Position = position;
        }

        public Transform(Vector3 position, Vector3 scale)
        {
            Position = position;
            Scale = scale;
        }

        public Transform(Vector3 position, Vector3 scale, Quaternion rotation)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
        }

        public Matrix4 GetMatrix()
        {
            return Matrix4.CreateScale(Scale) * Matrix4.CreateFromAxisAngle(new Vector3(Rotation.X, Rotation.Y, Rotation.Z), Rotation.W) * Matrix4.CreateTranslation(Position);
        }

        public static Transform Lerp(Transform a, Transform b, float t)
        {
            Transform c = new Transform();
            c.Position = Vector3.Lerp(a.Position, b.Position, t);
            c.Scale = Vector3.Lerp(a.Scale, b.Scale, t);
            c.Rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t);
            return c;
        }

        /// <summary>
        /// Projects a copy of Transform projected onto the XY-plane, the rotation simply uses the Quaternion's W (theta) value
        /// </summary>
        /// <returns></returns>
        public Transform2D GetTransform2D()
        {
            return new Transform2D(new Vector2(Position.X, Position.Y), new Vector2(Scale.X, Scale.Y), Rotation.W);
        }
    }
}
