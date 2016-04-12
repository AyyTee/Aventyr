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

namespace Game
{
    /// <summary>
    /// An object that exists within the world space and can be drawn
    /// </summary>
    [DataContract]
    public class Entity : SceneNodePlaceable, IEntity
    {
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

        #region Constructors
        public Entity(Scene scene)
            : base(scene)
        {
            Transform2 transform = GetTransform();
            SetTransform(transform);
            Visible = true;
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
            foreach (Model m in ModelList)
            {
                destination._models.Add(m.ShallowClone());
            }
        }

        public void AddModel(Model model)
        {
            _models.Add(model);
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

        public List<ClipModel> GetClipModels(int depth)
        {
            return ClipModelCompute.GetClipModels(this, Scene.GetPortalList(), depth);
        }
    }
}