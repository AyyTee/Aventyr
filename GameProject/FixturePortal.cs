using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xna = Microsoft.Xna.Framework;

namespace Game
{
    [Serializable]
    public class FixturePortal : Portal, IVertices2D
    {
        public override Entity EntityParent
        {
            get
            {
                if (Position != null)
                {
                    return FixtureExt.GetUserData(Position.Fixture).Entity;
                }
                return null;
            }
        }
        public FixtureEdgeCoord Position { get; private set; }
        public bool IsMirrored { get; set; }
        public Fixture CollisionFixtureNext;
        public Fixture CollisionFixturePrevious;
        public Body EntityBody
        {
            get
            {
                return EntityParent.Body;
            }
        }
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
            
            transform.Parent = FixtureExt.GetUserData(Position.Fixture).Entity.Transform;
            return transform;
        }

        public override Transform2D GetVelocity()
        {
            Transform2D transform = new Transform2D();
            if (Position == null)
            {
                return transform;
            }
            
            transform.Parent = FixtureExt.GetUserData(Position.Fixture).Entity.Transform;
            return transform;
        }

        private void Remove()
        {
            if (Position != null)
            {
                FixtureExt.GetUserData(Position.Fixture).RemovePortal(this);
            }
        }

        public void SetFixtureParent(FixtureEdgeCoord position)
        {
            Remove();
            Position = position;
            Debug.Assert(position.Entity != null);
            Debug.Assert(position.Fixture != null);
            if (Position != null)
            {
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
            width = (float)Math.Abs(Math.Cos(GetTransform().Rotation) * GetTransform().WorldScale.X) + margin * 2;
            height = (float)Math.Abs(Math.Sin(GetTransform().Rotation) * GetTransform().WorldScale.X) + margin * 2;
            return new Vector2[] {
                GetTransform().WorldPosition + new Vector2(-width/2f, -height/2f),
                GetTransform().WorldPosition + new Vector2(-width/2f, height/2f),
                GetTransform().WorldPosition + new Vector2(width/2f, height/2f),
                GetTransform().WorldPosition + new Vector2(width/2f, -height/2f)
            };
        }

        public Vector2[] GetBounds()
        {
            return GetBounds(0);
        }
    }
}
