using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using FarseerPhysics.Dynamics;
using Game.Common;
using Game.Portals;
using Game.Serialization;
using OpenTK;
using Vector2 = OpenTK.Vector2;
using Vector3 = OpenTK.Vector3;
using Xna = Microsoft.Xna.Framework;

namespace Game.Physics
{
    /// <summary>A SceneNode with rigid body physics.</summary>
    [DataContract, DebuggerDisplay(nameof(Actor) + " {" + nameof(Name) + "}")]
    public class Actor : SceneNode, IWall, IPortalable
    {
        public delegate void OnCollisionHandler(Actor collidingWith, bool firstEvent);
        public event OnCollisionHandler OnCollision;

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
        public bool IsSensor { get; private set; }
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
        public IList<Vector2> Vertices => _vertices.ToList();

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
        }

        //TODO: Fix serialization for scenes.
        //[OnDeserialized]
        //public void Deserialize(StreamingContext context)
        //{
        //    Body = Factory.CreatePolygon(Scene.World, _body.Transform, Vertices);
        //    BodyExt.SetData(Body, this);
        //    BodyExt.SetVelocity(Body, _body.Velocity);
        //}

        //[OnSerializing]
        //public void Serialize(StreamingContext context)
        //{
        //    _body = new BodyMemento(Body);
        //}

        public override IDeepClone ShallowClone()
        {
            var clone = new Actor(Scene, Vertices, GetTransform());
            ShallowClone(clone);
            return clone;
        }

        protected void ShallowClone(Actor destination)
        {
            base.ShallowClone(destination);
            BodyExt.SetData(destination.Body, destination);
            foreach (Fixture f in destination.Body.FixtureList)
            {
                FixtureExt.SetData(f);
            }
        }

        public override void SetParent(SceneNode parent)
        {
            Debug.Assert(parent == null, "Actor must be the root SceneNode.");
            base.SetParent(parent);
        }

        public void SetCollisionCategory(Category category)
        {
            foreach (BodyData data in Tree<BodyData>.GetAll(BodyExt.GetData(Body)))
            {
                data.Body.CollisionCategories = category;
            }
        }

        public void SetCollidesWith(Category category)
        {
            foreach (BodyData data in Tree<BodyData>.GetAll(BodyExt.GetData(Body)))
            {
                data.Body.CollidesWith = category;
            }
        }

        public void CallOnCollision(Actor collidingWith, bool firstEvent)
        {
            OnCollision?.Invoke(collidingWith, firstEvent);
        }

        public float GetMass()
        {
            float mass = 0;
            foreach (BodyData data in Tree<BodyData>.GetAll(BodyExt.GetData(Body)))
            {
                mass += BodyExt.GetLocalMassData(data.Body).Mass;
            }
            return mass;
        }

        /// <summary>
        /// Returns the center of mass in world coordinates.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCentroid()
        {
            var centroid = new Vector2();
            float massTotal = 0;
            foreach (BodyData data in Tree<BodyData>.GetAll(BodyExt.GetData(Body)))
            {
                var massData = BodyExt.GetLocalMassData(data.Body);
                centroid += UndoPortalTransform(data, new Transform2(massData.Centroid)).Position * massData.Mass;
                massTotal += massData.Mass;
            }
            Debug.Assert(massTotal == GetMass());
            centroid /= massTotal;
            return centroid;
        }

        public void SetBodyType(BodyType type)
        {
            BodyType = type;
            _setBodyType(Body, type);
        }

