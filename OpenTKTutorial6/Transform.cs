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
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Position { get; set; }
        public Matrix4 TransformMatrix;
        public bool IsChanged = true;
        public Transform()
        {
            Position = new Vector3();
            Rotation = new Vector3();
            Scale = new Vector3(1, 1, 1);
        }

        public Transform(Vector3 position)
        {
            Position = position;
            Rotation = new Vector3();
            Scale = new Vector3(1, 1, 1);
        }

        public Transform(Vector3 position, Vector3 scale)
        {
            Position = position;
            Scale = scale;
            Rotation = new Vector3();
        }

        public Transform(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
        }

        public Matrix4 GetMatrix()
        {
            TransformMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
            return TransformMatrix;
        }

        public static Transform operator +(Transform a, Transform b)
        {
            Transform c = new Transform();
            c.Position = a.Position + b.Position;
            c.Scale = a.Scale * b.Scale;
            c.Rotation = a.Rotation + b.Rotation;
            return c;
        }

        public static Transform operator *(Transform a, float b)
        {
            Transform c = new Transform();
            c.Position = a.Position * b;
            c.Scale = a.Scale * b;
            c.Rotation = a.Rotation * b;
            return c;
        }
    }
}
