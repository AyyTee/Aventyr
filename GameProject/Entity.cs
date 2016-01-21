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
    public class Entity : SceneNodePlaceable
    {
        [DataMember]
        List<Model> _models = new List<Model>();
        List<ClipModel> _clipModels = new List<ClipModel>();
        public List<ClipModel> ClipModels { get { return new List<ClipModel>(_clipModels); } }
        [DataMember]
        bool _isPortalable = false;
        /// <summary>
        /// If true then this model will not be drawn during portal rendering and will appear in front of any portal FOV.
        /// </summary>
        [DataMember]
        public bool DrawOverPortals = false;

        /// <summary>
        /// Represents the size of the cutLines array within the fragment shader
        /// </summary>
        const int CUT_LINE_ARRAY_MAX_LENGTH = 16;
        /// <summary>
        /// Whether or not this entity will interact with portals when intersecting them
        /// </summary>
        public bool IsPortalable
        {
            get { return _isPortalable; }
            set { _isPortalable = value; }
        }
        /// <summary>
        /// Gets or sets whether this Entity can be rendered.
        /// </summary>
        [DataMember]
        public bool Visible { get; set; }
        public List<Model> ModelList { get { return new List<Model>(_models); } }
        public class ClipModel
        {
            private Line[] _clipLines;
            public Line[] ClipLines { get { return _clipLines; } }
            private Model _model;
            public Model Model { get { return _model; } }
            private Matrix4 _transform;
            public Matrix4 Transform { get { return _transform; } }

            public ClipModel (Model model, Line[] clipLines, Matrix4 transform)
            {
                _model = model;
                _clipLines = clipLines;
                _transform = transform;
            }
        }

        #region Constructors
        public Entity(Scene scene)
            : base(scene)
        {
            Transform2D transform = GetTransform();
            transform.UniformScale = true;
            SetTransform(transform);
            Visible = true;
        }

        public Entity(Scene scene, Vector2 position)
            : this(scene)
        {
            SetTransform(new Transform2D(position));
        }

        public Entity(Scene scene, Transform2D transform) 
            : this(scene)
        {
            SetTransform(transform);
        }
        #endregion

        public override SceneNode Clone(Scene scene)
        {
            Entity clone = new Entity(scene);
            Clone(clone);
            return clone;
        }

        protected override void Clone(SceneNode destination)
        {
            base.Clone(destination);
            Entity destinationCast = (Entity)destination;
            destinationCast.IsPortalable = IsPortalable;
            destinationCast._models = ModelList;
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

        public void UpdatePortalClipping(int depth)
        {
            _clipModels.Clear();
            foreach (Model m in ModelList)
            {
                ModelPortalClipping(m, GetWorldTransform().Position, null, Matrix4.Identity, 4, 0);
            }
        }

        /// <param name="depth">Number of iterations.</param>
        /// <param name="clipModels">Adds the ClipModel instances to this list.</param>
        private void ModelPortalClipping(Model model, Vector2 centerPoint, Portal portalEnter, Matrix4 modelMatrix, int depth, int count)
        {
            if (depth <= 0)
            {
                return;
            }
            List<float> cutLines = new List<float>();
            List<Portal> collisions = new List<Portal>();
            foreach (Portal portal in Scene.PortalList)
            {
                //ignore any portal attached to this entity on the first recursive iteration
                if (portal.Parent == this && count == 0)
                {
                    continue;
                }
                Line portalLine = new Line(portal.GetWorldVerts());
                Vector2[] convexHull = Vector2Ext.Transform(model.GetWorldConvexHull(), this.GetWorldTransform().GetMatrix() * modelMatrix);

                if (portalLine.IsInsideOfPolygon(convexHull) && portal.IsValid())
                {
                    collisions.Add(portal);
                }
            }

            collisions = collisions.OrderBy(item => (item.GetWorldTransform().Position - centerPoint).Length).ToList();
            for (int i = 0; i < collisions.Count; i++)
            {
                Portal portal = collisions[i];
                for (int j = collisions.Count - 1; j > i; j--)
                {
                    Line currentLine = new Line(collisions[i].GetWorldVerts());
                    Line checkLine = new Line(collisions[j].GetWorldVerts());
                    Line.Side checkSide = currentLine.GetSideOf(checkLine);
                    if (checkSide != currentLine.GetSideOf(centerPoint))
                    {
                        collisions.RemoveAt(j);
                    }
                }
            }
            
            List<Line> clipLines = new List<Line>();
            foreach (Portal portal in collisions)
            {
                Vector2[] pv = portal.GetWorldVerts();
                Line clipLine = new Line(pv);

                Line portalLine = new Line(pv);
                Vector2 normal = portal.GetWorldTransform().GetRight();
                if (portal.GetWorldTransform().IsMirrored())
                {
                    normal = -normal;
                }

                Vector2 portalNormal = portal.GetWorldTransform().Position + normal;
                if (portalLine.GetSideOf(centerPoint) != portalLine.GetSideOf(portalNormal))
                {
                    normal *= Portal.EnterMinDistance;
                }
                else
                {
                    clipLine.Reverse();
                    normal *= -Portal.EnterMinDistance;
                }

                clipLines.Add(clipLine);
                if (portalEnter == null || portal != portalEnter.Linked)
                {
                    Vector2 centerPointNext = Vector2Ext.Transform(portal.GetWorldTransform().Position + normal, portal.GetPortalMatrix());
                    ModelPortalClipping(model, centerPointNext, portal, modelMatrix * portal.GetPortalMatrix(), depth - 1, count + 1);
                }
            }

            _clipModels.Add(new ClipModel(model, clipLines.ToArray(), modelMatrix));
        }
    }
}