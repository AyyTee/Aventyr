using FarseerPhysics.Dynamics;
using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public static class LevelExport
    {
        /// <summary>
        /// Creates a Scene from an EditorScene.  Scene is intended for standalone use.
        /// </summary>
        public static Scene Export(EditorScene level)
        {
            Scene scene = new Scene();

            #region create background
            Model background = Game.ModelFactory.CreatePlane();
            background.Texture = Renderer.Textures["grid.png"];
            background.SetColor(new Vector3(1, 1, 0.5f));
            background.Transform.Position = new Vector3(0, 0, -5f);
            float size = 50;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUv.Size = size;
            Entity back = new Entity(scene, new Vector2(0f, 0f));
            back.Name = "Background";
            back.AddModel(background);
            back.IsPortalable = false;
            #endregion

            HashSet<IDeepClone> toClone = new HashSet<IDeepClone>();
            //ICamera2 camera = level.ActiveCamera;
            //toClone.Add(level.ActiveCamera);
            foreach (EditorObject e in level.EditorObjects)
            {
                if (e is EditorEntity)
                {
                    EditorEntity editorEntity = (EditorEntity)e;
                    Entity entity = new Entity(scene);
                    entity.AddModelRange(editorEntity.Models);
                    entity.SetTransform(editorEntity.GetTransform());
                    //toClone.Add(editorEntity.Entity);
                }
                else if (e is EditorPortal)
                {
                    EditorPortal editorPortal = (EditorPortal)e;
                    /*if (editorPortal.Parent is EditorWall)
                    {

                    }*/
                }
                else if (e is EditorActor)
                {
                    EditorActor editorActor = (EditorActor)e;
                    Body body = ActorFactory.CreatePolygon(scene.World, editorActor.GetTransform(), editorActor.Vertices);
                    body.IsStatic = false;
                    Actor actor = new Actor(scene, body);
                    Entity entity = new Entity(scene);
                    Transform2.SetSize(entity, Transform2.GetSize(editorActor));
                    entity.AddModel(editorActor.GetActorModel());
                    entity.SetParent(actor);
                    /*Actor actor = ActorFactory.CreateEntityBox(new Entity(scene), Transform2.GetPosition(editorActor));
                    actor.SetTransform(editorActor.GetTransform());*/
                }
                else if (e is EditorWall)
                {
                    EditorWall editorWall = (EditorWall)e;
                    Actor actor = ActorFactory.CreateEntityPolygon(scene, editorWall.GetTransform(), editorWall.Vertices);
                    Entity entity = new Entity(scene);
                    //entity.AddModel(editorWall.GetWallModel());
                    //entity.ModelList[0].Wireframe = true;
                    entity.SetParent(actor);
                }
                /*List<FixturePortal> portals = new List<FixturePortal>();
                foreach (EditorPortal p in e.Children)
                {
                    FixtureEdgeCoord coord = p.GetFixtureEdgeCoord();
                    Fixture fixture = coord.Fixture;
                    int fixtureIndex = fixture.Body.FixtureList.FindIndex(item => item == fixture);
                    FixtureEdgeCoord clone = new FixtureEdgeCoord(actor.Body.FixtureList[fixtureIndex], coord.EdgeIndex, coord.EdgeT);
                    portals.Add(new FixturePortal(scene, clone));
                }
                if (portals.Count >= 2)
                {
                    portals[0].Linked = portals[1];
                    portals[1].Linked = portals[0];
                }*/
            }

            Dictionary<IDeepClone, IDeepClone> dictionary = DeepClone.Clone(toClone);
            /*Cast all the cloned instances to SceneNode.  
            There should not be any types other than SceneNode or its derived types.*/
            List<SceneNode> cloned = dictionary.Values.Cast<SceneNode>().ToList();

            SceneNode.SetScene(cloned, scene);
            scene.SetActiveCamera(level.ActiveCamera);
            return scene;
        }
    }
}
