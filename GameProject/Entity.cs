﻿using OpenTK;
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
    [Serializable]
    public class Entity : Placeable2D
    {
        List<Model> _models = new List<Model>();
        public List<ClipModel> ClipModels = new List<ClipModel>();
        bool _isPortalable = false;

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
        public bool Visible { get; set; }
        public Transform2D Velocity { get; private set; }
        public List<Model> Models { get { return _models; } set { _models = value; } }
        [DataContractAttribute]
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

        public int BodyId = -1;
        public Body Body
        {
            get
            {
                if (BodyId == -1)
                {
                    return null;
                }
                Debug.Assert(Scene != null, "Entity must be assigned to a scene.");
                Debug.Assert(Scene.World.BodyList.Exists(item => (item.BodyId == BodyId)), "Body id does not exist.");
                return Scene.World.BodyList.Find(item => (item.BodyId == BodyId));
            }
        }

        private Entity()
        {
        }

        public Entity(Scene scene)
            : base(scene)
        {
            if (scene != null)
            {
                scene.AddEntity(this);
            }
            Velocity = new Transform2D();
            Transform.UniformScale = true;
            Visible = true;
        }

        public Entity(Vector2 position)
            : this(null)
        {
            Transform.Position = position;
        }

        public Entity(Scene scene, Vector2 position)
            : this(scene)
        {
            Transform.Position = position;
        }

        public Entity(Scene scene, Transform2D transform) 
            : this(scene)
        {
            Transform.SetLocal(transform);
        }

        /*public Entity Clone()
        {
            Entity clone = new Entity(base.Scene);
            clone._isPortalable = _isPortalable;
            clone._models = _models;
            clone.BodyId = Body.DeepClone().BodyId;

            return clone;
        }*/
        
        public void PositionUpdate()
        {
            foreach (Portal portal in Scene.PortalList)
            {
                //position the entity slightly outside of the exit portal to avoid precision issues with portal collision checking
                Line exitLine = new Line(portal.GetWorldVerts());
                float distanceToPortal = exitLine.PointDistance(Transform.Position, true);
                if (distanceToPortal < Portal.EnterMinDistance)
                {
                    Vector2 exitNormal = portal.GetTransform().GetNormal();
                    if (exitLine.GetSideOf(Transform.Position) != exitLine.GetSideOf(exitNormal + portal.GetTransform().Position))
                    {
                        exitNormal = -exitNormal;
                    }

                    Vector2 pos = exitNormal * (Portal.EnterMinDistance - distanceToPortal);
                    /*if (Transform.Parent != null)
                    {
                        pos = Transform.Parent.WorldToLocal(pos);
                    }*/
                    Transform.Position += pos;
                    break;
                }
            }
        }

        public void Step()
        {
            if (Body != null)
            {
                Transform.Position = Vector2Ext.ConvertTo(Body.Position);
                Transform.Rotation = Body.Rotation;
                Velocity.Position = Vector2Ext.ConvertTo(Body.LinearVelocity);
                Velocity.Rotation = Body.AngularVelocity;
            }
            else
            {
                Transform.Position += Velocity.Position;
                Transform.Rotation += Velocity.Rotation;
                Transform.Scale *= Velocity.Scale;
            }
        }

        public void SetBody(Body body)
        {
            if (Body != null)
            {
                Scene.World.RemoveBody(Body);
            }

            Transform.UniformScale = true;
            BodyUserData userData = new BodyUserData(this, body);
            Debug.Assert(body.UserData == null, "This body has UserData already assigned to it.");
            BodyId = body.BodyId;

            BodyExt.SetTransform(body, Transform);
            BodyExt.SetUserData(body, this);
        }

        public void SetTransform(Transform2D transform)
        {
            Transform.SetLocal(transform);
            if (Body != null)
            {
                BodyExt.SetTransform(Body, transform);
            }
        }

        public Transform2D GetTransform()
        {
            return Transform.Copy();
        }

        public void UpdatePortalClipping(int depth)
        {
            ClipModels.Clear();
            foreach (Model m in Models)
            {
                ModelPortalClipping(m, Transform.WorldPosition, null, Matrix4.Identity, 4, 0, ref ClipModels);
            }
        }

        /// <param name="depth">Number of iterations.</param>
        /// <param name="clipModels">Adds the ClipModel instances to this list.</param>
        private void ModelPortalClipping(Model model, Vector2 centerPoint, Portal portalEnter, Matrix4 modelMatrix, int depth, int count, ref List<ClipModel> clipModels)
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
                if (portal.EntityParent == this && count == 0)
                {
                    continue;
                }
                Line portalLine = new Line(portal.GetWorldVerts());
                Vector2[] convexHull = Vector2Ext.Transform(model.GetWorldConvexHull(), this.Transform.GetWorldMatrix() * modelMatrix);

                if (portalLine.IsInsideOfPolygon(convexHull) && portal.Linked != null)
                {
                    collisions.Add(portal);
                }
            }

            collisions = collisions.OrderBy(item => (item.GetTransform().WorldPosition - centerPoint).Length).ToList();
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
                Vector2 normal = portal.GetTransform().GetWorldNormal();
                if (portal.GetTransform().IsWorldMirrored())
                {
                    normal = -normal;
                }

                Vector2 portalNormal = portal.GetTransform().WorldPosition + normal;
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
                    Vector2 centerPointNext = Vector2Ext.Transform(portal.GetTransform().WorldPosition + normal, portal.GetPortalMatrix());
                    ModelPortalClipping(model, centerPointNext, portal, modelMatrix * portal.GetPortalMatrix(), depth - 1, count + 1, ref clipModels);
                }
            }
            
            ClipModels.Add(new ClipModel(model, clipLines.ToArray(), modelMatrix));
        }
    }
}