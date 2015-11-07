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
    public class Entity : Placeable2D
    {
        private Transform2D _velocity = new Transform2D();
        private List<Model> _models = new List<Model>();
        private List<ClipModel> ClipModels = new List<ClipModel>();
        private bool _isPortalable = false;

        /// <summary>
        /// Represents the size of the cutLines array within the fragment shader
        /// </summary>
        private const int CUT_LINE_ARRAY_MAX_LENGTH = 16;
        /// <summary>
        /// Whether or not this entity will interact with portals when intersecting them
        /// </summary>
        public bool IsPortalable
        {
            get { return _isPortalable; }
            set { _isPortalable = value; }
        }
        public virtual Transform2D Velocity { get { return _velocity; } set { _velocity = value; } }
        public virtual List<Model> Models { get { return _models; } set { _models = value; } }
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
                Debug.Assert(Scene.PhysWorld.BodyList.Exists(item => (item.BodyId == BodyId)), "Body id does not exist.");
                return Scene.PhysWorld.BodyList.Find(item => (item.BodyId == BodyId));
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

            }
            Transform.UniformScale = true;
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
            }
        }

        public void SetBody(Body body)
        {
            if (Body != null)
            {
                Scene.PhysWorld.RemoveBody(Body);
            }

            Transform.UniformScale = true;
            BodyUserData userData = new BodyUserData(this);
            Debug.Assert(body.UserData == null, "This body has UserData already assigned to it.");
            BodyId = body.BodyId;

            body.Position = Vector2Ext.ConvertToXna(Transform.Position);
            body.Rotation = Transform.Rotation;
            //Scene.PhysWorld.ProcessChanges();
            BodyExt.SetUserData(body, this);
        }

        public void _RenderSetTransformMatrix(Model model, Matrix4 viewMatrix)
        {
            Matrix4 modelMatrix = model.Transform.GetMatrix() * Transform.GetWorldMatrix() * viewMatrix;
            GL.UniformMatrix4(model.Shader.GetUniform("modelMatrix"), false, ref modelMatrix);    
        }

        public void UpdatePortalClipping(int depth)
        {
            ClipModels.Clear();
            foreach (Model m in Models)
            {
                _ModelPortalClipping(m, Transform.WorldPosition, null, Matrix4.Identity, 4, 0, ref ClipModels);
            }
        }

        /// <param name="depth">Number of iterations.</param>
        /// <param name="clipModels">Adds the ClipModel instances to this list.</param>
        private void _ModelPortalClipping(Model model, Vector2 centerPoint, Portal portalEnter, Matrix4 modelMatrix, int depth, int count, ref List<ClipModel> clipModels)
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

                if (portalLine.IsInsideOfPolygon(convexHull))
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
                    _ModelPortalClipping(model, centerPointNext, portal, modelMatrix * portal.GetPortalMatrix(), depth - 1, count + 1, ref clipModels);
                }
            }
            
            ClipModels.Add(new ClipModel(model, clipLines.ToArray(), modelMatrix));
            
        }

        public void RenderClipModels(Matrix4 viewMatrix)
        {
            _RenderClipModels(ClipModels, viewMatrix);
        }

        private void _RenderClipModels(List<ClipModel> clipModels, Matrix4 viewMatrix)
        {
            Matrix4 ScaleMatrix;
            ScaleMatrix = viewMatrix * Matrix4.CreateTranslation(new Vector3(1, 1, 0)) * Matrix4.CreateScale(new Vector3(Controller.ClientSize.Width / (float)2, Controller.ClientSize.Height / (float)2, 0));

            Vector2[] mirrorTest = new Vector2[3] {
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(0, 0)
            };
            bool isMirrored;
            mirrorTest = Vector2Ext.Transform(mirrorTest, viewMatrix);
            isMirrored = MathExt.AngleDiff(MathExt.AngleVector(mirrorTest[0] - mirrorTest[2]), MathExt.AngleVector(mirrorTest[1] - mirrorTest[2])) > 0;
            foreach (ClipModel cm in clipModels)
            {
                List<float> cutLines = new List<float>();
                foreach (Line l in cm.ClipLines)
                {
                    if (isMirrored)
                    {
                        l.Reverse();
                    }
                    l.Transform(ScaleMatrix);
                    cutLines.AddRange(new float[4] {
                        l.Vertices[0].X,
                        l.Vertices[0].Y,
                        l.Vertices[1].X,
                        l.Vertices[1].Y
                    });
                }

                GL.Uniform1(cm.Model.Shader.GetUniform("cutLinesLength"), cutLines.Count);
                GL.Uniform1(GL.GetUniformLocation(cm.Model.Shader.ProgramID, "cutLines[0]"), cutLines.Count, cutLines.ToArray());
                _RenderSetTransformMatrix(cm.Model, cm.Transform * viewMatrix);
                GL.DrawElements(BeginMode.Triangles, cm.Model.Indices.Count, DrawElementsType.UnsignedInt, 0);
            }
        }
    }
}