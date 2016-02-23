using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public struct MementoDrag
    {
        public readonly ITransform2 Transformable;
        readonly Transform2 _transform;

        public MementoDrag(ITransform2 transformable)
        {
            Transformable = transformable;
            _transform = Transformable.GetTransform();
        }

        public void ResetTransform()
        {
            Transformable.SetTransform(_transform);
        }

        public Transform2 GetTransform()
        {
            return _transform.ShallowClone();
        }
    }
}
