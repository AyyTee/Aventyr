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

namespace Game
{
    public class Renderer
    {
        int sceneZDepth = 20;
        List<Scene> _scenes = new List<Scene>();
        Controller _controller;

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
            GL.PointSize(5f);
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

            Renderer.Shaders["textured"].EnableVertexAttribArrays();
            Renderer.Shaders["default"].EnableVertexAttribArrays();
            float TimeRenderDelta = 0;
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            for (int i = 0; i < _scenes.Count(); i++)
            {
                Scene scene = _scenes[i];
                Camera camera = scene.ActiveCamera;
                DrawScene(scene, camera.GetViewMatrix(), 0);

                TextWriter console = Console.Out;
                Console.SetOut(Controller.Log);
                DrawPortalAll(scene, scene.PortalList.ToArray(), camera.GetViewMatrix(), camera.Viewpoint, 6, TimeRenderDelta);
                Console.SetOut(console);
                GL.Clear(ClearBufferMask.DepthBufferBit);
            }
            
            Renderer.Shaders["textured"].DisableVertexAttribArrays();
            Renderer.Shaders["default"].DisableVertexAttribArrays();

            GL.Flush();
        }

        /// <summary>
        /// Draw all of the portals within the scene.
        /// </summary>
        /// <param name="portals"></param>
        /// <param name="viewMatrix"></param>
        /// <param name="viewPos"></param>
        /// <param name="depth">Maximum number of recursions.</param>
        /// <param name="timeDelta"></param>
        /// <param name="sceneMaxDepth">The difference between the nearest and farthest object in the scene.</param>
        public void DrawPortalAll(Scene scene, Portal[] portals, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta)
        {
            //stopgap solution. portals will only recursively draw themselves, not any other portals
            var portalSort = portals.OrderByDescending(item => new Line(item.GetWorldVerts()).PointDistance(viewPos, true));//(item.Transform.WorldPosition - viewPos).Length);
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
            pv = Vector2Ext.Transform(pv, portalEnter.GetTransform().GetWorldMatrix() * viewMatrix);

            Vector2[] pv2 = portalEnter.GetVerts();
            pv2 = Vector2Ext.Transform(pv2, portalEnter.GetTransform().GetWorldMatrix() * viewMatrixPrev);
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
                if (portalLine.GetSideOf(pv2[0] + portalEnter.GetTransform().GetWorldNormal()) != portalLine.GetSideOf(viewPos))
                {
                    return;
                }
            }

            Vector2[] pv = portalEnter.GetVerts();
            pv = Vector2Ext.Transform(pv, portalEnter.GetTransform().GetWorldMatrix() * viewMatrix);
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
            Vector2[] a = portalEnter.GetFOV(viewPos, 50);
            if (a.Length >= 3)
            {
                fov.Models.Add(ModelFactory.CreatePolygon(a));
                RenderEntity(fov, viewMatrix, timeDelta);
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
            Vector2[] verts = portalEnter.GetFOV(viewPos, 50, 2);
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
            RenderEntity(fovOutline, viewMatrix, timeDelta);
            GL.LineWidth(1f);
            DrawPortal(scene, portalEnter, portalMatrix, viewMatrix, Vector2Ext.Transform(viewPos, FixturePortal.GetPortalMatrix(portalEnter, portalEnter.Linked)), depth - 1, timeDelta, count + 1);
        }

        public void DrawScene(Scene scene, Matrix4 viewMatrix, float timeRenderDelta)
        {
            foreach (Entity v in scene.EntityList)
            {
                RenderEntity(v, viewMatrix, (float)Math.Min(timeRenderDelta, 1 / Controller.DrawsPerSecond));
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

        public virtual void RenderEntity(Entity entity, Matrix4 viewMatrix, float timeDelta)
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
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, v.IboElements);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StreamDraw);

                Matrix4 UVMatrix = v.TransformUv.GetMatrix();
                GL.UniformMatrix4(v.Shader.GetUniform("UVMatrix"), false, ref UVMatrix);
                if (v.Texture != null)
                {
                    GL.BindTexture(TextureTarget.Texture2D, v.Texture.Id);
                    if (v.Shader.GetAttribute("maintexture") != -1)
                    {
                        GL.Uniform1(v.Shader.GetAttribute("maintexture"), v.Texture.Id);
                    }
                }
                else
                {
                    GL.BindTexture(TextureTarget.Texture2D, -1);
                    if (v.Shader.GetAttribute("maintexture") != -1)
                    {
                        GL.Uniform1(v.Shader.GetAttribute("maintexture"), -1);
                    }
                }
                
                if (v.Wireframe)
                {
                    GL.Disable(EnableCap.CullFace);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                }

                if (entity.IsPortalable)
                {
                    entity.UpdatePortalClipping(4);
                    RenderClipModels(entity, viewMatrix);
                }
                else
                {
                    GL.Uniform1(v.Shader.GetUniform("cutLinesLength"), 0);
                    RenderSetTransformMatrix(entity, v, viewMatrix);
                    GL.DrawElements(BeginMode.Triangles, v.GetIndices().Length, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                }

                if (v.Wireframe)
                {
                    GL.Enable(EnableCap.CullFace);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
            }
        }

        /*public void RenderClipModels(Entity entity, Matrix4 viewMatrix)
        {
            _RenderClipModels(entity, viewMatrix);
        }*/

        private void RenderClipModels(Entity entity, Matrix4 viewMatrix)
        {
            List<Entity.ClipModel> clipModels = entity.ClipModels;
            Matrix4 ScaleMatrix;
            ScaleMatrix = viewMatrix * Matrix4.CreateTranslation(new Vector3(1, 1, 0)) * Matrix4.CreateScale(new Vector3(Controller.CanvasSize.Width / (float)2, Controller.CanvasSize.Height / (float)2, 0));

            /*Vector2[] mirrorTest = new Vector2[3] {
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(0, 0)
            };
            bool isMirrored;
            mirrorTest = Vector2Ext.Transform(mirrorTest, viewMatrix);
            isMirrored = MathExt.AngleDiff(MathExt.AngleVector(mirrorTest[0] - mirrorTest[2]), MathExt.AngleVector(mirrorTest[1] - mirrorTest[2])) > 0;
            */
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
                GL.DrawElements(BeginMode.Triangles, cm.Model.GetIndices().Length, DrawElementsType.UnsignedInt, 0);
            }
        }

        private void RenderSetTransformMatrix(Entity entity, Model model, Matrix4 viewMatrix)
        {
            Matrix4 modelMatrix = model.Transform.GetMatrix() * entity.Transform.GetWorldMatrix() * viewMatrix;
            UpdateCullFace(modelMatrix);
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
