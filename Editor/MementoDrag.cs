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
        public readonly ITransform2D Transformable;
        readonly Transform2D _transform;

        public MementoDrag(ITransform2D transformable)
        {
            Transformable = transformable;
            _transform = Transformable.GetTransform();
        }

        public void ResetTransform()
        {
            Transformable.SetTransform(_transform);
        }

        public Transform2D GetTransform()
        {
            return new Transform2D(_transform);
        }
    }
}
