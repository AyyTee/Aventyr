using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.IO;
using System.Drawing;

namespace Game
{
    public class Renderer
    {
        private int sceneDepth = 20;
        public List<Scene> RenderScenes = new List<Scene>();
        private Controller _controller;
        public Renderer(Controller controller)
        {
            _controller = controller;
        }

        public static void Init()
        {
            GL.ClearColor(Color.HotPink);
            GL.ClearStencil(0);
            GL.PointSize(5f);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public void Render()
        {
            GL.Viewport(0, 0, _controller.Width, _controller.Height);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            Controller.Shaders["textured"].EnableVertexAttribArrays();
            Controller.Shaders["default"].EnableVertexAttribArrays();
            float TimeRenderDelta = 0;
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            for (int i = 0; i < RenderScenes.Count(); i++)
            {
                Scene scene = RenderScenes[i];
                Camera camera = scene.ActiveCamera;
                DrawScene(scene, camera.GetViewMatrix(), 0);

                TextWriter console = Console.Out;
                Console.SetOut(Controller.Log);
                DrawPortalAll(scene, scene.PortalList.ToArray(), camera.GetViewMatrix(), camera.Viewpoint, 6, TimeRenderDelta);
                Console.SetOut(console);
                GL.Clear(ClearBufferMask.DepthBufferBit);
            }
            
            Controller.Shaders["textured"].DisableVertexAttribArrays();
            Controller.Shaders["default"].DisableVertexAttribArrays();

            GL.Flush();
            _controller.SwapBuffers();
        }

        /// <summary>
        /// Draw all of the portals within the scene.
        /// </summary>
        /// <param name="portals"></param>
        /// <param name="viewMatrix"></param>
        /// <param name="viewPos"></param>
        /// <param name="depth"></param>
        /// <param name="timeDelta"></param>
        /// <param name="sceneMaxDepth">The difference between the nearest and farthest object in the scene</param>
        public void DrawPortalAll(Scene scene, Portal[] portals, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta)
        {
            //stopgap solution. portals will only recursively draw themselves, not any other portals
            IOrderedEnumerable<Portal> portalSort = portals.OrderByDescending(item => (item.Transform.WorldPosition - viewPos).Length);
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
            pv = VectorExt2.Transform(pv, portalEnter.Transform.GetWorldMatrix() * viewMatrix);

            Vector2[] pv2 = portalEnter.GetVerts();
            pv2 = VectorExt2.Transform(pv2, portalEnter.Transform.GetWorldMatrix() * viewMatrixPrev);
            Line portalLine = new Line(pv2);
            Vector2 v = VectorExt2.Transform(viewPos, viewMatrix);
            if (portalLine.IsInsideFOV(v, new Line(pv)))
            {
                DrawPortal(scene, portalEnter, viewMatrix, viewPos, depth, timeDelta, count);
            }
        }

        public void DrawPortal(Scene scene, Portal portalEnter, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta, int count)
        {
            if (depth <= 0)
            {
                return;
            }

            if (portalEnter.OneSided)
            {
                Vector2[] pv2 = portalEnter.GetWorldVerts();

                Line portalLine = new Line(pv2);
                if (portalLine.GetSideOf(pv2[0] + portalEnter.Transform.GetWorldNormal()) != portalLine.GetSideOf(viewPos))
                {
                    return;
                }
            }

            Vector2[] pv = portalEnter.GetVerts();
            pv = VectorExt2.Transform(pv, portalEnter.Transform.GetWorldMatrix() * viewMatrix);
            //this will not correctly cull portals if the viewPos begins outside of the viewspace
            if (MathExt.LineInRectangle(new Vector2(-1, -1), new Vector2(1, 1), pv[0], pv[1]) == false)
            {
                return;
            }

            viewMatrix = Matrix4.CreateTranslation(new Vector3(0, 0, sceneDepth)) * viewMatrix;

            //Start using the stencil 
            GL.ColorMask(false, false, false, false);
            GL.DepthMask(false);
            GL.Enable(EnableCap.StencilTest);
            GL.Disable(EnableCap.DepthTest);
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilOp(StencilOp.Incr, StencilOp.Incr, StencilOp.Incr);

            Entity fov = new Entity(scene);
            Vector2[] a = portalEnter.GetFOV(viewPos, 50);
            if (a.Length >= 3)
            {
                fov.Models.Add(Model.CreatePolygon(a));
                fov.Render(viewMatrix, timeDelta);
            }
            fov.RemoveFromScene();

            GL.ColorMask(true, true, true, true);
            GL.DepthMask(true);
            GL.StencilFunc(StencilFunction.Less, count, 0xFF);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);


            GL.Enable(EnableCap.DepthTest);
            Matrix4 portalMatrix = Portal.GetPortalMatrix(portalEnter.Linked, portalEnter) * viewMatrix;
            DrawScene(scene, portalMatrix, timeDelta);

            //GL.Disable(EnableCap.StencilTest);

            Entity fovOutline = new Entity(scene);
            Vector2[] verts = portalEnter.GetFOV(viewPos, 50, 2);
            if (verts.Length > 0)
            {
                fovOutline.Models.Add(Model.CreateLine(new Vector2[] { verts[1], verts[2] }));
                fovOutline.Models.Add(Model.CreateLine(new Vector2[] { verts[0], verts[3] }));
                foreach (Model model in fovOutline.Models)
                {
                    Vector3 v = model.Transform.Position;
                    v.Z = sceneDepth * (depth + count);
                    model.Transform.Position = v;
                }
            }
            fovOutline.RemoveFromScene();

            GL.LineWidth(2f);
            fovOutline.Render(viewMatrix, timeDelta);
            GL.LineWidth(1f);

            DrawPortal(scene, portalEnter, portalMatrix, viewMatrix, VectorExt2.Transform(viewPos, Portal.GetPortalMatrix(portalEnter, portalEnter.Linked)), depth - 1, timeDelta, count + 1);
        }

        public void DrawScene(Scene scene, Matrix4 viewMatrix, float timeRenderDelta)
        {
            foreach (Entity v in scene.EntityList)
            {
                v.Render(viewMatrix, (float)Math.Min(timeRenderDelta, 1 / Controller.DrawsPerSecond));
            }
        }
    }
}
