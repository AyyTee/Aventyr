using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class Doodad : IRenderable, ITransform2
    {
        public bool DrawOverPortals { get { return false; } }
        public bool IsPortalable { get { return false; } }
        public bool Visible { get { return true; } }
        Transform2 _transform = new Transform2();
        public List<Model> Models = new List<Model>();

        public Doodad()
        {
        }

        public List<Model> GetModels()
        {
            return Models;
        }

        public Transform2 GetWorldTransform()
        {
            return GetTransform();
        }

        public Transform2 GetTransform()
        {
            return _transform.ShallowClone();
        }

        public void SetTransform(Transform2 transform)
        {
            _transform = transform.ShallowClone();
        }
    }
}
