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
    [Serializable]
    public class Entity : SceneNodePlaceable
    {
        List<Model> _models = new List<Model>();
        public List<ClipModel> ClipModels = new List<ClipModel>();
        bool _isPortalable = false;
        /// <summary>
        /// If true then this model will not be drawn during portal rendering and will appear in front of any portal FOV.
        /// </summary>
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
        public bool Visible { get; set; }
        public Transform2D Velocity { get; private set; }
        //public List<Model> Models { get { return _models; } set { _models = value; } }
        public List<Model> ModelList { get { return new List<Model>(_models); } }
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
                Debug.Assert(Scene.World.BodyList.Exists(item => (item.BodyId == BodyId)), "Body id does not exist.");
                return Scene.World.BodyList.Find(item => (item.BodyId == BodyId));
            }
        }

        #region constructors
        public Entity(Scene scene)
            : base(scene)
        {
            Velocity = new Transform2D();
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
            destinationCast.Velocity = new Transform2D(Velocity);
            destinationCast.BodyId = -1;
            if (Body != null)
            {
                destinationCast.BodyId = Body.DeepClone().BodyId;
            }
        }

        public override void SetTransform(Transform2D transform)
        {
            base.SetTransform(transform);
            if (Body != null)
            {
                BodyExt.SetTransform(Body, transform);
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

        public override void Remove()
        {
            if (Body != null)
            {
                Scene.World.RemoveBody(Body);
            }
            base.Remove();
        }

        public void PositionUpdate()
        {
            foreach (Portal portal in Scene.PortalList)
            {
                //position the entity slightly outside of the exit portal to avoid precision issues with portal collision checking
                Line exitLine = new Line(portal.GetWorldVerts());
                float distanceToPortal = exitLine.PointDistance(GetTransform().Position, true);
                if (distanceToPortal < Portal.EnterMinDistance)
                {
                    Vector2 exitNormal = portal.GetTransform().GetNormal();
                    if (exitLine.GetSideOf(GetTransform().Position) != exitLine.GetSideOf(exitNormal + portal.GetTransform().Position))
                    {
                        exitNormal = -exitNormal;
                    }

                    Vector2 pos = exitNormal * (Portal.EnterMinDistance - distanceToPortal);
                    /*if (Transform.Parent != null)
                    {
                        pos = Transform.Parent.WorldToLocal(pos);
                    }*/
                    GetTransform().Position += pos;
                    break;
                }
            }
        }

        public void Step()
        {
            Transform2D transform = GetTransform();
            if (Body != null)
            {
                transform.Position = Vector2Ext.ConvertTo(Body.Position);
                transform.Rotation = Body.Rotation;
                Velocity.Position = Vector2Ext.ConvertTo(Body.LinearVelocity);
                Velocity.Rotation = Body.AngularVelocity;
            }
            else
            {
                transform.Position += Velocity.Position;
                transform.Rotation += Velocity.Rotation;
                transform.Scale *= Velocity.Scale;
            }
            SetTransform(transform);
        }

        public void SetBody(Body body)
        {
            if (Body != null)
            {
                Scene.World.RemoveBody(Body);
            }

            Transform2D transform = GetTransform();
            transform.UniformScale = true;
            SetTransform(transform);
            BodyUserData userData = new BodyUserData(this, body);
            Debug.Assert(body.UserData == null, "This body has UserData already assigned to it.");
            BodyId = body.BodyId;

            BodyExt.SetTransform(body, GetTransform());
            BodyExt.SetUserData(body, this);
        }

        public void SetVelocity(Transform2D transform)
        {
            Velocity = transform.Clone();
        }

        public void UpdatePortalClipping(int depth)
        {
            ClipModels.Clear();
            foreach (Model m in ModelList)
            {
                ModelPortalClipping(m, GetWorldTransform().Position, null, Matrix4.Identity, 4, 0, ref ClipModels);
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
                Vector2 normal = portal.GetWorldTransform().GetNormal();
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
                    ModelPortalClipping(model, centerPointNext, portal, modelMatrix * portal.GetPortalMatrix(), depth - 1, count + 1, ref clipModels);
                }
            }
            
            ClipModels.Add(new ClipModel(model, clipLines.ToArray(), modelMatrix));
        }
    }
}