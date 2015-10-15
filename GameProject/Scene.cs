using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using FarseerPhysics.Dynamics;
using Xna = Microsoft.Xna.Framework;
using System.Diagnostics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using Poly2Tri;
using System.Xml.Serialization;
using System.IO;

namespace Game
{
    public class Scene
    {
        /*public ResourceMap<Portal> portalMap = new ResourceMap<Portal>();
        public ResourceMap<Entity> entityMap = new ResourceMap<Entity>();*/
        public World PhysWorld;
        float TimeStepSize = 1 / 60f;
        private float sceneDepth = 20;
        private Camera _activeCamera;

        public Camera ActiveCamera
        {
            get { return _activeCamera; }
            set { _activeCamera = value; }
        }
        private List<Portal> _portalList = new List<Portal>();
        public List<Portal> PortalList
        {
            get { return _portalList; }
        }
        private List<Entity> _entityList = new List<Entity>();

        public List<Entity> EntityList
        {
            get { return _entityList; }
        }

        public Scene()
        {
            PhysWorld = new World(new Xna.Vector2(0f, -1));
            
        }

        public void DrawScene(Matrix4 viewMatrix, float timeRenderDelta)
        {
            foreach (Entity v in EntityList)
            {
                v.Render(viewMatrix, (float)Math.Min(timeRenderDelta, 1 / Controller.DrawsPerSecond));
            }
        }

        public void Step()
        {
            PhysWorld.Step(TimeStepSize);
            foreach (Entity e in EntityList)
            {
                e.Step();
            }
        }

        private void AddEntity(Entity entity)
        {
            Debug.Assert(!EntityList.Exists(item => item.Equals(entity)), "This entity has already been added to this scene.");
            EntityList.Add(entity);
        }

        public Entity CreateEntity()
        {
            return CreateEntity(new Vector2(0, 0));
        }

        public Entity CreateEntity(Vector2 position)
        {
            Entity entity = new Entity(this, position);
            //AddEntity(entity);
            Debug.Assert(!EntityList.Exists(item => item.Equals(entity)), "This entity has already been added to this scene.");
            EntityList.Add(entity);
            return entity;
        }

        public Entity CreateEntityBox(Vector2 position, Vector2 scale)
        {
            Entity box = CreateEntity();
            box.Transform.Position = position;
            box.Models.Add(Model.CreatePlane(scale));

            //Body body = BodyFactory.CreateBody(scene.PhysWorld, VectorExt2.ConvertToXna(box.Transform.Position));
            Body body = BodyFactory.CreateRectangle(box.Scene.PhysWorld, scale.X, scale.Y, 1);
            body.Position = VectorExt2.ConvertToXna(position);
            box.LinkBody(body);
            body.BodyType = BodyType.Dynamic;

            //body.CreateFixture(new CircleShape(1f, 1f));*/

            return box;
        }

        public Entity CreateEntityPolygon(Vector2 position, Vector2 scale, Vector2[] vertices)
        {
            Entity entity = CreateEntity();
            entity.Transform.Position = position;

            Polygon polygon = PolygonFactory.CreatePolygon(vertices);

            entity.Models.Add(Model.CreatePolygon(polygon));

            Xna.Vector2 vPos = VectorExt2.ConvertToXna(entity.Transform.Position);

            List<FarseerPhysics.Common.Vertices> vList = new List<FarseerPhysics.Common.Vertices>();

            Body body = new Body(this.PhysWorld);
            body.Position = VectorExt2.ConvertToXna(position);
            for (int i = 0; i < polygon.Triangles.Count; i++)
            {
                var v1 = new FarseerPhysics.Common.Vertices();

                for (int j = 0; j < polygon.Triangles[i].Points.Count(); j++)
                {
                    v1.Add(VectorExt2.ConvertToXna(polygon.Triangles[i].Points[j]));
                }

                vList.Add(v1);
                PolygonShape shape = new PolygonShape(v1, 1);
                Fixture fixture = body.CreateFixture(shape);
                FixtureUserData userData = new FixtureUserData(fixture);
                for (int j = 0; j < polygon.Triangles[i].Neighbors.Count(); j++)
                {
                    //userData.EdgeIsExterior[j] = polygon.Triangles[i].NeighborAcrossFrom(polygon.Triangles[i].Points[j]) == null;
                    //polygon.Triangles[i].

                    userData.EdgeIsExterior[j] = polygon.Triangles[i].EdgeIsConstrained[(j + 2) % 3];
                }
            }

            entity.LinkBody(body);

            return entity;
        }

