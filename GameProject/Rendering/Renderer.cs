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
using OpenTK.Graphics;

namespace Game
{
    /// <summary>
    /// Handles OpenGL rendering.  Only one instance of Renderer should be instantiated during the process's lifetime.
    /// </summary>
    public class Renderer
    {
        public readonly static GraphicsMode DefaultGraphics = new GraphicsMode(32, 24, 8, 1);
        List<IRenderLayer> _layers = new List<IRenderLayer>();
        public bool PortalRenderEnabled { get; set; } = true;
        public int PortalRenderMax { get; set; } = 50;
        public int PortalClipDepth { get; set; } = 4;
        /// <summary>Number of bits in the stencil buffer.</summary>
        public int StencilBits { get; private set; }
        /// <summary>Flag for preventing rendering occuring.  Intended for benchmarking purposes.</summary>
        public bool RenderEnabled { get; set; } = true;
        ShaderProgram _activeShader;
        Dictionary<EnableCap, bool?> _enableCap = new Dictionary<EnableCap, bool?>();

        public static Dictionary<string, TextureFile> Textures = new Dictionary<string, TextureFile>();
        public static Dictionary<string, ShaderProgram> Shaders = new Dictionary<string, ShaderProgram>();
        public static bool IsInitialized { get; private set; }

        private static int IboElements;

        public Renderer()
        {
            foreach (EnableCap e in Enum.GetValues(typeof(EnableCap)))
            {
                _enableCap[e] = null;
            }

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

            GL.GenBuffers(1, out IboElements);
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

            var shaderList = Shaders.ToList();
            for (int i = 0; i < Shaders.Count; i++)
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

            for (int i = 0; i < Shaders.Count; i++)
            {
                shaderList[i].Value.DisableVertexAttribArrays();
            }

            GL.Flush();
        }

        private class DrawData
        {
            public int Index;
            public Model Model;
            public Matrix4 Offset;

            public DrawData(int index, Model model, Matrix4 offset)
            {
                Index = index;
                Model = model;
                Offset = offset;
            }
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
                depth = PortalRenderMax;
            }
            PortalView portalView = PortalView.CalculatePortalViews(layer.GetPortalList().ToArray(), cam, depth);
            List<PortalView> portalViewList = portalView.GetPortalViewList();

            int stencilValueMax = 1 << StencilBits;
            int stencilMask = stencilValueMax - 1;

