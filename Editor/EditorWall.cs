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
        //public Actor Wall { get; private set; }

        public EditorWall(EditorScene scene, IList<Vector2> vertices)
            : base(scene)
        {
            Vertices = new List<Vector2>(vertices);

        }

        public override IDeepClone ShallowClone()
        {
            EditorWall clone = new EditorWall(Scene, Vertices);
            base.ShallowClone(clone);
            return clone;
        }

        public override List<Model> GetModels()
        {
            List<Model> models = base.GetModels();
            models.Add(ModelFactory.CreatePolygon(Vertices));
            return models;
        }
    }
}
