using FarseerPhysics.Dynamics;
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
    public sealed class EditorWall : EditorObject, IWall
    {
        [DataMember]
        public IList<Vector2> Vertices { get; private set; }
        Model _wallModel;

        public EditorWall(EditorScene scene, IList<Vector2> vertices)
            : base(scene)
        {
            Vertices = new List<Vector2>(vertices);
            Initialize();
        }

        public override void Initialize()
        {
            _wallModel = Game.ModelFactory.CreatePolygon(Vertices);
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
            models.Add(_wallModel);
            return models;
        }

        public IList<Vector2> GetWorldVertices()
        {
            return Vector2Ext.Transform(Vertices, GetWorldTransform().GetMatrix());
        }
    }
}
