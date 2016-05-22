using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using ClipperLib;
using System.Diagnostics;
using OpenTK.Input;

namespace Game
{
    public class Renderer
    {
        List<IRenderLayer> _layers = new List<IRenderLayer>();
        readonly Controller _controller;
        public bool PortalRenderEnabled { get; set; }
        public int PortalRenderDepth { get; set; }
        public int PortalRenderMax { get; set; }
        public int PortalClipDepth { get; set; }
        /// <summary>Number of bits in the stencil buffer.</summary>
        public static int StencilBits { get; private set; }
        /// <summary>Flag for preventing rendering occuring.  Intended for benchmarking purposes.</summary>
        public bool RenderEnabled { get; set; }
        ShaderProgram _activeShader;
        Dictionary<EnableCap, bool?> _enableCap = new Dictionary<EnableCap, bool?>();

        public static Dictionary<string, TextureFile> Textures = new Dictionary<string, TextureFile>();
        public static Dictionary<string, ShaderProgram> Shaders = new Dictionary<string, ShaderProgram>();
        public static bool IsInitialized { get; private set; }
        
        public Renderer(Controller controller)
        {
            foreach (EnableCap e in Enum.GetValues(typeof(EnableCap)))
            {
                _enableCap[e] = null;
            }
            _controller = controller;
            PortalRenderEnabled = true;
            PortalRenderDepth = 5;
            PortalRenderMax = 50;
            PortalClipDepth = 4;
            RenderEnabled = true;
        }

