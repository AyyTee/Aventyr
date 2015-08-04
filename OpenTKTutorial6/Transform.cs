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
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Position { get; set; }
        public Matrix4 TransformMatrix;
        public bool IsChanged = true;
        public Transform()
        {
            Position = new Vector3();
            Rotation = new Quaternion();
            Scale = new Vector3(1, 1, 1);
        }

        public Transform(Vector3 position)
        {
            Position = position;
            Rotation = new Quaternion();
            Scale = new Vector3(1, 1, 1);
        }

        public Transform(Vector3 position, Vector3 scale)
        {
            Position = position;
            Scale = scale;
            Rotation = new Quaternion();
        }

        public Transform(Vector3 position, Vector3 scale, Quaternion rotation)
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

        public static Transform Lerp(Transform a, Transform b, float t)
        {
            Transform c = new Transform();
            c.Position = Vector3.Lerp(a.Position, b.Position, t);
            c.Scale = Vector3.Lerp(a.Scale, b.Scale, t);
            c.Rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t);
            return c;
        }
    }
}
