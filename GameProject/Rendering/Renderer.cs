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

        /// <summary>Flag for preventing rendering occuring.  Intended for benchmarking purposes.</summary>
        public bool RenderEnabled { get; set; } = true;
        Shader _activeShader;

        readonly Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();

        readonly GlStateManager _state;

        readonly int _iboElements;

        public List<IVirtualWindow> Windows { get; private set; } = new List<IVirtualWindow>();

        Vector2i ClientSize => _canvasSizeProvider.ClientSize;

        IClientSizeProvider _canvasSizeProvider;

        readonly TextureAssets _textures;

        public Renderer(IClientSizeProvider canvasSizeProvider, TextureAssets textures)
        {
            _canvasSizeProvider = canvasSizeProvider;
            _textures = textures;

            _state = new GlStateManager();

            GL.ClearColor(Color4.HotPink);
            GL.CullFace(CullFaceMode.Back);

            GL.ClearStencil(0);
            GL.PointSize(15f);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			
            // Load shaders from file
			_shaders.Add("uber", new Shader(
				Path.Combine(AssetPaths.ShaderFolder, "vs_uber.glsl"),
				Path.Combine(AssetPaths.ShaderFolder, "fs_uber.glsl"),
				true));

            // Skip display mode diagnostics on Mac as it doesn't seem to support GetInteger.
            if (Configuration.RunningOnMacOS)
            {
                StencilBits = 8;
            }
            else
            {
				StencilBits = 8;//= GL.GetInteger(GetPName.StencilBits);
				var depthBits = GL.GetInteger(GetPName.DepthBits);
				var samples = GL.GetInteger(GetPName.Samples);
				var rgbBits =
					GL.GetInteger(GetPName.RedBits) +
					GL.GetInteger(GetPName.GreenBits) +
					GL.GetInteger(GetPName.BlueBits) +
					GL.GetInteger(GetPName.AlphaBits);
				var version = GL.GetString(StringName.Version);
				DebugEx.Assert(StencilBits >= 8, "Stencil bit depth is too small.");
				DebugEx.Assert(depthBits == 24);
				DebugEx.Assert(samples == 1);
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
            public int Index;
            public readonly Model Model;
            public readonly Matrix4 Offset;

            public DrawData(int index, Model model, Matrix4 offset)
            {
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
                shutterTime,
                layer.Portals,
                cam,
                PortalRenderEnabled ? PortalRenderMax : 0);
            List<PortalView> portalViewList = portalView.GetPortalViewList();

            #region Draw portal FOVs to the stencil buffer.
            {
                GL.ColorMask(false, false, false, false);
                GL.DepthMask(false);
                using (_state.Push(EnableCap.DepthTest, false))
                using (_state.Push(EnableCap.StencilTest, true))
                {
                    GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
                    for (int i = 1; i < Math.Min(portalViewList.Count, StencilMaxValue); i++)
                    {
                        GL.StencilFunc(StencilFunction.Always, i, StencilMask);
                        var mesh = new Mesh();
                        for (int j = 0; j < portalViewList[i].Paths.Count; j++)
                        {
                            Vector2[] a = ClipperConvert.ToVector2(portalViewList[i].Paths[j]);
                            ModelFactory.AddPolygon(mesh, a);
                        }
                        RenderModel(new Model(mesh), cam.GetViewMatrix());
                    }
                }
            }
            #endregion

            List<DrawData> drawData = new List<DrawData>();

            #region Get models.
            {
                HashSet<Model> models = new HashSet<Model>();
                foreach (IRenderable e in layer.Renderables)
                {
                    if (!e.Visible)
                    {
                        continue;
                    }
                    List<Clip.ClipModel> clipModels = Clip.GetClipModels(e, layer.Portals, PortalClipDepth);
                    DebugEx.Assert(clipModels.All(item => item.Model != null));
                    foreach (Clip.ClipModel clip in clipModels)
                    {
                        if (clip.ClipLines.Length > 0)
                        {
                            Model model = clip.Model.DeepClone();
                            Matrix4 transform = clip.Entity.WorldTransform.GetMatrix() * clip.Transform;
                            for (int i = 0; i < clip.ClipLines.Length; i++)
                            {
                                model.Mesh = model.Mesh.Bisect(clip.ClipLines[i], clip.Model.Transform.GetMatrix() * transform, Side.Right);
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
                                clip.Entity.WorldTransform.GetMatrix() * clip.Transform));
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
                        Draw(drawData.ToArray(), portalViewList[i].ViewMatrix);
                    }
                    SetScissor(window);
                }
            }
            #endregion

            RenderPortalEdges(portalViewList, cam);
        }

        /// <summary>
        /// Draw the edges for each portal potentially including motion blur.
        /// </summary>
        /// <param name="portalViewList"></param>
        /// <param name="cam"></param>
        void RenderPortalEdges(List<PortalView> portalViewList, ICamera2 cam)
        {
            int iterations = Math.Min(portalViewList.Count, StencilMaxValue);
            /* Escape early if there aren't any visible portals.
             * The first iteration is just for the main view which doesn't have portal edges.*/
            if (iterations <= 1)
            {
                return;
            }

            using (_state.Push(EnableCap.StencilTest, false))
            using (_state.Push(EnableCap.DepthTest, false))
            {
                var portalEdges = new Model();
                portalEdges.IsTransparent = true;
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
                        int index = ModelFactory.AddPolygon((Mesh)portalEdges.Mesh, lineWidth);

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

                            mesh.GetVertices()[k] = new Vertex(pos, texCoord);
                        }
                    }
                }
                RenderModel(portalEdges, cam.GetViewMatrix(false));
            }
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
                DebugEx.Assert(data.Model != null);
                if (!RenderEnabled)
                {
                    return;
                }

                if (_activeShader.GetUniform("UVMatrix") != -1)
                {
                    Matrix4 uvMatrix = data.Model.TransformUv.GetMatrix();
                    GL.UniformMatrix4(_activeShader.GetUniform("UVMatrix"), false, ref uvMatrix);
                }

                if (data.Model.Texture != null)
                {
                    GL.Uniform1(_activeShader.GetUniform("isTextured"), 1);
                    GL.BindTexture(TextureTarget.Texture2D, data.Model.Texture.GetId());
                }
                else
                {
                    GL.Uniform1(_activeShader.GetUniform("isTextured"), 0);
                    //GL.BindTexture(TextureTarget.Texture2D, -1);
                }

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

        public void RenderModel(Model model, Matrix4 viewMatrix)
        {
            DebugEx.Assert(model != null);
            if (!RenderEnabled)
            {
                return;
            }

            BufferData(model.GetVerts(), model.GetColorData(), model.GetTextureCoords(), model.GetIndices(), _iboElements);

            if (_activeShader.GetUniform("UVMatrix") != -1)
            {
                Matrix4 uvMatrix = model.TransformUv.GetMatrix();
                GL.UniformMatrix4(_activeShader.GetUniform("UVMatrix"), false, ref uvMatrix);
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
            }
            using (_state.Push(EnableCap.CullFace, !model.Wireframe))
            using (_state.Push(EnableCap.Blend, model.IsTransparent))
            {
                RenderSetTransformMatrix(Matrix4.Identity, model, viewMatrix);
                GL.DrawElements(PrimitiveType.Triangles, model.GetIndices().Length, DrawElementsType.UnsignedInt, 0);
            }
            if (model.Wireframe)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
        }

        Dictionary<Model, int> BufferModels(Model[] models)
        {
            var indexList = new Dictionary<Model, int>();

            var vertices = new List<Vector3>();
            var colors = new List<Vector4>();
            var texCoords = new List<Vector2>();
            var indices = new List<int>();
            for (int i = 0; i < models.Length; i++)
            {
                indexList.Add(models[i], indices.Count);

                Vector3[] modelVerts = models[i].GetVerts();

                int[] modelIndices = models[i].GetIndices();
                for (int j = 0; j < modelIndices.Length; j++)
                {
                    DebugEx.Assert(modelIndices[j] >= 0 && modelIndices[j] < modelVerts.Length);
                    modelIndices[j] += vertices.Count;
                }
                indices.AddRange(modelIndices);

                vertices.AddRange(modelVerts);
                colors.AddRange(models[i].GetColorData());
                texCoords.AddRange(models[i].GetTextureCoords());
            }

            BufferData(vertices.ToArray(), colors.ToArray(), texCoords.ToArray(), indices.ToArray(), _iboElements);
            return indexList;
        }

        void BufferData(Vector3[] vertdata, Vector4[] coldata, Vector2[] texcoorddata, int[] indices, int indexBuffer)
        {
            DebugEx.Assert(coldata.Length == vertdata.Length);
            DebugEx.Assert(texcoorddata.Length == vertdata.Length);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _activeShader.GetBuffer("vPosition"));
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(_activeShader.GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            // Buffer vertex color if shader supports it
            if (_activeShader.GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _activeShader.GetBuffer("vColor"));
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector4.SizeInBytes), coldata, BufferUsageHint.StreamDraw);
                GL.VertexAttribPointer(_activeShader.GetAttribute("vColor"), 4, VertexAttribPointerType.Float, true, 0, 0);
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

        void RenderSetTransformMatrix(Matrix4 offset, Model model, Matrix4 viewMatrix)
        {
            Matrix4 modelMatrix = model.Transform.GetMatrix() * offset;
            UpdateCullFace(modelMatrix * viewMatrix);
            GL.UniformMatrix4(_activeShader.GetUniform("modelMatrix"), false, ref modelMatrix);
            GL.UniformMatrix4(_activeShader.GetUniform("viewMatrix"), false, ref viewMatrix);
        }
    }
}
