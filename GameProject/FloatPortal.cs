using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class FloatPortal : Portal
    {
        //public Transform2D Transform { get; private set; }
        public Transform2D Velocity { get; private set; }
        public FloatPortal(Scene scene, Vector2 position)
            : base(scene)
        {
            SetTransform(new Transform2D(position));
            Velocity = new Transform2D();
            Velocity.UniformScale = true;
        }

        public FloatPortal(Scene scene)
            :this(scene, new Vector2())
        {
        }

        public override Placeable2D DeepClone()
        {
            return DeepClone(Scene);
        }

        public override Placeable2D DeepClone(Scene scene)
        {
            FloatPortal clone = new FloatPortal(scene);
            DeepClone(this, clone);
            return clone;
        }

        public static void DeepClone(FloatPortal source, FloatPortal destination)
        {
            Placeable2D.DeepClone(source, destination);
            destination.SetTransform(source.GetTransform());
            destination.Velocity = source.Velocity;
        }

        public override void Step()
        {
            base.Step();
            Transform2D transform = GetTransform();
            transform.Position += Velocity.Position;
            transform.Rotation += Velocity.Rotation;
            transform.Scale *= Velocity.Scale;
            SetTransform(transform);
        }

        public override Transform2D GetVelocity()
        {
            return Velocity;
        }
    }
}
