using Game;
using Game.Animation;
using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Models;
using Game.Rendering;
using Game.Serialization;
using OpenTK.Graphics;

namespace EditorLogic
{
    [DataContract]
    public class EditorObject : ITreeNode<EditorObject>, IPortalable, IDeepClone, IRenderable, ISceneObject
    {
        [DataMember]
        EditorScene _scene;
        public EditorScene Scene
        {
            get { return _scene == null ? Parent.Scene : _scene; }
            private set { _scene = value; }
        }
        IScene IPortalCommon.Scene => Scene;

        [DataMember]
        public PortalPath Path { get; set; } = new PortalPath();
        [DataMember]
        Transform2 _worldTransformPrevious = null;
        public Transform2 WorldTransform
        {
            get { return _worldTransformPrevious?.ShallowClone(); }
            set { _worldTransformPrevious = value?.ShallowClone(); }
        }
        [DataMember]
        Transform2 _worldVelocityPrevious = null;
        public Transform2 WorldVelocity
        {
            get { return _worldVelocityPrevious?.ShallowClone(); }
            set { _worldVelocityPrevious = value?.ShallowClone(); }
        }
        [DataMember]
        public bool Visible { get; set; }
        [DataMember]
        public bool DrawOverPortals { get; set; }
        [DataMember]
        public bool IsPortalable { get; set; }
        [DataMember]
        public bool IsSelected { get; private set; }
        public bool OnEdge => PolygonTransform != null;

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        List<EditorObject> _children = new List<EditorObject>();
        public List<EditorObject> Children => new List<EditorObject>(_children);

        [DataMember]
        public EditorObject Parent { get; private set; }

        IPortalCommon ITreeNode<IPortalCommon>.Parent => Parent;
        List<IPortalCommon> ITreeNode<IPortalCommon>.Children => Children.ToList<IPortalCommon>();

        [DataMember]
        public Transform2 Transform { get; set; } = new Transform2();
        public Transform2 Velocity { get; set; } = Transform2.CreateVelocity();
        [DataMember]
        public CurveTransform2 AnimatedTransform;
        [DataMember]
        public IPolygonCoord PolygonTransform;
        public bool IsModified { get; set; }
        [DataMember]
        public Action<EnterCallbackData, Transform2, Transform2> EnterPortal { get; set; }

        public EditorObject(EditorScene editorScene)
        {
            Name = "";
            Debug.Assert(editorScene != null);
            SetParent(editorScene);
            Visible = true;
            IsPortalable = true;
        }

        public virtual void Initialize()
        {
        }

        public virtual List<Model> GetModels()
        {
            List<Model> models = new List<Model>();
            Model marker = Game.Rendering.ModelFactory.CreateCircle(new Vector3(), 0.05f, 10);
            marker.Transform.Position = new Vector3(0, 0, DrawDepth.EntityMarker);

            if (IsSelected)
            {
                marker.SetColor(new Color4(1f, 1f, 0f, 1f));
                marker.Transform.Scale *= 1.5f;
            }
            else
            {
                marker.SetColor(new Color4(1f, 0.5f, 0f, 1f));
            }

            models.Add(marker);
            return models;
        }

        public virtual void SetScene(EditorScene destination)
        {
            Scene.Children.Remove(this);
            Scene = destination;
            Scene.Children.Add(this);
        }

        public virtual HashSet<IDeepClone> GetCloneableRefs() => new HashSet<IDeepClone>(Children);

        public virtual void UpdateRefs(IReadOnlyDictionary<IDeepClone, IDeepClone> cloneMap)
        {
            if (Parent != null && cloneMap.ContainsKey(Parent))
            {
                Parent = (EditorObject)cloneMap[Parent];
            }
            else
            {
                Parent = null;
            }
            List<EditorObject> children = Children;
            _children.Clear();
            foreach (EditorObject e in children)
            {
                _children.Add((EditorObject)cloneMap[e]);
            }
        }

        public virtual IDeepClone ShallowClone()
        {
            EditorObject clone = new EditorObject(Scene);
            ShallowClone(clone);
            return clone;
        }

        protected void ShallowClone(EditorObject destination)
        {
            destination._children = Children;
            destination.IsSelected = IsSelected;
            destination.SetTransform(GetTransform());
            destination.Name = Name + " Clone";
        }

