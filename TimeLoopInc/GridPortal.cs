using Game.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Common;
using Game.Serialization;

namespace TimeLoopInc
{
    public class GridPortal : Entity, IPortal
    {
        public IPortal Linked { get; set; }

        public bool OneSided => true;

        public PortalPath Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Transform2 WorldTransform { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Transform2 WorldVelocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsPortalable => throw new NotImplementedException();

        public IPortalCommon Parent => throw new NotImplementedException();

        public List<IPortalCommon> Children => throw new NotImplementedException();

        public IScene Scene => throw new NotImplementedException();

        public HashSet<IDeepClone> GetCloneableRefs()
        {
            throw new NotImplementedException();
        }

        public Transform2 GetTransform()
        {
            throw new NotImplementedException();
        }

        public Transform2 GetVelocity()
        {
            throw new NotImplementedException();
        }

        public void Remove()
        {
            throw new NotImplementedException();
        }

        public IDeepClone ShallowClone()
        {
            throw new NotImplementedException();
        }

        public void UpdateRefs(IReadOnlyDictionary<IDeepClone, IDeepClone> cloneMap)
        {
            throw new NotImplementedException();
        }
    }
}