        void _setBodyType(Body body, BodyType type)
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
        }

        public override void Remove()
        {
            Scene sceneTemp = Scene;
            base.Remove();
            if (Body != null)
            {
                sceneTemp.World.RemoveBody(Body);
                Body = null;
            }
        }

        /// <summary>
        /// Applies a force at the center of mass.
        /// </summary>
        /// <param name="force">The force.</param>
        public void ApplyForce(Vector2 force)
        {
            _applyForce(force, (Vector2)Body.GetWorldPoint(new Xna.Vector2()));
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

        void _applyForce(Vector2 force, Vector2 point)
        {
            Body.ApplyForce((Xna.Vector2)force, (Xna.Vector2)point);
        }

        public void ApplyTorque(float torque)
        {
            Body.ApplyTorque(torque);
        }

        public void ApplyGravity(Vector2 force)
        {
            foreach (BodyData data in Tree<BodyData>.GetAll(BodyExt.GetData(Body)))
            {
                var massData = BodyExt.GetLocalMassData(data.Body);
                data.Body.ApplyForce(
                    (Xna.Vector2)force * massData.Mass,
                    (Xna.Vector2)massData.Centroid);
            }
        }

        public void SetSensor(bool isSensor)
        {
            IsSensor = isSensor;
            foreach (BodyData data in Tree<BodyData>.GetAll(BodyExt.GetData(Body)))
            {
                data.Body.IsSensor = isSensor;
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

        void _setTransform(Body body, Transform2 transform, bool checkScale = true)
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

        /// <summary>
        /// Returns polygon that is the local polygon with only the local transforms Scale component applied. 
        /// This is useful because the vertices should match up with vertices in the physics fixtures for this Actor's body (within rounding errors).
        /// </summary>
        public static List<Vector2> GetFixtureContour(Actor actor)
        {
            return GetFixtureContour(actor.Vertices, actor.GetTransform().Scale);
        }

        public static List<Vector2> GetFixtureContour(IList<Vector2> vertices, Vector2 scale)
        {
            Debug.Assert(scale.X != 0 && scale.Y != 0);
            Matrix4 scaleMat = Matrix4.CreateScale(new Vector3(scale));
            List<Vector2> contour = Vector2Ext.Transform(vertices, scaleMat);
            if (Math.Sign(scale.X) != Math.Sign(scale.Y))
            {
                contour.Reverse();
            }
            return contour;
        }

        public static void AssertTransform(Actor actor)
        {
            /*Bodies don't have a scale component so we use the default scale when comparing the Actor's
             * scale to that of the child bodies.*/
            Transform2 actorTransform = actor.WorldTransform;
            actorTransform.SetScale(Vector2.One);

            /*foreach (BodyData data in Tree<BodyData>.GetAll(BodyExt.GetData(actor.Body)))
            {
                Transform2 bodyTransform = UndoPortalTransform(data, BodyExt.GetTransform(data.Body));
                bodyTransform.SetScale(Vector2.One);
                Debug.Assert(bodyTransform.AlmostEqual(actorTransform, 0.01f, 0.01f));
            }*/
        }

        static Transform2 UndoPortalTransform(BodyData data, Transform2 transform)
        {
            Transform2 copy = transform.ShallowClone();
            if (data.Parent == null)
            {
                return copy;
            }
            return UndoPortalTransform(
                data.Parent,
                copy.Transform(Portal.GetLinkedTransform(data.BodyParent.Portal).Inverted()));
        }

        /// <summary>
        /// Verifies the BodyType for Actor bodies is correct.
        /// </summary>
        /// <returns></returns>
        public static void AssertBodyType(Actor actor)
        {
            if (actor.Body.BodyType != actor.BodyType)
            {
                Debug.Fail("");
            }
            foreach (BodyData data in BodyExt.GetData(actor.Body).Children)
            {
                _assertBodyType(data);
            }
        }

        static void _assertBodyType(BodyData bodyData)
        {
            Debug.Assert(
                (bodyData.Body.BodyType == BodyType.Dynamic && bodyData.Actor.BodyType == BodyType.Dynamic) ||
                (bodyData.Body.BodyType == BodyType.Kinematic && bodyData.Actor.BodyType != BodyType.Dynamic));
            foreach (BodyData data in bodyData.Children)
            {
                _assertBodyType(data);
            }
        }
    }
}
