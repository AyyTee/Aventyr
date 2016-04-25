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
    public sealed class EditorWall : EditorObject, IActor
    {
        [DataMember]
        public List<Vector2> Vertices { get; private set; }
        public Body Body { get; private set; }

        public EditorWall(EditorScene scene, IList<Vector2> vertices)
            : base(scene)
        {
            Vertices = new List<Vector2>(vertices);
            Initialize();
        }

        public override void Initialize()
        {
            Body = ActorFactory.CreatePolygon(Scene.World, new Transform2(), Vertices);
            BodyExt.SetUserData(Body, this);
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
            models.Add(GetWallModel());
            return models;
        }

        public Model GetWallModel()
        {
            return Game.ModelFactory.CreatePolygon(Vertices);
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
            Scene.World.RemoveBody(Body);
            base.Remove();
        }
    }
}
