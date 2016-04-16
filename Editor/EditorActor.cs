using Game;
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
    public class EditorActor : EditorObject
    {
        public Entity Entity { get; private set; }

        public EditorActor(EditorScene editorScene)
            : base(editorScene)
        {
            Initialize();
        }

        public override void Initialize()
        {
            /*Debug.Assert(Entity == null);
            Entity = new Entity(Scene.Scene);
            Entity.SetTransform(GetTransform());
            Model cube = ModelFactory.CreateCube();
            Entity.AddModel(cube);*/
        }

        public override IDeepClone ShallowClone()
        {
            EditorActor clone = new EditorActor(Scene);
            base.ShallowClone(clone);
            return clone;
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

        public override void SetScene(EditorScene destination)
        {
            base.SetScene(destination);
            //SceneNode.SetScene(Entity, destination.Scene);
        }
    }
}
