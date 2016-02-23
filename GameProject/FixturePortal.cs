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
    public class FixturePortal : Portal
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

        public FixturePortal(Scene scene)
            : this(scene, null)
        {
        }

        public FixturePortal(Scene scene, FixtureEdgeCoord position)
            : base(scene)
        {
            if (position != null)
            {
                SetFixtureParent(position);
            }
        }

        public override IDeepClone ShallowClone()
        {
            FixturePortal clone = new FixturePortal(Scene);
            ShallowClone(clone);
            return clone;
        }

        /// <summary>
        /// Returns a copy of the Transform local to the Body this is attached to.
        /// </summary>
        /// <returns></returns>
        public override Transform2 GetTransform()
        {
            Transform2 transform = new Transform2();
            if (Position == null)
            {
                return transform;
            }
            Line edge = Position.GetEdge();
            transform.Position = edge.Lerp(Position.EdgeT);
            transform.Rotation = -edge.Angle() + (float)Math.PI/2;
            if (IsMirrored)
            {
                //transform.Scale = new Vector2(1, -1);
                transform.Size = -1;
                transform.IsMirrored = true;
            }
            return transform;
        }

        protected override bool _isValid()
        {
            return base._isValid() && Position != null;
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
            Debug.Assert(position.Actor != null);
            Debug.Assert(position.Fixture != null);
            if (Position != null)
            {
                SetParent(Position.Actor);
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
