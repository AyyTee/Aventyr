using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    [DataContract]
    public abstract class EditorObject : SceneNodePlaceable
    {
        [DataMember]
        public Entity Marker { get; private set; }
        bool _isSelected = false;
        public bool IsSelected { get { return _isSelected; } }

        public EditorObject(Scene scene)
            : base(scene)
        {
            Marker = new Entity(Scene);
            Marker.SetParent(this);
            Marker.DrawOverPortals = true;
            Model circle = ModelFactory.CreateCircle(new Vector3(0, 0, 10), 0.05f, 10);
            circle.SetColor(new Vector3(1f, 0.5f, 0f));
            Marker.AddModel(circle);
        }

        protected override void DeepCloneFinalize(Dictionary<SceneNode, SceneNode> cloneMap)
        {
            base.DeepCloneFinalize(cloneMap);
            EditorObject clone = (EditorObject)cloneMap[this];
            clone.Marker = (Entity)cloneMap[Marker];
        }

        public override void SetTransform(Transform2D transform)
        {
            base.SetTransform(transform);
        }

        public override void SetPosition(Vector2 position)
        {
            base.SetPosition(position);
        }

        public virtual void SetSelected(bool isSelected)
        {
            _isSelected = isSelected;
            if (_isSelected)
            {
                Marker.ModelList[0].SetColor(new Vector3(1f, 1f, 0f));
                Marker.ModelList[0].Transform.Scale = new Vector3(1.5f, 1.5f, 1.5f);
            }
            else
            {
                Marker.ModelList[0].SetColor(new Vector3(1f, 0.5f, 0f));
                Marker.ModelList[0].Transform.Scale = new Vector3(1f, 1f, 1f);
            }
        }
    }
}
