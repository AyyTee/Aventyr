using Game;
using Game.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public class NodePortalable : SceneNode, IPortalable
    {
        public Transform2 Transform { get; set; } = new Transform2();
        public Transform2 Velocity { get; set; } = Transform2.CreateVelocity();
        public Action<EnterCallbackData, Transform2, Transform2> EnterPortal { get; set; }

        public NodePortalable(Scene scene)
            : base(scene)
        {
        }

        public List<IPortal> GetPortalChildren()
        {
            return Children.OfType<IPortal>().ToList();
        }

        public override Transform2 GetTransform()
        {
            return Transform.ShallowClone();
        }

        public override Transform2 GetVelocity()
        {
            return Velocity.ShallowClone();
        }

        public override void SetTransform(Transform2 transform)
        {
            Transform = transform.ShallowClone();
            base.SetTransform(transform);
        }

        public override void SetVelocity(Transform2 velocity)
        {
            Velocity = velocity.ShallowClone();
            base.SetVelocity(velocity);
        }
    }
}
