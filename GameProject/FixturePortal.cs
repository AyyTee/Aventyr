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
    public class FixturePortal : SceneNode, IPortal
    {
        [DataMember]
        public IPortal Linked { get; set; }
        /// <summary>
        /// If OneSided is true then the portal can only be viewed through it's front side.
        /// Entities can still travel though the portal in both directions however.
        /// </summary>
        [DataMember]
        public bool OneSided { get; set; }
        [DataMember]
        public bool IsMirrored { get; set; }

        [DataMember]
        public IPolygonCoord Position { get; private set; }
        public Fixture CollisionFixtureNext;
        public Fixture CollisionFixturePrevious;
        public override bool IgnoreScale { get { return true; } }
        public Fixture FixtureParent {
            get
            {
                /*if (Position != null)
                {
                    return Position.Fixture;
                }*/
                return null;
            }
        }
        
        public const float EdgeMargin = 0.02f;
        public const float CollisionMargin = 0.1f;

        public FixturePortal(Scene scene)
            : this(scene, null, null)
        {
        }

        public FixturePortal(Scene scene, IWall parent, IPolygonCoord position)
            : base(scene)
        {
            if (parent != null && position != null)
            {
                SetPosition(parent, position);
            }
        }

        public override IDeepClone ShallowClone()
        {
            FixturePortal clone = new FixturePortal(Scene);
            ShallowClone(clone);
            return clone;
        }

        protected void ShallowClone(FixturePortal destination)
        {
            base.ShallowClone(destination);
            destination.OneSided = OneSided;
            destination.IsMirrored = IsMirrored;
            destination.Linked = Linked;
        }

        public override void UpdateRefs(IReadOnlyDictionary<IDeepClone, IDeepClone> cloneMap)
        {
            base.UpdateRefs(cloneMap);
            //Portal clone = (Portal)cloneMap[this];
            if (Linked != null && cloneMap.ContainsKey(Linked))
            {
                Linked = (IPortal)cloneMap[Linked];
                //SetLinked((Portal)cloneMap[Linked]);
            }
            else
            {
                Linked = null;
            }
        }

        /// <summary>
        /// Returns a copy of the Transform local to the Body this is attached to.
        /// </summary>
        public override Transform2 GetTransform()
        {
            return PolygonExt.GetTransform(((IWall)Parent).Vertices, Position);
        }

        public override void Remove()
        {
            RemoveFixture();
            //SetLinked(null);
            base.Remove();
        }

        private void RemoveFixture()
        {
            if (Position != null)
            {
                //FixtureExt.GetUserData(Position.Fixture).RemovePortal(this);
            }
        }

        public void SetPosition(IWall wall, IPolygonCoord position)
        {
            RemoveFixture();
            Position = position;
            Debug.Assert(wall != null);
            Debug.Assert(wall is SceneNode);
            if (Position != null)
            {
                SetParent((SceneNode)wall);
                //FixtureExt.GetUserData(Position.Fixture).AddPortal(this);
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
