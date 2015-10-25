
using FarseerPhysics.Dynamics;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Game
{
    [Serializable]
    public abstract class Placeable2D
    {
        public int Id { get; private set; }
        public string Name { get; set; }

        public int BodyId = -1;
        public Body Body
        {
            get
            {
                if (BodyId == -1)
                {
                    return null;
                }
                Debug.Assert(Scene != null, "Entity must be assigned to a scene.");
                Debug.Assert(Scene.PhysWorld.BodyList.Exists(item => (item.BodyId == BodyId)), "Body id does not exist.");
                return Scene.PhysWorld.BodyList.Find(item => (item.BodyId == BodyId));
            }
        }
        
        private Transform2D _transform = new Transform2D();
        public Transform2D Transform
        {
            get { return _transform; }
        }

        private Scene _scene = null;
        public Scene Scene
        {
            get { return _scene; }
        }

        public Placeable2D()
        {
        }

        public Placeable2D(Scene scene)
        {
            _scene = scene;
            if (scene != null)
            {
                Id = scene.EntityIdCount;
            }
        }

        public void Step()
        {
            if (Body != null)
            {
                Transform.Position = VectorExt2.ConvertTo(Body.Position);
                Transform.Rotation = Body.Rotation;
            }
        }

        public void SetBody(Body body)
        {
            if (Body != null)
            {
                Scene.PhysWorld.RemoveBody(Body);
            }

            Transform.UniformScale = true;
            BodyUserData userData = new BodyUserData(this);
            Debug.Assert(body.UserData == null, "This body has UserData already assigned to it.");
            BodyId = body.BodyId;

            BodyExt.SetUserData(body, this);
        }
    }
}
