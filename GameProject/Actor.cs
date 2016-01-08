using FarseerPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Actor : SceneNodePlaceable
    {
        public Transform2D Velocity { get; private set; }
        public readonly int BodyId = -1;
        public Body Body
        {
            get
            {
                if (BodyId == -1)
                {
                    return null;
                }
                Debug.Assert(Scene.World.BodyList.Exists(item => (item.BodyId == BodyId)), "Body id does not exist.");
                return Scene.World.BodyList.Find(item => (item.BodyId == BodyId));
            }
        }

        public Actor(Scene scene, Body body)
            : base(scene)
        {
            Velocity = new Transform2D();
            BodyId = body.BodyId;
        }

        public override void Remove()
        {
            if (Body != null)
            {
                Scene.World.RemoveBody(Body);
            }
            base.Remove();
        }

        public void Step()
        {
            Transform2D transform = GetTransform();
            if (Body != null)
            {
                transform.Position = Vector2Ext.ConvertTo(Body.Position);
                transform.Rotation = Body.Rotation;
                Velocity.Position = Vector2Ext.ConvertTo(Body.LinearVelocity);
                Velocity.Rotation = Body.AngularVelocity;
            }
            else
            {
                transform.Position += Velocity.Position;
                transform.Rotation += Velocity.Rotation;
                transform.Scale *= Velocity.Scale;
            }
            SetTransform(transform);
        }

        public override void SetTransform(Transform2D transform)
        {
            base.SetTransform(transform);
            if (Body != null)
            {
                BodyExt.SetTransform(Body, transform);
            }
        }

        public void SetVelocity(Transform2D transform)
        {
            Velocity = transform.Clone();
        }

        public override SceneNode Clone(Scene scene)
        {
            Actor clone = new Actor(scene, Body);
            Clone(clone);
            return clone;
        }

        protected override void Clone(SceneNode destination)
        {
            base.Clone(destination);
            Actor destinationCast = (Actor)destination;
            destinationCast.Velocity = new Transform2D(Velocity);
        }

        /*public void SetBody(Body body)
        {
            if (Body != null)
            {
                Scene.World.RemoveBody(Body);
            }

            Transform2D transform = GetTransform();
            transform.UniformScale = true;
            SetTransform(transform);
            BodyUserData userData = new BodyUserData(this, body);
            Debug.Assert(body.UserData == null, "This body has UserData already assigned to it.");
            BodyId = body.BodyId;

            BodyExt.SetTransform(body, GetTransform());
            BodyExt.SetUserData(body, this);
        }*/
    }
}
