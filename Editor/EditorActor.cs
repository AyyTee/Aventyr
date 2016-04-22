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
    public sealed class EditorActor : EditorObject, IActor
    {
        public Body Body { get; private set; }

        public EditorActor(EditorScene editorScene)
            : base(editorScene)
        {
            Initialize();
        }

        public override void Initialize()
        {
            //Body = BodyExt.CreateBody(Scene.World, this);
            Body = ActorFactory.CreateBox(Scene.World, new Vector2(2,2));
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
            return BodyExt.GetTransform(Body);
        }

        

        /// <summary>
        /// Set the transform.  Scale is discarded since physics bodies do have a Scale field.
        /// </summary>
        public override void SetTransform(Transform2 transform)
        {
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
            models.Add(Game.ModelFactory.CreateCube(new Vector3(2,2,2)));
            return models;
        }
    }
}
