using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Models;
using Game.Rendering;
using System.Runtime.Serialization;

namespace EditorLogic
{
    [DataContract]
    public class Doodad : IRenderable, ITransformable2, ISceneObject
    {
        public bool DrawOverPortals => false;
        [DataMember]
        public bool IsPortalable { get; set; }
        [DataMember]
        public bool Visible { get; set; } = true;
        [DataMember]
        public string Name { get; set; } = "Doodad";
        [DataMember]
        Transform2 _transform = new Transform2();
        [DataMember]
        public List<Model> Models = new List<Model>();

        public Doodad(string name)
        {
            Name = name;
        }

        public Doodad(EditorScene scene, string name)
            : this(name)
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

        public void Remove()
        {
        }
    }
}
