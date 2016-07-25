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

namespace Game
{
    /// <summary>A SceneNode with rigid body physics.</summary>
    [DataContract, DebuggerDisplay("Actor {Name}")]
    public class Actor : SceneNode, IActor
    {
        [DataMember]
        public bool IsPortalable { get; set; }
        /// <summary>
        /// Physics rigid body associated with this Actor.
        /// </summary>
        public Body Body { get; private set; }
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
            IsPortalable = true;
            _vertices = vertices.ToArray();
            _scale = transform.Scale;
            Body = ActorFactory.CreatePolygon(Scene.World, transform, Vertices);
            BodyExt.SetUserData(Body, this);
        }

        [OnDeserialized]
        public void Deserialize(StreamingContext context)
        {
            Body = ActorFactory.CreatePolygon(Scene.World, _body.Transform, Vertices);
            BodyExt.SetUserData(Body, this);
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
            BodyUserData bodyData = BodyExt.SetUserData(destination.Body, destination);
            foreach (Fixture f in destination.Body.FixtureList)
            {
                FixtureUserData fixtureData = FixtureExt.SetUserData(f);
            }
        }

        public override void SetParent(SceneNode parent)
        {
            Debug.Assert(parent.IsRoot);
            base.SetParent(parent);
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
                Debug.Assert(!Scene.InWorldStep, "Scale cannot change during a physics step.");

                List<Xna.Vector2> contourPrev = Vector2Ext.ConvertToXna(ActorExt.GetFixtureContour(Vertices, GetTransform().Scale));
                _scale = transform.Scale;
                List<Xna.Vector2> contour = Vector2Ext.ConvertToXna(ActorExt.GetFixtureContour(Vertices, transform.Scale));

                foreach (Fixture f in Body.FixtureList)
                {
                    if (!FixtureExt.GetUserData(f).IsPortalParentless())
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
        public void SetVelocity(Transform2 velocity)
        {
            BodyExt.SetVelocity(Body, velocity);
        }

        /// <summary>
        /// Get world coordinates for collision mask.
        /// </summary>
        public IList<Vector2> GetWorldVertices()
        {
            Transform2 worldTransform = GetWorldTransform();
            Vector2[] worldVertices = Vector2Ext.Transform(Vertices, worldTransform.GetMatrix()).ToArray();
            return worldVertices;
            //return MathExt.SetWinding(worldVertices, MathExt.IsClockwise(Vertices));
        }

        public List<IPortal> GetPortalChildren()
        {
            return Children.OfType<IPortal>().ToList();
        }
    }
}