        /// <summary>
        /// Remove entity and the entity's linked physics body from the scene.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        /// <returns>If the entity existed within the scene.</returns>
        public bool RemoveEntity(Entity entity)
        {
            if (EntityList.Remove(entity))
            {
                if (entity.Body != null)
                {
                    PhysWorld.RemoveBody(entity.Body);
                }
                return true;
            }
            return false;
        }

        public Portal CreatePortal()
        {
            return CreatePortal(new Vector2(0, 0));
        }

        public Portal CreatePortal(Vector2 position)
        {
            Portal portal = new Portal(this, position);
            //AddPortal(portal);
            Debug.Assert(!PortalList.Exists(item => item.Equals(portal)), "This portal has already been added to this scene.");
            PortalList.Add(portal);
            return portal;
        }

        public void RemovePortal(Portal portal)
        {
            PortalList.Remove(portal);
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
        public void DrawPortalAll(Portal[] portals, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta)
        {
            //stopgap solution. portals will only recursively draw themselves, not any other portals
            IOrderedEnumerable<Portal> portalSort = portals.OrderByDescending(item => (item.Transform.WorldPosition - viewPos).Length);
            foreach (Portal p in portalSort)
            {
                GL.Clear(ClearBufferMask.StencilBufferBit | ClearBufferMask.DepthBufferBit);
                DrawPortal(p, viewMatrix, viewPos, depth, timeDelta, 0);
            }
            GL.Disable(EnableCap.StencilTest);
        }

        public void DrawPortal(Portal portalEnter, Matrix4 viewMatrix, Matrix4 viewMatrixPrev, Vector2 viewPos, int depth, float timeDelta, int count)
        {
            Vector2[] pv = portalEnter.Linked.GetVerts();
            pv = VectorExt2.Transform(pv, portalEnter.Transform.GetWorldMatrix() * viewMatrix);

            Vector2[] pv2 = portalEnter.GetVerts();
            pv2 = VectorExt2.Transform(pv2, portalEnter.Transform.GetWorldMatrix() * viewMatrixPrev);
            Line portalLine = new Line(pv2);
            Vector2 v = VectorExt2.Transform(viewPos, viewMatrix);
            if (portalLine.IsInsideFOV(v, new Line(pv)))
            {
                DrawPortal(portalEnter, viewMatrix, viewPos, depth, timeDelta, count);
            }
        }

        public void DrawPortal(Portal portalEnter, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta, int count)
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

            Entity fov = new Entity(this);
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
            DrawScene(portalMatrix, timeDelta);

            //GL.Disable(EnableCap.StencilTest);

            Entity fovOutline = new Entity(this);
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

            DrawPortal(portalEnter, portalMatrix, viewMatrix, VectorExt2.Transform(viewPos, Portal.GetPortalMatrix(portalEnter, portalEnter.Linked)), depth - 1, timeDelta, count + 1);
        }

        public void Save()
        {
            string path = "filepath";
            FileStream outFile = File.Create(path);
            var a = new FarseerPhysics.Common.WorldXmlSerializer();
            a.Serialize(PhysWorld, outFile);
            /*string path = "filepath";
            FileStream outFile = File.Create(path);*/
            /*XmlSerializer formatter = new XmlSerializer(GetType());
            formatter.Serialize(outFile, this);*/
        }
    }
}
