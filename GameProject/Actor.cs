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

        public override SceneNode Clone(Scene scene)
        {
            Actor clone = new Actor(scene, Body.DeepClone(scene.World));
            Clone(clone);
            return clone;
        }

        protected override void Clone(SceneNode destination)
        {
            base.Clone(destination);
            Actor destinationCast = (Actor)destination;
            BodyUserData bodyData = BodyExt.SetUserData(destinationCast.Body, destinationCast);
            foreach (Fixture f in destinationCast.Body.FixtureList)
            {
                FixtureUserData fixtureData = FixtureExt.SetUserData(f);
            }
        }

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
            Transform2D transform = GetTransform();
            transform.Position = Vector2Ext.ConvertTo(Body.Position);
            transform.Rotation = Body.Rotation;
            SetTransform(transform);
            Transform2D velocity = GetVelocity();
            velocity.Position = Vector2Ext.ConvertTo(Body.LinearVelocity);
            velocity.Rotation = Body.AngularVelocity;
            SetVelocity(velocity);
        }
    }
}
