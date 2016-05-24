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
        /// <summary>
        /// Physics rigid body associated with this Actor. This body's fixtures may be disposed and replaced over time. 
        /// </summary>
        public Body Body { get; private set; }
        [DataMember]
        Vector2 _scale = new Vector2(1, 1);
        [DataMember]
        Vector2[] _vertices;
        /// <summary>Copy of local coordinates for collision mask.</summary>
        public IList<Vector2> Vertices { get { return _vertices.ToList(); } }

        public Actor(Scene scene, IList<Vector2> vertices)
            : this(scene, vertices, new Transform2())
        {
        }

        public Actor(Scene scene, IList<Vector2> vertices, Transform2 transform)
            : base(scene)
        {
            _vertices = vertices.ToArray();
            _scale = transform.Scale;
            Body = ActorFactory.CreatePolygon(Scene.World, transform, Vertices);
            BodyExt.SetUserData(Body, this);
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

        public void SetTransform(Transform2 transform)
        {
            if (_scale != transform.Scale)
            {
                Debug.Assert(Scene.InWorldStep == false, "Scale cannot change during a physics step.");
                _scale = transform.Scale;
                
                
            }
            else
            {
                BodyExt.SetTransform(Body, transform);
            }
            _scale = transform.Scale;
        }

        public override Transform2 GetVelocity()
        {
            return BodyExt.GetVelocity(Body);
        }

        /// <summary>
        /// Set Actor's velocity.  The scale component is ignored.
        /// </summary>
        public void SetVelocity(Transform2 velocity)
        {
            BodyExt.SetVelocity(Body, velocity);
        }

        /// <summary>
        /// Get world coordinates for collision mask.
        /// </summary>
        public IList<Vector2> GetWorldVertices()
        {
            return Vector2Ext.Transform(Vertices, GetWorldTransform().GetMatrix());
        }
    }
}
