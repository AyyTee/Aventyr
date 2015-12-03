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
            _marker.Models.Add(ModelFactory.CreateCircle(new Vector3(0, 0, 10), 0.05f, 10));
        }

        public void Remove()
        {
            Entity.Scene.RemoveEntity(Entity);
            _marker.Scene.RemoveEntity(_marker);
        }

        public override void SetTransform(Transform2D transform)
        {
            throw new NotImplementedException();
        }

        public override Transform2D GetTransform()
        {
            return Entity.Transform;
        }
    }
}
