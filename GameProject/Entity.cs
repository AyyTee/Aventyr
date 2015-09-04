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

                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(v.Shader.GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

                // Buffer vertex color if shader supports it
                if (v.Shader.GetAttribute("vColor") != -1)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, v.Shader.GetBuffer("vColor"));
                    GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                    GL.VertexAttribPointer(v.Shader.GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
                }

                // Buffer texture coordinates if shader supports it
                if (v.Shader.GetAttribute("texcoord") != -1)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, v.Shader.GetBuffer("texcoord"));
                    GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StaticDraw);
                    GL.VertexAttribPointer(v.Shader.GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
                }

                GL.UseProgram(v.Shader.ProgramID);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                // Buffer index data
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, v.ibo_elements);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);

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
                    
                    foreach (Portal portal in scene.Portals)
                    {
                        Line portalLine = new Line(portal.GetWorldVerts());
                        Vector2[] convexHull = VectorExt2.Transform(v.GetWorldConvexHull(), this.Transform.GetMatrix());
                        if (portalLine.IsInsideOfPolygon(convexHull))
                        {
                            //GL.Enable(EnableCap.Blend);
                            Vector2[] pv = VectorExt2.Transform(portal.Linked.GetWorldVerts(), viewMatrix);
                            Matrix4 ScaleMatrix = Matrix4.CreateScale(new Vector3(Controller.CanvasSize.X / 2, Controller.CanvasSize.Y / 2, 0));
                            Vector2[] pvScreen = VectorExt2.Transform(pv, Matrix4.CreateTranslation(new Vector3(1, 1, 0)) * ScaleMatrix);
                            Vector2 pos = VectorExt2.Transform(new Vector2(v.Transform.Position.X, v.Transform.Position.Y), this.Transform.GetMatrix());
                            pos = VectorExt2.Transform(pos, portal.GetMatrix() * viewMatrix);
                            if (!new Line(pv).PointIsLeft(pos))
                            {
                                GL.Uniform2(v.Shader.GetUniform("cullLine0"), pvScreen[1]);
                                GL.Uniform2(v.Shader.GetUniform("cullLine1"), pvScreen[0]);
                            }
                            else
                            {
                                GL.Uniform2(v.Shader.GetUniform("cullLine0"), pvScreen[0]);
                                GL.Uniform2(v.Shader.GetUniform("cullLine1"), pvScreen[1]);
                            }
                            _RenderSetTransformMatrix(v, portal.GetMatrix() * viewMatrix);
                            GL.DrawElements(BeginMode.Triangles, v.Indices.Count, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));


                            pv = VectorExt2.Transform(portalLine.Vertices, viewMatrix);
                            //ScaleMatrix = Matrix4.CreateScale(new Vector3(Controller.CanvasSize.X / 2, Controller.CanvasSize.Y / 2, 0));
                            pvScreen = VectorExt2.Transform(pv, Matrix4.CreateTranslation(new Vector3(1, 1, 0)) * ScaleMatrix);
                            pos = VectorExt2.Transform(new Vector2(v.Transform.Position.X, v.Transform.Position.Y), this.Transform.GetMatrix());
                            pos = VectorExt2.Transform(pos, viewMatrix);
                            if (new Line(pv).PointIsLeft(pos))
                            {
                                GL.Uniform2(v.Shader.GetUniform("cullLine0"), pvScreen[1]);
                                GL.Uniform2(v.Shader.GetUniform("cullLine1"), pvScreen[0]);
                            }
                            else
                            {
                                GL.Uniform2(v.Shader.GetUniform("cullLine0"), pvScreen[0]);
                                GL.Uniform2(v.Shader.GetUniform("cullLine1"), pvScreen[1]);
                            }
                        }
                        
                    }
                }
                else
                {
                    //GL.Disable(EnableCap.Blend);
                    GL.Uniform2(v.Shader.GetUniform("cullLine0"), new Vector2());
                    GL.Uniform2(v.Shader.GetUniform("cullLine1"), new Vector2());
                }

                _RenderSetTransformMatrix(v, viewMatrix);
                GL.DrawElements(BeginMode.Triangles, v.Indices.Count, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                
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

        private void _RenderPortalClipping(Portal portalEnter, Matrix4 viewMatrix)
        {

        }
    }
}