        public static void Init()
        {
            GL.ClearColor(Color.HotPink);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.CullFace);
            GL.ClearStencil(0);
            GL.PointSize(15f);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.ScissorTest);
            IsInitialized = true;
            StencilBits = GL.GetInteger(GetPName.StencilBits);
            Debug.Assert(StencilBits >= 8, "Stencil bit depth is too small.");
        }

        public static TextureFile GetTexture(string name)
        {
            if (Textures.ContainsKey(name))
            {
                return Textures[name];
            }
            return null;
        }

        public void AddLayer(IRenderLayer layer)
        {
            _layers.Add(layer);
        }

        /// <summary>Remove a Scene from the Renderer.</summary>
        /// <returns>True if the Scene was in the Renderer, otherwise false.</returns>
        public bool RemoveLayer(IRenderLayer layer)
        {
            return _layers.Remove(layer);
        }

        private void SetShader(ShaderProgram shader)
        {
            if (shader != _activeShader)
            {
                GL.UseProgram(shader.ProgramID);
                _activeShader = shader;
            }
        }

        private void SetEnable(EnableCap enableCap, bool enable)
        {
            if (_enableCap[enableCap] == enable)
            {
                return;
            }
            _enableCap[enableCap] = enable;
            if (enable)
            {
                GL.Enable(enableCap);
            }
            else
            {
                GL.Disable(enableCap);
            }
        }

        public void Render()
        {
            GL.Viewport(0, 0, Controller.CanvasSize.Width, Controller.CanvasSize.Height);
            ResetScissor();
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.ColorBufferBit);

            var shaderList = Renderer.Shaders.ToList();
            for (int i = 0; i < Renderer.Shaders.Count; i++)
            {
                shaderList[i].Value.EnableVertexAttribArrays();
            }
            SetShader(Shaders["uber"]);
            GL.Enable(EnableCap.DepthTest);
            for (int i = 0; i < _layers.Count(); i++)
            {
                IRenderLayer layer = _layers[i];
                ICamera2 camera = layer.GetCamera();
                DrawPortalAll(layer);
                GL.Clear(ClearBufferMask.DepthBufferBit);
            }

            for (int i = 0; i < Renderer.Shaders.Count; i++)
            {
                shaderList[i].Value.DisableVertexAttribArrays();
            }

            GL.Flush();
        }

        public void DrawPortalAll(IRenderLayer layer)
        {
            ICamera2 cam = layer.GetCamera();
            if (cam == null)
            {
                return;
            }
            int depth = 0;
            if (PortalRenderEnabled)
            {
                depth = PortalRenderDepth;
            }
            PortalView portalView = PortalView.CalculatePortalViews(layer.GetPortalList().ToArray(), cam, depth);
            List<PortalView> portalViewList = portalView.GetPortalViewList(PortalRenderMax);

            int stencilValueMax = 1 << StencilBits;
            int stencilMask = stencilValueMax - 1;
            //Draw portal FOVs to the stencil buffer.
            {
                GL.ColorMask(false, false, false, false);
                GL.DepthMask(false);
                GL.Disable(EnableCap.DepthTest);
                SetEnable(EnableCap.StencilTest, true);
                GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
                for (int i = 1; i < Math.Min(portalViewList.Count, stencilValueMax); i++)
                {
                    GL.StencilFunc(StencilFunction.Always, i, stencilMask);
                    Mesh mesh = new Mesh();
                    for (int j = 0; j < portalViewList[i].Paths.Count; j++)
                    {
                        Vector2[] a = ClipperConvert.ToVector2(portalViewList[i].Paths[j]);
                        ModelFactory.AddPolygon(mesh, a);
                    }
                    RenderModel(new Model(mesh), cam.GetViewMatrix(), Matrix4.Identity);
                }
            }
            
            //Draw the scenes within each portal's FOV.
            {
                GL.ColorMask(true, true, true, true);
                GL.DepthMask(true);
                GL.Enable(EnableCap.DepthTest);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                for (int i = 0; i < Math.Min(portalViewList.Count, stencilValueMax); i++)
                {
                    SetScissor(portalViewList[i], cam.GetViewMatrix());
                    GL.StencilFunc(StencilFunction.Equal, i, stencilMask);
                    DrawLayer(layer, portalViewList[i].ViewMatrix, 0, i > 0);
                }
                ResetScissor();
            }
            
            //Draw the portal edges.
            {
                GL.Clear(ClearBufferMask.DepthBufferBit);
                SetEnable(EnableCap.StencilTest, false);
                GL.Disable(EnableCap.DepthTest);
                GL.Clear(ClearBufferMask.StencilBufferBit);
                Clipper c = new Clipper();
                c.StrictlySimple = true;

                Model portalEdges = new Model();
                portalEdges.SetTexture(Textures["lineBlur.png"]);
                SetEnable(EnableCap.Blend, true);
                for (int i = 1; i < Math.Min(portalViewList.Count, stencilValueMax); i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Line line = portalViewList[i].FovLines[j];
                        float minWidth = Math.Abs(cam.GetWorldTransform().Size) / 300;
                        double angleDiff = GetLineBlurAngle(line, portalViewList[i].FovLinesPrevious[j]);
                        float widthEnd = (float)Math.Tan(angleDiff) * line.Length;
                        widthEnd = Math.Max(widthEnd, minWidth);
                        
                        Vector2[] lineWidth = PolygonFactory.CreateLineWidth(line, minWidth);

                        Vector2 camPos = cam.GetWorldTransform().Position;
                        Vector2[] lineWidthOff = Vector2Ext.Transform(lineWidth, Matrix4.CreateTranslation(new Vector3(-camPos)));
                        Vector2[] lineTarget = PolygonFactory.CreateLineWidth(line.Translate(-camPos), minWidth, widthEnd);
                        Matrix4d homography = Matrix4d.CreateTranslation(new Vector3d((Vector2d)(-camPos)));
                        homography *= MathExt.GetHomography(lineWidthOff, lineTarget);
                        homography *= Matrix4d.CreateTranslation(new Vector3d((Vector2d)camPos));

                        bool obscured = true;
                        for (int k = 0; k < portalViewList[i].Parent.Paths.Count; k++)
                        {
                            List<IntPoint> path = portalViewList[i].Parent.Paths[k];
                            if (Clipper.PointInPolygon(ClipperConvert.ToIntPoint(line[0]), path) == 1)
                            {
                                obscured = false;
                                break;
                            }
                        }
                        if (obscured)
                        {
                            continue;
                        }

                        foreach (PortalView p in portalViewList[i].Parent.Children)
                        {
                            if (p == portalViewList[i])
                            {
                                continue;
                            }
                            if (p.PortalLine.IsInsideFOV(camPos, line[0]))
                            {
                                obscured = true;
                                break;
                            }
                        }
                        if (obscured)
                        {
                            continue;
                        }
                        int index = ModelFactory.AddPolygon((Mesh)portalEdges.Mesh, lineWidth);

                        IMesh mesh = portalEdges.Mesh;
                        for (int k = index; k < mesh.GetVertices().Count; k++)
                        {
                            Vertex vertex = mesh.GetVertices()[k];
                            Vector3 pos = Vector3Ext.Transform(vertex.Position, homography);
                            pos.Z = CameraExt.UnitZToWorld(cam, pos.Z);
                            /*vertex.Position = Vector3Ext.Transform(vertex.Position, homography);
                            vertex.Position.Z = cam.UnitZToWorld(vertex.Position.Z);*/

                            Vector2 texCoord;
                            Vector2 v = new Vector2(vertex.Position.X, vertex.Position.Y);
                            double distance = MathExt.PointLineDistance(v, line.GetPerpendicularLeft(), false);
                            double texCoordX = MathExt.PointLineDistance(v, line, false) / minWidth;
                            if (line.GetSideOf(v) == Side.Left)
                            {
                                texCoordX *= -1;
                            }
                            texCoordX += 0.5;
                            texCoord = new Vector2((float)texCoordX, (float)(distance / line.Length));

                            mesh.GetVertices()[k] = new Vertex(pos, texCoord);
                        }
                    }
                }
                RenderModel(portalEdges, cam.GetViewMatrix(false), Matrix4.Identity);
                SetEnable(EnableCap.Blend, false);
                GL.Enable(EnableCap.DepthTest);
            }
        }

        private float GetLineBlurAngle(Line line, Line linePrev)
        {
            const float angleMax = (float)(1f * Math.PI / 4);
            float angleScale = 1f;
            float angleDiff = (float)Math.Abs(MathExt.AngleDiff(line.Angle(), linePrev.Angle()) * angleScale);
            return Math.Min(angleDiff, angleMax);
        }

        private void DrawLayer(IRenderLayer layer, Matrix4 viewMatrix, float timeRenderDelta, bool isPortalRender)
        {
            SetShader(Shaders["uber"]);
            List<IRenderable> renderList = layer.GetRenderList();
            foreach (IRenderable e in renderList)
            {
                if (!e.Visible)
                {
                    continue;
                }
                if (isPortalRender && e.DrawOverPortals)
                {
                    continue;
                }
                List<ClipModel> clipModels = ClipModelCompute.GetClipModels(e, layer.GetPortalList(), PortalClipDepth);
                foreach (ClipModel clip in clipModels)
                {
                    if (clip.ClipLines.Length > 0)
                    {
                        Model model = clip.Model.DeepClone();
                        Matrix4 transform = clip.Entity.GetWorldTransform().GetMatrix() * clip.Transform;
                        for (int i = 0; i < clip.ClipLines.Length; i++)
                        {
                            model.Mesh = MathExt.BisectMesh(model.Mesh, clip.ClipLines[i], transform, Side.Right);
                        }
                        RenderModel(model, viewMatrix, transform);
                    }
                    else
                    {
                        RenderModel(clip.Model, viewMatrix, clip.Entity.GetWorldTransform().GetMatrix() * clip.Transform);
                    }
                }
            }
        }

        /// <summary>Sets scissor region around a portalview.</summary>
        /// <param name="viewMatrix">Camera view matrix, do not use view matrix for the portalview.</param>
        private void SetScissor(PortalView view, Matrix4 viewMatrix)
        {
            Debug.Assert(view != null);
            if (view.Paths == null)
            {
                ResetScissor();
                return;
            }
            Matrix4 ScaleMatrix;
            ScaleMatrix = viewMatrix * Matrix4.CreateTranslation(new Vector3(1, 1, 0));
            ScaleMatrix = ScaleMatrix * Matrix4.CreateScale(new Vector3(Controller.CanvasSize.Width / (float)2, Controller.CanvasSize.Height / (float)2, 0));

            Vector2 vMin, vMax;
            vMin = ClipperConvert.ToVector2(view.Paths[0][0]);
            vMax = ClipperConvert.ToVector2(view.Paths[0][0]);
            for (int i = 0; i < view.Paths.Count; i++)
            {
                for (int j = 0; j < view.Paths[i].Count; j++)
                {
                    Vector2 vTransform = Vector2Ext.Transform(ClipperConvert.ToVector2(view.Paths[i][j]), ScaleMatrix);
                    vMax = Vector2.ComponentMax(vMax, vTransform);
                    vMin = Vector2.ComponentMin(vMin, vTransform);
                }
            }
            //The -1 and +3 are margins to prevent rounding errors from making the scissor box too small.
            GL.Scissor((int)vMin.X - 1, (int)vMin.Y - 1, (int)(vMax.X - vMin.X) + 3, (int)(vMax.Y - vMin.Y) + 3);
        }

        private void ResetScissor()
        {
            GL.Scissor(0, 0, Controller.CanvasSize.Width, Controller.CanvasSize.Height);
        }

        private void UpdateCullFace(Matrix4 viewMatrix)
        {
            SetEnable(EnableCap.CullFace, false);
            if (Matrix4Ext.IsMirrored(viewMatrix))
            {
                GL.CullFace(CullFaceMode.Front);
            }
            else
            {
                GL.CullFace(CullFaceMode.Back);
            }
        }

        public void RenderModel(Model model, Matrix4 viewMatrix, Matrix4 offset)
        {
            Debug.Assert(model != null);
            if (!RenderEnabled)
            {
                return;
            }
            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector2> texcoords = new List<Vector2>();

            // Assemble vertex and indice data for all volumes
            int vertcount = 0;

            verts.AddRange(model.GetVerts().ToList());
            inds.AddRange(model.GetIndices().ToList());
            colors.AddRange(model.GetColorData().ToList());
            texcoords.AddRange(model.GetTextureCoords());
            vertcount += model.Mesh.GetVertices().Count;

            Vector3[] vertdata;
            Vector3[] coldata;
            Vector2[] texcoorddata;
            int[] indicedata;

            vertdata = verts.ToArray();
            indicedata = inds.ToArray();
            coldata = new Vector3[colors.Count];
            for (int i = 0; i < colors.Count; i++)
            {
                coldata[i] = colors[i] * (1 - model.Color.W) + new Vector3(model.Color * model.Color.W);
            }
            //coldata = colors.ToArray();
            texcoorddata = texcoords.ToArray();

            Debug.Assert(indicedata.Length % 3 == 0, "Model must have a multiple of 3 vertex indices.");
            foreach (int a in indicedata)
            {
                Debug.Assert(a >= 0 && a < vertdata.Length);
            }
            Debug.Assert(coldata.Length == vertdata.Length);
            Debug.Assert(texcoorddata.Length == vertdata.Length);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _activeShader.GetBuffer("vPosition"));
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(_activeShader.GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            // Buffer vertex color if shader supports it
            if (_activeShader.GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _activeShader.GetBuffer("vColor"));
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StreamDraw);
                GL.VertexAttribPointer(_activeShader.GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }

            // Buffer texture coordinates if shader supports it
            if (_activeShader.GetAttribute("texcoord") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _activeShader.GetBuffer("texcoord"));
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StreamDraw);
                GL.VertexAttribPointer(_activeShader.GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
            }
            //GL.BindBuffer(BufferTarget.ArrayBuffer, 0); I don't know why this is here so I've commented it out until something breaks.

            if (_activeShader.GetUniform("UVMatrix") != -1)
            {
                Matrix4 UVMatrix = model.TransformUv.GetMatrix();
                GL.UniformMatrix4(_activeShader.GetUniform("UVMatrix"), false, ref UVMatrix);
            }

            if (model.Texture != null)
            {
                GL.Uniform1(_activeShader.GetUniform("isTextured"), 1);
                GL.BindTexture(TextureTarget.Texture2D, model.Texture.GetId());
                //GL.Uniform1(v.Shader.GetAttribute("maintexture"), v.Texture.Id);
            }
            else
            {
                GL.Uniform1(_activeShader.GetUniform("isTextured"), 0);
                GL.BindTexture(TextureTarget.Texture2D, -1);
                //GL.Uniform1(v.Shader.GetAttribute("maintexture"), -1);
            }

            // Buffer index data
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, model.GetIbo());
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StreamDraw);

            if (model.Wireframe)
            {
                SetEnable(EnableCap.CullFace, false);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            if (model.IsTransparent)
            {
                SetEnable(EnableCap.Blend, true);
            }

            RenderSetTransformMatrix(offset, model, viewMatrix);
            GL.DrawElements(BeginMode.Triangles, model.GetIndices().Length, DrawElementsType.UnsignedInt, 0);

            if (model.Wireframe)
            {
                SetEnable(EnableCap.CullFace, true);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
            if (model.IsTransparent)
            {
                SetEnable(EnableCap.Blend, false);
            }
        }

        private void RenderSetTransformMatrix(Matrix4 offset, Model model, Matrix4 viewMatrix)
        {
            Matrix4 modelMatrix = model.Transform.GetMatrix() * offset;
            UpdateCullFace(modelMatrix * viewMatrix);
            GL.UniformMatrix4(_activeShader.GetUniform("modelMatrix"), false, ref modelMatrix);
            GL.UniformMatrix4(_activeShader.GetUniform("viewMatrix"), false, ref viewMatrix);
        }
    }
}
