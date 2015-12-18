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

namespace Game
{
    public class Renderer
    {
        int sceneZDepth = 20;
        List<Scene> _scenes = new List<Scene>();
        Controller _controller;
        bool temp = true;

        public static Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();
        public static Dictionary<string, ShaderProgram> Shaders = new Dictionary<string, ShaderProgram>();
        
        public Renderer(Controller controller)
        {
            _controller = controller;
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
            
            float TimeRenderDelta = 0;
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            for (int i = 0; i < _scenes.Count(); i++)
            {
                Scene scene = _scenes[i];
                Camera2D camera = scene.ActiveCamera;
                PortalView portalView = CalculatePortalViews(scene, scene.PortalList.ToArray(), camera.GetViewMatrix(), camera.Viewpoint, 3);
                Entity entity = new Entity(scene);
                //GetPortalViewModels(portalView, entity);
                
                //DrawScene(scene, camera.GetViewMatrix(), 0, portalView);
                foreach (Entity v in scene.EntityList)
                {
                    RenderEntity(v, camera.GetViewMatrix(), 0, portalView);
                }

                TextWriter console = Console.Out;
                Console.SetOut(Controller.Log);
                //DrawPortalAll(scene, scene.PortalList.ToArray(), camera.GetViewMatrix(), camera.Viewpoint, 6, TimeRenderDelta);
                Console.SetOut(console);
                GL.Clear(ClearBufferMask.DepthBufferBit);
                scene.RemoveEntity(entity);
            }

            for (int i = 0; i < Renderer.Shaders.Count; i++)
            {
                shaderList[i].Value.DisableVertexAttribArrays();
            }

            GL.Flush();
        }

        public void GetPortalViewModels(PortalView portalView, Entity entity)
        {
            GetPortalViewModels(portalView, entity, 0, new Vector3(0,0,0));
        }

        public void GetPortalViewModels(PortalView portalView, Entity entity, float offset, Vector3 color)
        {
            foreach (PortalView p in portalView.Children)
            {
                Model polygon = ModelFactory.CreatePolygon(ClipperExt.ConvertToVector2(p.Path), new Vector3(0, 0, offset));
                polygon.SetColor(color);
                entity.Models.Add(polygon);
                //Random r = new Random((int)(color.LengthFast * 1000000));
                GetPortalViewModels(p, entity, offset + 1, color + new Vector3(.1f, .1f, .1f));//new Vector3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble()));
            }
        }

        public PortalView CalculatePortalViews(Scene scene, Portal[] portals, Matrix4 viewMatrix, Vector2 viewPos, int depth)
        {
            List<IntPoint> view = ClipperExt.ConvertToIntPoint(scene.ActiveCamera.GetWorldVerts());
            PortalView portalView = new PortalView(null, viewMatrix, view);
            CalculatePortalViews(scene, null, portals, viewMatrix, viewPos, depth, portalView, Matrix4.Identity);
            return portalView;
        }

        private void CalculatePortalViews(Scene scene, Portal portalEnter, Portal[] portals, Matrix4 viewMatrix, Vector2 viewPos, int depth, PortalView portalView, Matrix4 portalMatrix)
        {
            if (depth <= 0)
            {
                return;
            }
            foreach (Portal p in portals)
            {
                if (!p.IsValid() || (portalEnter != null && p == portalEnter.Linked))
                {
                    continue;
                }
                if (p.OneSided)
                {
                    Vector2[] pv2 = p.GetWorldVerts();

                    Line portalLine = new Line(pv2);
                    if (portalLine.GetSideOf(pv2[0] + p.GetWorldTransform().GetNormal()) != portalLine.GetSideOf(viewPos))
                    {
                        continue;
                    }
                }
                Matrix4 viewMatrixNew = Matrix4.CreateTranslation(new Vector3(0, 0, sceneZDepth)) * viewMatrix;
                Vector2[] fov = p.GetFov(viewPos, 500);
                fov = Vector2Ext.Transform(fov, portalMatrix);
                List<IntPoint> pathFov = ClipperExt.ConvertToIntPoint(fov);

                var viewNew = new List<List<IntPoint>>();
                Clipper c = new Clipper();
                c.AddPath(pathFov, PolyType.ptSubject, true);
                c.AddPath(portalView.Path, PolyType.ptClip, true);
                c.Execute(ClipType.ctIntersection, viewNew);
                
                Debug.Assert(viewNew.Count <= 1, "It isn't possible to get more than one path when intersecting two convex paths.");
                if (viewNew.Count > 0)
                {
                    Vector2 viewPosNew = Vector2Ext.Transform(viewPos, FixturePortal.GetPortalMatrix(p, p.Linked));
                    viewMatrixNew = FixturePortal.GetPortalMatrix(p.Linked, p) * viewMatrixNew;
                    PortalView portalViewNew = new PortalView(portalView, viewMatrixNew, viewNew[0]);
                    CalculatePortalViews(scene, p, portals, viewMatrixNew, viewPosNew, depth - 1, portalViewNew, FixturePortal.GetPortalMatrix(p.Linked, p) * portalMatrix);
                }
            }
        }

