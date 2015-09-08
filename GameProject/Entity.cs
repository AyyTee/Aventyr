using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Game
{
    /// <summary>
    /// An object that exists within the world space and can be drawn
    /// </summary>
    public class Entity
    {
        private Transform2D _velocity = new Transform2D();
        private Transform2D _transform = new Transform2D();
        private List<Model> _models = new List<Model>();
        private bool _isPortalable = false;

        /// <summary>
        /// Whether or not this entity will interact with portals when intersecting them
        /// </summary>
        public bool IsPortalable
        {
            get { return _isPortalable; }
            set { _isPortalable = value; }
        }
        public virtual Transform2D Velocity { get { return _velocity; } set { _velocity = value; } }
        public virtual Transform2D Transform { get { return _transform; } set { _transform = value; } }
        public virtual List<Model> Models { get { return _models; } set { _models = value; } }
        public Entity()
        {
        }

        public Entity(Vector2 Position)
        {
            Transform = new Transform2D(Position);
        }

        public Entity(Transform2D transform)
        {
            Transform = transform;
        }

        public void BufferModels()
        {

        }

        public virtual void StepUpdate()
        {

        }

        public virtual void Render(Scene scene, Matrix4 viewMatrix, float timeDelta)
        {
            Transform.GetMatrix();
            foreach (Model v in Models)
            {
                List<Vector3> verts = new List<Vector3>();
                List<int> inds = new List<int>();
                List<Vector3> colors = new List<Vector3>();
                List<Vector2> texcoords = new List<Vector2>();

                // Assemble vertex and indice data for all volumes
                int vertcount = 0;
                
                verts.AddRange(v.GetVerts().ToList());
                inds.AddRange(v.GetIndices().ToList());
                colors.AddRange(v.GetColorData().ToList());
                texcoords.AddRange(v.GetTextureCoords());
                vertcount += v.Vertices.Count;

                Vector3[] vertdata;
                Vector3[] coldata;
                Vector2[] texcoorddata;
                int[] indicedata;
                int indiceat = 0;

                vertdata = verts.ToArray();
                indicedata = inds.ToArray();
                coldata = colors.ToArray();
                texcoorddata = texcoords.ToArray();

                GL.BindBuffer(BufferTarget.ArrayBuffer, v.Shader.GetBuffer("vPosition"));

                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StreamDraw);
                GL.VertexAttribPointer(v.Shader.GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

                // Buffer vertex color if shader supports it
                if (v.Shader.GetAttribute("vColor") != -1)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, v.Shader.GetBuffer("vColor"));
                    GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StreamDraw);
                    GL.VertexAttribPointer(v.Shader.GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
                }

                // Buffer texture coordinates if shader supports it
                if (v.Shader.GetAttribute("texcoord") != -1)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, v.Shader.GetBuffer("texcoord"));
                    GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StreamDraw);
                    GL.VertexAttribPointer(v.Shader.GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
                }

                GL.UseProgram(v.Shader.ProgramID);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                // Buffer index data
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, v.ibo_elements);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StreamDraw);

                GL.BindTexture(TextureTarget.Texture2D, v.TextureID);
                
                Matrix4 UVMatrix = v.TransformUV.GetMatrix();
                GL.UniformMatrix4(v.Shader.GetUniform("UVMatrix"), false, ref UVMatrix);

                if (v.Shader.GetAttribute("maintexture") != -1)
                {
                    GL.Uniform1(v.Shader.GetAttribute("maintexture"), v.TextureID);
                }

                if (v.Wireframe)
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                }
                
                if (IsPortalable)
                {
                    _RenderPortalClipping(scene, v, Transform.Position, null, Matrix4.Identity, viewMatrix, 4);
                }
                else
                {
                    GL.Uniform1(v.Shader.GetUniform("cutLinesLength"), 0);
                    _RenderSetTransformMatrix(v, viewMatrix);
                    GL.DrawElements(BeginMode.Triangles, v.Indices.Count, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                }
                
                if (v.Wireframe)
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
                //indiceat += v.IndiceCount;
            }
        }

        private void _RenderSetTransformMatrix(Model model, Matrix4 viewMatrix)
        {
            Matrix4 modelMatrix = model.Transform.GetMatrix() * Transform.GetMatrix() * viewMatrix;
            GL.UniformMatrix4(model.Shader.GetUniform("modelMatrix"), false, ref modelMatrix);    
        }

        private void _RenderPortalClipping(Scene scene, Model model, Vector2 centerPoint, Portal portalEnter, Matrix4 modelMatrix, Matrix4 viewMatrix, int depth)
        {
            if (depth <= 0)
            {
                return;
            }
            List<float> cutLines = new List<float>();
            List<Portal> collisions = new List<Portal>();
            foreach (Portal portal in scene.Portals)
            {
                Line portalLine = new Line(portal.GetWorldVerts());
                Vector2[] convexHull = VectorExt2.Transform(model.GetWorldConvexHull(), this.Transform.GetMatrix() * modelMatrix);
                
                if (portalLine.IsInsideOfPolygon(convexHull))
                {
                    collisions.Add(portal);
                }
            }

            collisions = collisions.OrderBy(item => (item.Transform.Position - centerPoint).Length).ToList();
            for (int i = 0; i < collisions.Count; i++)
            {
                Portal portal = collisions[i];
                for (int j = collisions.Count - 1; j > i; j--)
                {
                    Line currentLine = new Line(collisions[i].GetWorldVerts());
                    Line checkLine = new Line(collisions[j].GetWorldVerts());
                    Line.Side checkSide = currentLine.GetSideOf(checkLine);
                    if (checkSide != Line.Side.IsNeither && checkSide != currentLine.GetSideOf(centerPoint))
                    {
                        collisions.RemoveAt(j);
                    }
                }
            }
            Matrix4 ScaleMatrix;
            ScaleMatrix = viewMatrix * Matrix4.CreateTranslation(new Vector3(1, 1, 0)) * Matrix4.CreateScale(new Vector3(Controller.CanvasSize.Width / 2, Controller.CanvasSize.Height / 2, 0));
            
            foreach (Portal portal in collisions)
            {
                Vector2[] pv = portal.GetWorldVerts();
                Vector2[] pvScreen = VectorExt2.Transform(pv, ScaleMatrix);

                Line portalLine = new Line(pv);
                Vector2 normal = portal.Transform.GetNormal();
                if (portal.Transform.IsMirrored())
                {
                    normal = -normal;
                }
                Vector2 portalNormal = portal.Transform.Position + normal;
                if (portalLine.GetSideOf(centerPoint) != portalLine.GetSideOf(portalNormal))
                {
                    cutLines.AddRange(new float[4] {
                        pvScreen[0].X, pvScreen[0].Y,
                        pvScreen[1].X, pvScreen[1].Y,
                    });
                    normal *= Portal.EntityMinDistance;
                }
                else
                {
                    cutLines.AddRange(new float[4] {
                        pvScreen[1].X, pvScreen[1].Y,
                        pvScreen[0].X, pvScreen[0].Y,
                    });
                    normal *= -Portal.EntityMinDistance;
                }

                if (portalEnter == null || portal != portalEnter.Linked)
                {
                    Vector2 centerPointNext = VectorExt2.Transform(portal.Transform.Position + normal, portal.GetMatrix());
                    _RenderPortalClipping(scene, model, centerPointNext, portal, modelMatrix * portal.GetMatrix(), viewMatrix, depth - 1);
                }
            }
            GL.Uniform1(model.Shader.GetUniform("cutLinesLength"), cutLines.Count);
            GL.Uniform1(model.Shader.GetUniform("cutLines"), cutLines.Count, cutLines.ToArray());
            _RenderSetTransformMatrix(model, modelMatrix * viewMatrix);
            GL.DrawElements(BeginMode.Triangles, model.Indices.Count, DrawElementsType.UnsignedInt, 0);
        }
    }
}