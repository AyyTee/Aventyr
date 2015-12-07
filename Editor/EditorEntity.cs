using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class EditorEntity : EditorObject
    {
        public Entity Entity { get; private set; }
        Entity _marker;

        public EditorEntity(Scene scene)
        {
            Entity = new Entity(scene);
            _marker = new Entity(scene);
            _marker.Transform.Parent = Entity.Transform;
            Model circle = ModelFactory.CreateCircle(new Vector3(0, 0, 10), 0.05f, 10);
            circle.SetColor(new Vector3(0.8f, 0.8f, 0));
            _marker.Models.Add(circle);
        }

        public void Remove()
        {
            Entity.Scene.RemoveEntity(Entity);
            _marker.Scene.RemoveEntity(_marker);
        }

        public override void SetTransform(Transform2D transform)
        {
            Entity.Transform.SetLocal(transform);
        }

        public override Transform2D GetTransform()
        {
            return Entity.Transform;
        }
    }
}
