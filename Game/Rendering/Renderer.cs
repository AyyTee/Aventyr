using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClipperLib;
using Game.Common;
using Game.Models;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Text;
using System.IO;

namespace Game.Rendering
{
    /// <summary>
    /// Handles OpenGL rendering.  Only one instance of Renderer should be instantiated during the process's lifetime.
    /// </summary>
    public class Renderer : IRenderer
    {
        public bool PortalRenderEnabled { get; set; } = true;
        public int PortalRenderMax { get; set; } = 50;
        public int PortalClipDepth { get; set; } = 4;
        /// <summary>Number of bits in the stencil buffer.</summary>
        public int StencilBits { get; }
        public int StencilMaxValue => 1 << StencilBits;
        public int StencilMask => StencilMaxValue - 1;

        Shader _activeShader;

        readonly Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();

        readonly GlStateManager _state;

        readonly int _iboElements;

        public List<IVirtualWindow> Windows { get; private set; } = new List<IVirtualWindow>();

        Vector2i ClientSize => _canvasSizeProvider.ClientSize;

        IClientSizeProvider _canvasSizeProvider;

        readonly Resources _textures;

        public Renderer(IClientSizeProvider canvasSizeProvider, Resources textures)
        {
            _canvasSizeProvider = canvasSizeProvider;
            _textures = textures;

            _state = new GlStateManager();

            GL.ClearColor(Color4.HotPink);
            GL.CullFace(CullFaceMode.Back);

            GL.ClearStencil(0);
            GL.PointSize(15f);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            _shaders.Add("uber", new Shader(UberShader.GetVertexShader(), UberShader.GetFragmentShader()));

            // Skip display mode diagnostics on Mac as it doesn't seem to support GetInteger.
            if (Configuration.RunningOnMacOS)
            {
                StencilBits = 8;
            }
            else
            {
                StencilBits = GL.GetInteger(GetPName.StencilBits);
                var depthBits = GL.GetInteger(GetPName.DepthBits);
                var samples = GL.GetInteger(GetPName.Samples);
                var rgbBits =
                    GL.GetInteger(GetPName.RedBits) +
                    GL.GetInteger(GetPName.GreenBits) +
                    GL.GetInteger(GetPName.BlueBits) +
                    GL.GetInteger(GetPName.AlphaBits);
                DebugEx.Assert(StencilBits >= 8, "Stencil bit depth is too small.");
                DebugEx.Assert(depthBits == 24);
                DebugEx.Assert(samples == 0);
                DebugEx.Assert(rgbBits == 32);
            }

            GL.GenBuffers(1, out _iboElements);
        }

        void SetShader(Shader shader)
        {
            if (shader != _activeShader)
            {
                GL.UseProgram(shader.ProgramId);
                _activeShader = shader;
            }
        }

        public void Render()
        {
            var glError = GL.GetError();
            DebugEx.Assert(glError == ErrorCode.NoError);

            SetScissor(null);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.ColorBufferBit);

            var shaderList = _shaders.ToList();
            for (int i = 0; i < _shaders.Count; i++)
            {
                shaderList[i].Value.EnableVertexAttribArrays();
            }
            SetShader(_shaders["uber"]);

            foreach (var window in Windows)
            {
                SetScissor(window);
                GL.Viewport(window.CanvasPosition.X, window.CanvasPosition.Y, window.CanvasSize.X, window.CanvasSize.Y);
                foreach (var layer in window.Layers)
                {
                    using (_state.Push(EnableCap.DepthTest, layer.DepthTest))
                    {
                        DrawPortalAll(window, layer, 1 / window.RendersPerSecond);
                    }
                    GL.Clear(ClearBufferMask.DepthBufferBit);
                    GL.Clear(ClearBufferMask.StencilBufferBit);
                }
            }

            for (int i = 0; i < _shaders.Count; i++)
            {
                shaderList[i].Value.DisableVertexAttribArrays();
            }

            GL.Flush();
        }

        class DrawData
        {
            public int Index { get; }
            public Model Model { get; }
            public Matrix4 Offset { get; }

            public DrawData(int index, Model model, Matrix4 offset)
            {
                Debug.Assert(model != null);
                Index = index;
                Model = model;
                Offset = offset;
            }
        }

