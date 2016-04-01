using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class EditorActor : EditorObject
    {
        public Entity Entity { get; private set; }

        public EditorActor(EditorScene editorScene)
            : base(editorScene)
        {
            Entity = new Entity(Scene.Scene);
            Model cube = ModelFactory.CreateCube();
            Entity.AddModel(cube);
        }

        public override IDeepClone ShallowClone()
        {
            return new EditorActor(Scene);
        }

        public override void SetTransform(Transform2 transform)
        {
            base.SetTransform(transform);
            Entity.SetTransform(transform);
        }

        public override void Remove()
        {
            base.Remove();
            Entity.Remove();
        }
    }
}
