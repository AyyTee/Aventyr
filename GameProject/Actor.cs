using FarseerPhysics.Dynamics;
using Xna = Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Game
{
    /// <summary>A SceneNode with rigid body physics.</summary>
    [DataContract]
    public class Actor : SceneNode, IActor, IPortalable
    {
        [DataMember]
        public int BodyId { get; private set; }
        public Body Body { get; private set; }
        [DataMember]
        Vector2 _scale = new Vector2(1, 1);
        /// <summary>Collision polygon for physics and for portal placement.</summary>
        [DataMember]
        public IList<Vector2> Vertices { get; set; }

        public Actor(Scene scene, IList<Vector2> vertices)
            : this(scene, vertices, new Transform2())
        {
        }

        public Actor(Scene scene, IList<Vector2> vertices, Transform2 transform)
            : base(scene)
        {
            Vertices = vertices;
            _scale = transform.Scale;
            Body = ActorFactory.CreatePolygon(scene.World, transform, Vertices);
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
            Actor clone = new Actor(Scene, Vertices, GetTransform());
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
            Transform2 bodyTransform = BodyExt.GetTransform(Body);
            bodyTransform.SetScale(_scale);
            return bodyTransform;
        }

        /// <summary>
        /// Set the transform.  Scale is discarded since physics bodies do not have a Scale field.
        /// </summary>
        public void SetTransform(Transform2 transform)
        {
            BodyExt.SetTransform(Body, transform);
            if (_scale != transform.Scale)
            {
                Debug.Assert(false);
            }
            _scale = transform.Scale;
        }

        public override Transform2 GetVelocity()
        {
            return BodyExt.GetVelocity(Body);
        }

        public void SetVelocity(Transform2 velocity)
        {
            BodyExt.SetVelocity(Body, velocity);
        }

        public IList<Vector2> GetWorldVertices()
        {
            return Vector2Ext.Transform(Vertices, GetWorldTransform().GetMatrix());
        }
    }
}
