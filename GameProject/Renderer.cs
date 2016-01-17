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
        List<Scene> _scenes = new List<Scene>();
        readonly Controller _controller;
        public bool PortalRenderEnabled { get; set; }
        public int PortalRenderDepth { get; set; }

        public static Dictionary<string, TextureFile> Textures = new Dictionary<string, TextureFile>();
        public static Dictionary<string, ShaderProgram> Shaders = new Dictionary<string, ShaderProgram>();
        
        public Renderer(Controller controller)
        {
            _controller = controller;
            PortalRenderEnabled = true;
            PortalRenderDepth = 3;
        }

        public static void Init()
        {
            GL.ClearColor(Color.HotPink);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.CullFace);
            GL.ClearStencil(0);
            GL.PointSize(15f);
            //GL.LineWidth(2f);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.ScissorTest);
        }

        public void AddScene(Scene scene)
        {
            _scenes.Add(scene);
        }

        /// <summary>Remove a Scene from the Renderer.</summary>
        /// <returns>True if the Scene was in the Renderer, otherwise false.</returns>
        public bool RemoveScene(Scene scene)
        {
            return _scenes.Remove(scene);
        }

        public void Render()
        {
            GL.Viewport(0, 0, Controller.CanvasSize.Width, Controller.CanvasSize.Height);
            GL.Scissor(0, 0, Controller.CanvasSize.Width, Controller.CanvasSize.Height);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.ColorBufferBit);

            var shaderList = Renderer.Shaders.ToList();
            for (int i = 0; i < Renderer.Shaders.Count; i++)
            {
                shaderList[i].Value.EnableVertexAttribArrays();
            }
            
            GL.Enable(EnableCap.DepthTest);
            //GL.Enable(EnableCap.Blend);
            for (int i = 0; i < _scenes.Count(); i++)
            {
                Scene scene = _scenes[i];
                Camera2D camera = scene.ActiveCamera;
                DrawPortalAll(scene);
                GL.Clear(ClearBufferMask.DepthBufferBit);
            }

            for (int i = 0; i < Renderer.Shaders.Count; i++)
            {
                shaderList[i].Value.DisableVertexAttribArrays();
            }

            GL.Flush();
        }
        #region temp
        /*public void GetPortalViewModels(PortalView portalView, Entity entity)
        {
            GetPortalViewModels(portalView, entity, 0, new Vector3(0,0,0));
        }

        public void GetPortalViewModels(PortalView portalView, Entity entity, float offset, Vector3 color)
        {
            foreach (PortalView p in portalView.Children)
            {
                Model polygon = ModelFactory.CreatePolygon(p.Path, new Vector3(0, 0, offset));
                polygon.SetColor(color);
                entity.Models.Add(polygon);
                //Random r = new Random((int)(color.LengthFast * 1000000));
                GetPortalViewModels(p, entity, offset + 1, color + new Vector3(.1f, .1f, .1f));//new Vector3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble()));
            }
        }*/
        #endregion
        public PortalView CalculatePortalViews(Portal[] portals, Camera2D camera, int depth)
        {
            List<IntPoint> view = ClipperConvert.ToIntPoint(camera.GetWorldVerts());
            List<List<IntPoint>> paths = new List<List<IntPoint>>();
            paths.Add(view);
            PortalView portalView = new PortalView(null, camera.GetViewMatrix(), view, new Line[0], new Line[0]);
            CalculatePortalViews(null, portals, camera.GetViewMatrix(), camera.GetWorldViewpos(), camera.GetWorldViewpos() - camera.GetWorldVelocity().Position, depth, portalView, Matrix4.Identity);
            return portalView;
        }

        private void CalculatePortalViews(Portal portalEnter, Portal[] portals, Matrix4 viewMatrix, Vector2 viewPos, Vector2 viewPosPrevious, int depth, PortalView portalView, Matrix4 portalMatrix)
        {
            if (depth <= 0)
            {
                return;
            }

            Clipper c = new Clipper();
            //The clipper must be set to strictly simple. Otherwise polygons might have duplicate vertices which causes poly2tri to generate incorrect results.
            c.StrictlySimple = true;
            foreach (Portal p in portals)
            {
                //skip this portal if it isn't linked 
                if (!p.IsValid())
                {
                    continue;
                }
                //or it's the exit portal
                if (portalEnter != null && p == portalEnter.Linked)
                {
                    continue;
                }
                //or if the portal is one sided and the view point is on the wrong side
                Vector2[] pv2 = p.GetWorldVerts();
                Line portalLine = new Line(pv2);
                if (p.OneSided)
                {
                    if (portalLine.GetSideOf(pv2[0] + p.GetWorldTransform().GetNormal()) != portalLine.GetSideOf(viewPos))
                    {
                        continue;
                    }
                }
                //or if this portal isn't inside the fov of the exit portal
                if (portalEnter != null)
                {
                    Line portalEnterLine = new Line(portalEnter.Linked.GetWorldVerts());
                    if (!portalEnterLine.IsInsideFOV(viewPos, portalLine))
                    {
                        continue;
                    }
                }
                
                Vector2[] fov = Vector2Ext.Transform(p.GetFov(viewPos, 500, 3), portalMatrix);
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
                foreach (Portal other in portals)
                {
                    if (other == p || !other.IsValid())
                    {
                        continue;
                    }
                    if (portalEnter != null && other == portalEnter.Linked)
                    {
                        continue;
                    }
                    Vector2[] pOther = other.GetWorldVerts();
                    Line portalOtherLine = new Line(pOther);
                    if (other.OneSided)
                    {
                        if (portalOtherLine.GetSideOf(pOther[0] + other.GetWorldTransform().GetNormal()) != portalOtherLine.GetSideOf(viewPos))
                        {
                            continue;
                        }
                    }
                    if (portalEnter != null)
                    {
                        Line portalEnterLine = new Line(portalEnter.Linked.GetWorldVerts());
                        if (!portalEnterLine.IsInsideFOV(viewPos, portalOtherLine))
                        {
                            continue;
                        }
                    }
                    //Skip this portal if it's inside the current portal's FOV.
                    if (portalLine.IsInsideFOV(viewPos, portalOtherLine))
                    {
                        continue;
                    }
                    Vector2[] otherFov = Vector2Ext.Transform(other.GetFov(viewPos, 500, 3), portalMatrix);
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

                Vector2 viewPosNew = Vector2Ext.Transform(viewPos, FixturePortal.GetPortalMatrix(p, p.Linked));
                Vector2 viewPosPreviousNew = Vector2Ext.Transform(viewPosPrevious, FixturePortal.GetPortalMatrix(p, p.Linked));

                Matrix4 portalMatrixNew = FixturePortal.GetPortalMatrix(p.Linked, p) * portalMatrix;
                Matrix4 viewMatrixNew = portalMatrixNew * viewMatrix;

                Line[] lines = p.GetFovLines(viewPos, 500);
                lines[0].Transform(portalMatrix);
                lines[1].Transform(portalMatrix);
                Line[] linesPrevious = p.GetFovLines(viewPosPrevious, 500);
                linesPrevious[0].Transform(portalMatrix);
                linesPrevious[1].Transform(portalMatrix);

                Line portalWorldLine = new Line(p.GetWorldVerts());
                portalWorldLine.Transform(portalMatrix);
                PortalView portalViewNew = new PortalView(portalView, viewMatrixNew, viewNewer, lines, linesPrevious, portalWorldLine);

                CalculatePortalViews(p, portals, viewMatrix, viewPosNew, viewPosPreviousNew, depth - 1, portalViewNew, portalMatrixNew);
            }
        }

        public void DrawPortalAll(Scene scene)
        {
            Camera2D cam = scene.ActiveCamera;
            int depth = 0;
            if (PortalRenderEnabled)
            {
                depth = PortalRenderDepth;
            }
            Vector2 viewPos = cam.GetWorldViewpos();
            Vector2 viewPosPrevious = cam.GetWorldViewpos() - cam.GetWorldVelocity().Position;
            PortalView portalView = CalculatePortalViews(scene.PortalList.ToArray(), cam, depth);
            List<PortalView> portalViewList = portalView.GetPortalViewList(cam.Viewpos);
            
            int stencilValueMax = 1 << GL.GetInteger(GetPName.StencilBits);
            int stencilMask = stencilValueMax - 1;
            GL.ColorMask(false, false, false, false);
            GL.DepthMask(false);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.StencilTest);
            
            GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
            //Draw portal FOVs to the stencil buffer.
            for (int i = 1; i < Math.Min(portalViewList.Count, stencilValueMax); i++)
            {
                GL.StencilFunc(StencilFunction.Always, i, stencilMask);
                for (int j = 0; j < portalViewList[i].Paths.Count; j++)
                {
                    Vector2[] a = ClipperConvert.ToVector2(portalViewList[i].Paths[j]);
                    RenderModel(ModelFactory.CreatePolygon(a), cam.GetViewMatrix(), 0, Matrix4.Identity);
                }
            }

            GL.ColorMask(true, true, true, true);
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
            for (int i = 0; i < Math.Min(portalViewList.Count, stencilValueMax); i++)
            {
                GL.StencilFunc(StencilFunction.Equal, i, stencilMask);
                DrawScene(scene, portalViewList[i].ViewMatrix, 0, i > 0);
            }

            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.StencilTest);
            GL.Disable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.StencilBufferBit);

            Clipper c = new Clipper();
            c.StrictlySimple = true;
            Model portalEdges = new Model();
            //portalEdges.SetTexture(Textures["default.png"]);
            for (int i = 1; i < Math.Min(portalViewList.Count, stencilValueMax); i++)
            {
                //ModelFactory.AddLinesWidth(portalEdges, new Line[] { portalViewList[i].PortalLine }, 0.2f);
                for (int j = 0; j < 2; j++)
                {
                    Vector2[] line0 = GetPortalEdge(portalViewList[i].FovLines[j], portalViewList[i].FovLinesPrevious[j], cam);
                    c.AddPath(ClipperConvert.ToIntPoint(line0), PolyType.ptSubject, true);
                    c.AddPaths(portalViewList[i].Parent.Paths, PolyType.ptClip, true);
                    List<List<IntPoint>> solution = new List<List<IntPoint>>();
                    c.Execute(ClipType.ctIntersection, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
                    c.Clear();

                    c.AddPaths(solution, PolyType.ptSubject, true);
                    foreach (PortalView p in portalViewList[i].Parent.Children)
                    {
                        if (p == portalViewList[i])
                        {
                            continue;
                        }
                        if (p.PortalLine.IsInsideFOV(cam.GetWorldViewpos(), portalViewList[i].PortalLine))
                        {
                            c.AddPaths(p.Paths, PolyType.ptClip, true);
                        }
                    }
                    PolyTree solutionNew = new PolyTree();
                    c.Execute(ClipType.ctDifference, solutionNew, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
                    c.Clear();
                    ModelFactory.AddPolygon(portalEdges, solutionNew);
                }
            }
            portalEdges.SetColor(new Vector3(0.1f, 0.1f, 0.1f));
            RenderModel(portalEdges, cam.GetViewMatrix(), 0, Matrix4.Identity);

            GL.Enable(EnableCap.DepthTest);
        }

        private Vector2[] GetPortalEdge(Line line, Line linePrevious, Camera2D camera)
        {
            const double maxAngle = 0.9f * Math.PI / 4;
            float angleScale = 0.5f;
            float minWidth = camera.Scale / 400;
            double angleDiff0 = Math.Abs(MathExt.AngleDiff(line.Angle(), linePrevious.Angle()) * angleScale);
            float widthEnd0 = (float)(Math.Tan(Math.Min(angleDiff0, maxAngle)) * line.Length + minWidth);
            return PolygonFactory.CreateLineWidth(line, minWidth, widthEnd0);
        }

        public void DrawScene(Scene scene, Matrix4 viewMatrix, float timeRenderDelta, bool isPortalRender)
        {
            foreach (Entity v in scene.EntityList)
            {
                RenderEntity(v, viewMatrix, (float)Math.Min(timeRenderDelta, 1 / Controller.DrawsPerSecond), isPortalRender);
            }
        }

        private void UpdateCullFace(Matrix4 viewMatrix)
        {
            if (Matrix4Ext.IsMirrored(viewMatrix))
            {
                GL.CullFace(CullFaceMode.Front);
            }
            else
            {
                GL.CullFace(CullFaceMode.Back);
            }
        }

        public virtual void RenderEntity(Entity entity, Matrix4 viewMatrix, float timeDelta, bool isPortalRender)
        {
            if (!entity.Visible)
            {
                return;
            }
            if (entity.DrawOverPortals)
            {
                if (isPortalRender)
                {
                    return;
                }
                GL.Disable(EnableCap.StencilTest);
            }
            
            foreach (Model v in entity.ModelList)
            {
                RenderModel(v, viewMatrix, timeDelta, entity.GetWorldTransform().GetMatrix());
            }
            if (entity.DrawOverPortals)
            {
                GL.Enable(EnableCap.StencilTest);
            }
        }

        public void RenderModel(Model model, Matrix4 viewMatrix, float timeDelta, Matrix4 offset)
        {
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
            vertcount += model.Vertices.Count;

            Vector3[] vertdata;
            Vector3[] coldata;
            Vector2[] texcoorddata;
            int[] indicedata;

            vertdata = verts.ToArray();
            indicedata = inds.ToArray();
            coldata = colors.ToArray();
            texcoorddata = texcoords.ToArray();

            Debug.Assert(indicedata.Length % 3 == 0, "Model must have a multiple of 3 vertex indices.");
            foreach (int a in indicedata)
            {
                Debug.Assert(a >= 0 && a < vertdata.Length);
            }
            Debug.Assert(coldata.Length == vertdata.Length);
            Debug.Assert(texcoorddata.Length == vertdata.Length);
            GL.BindBuffer(BufferTarget.ArrayBuffer, model.Shader.GetBuffer("vPosition"));
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(model.Shader.GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.UseProgram(model.Shader.ProgramID);

            // Buffer vertex color if shader supports it
            if (model.Shader.GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, model.Shader.GetBuffer("vColor"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StreamDraw);
                GL.VertexAttribPointer(model.Shader.GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }

            // Buffer texture coordinates if shader supports it
            if (model.Shader.GetAttribute("texcoord") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, model.Shader.GetBuffer("texcoord"));
                GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StreamDraw);
                GL.VertexAttribPointer(model.Shader.GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
            }
            //GL.BindBuffer(BufferTarget.ArrayBuffer, 0); I don't know why this is here so I've commented it out until something breaks.

            if (model.Shader.GetUniform("UVMatrix") != -1)
            {
                Matrix4 UVMatrix = model.TransformUv.GetMatrix();
                GL.UniformMatrix4(model.Shader.GetUniform("UVMatrix"), false, ref UVMatrix);
            }

            if (model.Texture != null)
            {
                GL.Uniform1(model.Shader.GetUniform("isTextured"), 1);
                GL.BindTexture(TextureTarget.Texture2D, model.Texture.GetId());
                //GL.Uniform1(v.Shader.GetAttribute("maintexture"), v.Texture.Id);
            }
            else
            {
                GL.Uniform1(model.Shader.GetUniform("isTextured"), 0);
                GL.BindTexture(TextureTarget.Texture2D, -1);
                //GL.Uniform1(v.Shader.GetAttribute("maintexture"), -1);
            }

            // Buffer index data
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, model.GetIbo());
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StreamDraw);

            if (model.Wireframe)
            {
                GL.Disable(EnableCap.CullFace);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            if (model.IsTransparent)
            {
                GL.Enable(EnableCap.Blend);
            }

            RenderSetTransformMatrix(offset, model, viewMatrix);
            /*List<PortalView> portalViewList = portalView.GetPortalViewList();
            for (int i = 0; i < portalViewList.Count; i++)*/
            //for (int i = 0; i < 1; i++)
            {
                if (model.Shader.GetUniform("cutLinesLength") != -1)
                {
                    GL.Uniform1(model.Shader.GetUniform("cutLinesLength"), 0);
                }
                Matrix4 mat = viewMatrix;//portalViewList[i].ViewMatrix;
                /*List<Line> clipLines = portalViewList[i].GetClipLines();
                SetClipLines(v.Shader, clipLines, viewMatrix);*/

                GL.UniformMatrix4(model.Shader.GetUniform("viewMatrix"), false, ref mat);
                GL.DrawElements(BeginMode.Triangles, model.GetIndices().Length, DrawElementsType.UnsignedInt, 0);
            }

            if (model.Wireframe)
            {
                GL.Enable(EnableCap.CullFace);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
            if (model.IsTransparent)
            {
                GL.Disable(EnableCap.Blend);
            }
        }

        private void RenderClipModels(Entity entity, Matrix4 viewMatrix)
        {
            List<Entity.ClipModel> clipModels = entity.ClipModels;
            Matrix4 ScaleMatrix;
            ScaleMatrix = viewMatrix * Matrix4.CreateTranslation(new Vector3(1, 1, 0)) * Matrix4.CreateScale(new Vector3(Controller.CanvasSize.Width / (float)2, Controller.CanvasSize.Height / (float)2, 0));

            bool isMirrored = Matrix4Ext.IsMirrored(viewMatrix);
            foreach (Entity.ClipModel cm in clipModels)
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
                        l[0].X,
                        l[0].Y,
                        l[1].X,
                        l[1].Y
                    });
                }

                GL.Uniform1(cm.Model.Shader.GetUniform("cutLinesLength"), cutLines.Count);
                GL.Uniform1(GL.GetUniformLocation(cm.Model.Shader.ProgramID, "cutLines[0]"), cutLines.Count, cutLines.ToArray());
                RenderSetTransformMatrix(entity.GetWorldTransform().GetMatrix(), cm.Model, cm.Transform * viewMatrix);
                Matrix4 view = cm.Transform * viewMatrix;
                GL.UniformMatrix4(cm.Model.Shader.GetUniform("viewMatrix"), false, ref view);

                GL.DrawElements(BeginMode.Triangles, cm.Model.GetIndices().Length, DrawElementsType.UnsignedInt, 0);
            }
        }

        private void SetClipLines(ShaderProgram shader, List<Line> clipLines, Matrix4 viewMatrix)
        {
            Matrix4 ScaleMatrix;
            ScaleMatrix = viewMatrix * Matrix4.CreateTranslation(new Vector3(1f, 1f, 0)) * Matrix4.CreateScale(new Vector3(Controller.CanvasSize.Width / (float)2, Controller.CanvasSize.Height / (float)2, 0));
            bool isMirrored = Matrix4Ext.IsMirrored(viewMatrix);
            List<float> cutLines = new List<float>();
            foreach (Line l in clipLines)
            {
                if (isMirrored)
                {
                    l.Reverse();
                }
                l.Transform(ScaleMatrix);
                cutLines.AddRange(new float[4] { l[0].X, l[0].Y, l[1].X, l[1].Y });
            }

            Vector2 vMax, vMin;
            MathExt.GetBBox(clipLines.ToArray(), out vMin, out vMax);
            GL.Scissor((int)vMin.X, (int)vMin.Y, (int)vMax.X + 1, (int)vMax.Y + 1);

            GL.Uniform1(shader.GetUniform("cutLinesLength"), cutLines.Count);
            GL.Uniform1(GL.GetUniformLocation(shader.ProgramID, "cutLines[0]"), cutLines.Count, cutLines.ToArray());
        }

        private void RenderSetTransformMatrix(Matrix4 offset, Model model, Matrix4 viewMatrix)
        {
            Matrix4 modelMatrix = model.Transform.GetMatrix() * offset;
            UpdateCullFace(modelMatrix * viewMatrix);
            GL.UniformMatrix4(model.Shader.GetUniform("modelMatrix"), false, ref modelMatrix);
        }

        /*public static Texture LoadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            Texture texture = new Texture(texID);
            return texture;
        }

        public static Texture LoadImage(string filename)
        {
            try
            {
                Bitmap file = new Bitmap(filename);
                Texture texture = LoadImage(file);
                texture.SetFilepath(filename);
                return texture;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }*/
    }
}
