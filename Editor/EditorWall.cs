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
    public class EditorWall : EditorObject
    {
        [DataMember]
        public List<Vector2> Vertices { get; private set; }
        public Actor Wall { get; private set; }

        public EditorWall(EditorScene scene, IList<Vector2> vertices)
            : base(scene)
        {
            Vertices = new List<Vector2>(vertices);
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            Debug.Assert(Wall == null);
            Wall = ActorFactory.CreateEntityPolygon(Scene.Scene, GetTransform(), Vertices);
            /*Wall = new Entity(Scene.Scene);
            Wall.AddModel(ModelFactory.CreatePolygon(Vertices));
            Wall.SetTransform(GetTransform());*/
        }

        public override IDeepClone ShallowClone()
        {
            EditorWall clone = new EditorWall(Scene, Vertices);
            base.ShallowClone(clone);
            return clone;
        }

        public override void Remove()
        {
            base.Remove();
            Wall.Remove();
        }

        public override void SetTransform(Transform2 transform)
        {
            base.SetTransform(transform);
            Wall.SetTransform(transform);
        }

        public override void SetScene(EditorScene destination)
        {
            base.SetScene(destination);
            SceneNode.SetScene(Wall, destination.Scene);
        }
    }
}
