using Game.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Common;
using Game.Serialization;
using Game.Rendering;

namespace TimeLoopInc
{
    public class GridPortal : Entity, IPortalRenderable
    {
        public IPortalRenderable Linked { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool OneSided => throw new NotImplementedException();

        public Transform2 WorldTransform => throw new NotImplementedException();

        public Transform2 GetWorldTransform()
        {
            throw new NotImplementedException();
        }
    }
}
