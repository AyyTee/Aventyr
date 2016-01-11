using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xna = Microsoft.Xna.Framework;

namespace Game
{
    [DataContract]
    public class FixturePortal : Portal, IVertices2D
    {
        [DataMember]
        public FixtureEdgeCoord Position { get; private set; }
        public Fixture CollisionFixtureNext;
        public Fixture CollisionFixturePrevious;
        public Fixture FixtureParent {
            get
            {
                if (Position != null)
                {
                    return Position.Fixture;
                }
                return null;
            }
        }
        
        public const float EdgeMargin = 0.02f;
        public const float CollisionMargin = 0.1f;

        public FixturePortal(Scene scene, FixtureEdgeCoord position)
            : base(scene)
        {
            if (position != null)
            {
                SetFixtureParent(position);
            }
        }

        public override SceneNode Clone(Scene scene)
        {
            return new FixturePortal(scene, null);
        }

        protected override void Clone(SceneNode destination)
        {
            base.Clone(destination);
        }

        /// <summary>
        /// Returns a copy of the Transform local to the Body this is attached to.
        /// </summary>
        /// <returns></returns>
        public override Transform2D GetTransform()
        {
            Transform2D transform = new Transform2D();
            if (Position == null)
            {
                return transform;
            }
            Line edge = Position.GetEdge();
            transform.Position = edge.Lerp(Position.EdgeT);
            transform.Rotation = -edge.Angle() + (float)Math.PI/2;
            if (IsMirrored)
            {
                transform.Scale = new Vector2(1, -1);
            }
            return transform;
        }

        protected override bool _isValid()
        {
            return base._isValid() && Position != null;
        }

        public override Transform2D GetVelocity()
        {
            Transform2D transform = new Transform2D();
            if (Position == null)
            {
                return transform;
            }
            
            //transform.Parent = FixtureExt.GetUserData(Position.Fixture).Entity.GetTransform();
            return transform;
        }

        public override void Remove()
        {
            RemoveFixture();
            base.Remove();
        }

        private void RemoveFixture()
        {
            if (Position != null)
            {
                FixtureExt.GetUserData(Position.Fixture).RemovePortal(this);
            }
        }

        public void SetFixtureParent(FixtureEdgeCoord position)
        {
            RemoveFixture();
            Position = position;
            Debug.Assert(position.Entity != null);
            Debug.Assert(position.Fixture != null);
            if (Position != null)
            {
                SetParent(Position.Entity);
                FixtureExt.GetUserData(Position.Fixture).AddPortal(this);
                //wake up all the bodies so that they will fall if there is now a portal entrance below them
                foreach (Body b in Scene.World.BodyList)
                {
                    b.Awake = true;
                }
            }
        }

        public Vector2[] GetBounds(float margin)
        {
            float width, height;
            width = (float)Math.Abs(Math.Cos(GetTransform().Rotation) * GetWorldTransform().Scale.X) + margin * 2;
            height = (float)Math.Abs(Math.Sin(GetTransform().Rotation) * GetWorldTransform().Scale.X) + margin * 2;
            return new Vector2[] {
                GetWorldTransform().Position + new Vector2(-width/2f, -height/2f),
                GetWorldTransform().Position + new Vector2(-width/2f, height/2f),
                GetWorldTransform().Position + new Vector2(width/2f, height/2f),
                GetWorldTransform().Position + new Vector2(width/2f, -height/2f)
            };
        }

        public Vector2[] GetBounds()
        {
            return GetBounds(0);
        }
    }
}
