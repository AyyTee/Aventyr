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
    public class Actor : SceneNode, IActor
    {
        public Transform2 Transform
        {
            get { return GetTransform(); }
            set { _setTransform(value); }
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
            Body.IsStatic = false;
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
            var local = Body.GetLocalPoint(Vector2Ext.ToXna(point));
            var bodyTree = Tree<BodyData>.GetAll(BodyExt.GetData(Body));
            foreach (BodyData data in bodyTree)
            {
                data.Body.ApplyForce(
                    Vector2Ext.ToXna(force * bodyTree.Count / Mass),
                    data.Body.GetWorldPoint(local));
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
            _setTransform(transform);
            base.SetTransform(transform);
        }

        private void _setTransform(Transform2 transform)
        {
            if (_scale != transform.Scale)
            {
                Debug.Assert(!Scene.InWorldStep, "Scale cannot change during a physics step.");

                List<Xna.Vector2> contourPrev = Vector2Ext.ToXna(ActorExt.GetFixtureContour(Vertices, GetTransform().Scale));
                _scale = transform.Scale;
                List<Xna.Vector2> contour = Vector2Ext.ToXna(ActorExt.GetFixtureContour(Vertices, transform.Scale));

                foreach (Fixture f in Body.FixtureList)
                {
                    if (!FixtureExt.GetData(f).IsPortalParentless())
                    {
                        continue;
                    }
                    PolygonShape shape = (PolygonShape)f.Shape;
                    //Make a copy of the vertices and manipulate those before assigning it back to the fixture.
                    //Modifying the vertices directly causes Farseer to not update internal values.
                    FarseerPhysics.Common.Vertices vertices = new FarseerPhysics.Common.Vertices(shape.Vertices);
                    for (int i = 0; i < shape.Vertices.Count; i++)
                    {
                        int verticeIndex = contourPrev.FindIndex(item => item == vertices[i]);
                        Debug.Assert(verticeIndex != -1);
                        vertices[i] = contour[verticeIndex];
                    }
                    shape.Vertices = vertices;
                    PolygonExt.SetInterior(shape.Vertices);
                }
            }
            BodyExt.SetTransform(Body, transform);
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
