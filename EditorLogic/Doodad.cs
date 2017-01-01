using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Models;
using Game.Rendering;

namespace EditorLogic
{
    public class Doodad : IRenderable, ITransformable2
    {
        public bool DrawOverPortals { get { return false; } }
        public bool IsPortalable { get; set; }
        public bool Visible { get; set; }
        Transform2 _transform = new Transform2();
        public List<Model> Models = new List<Model>();

        public Doodad()
        {
            Visible = true;
        }

        public Doodad(EditorScene scene)
            : this()
        {
            scene.Doodads.Add(this);
        }

        public List<Model> GetModels()
        {
            return Models;
        }

        public Transform2 GetWorldTransform(bool ignorePortals = false)
        {
            return GetTransform();
        }

        public Transform2 GetWorldVelocity(bool ignorePortals = false)
        {
            return new Transform2();
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