        /// <summary>Draw all of the portals within the scene.</summary>
        /// <param name="depth">Maximum number of recursions.</param>
        /// <param name="sceneMaxDepth">The difference between the nearest and farthest object in the scene.</param>
        /*public void DrawPortalAll(Scene scene, Portal[] portals, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta)
        {
            //stopgap solution. portals will only recursively draw themselves, not any other portals
            var portalSort = portals.OrderByDescending(item => new Line(item.GetWorldVerts()).PointDistance(viewPos, true));
            foreach (Portal p in portalSort)
            {
                GL.Clear(ClearBufferMask.StencilBufferBit | ClearBufferMask.DepthBufferBit);
                DrawPortal(scene, p, viewMatrix, viewPos, depth, timeDelta, 0);
            }
            GL.Disable(EnableCap.StencilTest);
        }

        public void DrawPortal(Scene scene, Portal portalEnter, Matrix4 viewMatrix, Matrix4 viewMatrixPrev, Vector2 viewPos, int depth, float timeDelta, int count)
        {
            Vector2[] pv = portalEnter.Linked.GetVerts();
            pv = Vector2Ext.Transform(pv, portalEnter.GetWorldTransform().GetMatrix() * viewMatrix);

            Vector2[] pv2 = portalEnter.GetVerts();
            pv2 = Vector2Ext.Transform(pv2, portalEnter.GetWorldTransform().GetMatrix() * viewMatrixPrev);
            Line portalLine = new Line(pv2);
            Vector2 v = Vector2Ext.Transform(viewPos, viewMatrix);
            if (portalLine.IsInsideFOV(v, new Line(pv)))
            {
                DrawPortal(scene, portalEnter, viewMatrix, viewPos, depth, timeDelta, count);
            }
        }

        public void DrawPortal(Scene scene, Portal portalEnter, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta, int count)
        {
            if (!portalEnter.IsValid())
            {
                return;
            }
            if (depth <= 0)
            {
                return;
            }

            if (portalEnter.OneSided)
            {
                Vector2[] pv2 = portalEnter.GetWorldVerts();

                Line portalLine = new Line(pv2);
                if (portalLine.GetSideOf(pv2[0] + portalEnter.GetWorldTransform().GetNormal()) != portalLine.GetSideOf(viewPos))
                {
                    return;
                }
            }

            Vector2[] pv = portalEnter.GetVerts();
            pv = Vector2Ext.Transform(pv, portalEnter.GetWorldTransform().GetMatrix() * viewMatrix);
            //this will not correctly cull portals if the viewPos begins outside of the viewspace
            if (MathExt.LineInRectangle(new Vector2(-1, -1), new Vector2(1, 1), pv[0], pv[1]) == false)
            {
                return;
            }

            viewMatrix = Matrix4.CreateTranslation(new Vector3(0, 0, sceneZDepth)) * viewMatrix;
            //Start using the stencil 
            GL.ColorMask(false, false, false, false);
            GL.DepthMask(false);
            GL.Enable(EnableCap.StencilTest);
            GL.Disable(EnableCap.DepthTest);
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilOp(StencilOp.Incr, StencilOp.Incr, StencilOp.Incr);

            Entity fov = new Entity(new Vector2());
            Vector2[] a = portalEnter.GetFov(viewPos, 50);
            if (a.Length >= 3)
            {
                fov.Models.Add(ModelFactory.CreatePolygon(a));
                RenderEntity(fov, viewMatrix, timeDelta, null);
            }

            GL.ColorMask(true, true, true, true);
            GL.DepthMask(true);
            GL.StencilFunc(StencilFunction.Less, count, 0xFF);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);


            GL.Enable(EnableCap.DepthTest);
            Matrix4 portalMatrix = FixturePortal.GetPortalMatrix(portalEnter.Linked, portalEnter) * viewMatrix;
            DrawScene(scene, portalMatrix, timeDelta);

            //GL.Disable(EnableCap.StencilTest);

            Entity fovOutline = new Entity(new Vector2());
            Vector2[] verts = portalEnter.GetFov(viewPos, 50, 2);
            if (verts.Length > 0)
            {
                fovOutline.Models.Add(ModelFactory.CreateLineStrip(new Vector2[] { verts[1], verts[2] }));
                fovOutline.Models.Add(ModelFactory.CreateLineStrip(new Vector2[] { verts[0], verts[3] }));
                foreach (Model model in fovOutline.Models)
                {
                    Vector3 v = model.Transform.Position;
                    v.Z = sceneZDepth * (depth + count);
                    model.Transform.Position = v;
                }
            }

            GL.LineWidth(2f);
            RenderEntity(fovOutline, viewMatrix, timeDelta, null);
            GL.LineWidth(1f);
            DrawPortal(scene, portalEnter, portalMatrix, viewMatrix, Vector2Ext.Transform(viewPos, FixturePortal.GetPortalMatrix(portalEnter, portalEnter.Linked)), depth - 1, timeDelta, count + 1);
        }*/

