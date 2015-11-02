using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class FloatPortal : Portal
    {
        public Transform2D Transform { get; set; }
        public override Entity EntityParent
        {
            get
            {
                return null;
            }
        }
        public FloatPortal(Scene scene)
            : base(scene)
        {
            Transform = new Transform2D();
        }

        public override Transform2D GetTransform()
        {
            return Transform;
        }
    }
}