        void DrawPortalAll(IVirtualWindow window, IRenderLayer layer, float shutterTime)
        {
            DebugEx.Assert(window.Layers.Contains(layer));

            ICamera2 cam = layer.Camera;
            if (cam == null)
            {
                return;
            }
            PortalView portalView = PortalView.CalculatePortalViews(
                shutterTime * layer.MotionBlurFactor,
                layer.Portals,
                cam,
                PortalRenderEnabled ? PortalRenderMax : 0);
            List<PortalView> portalViewList = portalView.GetPortalViewList();

            
            var data = new Data();

            List<DrawData> portalViewModels = new List<DrawData>();
            for (int i = 1; i < Math.Min(portalViewList.Count, StencilMaxValue); i++)
            {
                var mesh = new Mesh();
                for (int j = 0; j < portalViewList[i].Paths.Count; j++)
                {
                    Vector2[] a = ClipperConvert.ToVector2(portalViewList[i].Paths[j]);
                    ModelFactory.AddPolygon(mesh, a, Color4.White);
                }

                portalViewModels.Add(data.BufferModel(new Model(mesh), Matrix4.Identity));
            }

            List<DrawData> sceneModels = new List<DrawData>();
            #region Get models.
            {
                foreach (IRenderable e in layer.Renderables)
                {
                    if (!e.Visible)
                    {
                        continue;
                    }
                    var clipModels = Clip.GetClipModels(e, layer.Portals, PortalClipDepth, cam, window.CanvasSize);
                    DebugEx.Assert(clipModels.All(item => item.Model != null));
                    foreach (var clip in clipModels)
                    {
                        if (clip.ClipLines.Length > 0)
                        {
                            Model model = clip.Model.DeepClone();
                            Matrix4 transform = clip.WorldTransform.GetMatrix() * clip.Transform;
                            for (int i = 0; i < clip.ClipLines.Length; i++)
                            {
                                model.Mesh = model.Mesh.Bisect(clip.ClipLines[i], clip.Model.Transform.GetMatrix() * transform, Side.Right);
                            }

                            sceneModels.Add(data.BufferModel(model, transform));
                        }
                        else
                        {
                            sceneModels.Add(data.BufferModel(clip.Model, clip.WorldTransform.GetMatrix() * clip.Transform));
                        }
                    }
                }
            }
            #endregion

            var portalEdgesData = data.BufferModel(
                GetPortalEdges(portalViewList, cam), 
                Matrix4.Identity);

            BufferData(
                data.Vertices.ToArray(), 
                data.Colors.ToArray(), 
                data.TexCoords.ToArray(), 
                data.Indices.ToArray(), 
                _iboElements);


            GL.ColorMask(false, false, false, false);
            GL.DepthMask(false);
            using (_state.Push(EnableCap.DepthTest, false))
            using (_state.Push(EnableCap.StencilTest, true))
            {
                GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
                for (int i = 1; i < Math.Min(portalViewList.Count, StencilMaxValue); i++)
                {
                    GL.StencilFunc(StencilFunction.Always, i, StencilMask);
                    Draw(new[] { portalViewModels[i - 1] }, cam.GetViewMatrix());
                }
            }

            #region Draw the scenes within each portal's Fov.
            {
                GL.ColorMask(true, true, true, true);
                GL.DepthMask(true);
                using (_state.Push(EnableCap.StencilTest, true))
                {
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                    for (int i = 0; i < Math.Min(portalViewList.Count, StencilMaxValue); i++)
                    {
                        SetScissor(window, portalViewList[i], cam.GetViewMatrix());
                        GL.StencilFunc(StencilFunction.Equal, i, StencilMask);
                        Draw(sceneModels.ToArray(), portalViewList[i].ViewMatrix);
                    }
                    SetScissor(window);
                }
            }
            #endregion

            using (_state.Push(EnableCap.StencilTest, false))
            using (_state.Push(EnableCap.DepthTest, false))
            {
                Draw(new[] { portalEdgesData }, cam.GetViewMatrix(false));
            }
        }

