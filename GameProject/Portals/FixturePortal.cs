using FarseerPhysics.Dynamics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Game.Common;
using Game.Serialization;
using Game.Rendering;

namespace Game.Portals
{
    [DataContract, DebuggerDisplay("FixturePortal {Name}")]
    public class FixturePortal : SceneNode, IPortal, ISceneObject
    {
        [DataMember]
        public IPortal Linked { get; set; }
        IPortalRenderable IPortalRenderable.Linked => Linked;
        /// <summary>
        /// If OneSided is true then the portal can only be viewed through it's front side.
        /// Entities can still travel though the portal in both directions however.
        /// </summary>
        [DataMember]
        public bool OneSided { get; set; } = true;
        [DataMember]
        public bool MirrorX { get; set; }
        [DataMember]
        float _size = 1;
        //Size of portal. Must not be equal to 0.
        public float Size {
            get { return _size; }
            set
            {
                Debug.Assert(_size != 0);
                _size = value;
            }
        }

        [DataMember]
        public IPolygonCoord Position { get; private set; }
        
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
                SetPosition(new WallCoord(parent, position));
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
            destination.MirrorX = MirrorX;
            destination.Linked = Linked;
        }

        public override void UpdateRefs(IReadOnlyDictionary<IDeepClone, IDeepClone> cloneMap)
        {
            base.UpdateRefs(cloneMap);
            if (Linked != null && cloneMap.ContainsKey(Linked))
            {
                Linked = (IPortal)cloneMap[Linked];
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
            if (Position == null || Parent == null)
            {
                return null;
            }
            return PolygonEx.GetTransform(((IWall)Parent).Vertices, Position)
                .WithSize(Size)
                .WithMirrorX(MirrorX); 
        }

        public override void Remove()
        {
            if (Linked != null)
            {
                Linked.Linked = null;
                Linked = null;
            }
            //SetLinked(null);
            base.Remove();
        }

        public override void SetParent(SceneNode parent)
        {
            Debug.Fail("Use " + nameof(SetPosition) + " method instead.");
        }

        public WallCoord GetPosition()
        {
            return Parent == null ? null : new WallCoord((IWall)Parent, Position);
        }

        public void SetPosition(WallCoord coord)
        {
            bool update = (coord == null) != (Position == null);
            
            if (coord == null)
            {
                base.SetParent(null);
            }
            else
            {
                Position = new PolygonCoord(coord.EdgeIndex, coord.EdgeT);
                base.SetParent((SceneNode)coord.Wall);
            }

            if (update)
            {
                //wake up all the bodies so that they will fall if there is now a portal entrance below them
                foreach (Body b in Scene.World.BodyList)
                {
                    b.Awake = true;
                }
            }
        }

        public void SetPosition(WallCoord coord, float size, bool mirrorX)
        {
            MirrorX = mirrorX;
            Size = size;
            SetPosition(coord);
        }

        public void SetMirrorX(bool mirrorX)
        {
            MirrorX = mirrorX;
        }

        public void SetSize(float size)
        {
            Size = size;
        }
    }
}
