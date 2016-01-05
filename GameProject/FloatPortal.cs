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
        public FloatPortal(Scene scene, Vector2 position)
            : base(scene)
        {
        }

        public FloatPortal(Scene scene)
            :this(scene, new Vector2())
        {
        }

        public override SceneNode DeepClone()
        {
            return DeepClone(Scene);
        }

        public override SceneNode DeepClone(Scene scene)
        {
            FloatPortal clone = new FloatPortal(scene);
            DeepClone(this, clone);
            return clone;
        }

        protected static void DeepClone(FloatPortal source, FloatPortal destination)
        {
            SceneNode.DeepClone(source, destination);
        }

        public override Transform2D GetVelocity()
        {
            return new Transform2D();
        }
    }
}
