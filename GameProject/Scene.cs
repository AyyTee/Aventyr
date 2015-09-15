using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Game
{
    public class Scene
    {
        private List<Portal> _portals = new List<Portal>();

        public List<Portal> Portals
        {
            get { return _portals; }
        }
        private List<Entity> _entities = new List<Entity>();
        private Controller _window;

        public Controller Window
        {
            get { return _window; }
            set { _window = value; }
        }

        public List<Entity> EntityList
        {
            get { return _entities; }
        }

        public Scene(Controller controller)
        {
            _window = controller;   
        }

        public void DrawScene(Matrix4 viewMatrix, float timeRenderDelta)
        {
            foreach (Entity v in EntityList)
            {
                v.Render(this, viewMatrix, (float)Math.Min(timeRenderDelta, 1 / _window.UpdateFrequency));
            }
        }

        public void AddEntity(Entity entity)
        {
            _entities.Add(entity);
        }

        public void AddPortal(Portal portal)
        {
            _portals.Add(portal);
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
        public void DrawPortalAll(Portal[] portals, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta, float sceneDepth)
        {
            //stopgap solution. portals will only recursively draw themselves, not any other portals
            IOrderedEnumerable<Portal> portalSort = portals.OrderByDescending(item => (item.Transform.Position - viewPos).Length);
            foreach (Portal p in portalSort)
            {
                GL.Clear(ClearBufferMask.StencilBufferBit | ClearBufferMask.DepthBufferBit);
                DrawPortal(p, viewMatrix, viewPos, depth, timeDelta, 0, sceneDepth);
                //break;
            }
            GL.Disable(EnableCap.StencilTest);
        }

        public void DrawPortal(Portal portalEnter, Matrix4 viewMatrix, Matrix4 viewMatrixPrev, Vector2 viewPos, int depth, float timeDelta, int count, float sceneDepth)
        {
            Vector2[] pv = portalEnter.Linked.GetVerts();
            pv = VectorExt2.Transform(pv, portalEnter.Transform.GetMatrix() * viewMatrix);

            Vector2[] pv2 = portalEnter.GetVerts();
            pv2 = VectorExt2.Transform(pv2, portalEnter.Transform.GetMatrix() * viewMatrixPrev);
            Line portalLine = new Line(pv2);
            Vector2 v = VectorExt2.Transform(viewPos, viewMatrix);
            if (portalLine.IsInsideFOV(v, new Line(pv)))
            {
                DrawPortal(portalEnter, viewMatrix, viewPos, depth, timeDelta, count, sceneDepth);
            }
        }

        public void DrawPortal(Portal portalEnter, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta, int count, float sceneDepth)
        {
            if (depth <= 0)
            {
                return;
            }

            if (portalEnter.OneSided)
            {
                Vector2[] pv2 = portalEnter.GetWorldVerts();

                Line portalLine = new Line(pv2);
                if (portalLine.GetSideOf(pv2[0] + portalEnter.Transform.GetNormal()) != portalLine.GetSideOf(viewPos))
                {
                    return;
                }
            }

            Vector2[] pv = portalEnter.GetVerts();
            pv = VectorExt2.Transform(pv, portalEnter.Transform.GetMatrix() * viewMatrix);
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

            Entity fov = new Entity();
            Vector2[] a = portalEnter.GetFOV(viewPos, 50);
            if (a.Length >= 3)
            {
                fov.Models.Add(Model.CreatePolygon(a));
                fov.Render(this, viewMatrix, timeDelta);
            }
            

            GL.ColorMask(true, true, true, true);
            GL.DepthMask(true);
            GL.StencilFunc(StencilFunction.Less, count, 0xFF);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);


            GL.Enable(EnableCap.DepthTest);
            Matrix4 portalMatrix = Portal.GetMatrix(portalEnter.Linked, portalEnter) * viewMatrix;
            DrawScene(portalMatrix, timeDelta);

            //GL.Disable(EnableCap.StencilTest);

            Entity fovOutline = new Entity();
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

            GL.LineWidth(2f);
            fovOutline.Render(this, viewMatrix, timeDelta);
            GL.LineWidth(1f);

            DrawPortal(portalEnter, portalMatrix, viewMatrix, VectorExt2.Transform(viewPos, Portal.GetMatrix(portalEnter, portalEnter.Linked)), depth - 1, timeDelta, count + 1, sceneDepth);
        }
    }
}
