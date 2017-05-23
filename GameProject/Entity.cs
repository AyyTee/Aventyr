using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.Serialization;
using Game.Common;
using Game.Models;
using Game.Portals;
using Game.Rendering;
using Game.Serialization;

namespace Game
{
    /// <summary>
    /// An object that exists within the world space and can be drawn
    /// </summary>
    [DataContract, DebuggerDisplay(nameof(Entity) + " {" + nameof(Name) + "}")]
    public class Entity : SceneNode, IRenderable, IPortalable
    {
        [DataMember]
        public Transform2 Transform { get; set; } = new Transform2();
        [DataMember]
        public Transform2 Velocity { get; set; } = Transform2.CreateVelocity();
        [DataMember]
        List<Model> _models = new List<Model>();
        /// <summary>
        /// If true then this model will not be drawn during portal rendering and will appear in front of any portal Fov.
        /// </summary>
        [DataMember]
        public bool DrawOverPortals { get; set; }
        /// <summary>
        /// Gets or sets whether this Entity can be rendered.
        /// </summary>
        [DataMember]
        public bool Visible { get; set; }
        public List<Model> ModelList => new List<Model>(_models);

        [DataMember]
        public Action<EnterCallbackData, Transform2, Transform2> EnterPortal { get; set; }

        public Entity(Scene scene)
            : base(scene)
        {
            Transform2 transform = GetTransform();
            SetTransform(transform);
            Visible = true;
        }

        public Entity(Scene scene, Transform2 transform) 
            : this(scene)
        {
            SetTransform(transform);
        }

        public override IDeepClone ShallowClone()
        {
            Entity clone = new Entity(Scene);
            ShallowClone(clone);
            return clone;
        }

        protected void ShallowClone(Entity destination)
        {
            base.ShallowClone(destination);
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
    }
}