        Model GetPortalEdges(List<PortalView> portalViewList, ICamera2 cam)
        {
            int iterations = Math.Min(portalViewList.Count, StencilMaxValue);
            /* Escape early if there aren't any visible portals.
             * The first iteration is just for the main view which doesn't have portal edges.*/
            if (iterations <= 1)
            {
                return new Model();
            }

            var portalEdges = new Model();
            portalEdges.SetTexture(_textures.LineBlur);

            for (int i = 1; i < iterations; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    LineF line = portalViewList[i].FovLines[j];
                    float minWidth = Math.Abs(cam.WorldTransform.Size) / 300;
                    double angleDiff = GetLineBlurAngle(line, portalViewList[i].FovLinesPrevious[j]);
                    float widthEnd = (float)Math.Tan(angleDiff) * line.Length;
                    widthEnd = Math.Max(widthEnd, minWidth);

                    Vector2[] lineWidth = PolygonFactory.CreateLineWidth(line, minWidth);

                    Vector2 camPos = cam.WorldTransform.Position;
                    Vector2[] lineWidthOff = Vector2Ex.Transform(lineWidth, Matrix4.CreateTranslation(new Vector3(-camPos)));
                    Vector2[] lineTarget = PolygonFactory.CreateLineWidth(line.Translate(-camPos), minWidth, widthEnd);
                    Matrix4d homography = Matrix4d.CreateTranslation(new Vector3d((Vector2d)(-camPos)));
                    homography *= MathEx.GetHomography(lineWidthOff, lineTarget);
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
                        if (p.PortalLine.IsInsideFov(camPos, line[0]))
                        {
                            obscured = true;
                            break;
                        }
                    }
                    if (obscured)
                    {
                        continue;
                    }
                    int index = ModelFactory.AddPolygon((Mesh)portalEdges.Mesh, lineWidth, Color4.White);

                    IMesh mesh = portalEdges.Mesh;
                    for (int k = index; k < mesh.GetVertices().Count; k++)
                    {
                        Vertex vertex = mesh.GetVertices()[k];
                        Vector3 pos = Vector3Ex.Transform(vertex.Position, homography);
                        pos.Z = cam.UnitZToWorld(pos.Z);

                        var v = new Vector2(vertex.Position.X, vertex.Position.Y);
                        double distance = MathEx.PointLineDistance(v, line.GetPerpendicularLeft(), false);
                        double texCoordX = MathEx.PointLineDistance(v, line, false) / minWidth;
                        if (line.GetSideOf(v) == Side.Left)
                        {
                            texCoordX *= -1;
                        }
                        texCoordX += 0.5;
                        var texCoord = new Vector2((float)texCoordX, (float)(distance / line.Length));

                        mesh.GetVertices()[k] = new Vertex(pos, texCoord, Color4.Black);
                    }
                }
            }

            return portalEdges;
        }

        float GetLineBlurAngle(LineF line, LineF linePrev)
        {
            const float angleMax = (float)(1f * Math.PI / 4);
            float angleScale = 80f;
            float angleDiff = (float)Math.Abs(MathEx.AngleDiff(line.Angle(), linePrev.Angle()) * angleScale);
            return Math.Min(angleDiff, angleMax);
        }

        void Draw(DrawData[] drawData, Matrix4 viewMatrix)
        {
            for (int i = 0; i < drawData.Length; i++)
            {
                DrawData data = drawData[i];

                if (!data.Model.GetIndices().Any())
                {
                    continue;
                }

                Matrix4 uvMatrix = data.Model.TransformUv.GetMatrix();
                GL.UniformMatrix4(_activeShader.Uniforms[UberShader.UvMatrix].Address, false, ref uvMatrix);

                GL.Uniform1(
                    _activeShader.Uniforms[UberShader.IsTextured].Address, 
                    data.Model.Texture != null ? 1 : 0);
                if (data.Model.Texture != null)
                {
                    GL.Uniform1(_activeShader.Uniforms[UberShader.MainTexture].Address, 0);
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, data.Model.Texture.Id);
                }

                GL.Uniform1(
                    _activeShader.Uniforms[UberShader.IsDithered].Address, 
                    data.Model.IsDithered ? 1 : 0);

                if (data.Model.Wireframe)
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                }
                using (_state.Push(EnableCap.CullFace, !data.Model.Wireframe))
                using (_state.Push(EnableCap.Blend, data.Model.IsTransparent))
                {
                    RenderSetTransformMatrix(data.Offset, data.Model, viewMatrix);
                    GL.DrawElements(PrimitiveType.Triangles, data.Model.GetIndices().Length, DrawElementsType.UnsignedInt, (IntPtr)(data.Index * sizeof(int)));
                }
                if (data.Model.Wireframe)
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
            }
        }

        /// <summary>Sets scissor region around a portalview.</summary>
        /// <param name="view"></param>
        /// <param name="viewMatrix">Camera view matrix, do not use view matrix for the portalview.</param>
        void SetScissor(IVirtualWindow window, PortalView view, Matrix4 viewMatrix)
        {
            //return;
            DebugEx.Assert(view != null);
            if (view.Paths == null)
            {
                SetScissor(window);
                return;
            }
            Matrix4 scaleMatrix = viewMatrix * Matrix4.CreateTranslation(new Vector3(1, 1, 0));
            scaleMatrix = scaleMatrix * Matrix4.CreateScale(new Vector3(ClientSize.X / (float)2, ClientSize.Y / (float)2, 0));

            Vector2 vMin = ClipperConvert.ToVector2(view.Paths[0][0]);
            Vector2 vMax = ClipperConvert.ToVector2(view.Paths[0][0]);
            for (int i = 0; i < view.Paths.Count; i++)
            {
                for (int j = 0; j < view.Paths[i].Count; j++)
                {
                    Vector2 vTransform = Vector2Ex.Transform(ClipperConvert.ToVector2(view.Paths[i][j]), scaleMatrix);
                    vMax = Vector2.ComponentMax(vMax, vTransform);
                    vMin = Vector2.ComponentMin(vMin, vTransform);
                }
            }

            vMin += new Vector2(window.CanvasPosition.X, window.CanvasPosition.Y);
            vMax += new Vector2(window.CanvasPosition.X, window.CanvasPosition.Y);

            //The -1 and +3 are margins to prevent rounding errors from making the scissor box too small.
            GL.Scissor((int)vMin.X - 1, (int)vMin.Y - 1, (int)(vMax.X - vMin.X) + 3, (int)(vMax.Y - vMin.Y) + 3);
        }

        void SetScissor(IVirtualWindow window)
        {
            if (window == null)
            {
                GL.Scissor(0, 0, ClientSize.X, ClientSize.Y);
                return;
            }
            GL.Scissor(window.CanvasPosition.X, window.CanvasPosition.Y, window.CanvasSize.X, window.CanvasSize.Y);
        }

        void UpdateCullFace(Matrix4 viewMatrix)
        {
            //EnableCapState.Push(EnableCap.CullFace, false);
            GL.CullFace(Matrix4Ex.IsMirrored(viewMatrix) ? CullFaceMode.Front : CullFaceMode.Back);
        }

        class Data
        {
            public List<Vector3> Vertices { get; } = new List<Vector3>();
            public List<Vector4> Colors { get; } = new List<Vector4>();
            public List<Vector2> TexCoords { get; } = new List<Vector2>();
            public List<int> Indices { get; } = new List<int>();

            public DrawData BufferModel(Model model, Matrix4 offset)
            {
                var index = Indices.Count;

                Vector3[] modelVerts = model.GetVerts();

                int[] modelIndices = model.GetIndices();
                for (int j = 0; j < modelIndices.Length; j++)
                {
                    DebugEx.Assert(modelIndices[j] >= 0 && modelIndices[j] < modelVerts.Length);
                    modelIndices[j] += Vertices.Count;
                }
                Indices.AddRange(modelIndices);

                Vertices.AddRange(modelVerts);
                Colors.AddRange(model.GetColorData());
                TexCoords.AddRange(model.GetTextureCoords());

                return new DrawData(index, model, offset);
            }
        }

        void BufferData(Vector3[] vertdata, Vector4[] coldata, Vector2[] texcoorddata, int[] indices, int indexBuffer)
        {
            DebugEx.Assert(coldata.Length == vertdata.Length);
            DebugEx.Assert(texcoorddata.Length == vertdata.Length);

            // Buffer vertex coordinates.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _activeShader.Buffers[UberShader.VertPosition]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(_activeShader.Attributes[UberShader.VertPosition].Address, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Buffer vertex colors.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _activeShader.Buffers[UberShader.VertColor]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector4.SizeInBytes), coldata, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(_activeShader.Attributes[UberShader.VertColor].Address, 4, VertexAttribPointerType.Float, true, 0, 0);

            // Buffer vertex UV coordinates.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _activeShader.Buffers[UberShader.VertUvCoord]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(_activeShader.Attributes[UberShader.VertUvCoord].Address, 2, VertexAttribPointerType.Float, true, 0, 0);

            // Buffer indices.
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StreamDraw);
        }

        void RenderSetTransformMatrix(Matrix4 offset, Model model, Matrix4 viewMatrix)
        {
            Matrix4 modelMatrix = model.Transform.GetMatrix() * offset;
            UpdateCullFace(modelMatrix * viewMatrix);
            GL.UniformMatrix4(_activeShader.Uniforms[UberShader.ModelMatrix].Address, false, ref modelMatrix);
            GL.UniformMatrix4(_activeShader.Uniforms[UberShader.ViewMatrix].Address, false, ref viewMatrix);
        }
    }
}
