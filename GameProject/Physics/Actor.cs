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
using FarseerPhysics.Collision.Shapes;
using Game.Portals;
using Game.Physics;

namespace Game
{
    /// <summary>A SceneNode with rigid body physics.</summary>
    [DataContract, DebuggerDisplay("Actor {Name}")]
    public class Actor : SceneNode, IWall, ISceneObject, IPortalable
    {
        public Transform2 Transform
        {
            get { return GetTransform(); }
            set { SetTransform(value); }
        }
        public Transform2 Velocity
        {
            get { return GetVelocity(); }
            set { BodyExt.SetVelocity(Body, value); }
        }
        /// <summary>
        /// Physics rigid body associated with this Actor.
        /// </summary>
        public Body Body { get; private set; }
        [DataMember]
        public float Mass { get; private set; } = 1;
        [DataMember]
        public BodyType BodyType { get; private set; }
        [DataMember]
        Vector2 _scale = new Vector2(1, 1);
        /// <summary>
        /// Used for storing body data when serialized.
        /// </summary>
        [DataMember]
        BodyMemento _body;
        [DataMember]
        Vector2[] _vertices;
        /// <summary>Copy of local coordinates for collision mask.</summary>
        public IList<Vector2> Vertices { get { return _vertices.ToList(); } }
        [DataMember]
        public Action<EnterCallbackData, Transform2, Transform2> EnterPortal { get; set; }

        public Actor(Scene scene, IList<Vector2> vertices)
            : this(scene, vertices, new Transform2())
        {
        }

        public Actor(Scene scene, IList<Vector2> vertices, Transform2 transform)
            : base(scene)
        {
            _vertices = vertices.ToArray();
            _scale = transform.Scale;
            Body = Factory.CreatePolygon(Scene.World, transform, Vertices);
            BodyExt.SetData(Body, this);
            SetBodyType(BodyType.Dynamic);
            SetMass(Body.Mass);
        }

        [OnDeserialized]
        public void Deserialize(StreamingContext context)
        {
            Body = Factory.CreatePolygon(Scene.World, _body.Transform, Vertices);
            BodyExt.SetData(Body, this);
            BodyExt.SetVelocity(Body, _body.Velocity);
        }

        [OnSerializing]
        public void Serialize(StreamingContext context)
        {
            _body = new BodyMemento(Body);
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
            BodyData bodyData = BodyExt.SetData(destination.Body, destination);
            foreach (Fixture f in destination.Body.FixtureList)
            {
                FixtureData fixtureData = FixtureExt.SetData(f);
            }
        }

        public override void SetParent(SceneNode parent)
        {
            Debug.Assert(parent == null, "Actor must be the root SceneNode.");
            base.SetParent(parent);
        }

        public void SetMass(float mass)
        {
            Mass = mass;
            BodyExt.GetData(Body).SetMass(mass);
        }

        public void SetBodyType(BodyType type)
        {
            BodyType = type;
            _setBodyType(Body, type);
        }

        private void _setBodyType(Body body, BodyType type)
        {
            Debug.Assert(!Scene.InWorldStep);
            body.BodyType = type;
            foreach (var b in BodyExt.GetData(body).BodyChildren)
            {
                _setBodyType(b.Body, BodyType == BodyType.Dynamic ? BodyType.Dynamic : BodyType.Kinematic);
            }
        }

        public void Update()
        {
            BodyExt.GetData(Body).Update();
            SetMass(Mass);
        }

        public override void Remove()
        {
            if (Body != null)
            {
                Scene.World.RemoveBody(Body);
            }
            base.Remove();
        }

        /// <summary>
        /// Applies a force at the center of mass.
        /// </summary>
        /// <param name="force">The force.</param>
        public void ApplyForce(Vector2 force)
        {
            _applyForce(force, Vector2Ext.ToOtk(Body.GetWorldPoint(new Xna.Vector2())));
        }

        /// <summary>
        /// Apply a force at a world point. If the force is not
        /// applied at the center of mass, it will generate a torque and
        /// affect the angular velocity. This wakes up the body.
        /// </summary>
        /// <param name="force">The world force vector, usually in Newtons (N).</param>
        /// <param name="point">The world position of the point of application.</param>
        public void ApplyForce(Vector2 force, Vector2 point)
        {
            _applyForce(force, point);
        }

        private void _applyForce(Vector2 force, Vector2 point)
        {
            Body.ApplyForce(Vector2Ext.ToXna(force), Vector2Ext.ToXna(point));
        }

        public void ApplyGravity(Vector2 force)
        {
            var bodyTree = Tree<BodyData>.GetAll(BodyExt.GetData(Body));
            foreach (BodyData data in bodyTree)
            {
                var massData = BodyExt.GetLocalMassData(data.Body);
                
                data.Body.ApplyForce(
                    Vector2Ext.ToXna(force * massData.Mass),
                    Vector2Ext.ToXna(massData.Centroid));
            }
        }

        public override Transform2 GetTransform()
        {
            Transform2 bodyTransform = BodyExt.GetTransform(Body);
            bodyTransform.SetScale(_scale);
            return bodyTransform;
        }

        public override void SetTransform(Transform2 transform)
        {
            _setTransform(Body, transform);
            _scale = transform.Scale;
            base.SetTransform(transform);
        }

        private void _setTransform(Body body, Transform2 transform, bool checkScale = true)
        {
            if (checkScale && _scale != transform.Scale)
            {
                Debug.Assert(!Scene.InWorldStep, "Scale cannot change during a physics step.");

                BodyExt.ScaleFixtures(body, transform.Scale);
            }
            BodyExt.SetTransform(body, transform);

            foreach (BodyData data in BodyExt.GetData(body).Children)
            {
                _setTransform(data.Body, Portal.Enter(data.BodyParent.Portal, transform));
            }
        }

        public override Transform2 GetVelocity()
        {
            return BodyExt.GetVelocity(Body);
        }

        /// <summary>
        /// Set Actor's velocity.  The scale component is ignored.
        /// </summary>
        public override void SetVelocity(Transform2 velocity)
        {
            BodyExt.SetVelocity(Body, velocity);
            base.SetVelocity(velocity);
        }

        /// <summary>
        /// Get world coordinates for collision mask.
        /// </summary>
        public IList<Vector2> GetWorldVertices()
        {
            Vector2[] worldVertices = Vector2Ext.Transform(Vertices, GetWorldTransform().GetMatrix()).ToArray();
            return worldVertices;
        }

        public List<IPortal> GetPortalChildren()
        {
            return Children.OfType<IPortal>().ToList();
        }
    }
}