        public virtual void SetParent(EditorScene scene)
        {
            Debug.Assert(scene != null);
            RemoveSelf();
            Scene = scene;
            Scene.Children.Add(this);
            Parent = null;
        }

        public virtual void SetParent(EditorObject parent)
        {
            Debug.Assert(parent != null);
            RemoveSelf();
            Parent = parent;
            parent._children.Add(this);
            Scene = null;
            Debug.Assert(!Tree<EditorObject>.ParentLoopExists(this), "Cannot have cycles in Parent tree.");
        }

        public virtual void SetTransform(Transform2 transform)
        {
            /*if (PolygonTransform != null)
            {
                PolygonTransform = null;
                SetParent(Scene);
            }*/
            Transform = transform.ShallowClone();
        }

        public virtual Transform2 GetTransform()
        {
            /*if (_transform == null)
            {
                return PolygonExt.GetTransform(((IWall)Parent).Vertices, PolygonTransform);
            }
            return _transform.ShallowClone();*/
            if (PolygonTransform != null)
            {
                return Transform.Transform(PolygonExt.GetTransform(((IWall)Parent).Vertices, PolygonTransform));
            }
            return Transform.ShallowClone();
        }

        public Transform2 GetTransformWithPolygon()
        {
            if (PolygonTransform != null)
            {
                return Transform.Transform(PolygonExt.GetTransform(((IWall)Parent).Vertices, PolygonTransform));
            }
            return Transform.ShallowClone();
        }

        public Transform2 GetWorldTransform(bool ignorePortals = false)
        {
            Transform2 local;
            if (PolygonTransform != null)
            {
                local = Transform.Transform(PolygonExt.GetTransform(((IWall)Parent).Vertices, PolygonTransform));
            }
            else
            {
                local = GetTransform();
            }

            if (Parent == null)
            {
                return local;
            }
            local = local.Transform(Parent.GetWorldTransform());

            return local;

            /*Transform2 parent = Parent.GetWorldTransform();
            Transform2 t = local.Transform(parent);

            t.Transform(PortalPath.GetPortalTransform());*/

            /*Ray.Settings settings = new Ray.Settings();
            settings.IgnorePortalVelocity = true;
            IPortalable portalable = new Portalable(new Transform2(parent.Position, t.Size, t.Rotation, t.MirrorX), Transform2.CreateVelocity(t.Position - parent.Position));
            List<IPortal> portals = Scene.GetPortalList();
            portals.Remove(this as IPortal);
            Ray.RayCast(portalable, portals, settings);
            t = portalable.GetTransform();*/
            //return t;
        }

        public Transform2 GetVelocity() => new Transform2();

        public void SetVelocity(Transform2 velocity)
        {
        }

        public Transform2 GetWorldVelocity(bool ignorePortals = false)
        {
            return new Transform2();
        }

        void RemoveSelf()
        {
            if (Parent != null)
            {
                Parent._children.Remove(this);
                Parent = null;
            }
            if (_scene != null)
            {
                _scene.Children.Remove(this);
                Scene = null;
            }
        }

        public virtual void Remove()
        {
            foreach (EditorObject e in Children)
            {
                e.Remove();
            }
            RemoveSelf();
        }

        public virtual void SetSelected(bool isSelected) => IsSelected = isSelected;

        /// <summary>
        /// Set transform as FixtureEdgeCoord.  This EditorPortal's parent will become the EditorObject 
        /// associated with the FixtureEdgeCoord's fixture.
        /// </summary>
        /// <param name="transform"></param>
        public void SetTransform(IWall wall, IPolygonCoord transform)
        {
            Transform = new Transform2();
            PolygonTransform = transform;
            SetParent((EditorObject)wall);
        }

        public IPolygonCoord GetPolygonCoord() => OnEdge ? PolygonTransform.ShallowClone() : null;

        /// <summary>
        /// Set name of this EditorObject and flag it as modified.
        /// </summary>
        public void SetName(string name)
        {
            if (name != Name)
            {
                Name = name;
                IsModified = true;
            }
        }

        public List<IPortal> GetPortalChildren() => Children.OfType<IPortal>().ToList();
    }
}
