using FarseerPhysics.Dynamics;
using Xna = Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public class Actor : SceneNodePlaceable
    {
        [DataMember]
        public int BodyId { get; private set; }
        public Body Body { get; private set; }

        public Actor(Scene scene, Body body)
            : base(scene)
        {
            SetBody(body);
            BodyExt.SetUserData(Body, this);
        }

        public void SetBody(Body body)
        {
            Debug.Assert(body != null, "Actor must be assigned a Body.");
            Body = body;
            BodyId = body.BodyId;
        }

        public override IDeepClone ShallowClone()
        {
            Actor clone = new Actor(Scene, Body.DeepClone(Scene.World));
            ShallowClone(clone);
            return clone;
        }

        protected void ShallowClone(Actor destination)
        {
            base.ShallowClone(destination);
            BodyUserData bodyData = BodyExt.SetUserData(destination.Body, destination);
            foreach (Fixture f in destination.Body.FixtureList)
            {
                FixtureUserData fixtureData = FixtureExt.SetUserData(f);
            }
        }

        /*public override void SetScene(Scene scene)
        {
            Debug.Assert(scene != null);
            Body BodyClone = Body.DeepClone(scene.World);
            Scene.World.RemoveBody(Body);
            Body = BodyClone;
            base.SetScene(scene);
        }*/

        public override void Remove()
        {
            if (Body != null)
            {
                Scene.World.RemoveBody(Body);
            }
            base.Remove();
        }

        public override void StepBegin()
        {
            base.StepBegin();
            Xna.Vector2 v0 = Vector2Ext.ConvertToXna(GetWorldTransform().Position);
            Body.SetTransform(v0, GetWorldTransform().Rotation);
            Body.LinearVelocity = Vector2Ext.ConvertToXna(GetVelocity().Position);
            Body.AngularVelocity = GetVelocity().Rotation;
        }

        public override void StepEnd()
        {
            base.StepEnd();
            Transform2 transform = GetTransform();
            transform.Position = Vector2Ext.ConvertTo(Body.Position);
            transform.Rotation = Body.Rotation;
            SetTransform(transform);
            Transform2 velocity = GetVelocity();
            velocity.Position = Vector2Ext.ConvertTo(Body.LinearVelocity);
            velocity.Rotation = Body.AngularVelocity;
            SetVelocity(velocity);
        }
    }
}
