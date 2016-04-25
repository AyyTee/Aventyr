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
    public class Actor : SceneNode, IActor, IPortalable
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

        public override void Remove()
        {
            if (Body != null)
            {
                Scene.World.RemoveBody(Body);
            }
            base.Remove();
        }

        public override Transform2 GetTransform()
        {
            return BodyExt.GetTransform(Body);
        }

        /// <summary>
        /// Set the transform.  Scale is discarded since physics bodies do not have a Scale field.
        /// </summary>
        public void SetTransform(Transform2 transform)
        {
            BodyExt.SetTransform(Body, transform);
        }

        public override Transform2 GetVelocity()
        {
            return BodyExt.GetVelocity(Body);
        }

        public void SetVelocity(Transform2 velocity)
        {
            BodyExt.SetVelocity(Body, velocity);
        }
    }
}
