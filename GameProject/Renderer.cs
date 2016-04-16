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
        public static int StencilBits { get; private set; }
        ShaderProgram _activeShader;
        Dictionary<EnableCap, bool?> _enableCap = new Dictionary<EnableCap, bool?>();

        public static Dictionary<string, TextureFile> Textures = new Dictionary<string, TextureFile>();
        public static Dictionary<string, ShaderProgram> Shaders = new Dictionary<string, ShaderProgram>();
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Represents the size of the cutLines array within the fragment shader
        /// </summary>
        const int PORTAL_CLIP_MAX = 8;
        
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
            Debug.Assert(StencilBits >= 8, "Stencil bith depth is too small.");
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

        public PortalView CalculatePortalViews(IPortal[] portals, ICamera2 camera, int depth)
        {
            Debug.Assert(camera != null);
            Debug.Assert(depth >= 0);
            Debug.Assert(portals != null);
            List<IntPoint> view = ClipperConvert.ToIntPoint(CameraExt.GetWorldVerts(camera));
            List<List<IntPoint>> paths = new List<List<IntPoint>>();
            paths.Add(view);
            PortalView portalView = new PortalView(null, camera.GetViewMatrix(), view, new Line[0], new Line[0]);
            Vector2 camPos = camera.GetWorldTransform().Position;
            CalculatePortalViews(null, portals, camera.GetViewMatrix(), camPos, camPos - camera.GetWorldVelocity().Position, depth, portalView, Matrix4.Identity);
            return portalView;
        }

        private void CalculatePortalViews(IPortal portalEnter, IPortal[] portals, Matrix4 viewMatrix, Vector2 viewPos, Vector2 viewPosPrevious, int depth, PortalView portalView, Matrix4 portalMatrix)
        {
            if (depth <= 0)
            {
                return;
            }
            const float AREA_EPSILON = 0.0001f;
            Clipper c = new Clipper();
            //The clipper must be set to strictly simple. Otherwise polygons might have duplicate vertices which causes poly2tri to generate incorrect results.
            c.StrictlySimple = true;
            foreach (IPortal p in portals)
            {
                if (!_isPortalValid(portalEnter, p, viewPos))
                {
                    continue;
                }

                Vector2[] fov = Vector2Ext.Transform(Portal.GetFov(p, viewPos, 500, 3), portalMatrix);
                if (MathExt.GetArea(fov) < AREA_EPSILON)
                {
                    continue;
                }
                List<IntPoint> pathFov = ClipperConvert.ToIntPoint(fov);
                
                var viewNew = new List<List<IntPoint>>();
                
                c.AddPath(pathFov, PolyType.ptSubject, true);
                c.AddPaths(portalView.Paths, PolyType.ptClip, true);
                c.Execute(ClipType.ctIntersection, viewNew);
                c.Clear();

                if (viewNew.Count <= 0)
                {
                    continue;
                }
                c.AddPaths(viewNew, PolyType.ptSubject, true);
                foreach (IPortal other in portals)
                {
                    if (other == p)
                    {
                        continue;
                    }
                    if (!_isPortalValid(portalEnter, other, viewPos))
                    {
                        continue;
                    }
                    //Skip this portal if it's inside the current portal's FOV.
                    Line portalLine = new Line(Portal.GetWorldVerts(p));
                    Line portalOtherLine = new Line(Portal.GetWorldVerts(other));
                    if (portalLine.IsInsideFOV(viewPos, portalOtherLine))
                    {
                        continue;
                    }
                    Vector2[] otherFov = Vector2Ext.Transform(Portal.GetFov(other, viewPos, 500, 3), portalMatrix);
                    if (MathExt.GetArea(otherFov) < AREA_EPSILON)
                    {
                        continue;
                    }
                    otherFov = MathExt.SetHandedness(otherFov, true);
                    List<IntPoint> otherPathFov = ClipperConvert.ToIntPoint(otherFov);
                    c.AddPath(otherPathFov, PolyType.ptClip, true);
                }
                var viewNewer = new List<List<IntPoint>>();
                c.Execute(ClipType.ctDifference, viewNewer, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
                c.Clear();
                if (viewNewer.Count <= 0)
                {
                    continue;
                }

                Vector2 viewPosNew = Vector2Ext.Transform(viewPos, Portal.GetPortalMatrix(p, p.Linked));
                Vector2 viewPosPreviousNew = Vector2Ext.Transform(viewPosPrevious, Portal.GetPortalMatrix(p, p.Linked));

                Matrix4 portalMatrixNew = Portal.GetPortalMatrix(p.Linked, p) * portalMatrix;
                Matrix4 viewMatrixNew = portalMatrixNew * viewMatrix;

                Line[] lines = Portal.GetFovLines(p, viewPos, 500);
                lines[0] = lines[0].Transform(portalMatrix);
                lines[1] = lines[1].Transform(portalMatrix);
                Line[] linesPrevious = Portal.GetFovLines(p, viewPosPrevious, 500);
                linesPrevious[0] = linesPrevious[0].Transform(portalMatrix);
                linesPrevious[1] = linesPrevious[1].Transform(portalMatrix);

                Line portalWorldLine = new Line(Portal.GetWorldVerts(p));
                portalWorldLine = portalWorldLine.Transform(portalMatrix);
                PortalView portalViewNew = new PortalView(portalView, viewMatrixNew, viewNewer, lines, linesPrevious, portalWorldLine);

                CalculatePortalViews(p, portals, viewMatrix, viewPosNew, viewPosPreviousNew, depth - 1, portalViewNew, portalMatrixNew);
            }
        }

        private bool _isPortalValid(IPortal previous, IPortal next, Vector2 viewPos)
        {
            //skip this portal if it isn't linked 
            if (!Portal.IsValid(next))
            {
                return false;
            }
            //or it's the exit portal
            if (previous != null && next == previous.Linked)
            {
                return false;
            }
            //or if the portal is one sided and the view point is on the wrong side
            Vector2[] pv2 = Portal.GetWorldVerts(next);
            Line portalLine = new Line(pv2);
            if (next.OneSided)
            {
                if (portalLine.GetSideOf(pv2[0] + next.GetWorldTransform().GetRight()) != portalLine.GetSideOf(viewPos))
                {
                    return false;
                }
            }
            //or if this portal isn't inside the fov of the exit portal
            if (previous != null)
            {
                Line portalEnterLine = new Line(Portal.GetWorldVerts(previous.Linked));
                if (!portalEnterLine.IsInsideFOV(viewPos, portalLine))
                {
                    return false;
                }
            }
            return true;
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
            PortalView portalView = CalculatePortalViews(layer.GetPortalList().ToArray(), cam, depth);
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
                    for (int j = 0; j < portalViewList[i].Paths.Count; j++)
                    {
                        Vector2[] a = ClipperConvert.ToVector2(portalViewList[i].Paths[j]);
                        RenderModel(ModelFactory.CreatePolygon(a), cam.GetViewMatrix(), Matrix4.Identity);
                    }
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
                            double distance = line.GetPerpendicularLeft().PointDistance(v, false);
                            double texCoordX = line.PointDistance(v, false) / minWidth;
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
                //foreach (ClipModel clip in e.GetClipModels(PortalClipDepth))
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
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(_activeShader.GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            // Buffer vertex color if shader supports it
            if (_activeShader.GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _activeShader.GetBuffer("vColor"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StreamDraw);
                GL.VertexAttribPointer(_activeShader.GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }

            // Buffer texture coordinates if shader supports it
            if (_activeShader.GetAttribute("texcoord") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _activeShader.GetBuffer("texcoord"));
                GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StreamDraw);
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

            //viewMatrix.M43 = 0.5f;
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
