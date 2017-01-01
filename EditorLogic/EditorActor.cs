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
using Game.Common;
using Game.Models;
using Game.Serialization;

namespace EditorLogic
{
    [DataContract]
    public sealed class EditorActor : EditorObject, IWall
    {
        [DataMember]
        public IList<Vector2> Vertices { get; private set; }

        public EditorActor(EditorScene editorScene)
            : this(editorScene, PolygonFactory.CreateRectangle(1, 1))
        {
            
        }

        public EditorActor(EditorScene editorScene, IList<Vector2> vertices)
            : base(editorScene)
        {
            Vertices = vertices;
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
            models.Add(GetActorModel(this));
            return models;
        }

        public IList<Vector2> GetWorldVertices()
        {
            return Vector2Ext.Transform(Vertices, GetWorldTransform().GetMatrix());
        }

        public Model GetActorModel(EditorActor actor)
        {
            Model model = Game.Rendering.ModelFactory.CreatePolygon(actor.Vertices);
            model.SetTexture(Scene.Renderer.GetTexture("default.png"));
            return model;
        }
    }
}