        /*public void DrawScene(Scene scene, Matrix4 viewMatrix, float timeRenderDelta, PortalView portalView)
        {
            foreach (Entity v in scene.EntityList)
            {
                RenderEntity(v, viewMatrix, (float)Math.Min(timeRenderDelta, 1 / Controller.DrawsPerSecond), portalView);
            }
        }*/

        private void UpdateCullFace(Matrix4 viewMatrix)
        {
            GL.Disable(EnableCap.CullFace);
            /*if (Matrix4Ext.IsMirrored(viewMatrix))
            {
                GL.CullFace(CullFaceMode.Front);
            }
            else
            {
                GL.CullFace(CullFaceMode.Back);
            }*/
        }

        public virtual void RenderEntity(Entity entity, Matrix4 viewMatrix, float timeDelta, PortalView portalView)
        {
            if (!entity.Visible)
            {
                return;
            }
            foreach (Model v in entity.Models)
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
                
                GL.UseProgram(v.Shader.ProgramID);

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
                //GL.BindBuffer(BufferTarget.ArrayBuffer, 0); I don't know why this is here so I've commented it out until something breaks.

                if (v.Shader.GetUniform("UVMatrix") != -1)
                {
                    Matrix4 UVMatrix = v.TransformUv.GetMatrix();
                    GL.UniformMatrix4(v.Shader.GetUniform("UVMatrix"), false, ref UVMatrix);
                }

                if (v.Texture != null)
                {
                    GL.Uniform1(v.Shader.GetUniform("isTextured"), 1);
                    GL.BindTexture(TextureTarget.Texture2D, v.Texture.Id);
                    //GL.Uniform1(v.Shader.GetAttribute("maintexture"), v.Texture.Id);
                }
                else
                {
                    GL.Uniform1(v.Shader.GetUniform("isTextured"), 0);
                    GL.BindTexture(TextureTarget.Texture2D, -1);
                    //GL.Uniform1(v.Shader.GetAttribute("maintexture"), -1);
                }
                
                // Buffer index data
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, v.IboElements);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StreamDraw);
                
                if (v.Wireframe)
                {
                    GL.Disable(EnableCap.CullFace);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                }
 
                RenderSetTransformMatrix(entity, v, viewMatrix);
                List<PortalView> portalViewList = portalView.GetPortalViewList();
                for (int i = 0; i < portalViewList.Count; i++)
                //for (int i = 0; i < 1; i++)
                {
                    if (v.Shader.GetUniform("cutLinesLength") != -1)
                    {
                        GL.Uniform1(v.Shader.GetUniform("cutLinesLength"), 0);
                    }
                    Matrix4 mat = portalViewList[i].ViewMatrix;
                    /*List<Line> clipLines = portalViewList[i].GetClipLines();
                    SetClipLines(v.Shader, clipLines, viewMatrix);*/

                    GL.UniformMatrix4(v.Shader.GetUniform("viewMatrix"), false, ref mat);
                    GL.DrawElements(BeginMode.Triangles, v.GetIndices().Length, DrawElementsType.UnsignedInt, indiceat * sizeof(int));
                }

                if (v.Wireframe)
                {
                    GL.Enable(EnableCap.CullFace);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
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
                RenderSetTransformMatrix(entity, cm.Model, cm.Transform * viewMatrix);
                Matrix4 view = cm.Transform * viewMatrix;
                GL.UniformMatrix4(cm.Model.Shader.GetUniform("viewMatrix"), false, ref view);
                if (temp)
                {
                    GL.DrawElements(BeginMode.Triangles, cm.Model.GetIndices().Length, DrawElementsType.UnsignedInt, 0);
                }
                else
                {
                    int indices = 0 * sizeof(int);
                    GL.DrawElementsInstanced(PrimitiveType.Triangles, cm.Model.GetIndices().Length, DrawElementsType.UnsignedInt, ref indices, 1);
                }
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

        private void RenderSetTransformMatrix(Entity entity, Model model, Matrix4 viewMatrix)
        {
            Matrix4 modelMatrix = model.Transform.GetMatrix() * entity.GetWorldTransform().GetMatrix();
            UpdateCullFace(modelMatrix * viewMatrix);
            GL.UniformMatrix4(model.Shader.GetUniform("modelMatrix"), false, ref modelMatrix);
        }

        public static Texture LoadImage(Bitmap image)
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
            catch (FileNotFoundException e)
            {
                return null;
            }
        }
    }
}
