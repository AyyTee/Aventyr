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
    public sealed class EditorActor : EditorObject, IActor
    {
        public Body Body { get; private set; }
        [DataMember]
        float _size = 1;

        public EditorActor(EditorScene editorScene)
            : base(editorScene)
        {
            Initialize();
        }

        public override void Initialize()
        {
            Body = ActorFactory.CreateBox(Scene.World, new Vector2(1,1) * _size);
            Body.IsStatic = false;
            BodyExt.SetUserData(Body, this);
        }

        public override IDeepClone ShallowClone()
        {
            EditorActor clone = new EditorActor(Scene);
            base.ShallowClone(clone);
            return clone;
        }

        public override Transform2 GetTransform()
        {
            Transform2 transform = BodyExt.GetTransform(Body);
            transform.Size = _size;
            return transform;
        }

        public override void SetTransform(Transform2 transform)
        {
            if (transform.Size != _size)
            {
                float scaleFactor = transform.Size / _size;
                _size = transform.Size;
                var vertices = ((PolygonShape)Body.FixtureList[0].Shape).Vertices;
                for (int i = 0; i < vertices.Count; i++)
                {
                    vertices[i] = vertices[i] * scaleFactor;
                }
                /*for (int i = Body.FixtureList.Count - 1; i >= 0; i--)
                {
                    Body.DestroyFixture(Body.FixtureList[i]);
                }
                ActorFactory.CreateBox(Body, new Vector2(1, 1) * _size);*/
                //Scene.World.ProcessChanges();
            }
            BodyExt.SetTransform(Body, transform);
        }

        public override void Remove()
        {
            Debug.Assert(Body != null);
            Scene.World.RemoveBody(Body);
            base.Remove();
        }

        public override List<Model> GetModels()
        {
            List<Model> models = base.GetModels();
            models.Add(GetActorModel());
            return models;
        }

        public Model GetActorModel()
        {
            Model model = Game.ModelFactory.CreateCube(new Vector3(1, 1, 1));
            model.SetTexture(Renderer.Textures["default.png"]);
            return model;
        }
    }
}
