using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public struct MementoTransform2D
    {
        public ITransform2D Transformable;
        public Transform2D Transform;
        public MementoTransform2D(ITransform2D transformable)
        {
            Transformable = transformable;
            Transform = Transformable.GetTransform();
        }

        public void ResetTransform()
        {
            Transformable.SetTransform(Transform);
        }
    }
}
