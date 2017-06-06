using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;

namespace Game.Portals
{
    public class SimplePortal : IPortalRenderable
    {
        public IPortalRenderable Linked { get; set; }

        public bool OneSided { get; set; }

        public Transform2 WorldTransform { get; set; } = new Transform2();

        public Transform2 WorldVelocity { get; set; } = Transform2.CreateVelocity();

        public SimplePortal()
        {
        }

        public SimplePortal(Transform2 worldTransform)
        {
            WorldTransform = worldTransform;
        }
    }
}
