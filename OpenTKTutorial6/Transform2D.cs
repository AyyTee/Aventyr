using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    public class Transform2D
    {
        private Vector2 _position = new Vector2();
        private float _rotation = 0;
        private Vector2 _scale = new Vector2(1, 1);

        public float Rotation { get { return _rotation; } set { _rotation = value; } }
        public Vector2 Scale { get { return _scale; } set { _scale = value; } }
        public Vector2 Position { get { return _position; } set { _position = value; } }

        public Transform2D()
        {
        }

        public Transform2D(Vector2 position)
        {
            Position = position;
        }

        public Transform2D(Vector2 position, Vector2 scale)
        {
            Position = position;
            Scale = scale;
        }

        public Transform2D(Vector2 position, Vector2 scale, float rotation)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
        }

        public Transform Get3D()
        {
            return new Transform(new Vector3(Position.X, Position.Y, 0), new Vector3(Scale.X, Scale.Y, 1), new Quaternion(0, 0, 1, Rotation));
        }

        public Matrix4 GetMatrix()
        {
            return Matrix4.CreateScale(new Vector3(Scale.X, Scale.Y, 1)) * Matrix4.CreateRotationZ(Rotation) * Matrix4.CreateTranslation(new Vector3(Position.X, Position.Y, 0));

        }
    }
}