            #region Draw portal FOVs to the stencil buffer.
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
                    RenderModel(new Model(mesh), CameraExt.GetViewMatrix(cam));
                }
            }
            #endregion

            List<DrawData> drawData = new List<DrawData>();

            #region Get models.
            {
                HashSet<Model> models = new HashSet<Model>();
                List<IRenderable> renderList = layer.GetRenderList();
                foreach (IRenderable e in renderList)
                {
                    if (!e.Visible)
                    {
                        continue;
                    }
                    List<Clip.ClipModel> clipModels = Clip.GetClipModels(e, layer.GetPortalList(), PortalClipDepth);
                    foreach (Clip.ClipModel clip in clipModels)
                    {
                        if (clip.ClipLines.Length > 0)
                        {
                            Model model = clip.Model.DeepClone();
                            Matrix4 transform = clip.Entity.GetWorldTransform().GetMatrix() * clip.Transform;
                            for (int i = 0; i < clip.ClipLines.Length; i++)
                            {
                                model.Mesh = MathExt.BisectMesh(model.Mesh, clip.ClipLines[i], transform, Side.Right);
                            }
                            models.Add(model);
                            drawData.Add(new DrawData(-1, model, transform));
                        }
                        else
                        {
                            models.Add(clip.Model);
                            drawData.Add(new DrawData(
                                -1, 
                                clip.Model, 
                                clip.Entity.GetWorldTransform().GetMatrix() * clip.Transform));
                        }
                    }
                }
                var indexList = BufferModels(models.ToArray());
                for (int i = 0; i < drawData.Count; i++)
                {
                    DrawData d = drawData[i];
                    d.Index = indexList[d.Model];
                }
            }
            #endregion

            #region Draw the scenes within each portal's FOV.
            {
                GL.ColorMask(true, true, true, true);
                GL.DepthMask(true);
                GL.Enable(EnableCap.DepthTest);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                for (int i = 0; i < Math.Min(portalViewList.Count, stencilValueMax); i++)
                {
                    SetScissor(portalViewList[i], CameraExt.GetViewMatrix(cam));
                    GL.StencilFunc(StencilFunction.Equal, i, stencilMask);
                    Draw(drawData.ToArray(), portalViewList[i].ViewMatrix);
                }
                ResetScissor();
            }
            #endregion

            #region Draw the portal edges.
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
                RenderModel(portalEdges, CameraExt.GetViewMatrix(cam, false));
                SetEnable(EnableCap.Blend, false);
                GL.Enable(EnableCap.DepthTest);
            }
            #endregion
        }

        private float GetLineBlurAngle(Line line, Line linePrev)
        {
            const float angleMax = (float)(1f * Math.PI / 4);
            float angleScale = 80f;
            float angleDiff = (float)Math.Abs(MathExt.AngleDiff(line.Angle(), linePrev.Angle()) * angleScale);
            return Math.Min(angleDiff, angleMax);
        }

        private void Draw(DrawData[] drawData, Matrix4 viewMatrix)
        {
            for (int i = 0; i < drawData.Length; i++)
            {
                DrawData data = drawData[i];
                Debug.Assert(data.Model != null);
                if (!RenderEnabled)
                {
                    return;
                }

                if (_activeShader.GetUniform("UVMatrix") != -1)
                {
                    Matrix4 UVMatrix = data.Model.TransformUv.GetMatrix();
                    GL.UniformMatrix4(_activeShader.GetUniform("UVMatrix"), false, ref UVMatrix);
                }

                if (data.Model.Texture != null)
                {
                    GL.Uniform1(_activeShader.GetUniform("isTextured"), 1);
                    GL.BindTexture(TextureTarget.Texture2D, data.Model.Texture.GetId());
                }
                else
                {
                    GL.Uniform1(_activeShader.GetUniform("isTextured"), 0);
                    GL.BindTexture(TextureTarget.Texture2D, -1);
                }

                if (data.Model.Wireframe)
                {
                    SetEnable(EnableCap.CullFace, false);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                }
                if (data.Model.IsTransparent)
                {
                    SetEnable(EnableCap.Blend, true);
                }

                RenderSetTransformMatrix(data.Offset, data.Model, viewMatrix);
                GL.DrawElements(PrimitiveType.Triangles, data.Model.GetIndices().Length, DrawElementsType.UnsignedInt, (IntPtr)(data.Index * sizeof(int)));
                if (data.Model.Wireframe)
                {
                    SetEnable(EnableCap.CullFace, true);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
                if (data.Model.IsTransparent)
                {
                    SetEnable(EnableCap.Blend, false);
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

        public void RenderModel(Model model, Matrix4 viewMatrix)
        {
            Debug.Assert(model != null);
            if (!RenderEnabled)
            {
                return;
            }

            BufferData(model.GetVerts(), model.GetColorData(), model.GetTextureCoords(), model.GetIndices(), IboElements);

            if (_activeShader.GetUniform("UVMatrix") != -1)
            {
                Matrix4 UVMatrix = model.TransformUv.GetMatrix();
                GL.UniformMatrix4(_activeShader.GetUniform("UVMatrix"), false, ref UVMatrix);
            }

            if (model.Texture != null)
            {
                GL.Uniform1(_activeShader.GetUniform("isTextured"), 1);
                GL.BindTexture(TextureTarget.Texture2D, model.Texture.GetId());
            }
            else
            {
                GL.Uniform1(_activeShader.GetUniform("isTextured"), 0);
                GL.BindTexture(TextureTarget.Texture2D, -1);
            }

            if (model.Wireframe)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                SetEnable(EnableCap.CullFace, false);
            }
            if (model.IsTransparent)
            {
                SetEnable(EnableCap.Blend, true);
            }

            RenderSetTransformMatrix(Matrix4.Identity, model, viewMatrix);
            GL.DrawElements(PrimitiveType.Triangles, model.GetIndices().Length, DrawElementsType.UnsignedInt, 0);

            if (model.Wireframe)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                SetEnable(EnableCap.CullFace, true);
            }
            if (model.IsTransparent)
            {
                SetEnable(EnableCap.Blend, false);
            }
        }

        private Dictionary<Model, int> BufferModels(Model[] models)
        {
            Dictionary<Model, int> indexList = new Dictionary<Model, int>();

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector2> texCoords = new List<Vector2>();
            List<int> indices = new List<int>();
            for (int i = 0; i < models.Length; i++)
            {
                indexList.Add(models[i], indices.Count);

                Vector3[] modelVerts = models[i].GetVerts();

                int[] modelIndices = models[i].GetIndices();
                for (int j = 0; j < modelIndices.Length; j++)
                {
                    Debug.Assert(modelIndices[j] >= 0 && modelIndices[j] < modelVerts.Length);
                    modelIndices[j] += vertices.Count;
                }
                indices.AddRange(modelIndices);

                vertices.AddRange(modelVerts);
                colors.AddRange(models[i].GetColorData());
                texCoords.AddRange(models[i].GetTextureCoords());
            }

            BufferData(vertices.ToArray(), colors.ToArray(), texCoords.ToArray(), indices.ToArray(), IboElements);
            return indexList;
        }

        private void BufferData(Vector3[] vertdata, Vector3[] coldata, Vector2[] texcoorddata, int[] indices, int indexBuffer)
        {
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

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StreamDraw);
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
