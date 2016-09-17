using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Dynamics;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Game.Portals;

namespace Game
{
    /// <summary>
    /// An object that exists within the world space and can be drawn
    /// </summary>
    [DataContract, DebuggerDisplay("Entity {Name}")]
    public class Entity : SceneNode, IRenderable, IPortalable
    {
        [DataMember]
        public bool IsPortalable { get; set; }
        [DataMember]
        public Transform2 Transform { get; set; } = new Transform2();
        [DataMember]
        public Transform2 Velocity { get; set; } = Transform2.CreateVelocity();
        [DataMember]
        List<Model> _models = new List<Model>();
        /// <summary>
        /// If true then this model will not be drawn during portal rendering and will appear in front of any portal FOV.
        /// </summary>
        [DataMember]
        public bool DrawOverPortals { get; set; }
        /// <summary>
        /// Gets or sets whether this Entity can be rendered.
        /// </summary>
        [DataMember]
        public bool Visible { get; set; }
        public List<Model> ModelList { get { return new List<Model>(_models); } }
        [DataMember]
        public Action<EnterCallbackData, Transform2, Transform2> EnterPortal { get; set; }
        [DataMember]
        private bool _isBackground = false;
        public override bool IsBackground
        {
            get
            {
                return _isBackground;
            }
        }

        #region Constructors
        public Entity(Scene scene)
            : base(scene)
        {
            Transform2 transform = GetTransform();
            SetTransform(transform);
            Visible = true;
            IsPortalable = true;
        }

        public Entity(Scene scene, Vector2 position)
            : this(scene)
        {
            SetTransform(new Transform2(position));
        }

        public Entity(Scene scene, Transform2 transform) 
            : this(scene)
        {
            SetTransform(transform);
        }
        #endregion

        public override IDeepClone ShallowClone()
        {
            Entity clone = new Entity(Scene);
            ShallowClone(clone);
            return clone;
        }

        protected void ShallowClone(Entity destination)
        {
            base.ShallowClone(destination);
            destination.IsPortalable = IsPortalable;
            destination._isBackground = _isBackground;
            foreach (Model m in ModelList)
            {
                destination._models.Add(m.ShallowClone());
            }
        }

        public void AddModel(Model model)
        {
            _models.Add(model);
        }

        public void AddModelRange(IList<Model> models)
        {
            _models.AddRange(models);
        }

        public void RemoveModel(Model model)
        {
            _models.Remove(model);
        }

        public void RemoveAllModels()
        {
            _models.Clear();
        }

        public List<Model> GetModels()
        {
            return ModelList;
        }

        public override void SetTransform(Transform2 transform)
        {
            Transform = transform.ShallowClone();
            base.SetTransform(transform);
        }

        public override Transform2 GetTransform()
        {
            return Transform.ShallowClone();
        }

        public override Transform2 GetVelocity()
        {
            return Velocity.ShallowClone();
        }

        public override void SetVelocity(Transform2 velocity)
        {
            Velocity = velocity.ShallowClone();
            base.SetVelocity(velocity);
        }

        public List<IPortal> GetPortalChildren()
        {
            return Children.OfType<IPortal>().ToList();
        }

        public void SetBackground(bool isBackground)
        {
            _isBackground = isBackground;
        }
    }
}