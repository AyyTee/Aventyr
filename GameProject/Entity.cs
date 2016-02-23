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

        public List<ClipModel> GetClipModels(int depth)
        {
            List<ClipModel> clipModels = new List<ClipModel>();
            if (IsPortalable && !DrawOverPortals)
            {
                foreach (Model m in ModelList)
                {
                    clipModels.AddRange(_getClipModels(m, GetWorldTransform().Position, null, Matrix4.Identity, depth, 0));
                }
            }
            else
            {
                foreach (Model m in ModelList)
                {
                    clipModels.Add(new ClipModel(this, m, new Line[0], Matrix4.Identity));
                }
            }
            return clipModels;
        }

        /// <param name="depth">Number of iterations.</param>
        /// <param name="clipModels">Adds the ClipModel instances to this list.</param>
        private List<ClipModel> _getClipModels(Model model, Vector2 centerPoint, Portal portalEnter, Matrix4 modelMatrix, int depth, int count)
        {
            List<ClipModel> clipModels = new List<ClipModel>();
            if (depth <= 0)
            {
                return clipModels;
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
                if (portal.GetWorldTransform().IsMirrored)
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
                    clipModels.AddRange(_getClipModels(model, centerPointNext, portal, modelMatrix * portal.GetPortalMatrix(), depth - 1, count + 1));
                }
            }
            clipModels.Add(new ClipModel(this, model, clipLines.ToArray(), modelMatrix));
            return clipModels;
        }
    }
}