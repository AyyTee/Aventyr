using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Editor
{
    [DataContract]
    public sealed class EditorActor : EditorObject, IWall
    {
        public IList<Vector2> Vertices { get; private set; }

        public EditorActor(EditorScene editorScene)
            : base(editorScene)
        {
            Initialize();
        }

        public override void Initialize()
        {
            Vertices = PolygonFactory.CreateRectangle();
        }

        public override IDeepClone ShallowClone()
        {
            EditorActor clone = new EditorActor(Scene);
            base.ShallowClone(clone);
            return clone;
        }

        public override List<Model> GetModels()
        {
            List<Model> models = base.GetModels();
            models.Add(GetActorModel());
            return models;
        }

        public IList<Vector2> GetWorldVertices()
        {
            return Vector2Ext.Transform(Vertices, GetWorldTransform().GetMatrix());
        }

        public Model GetActorModel()
        {
            Model model = Game.ModelFactory.CreateCube(new Vector3(1, 1, 1));
            model.SetTexture(Renderer.Textures["default.png"]);
            return model;
        }
    }
}
