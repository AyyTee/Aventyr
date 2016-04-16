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
            Model background = ModelFactory.CreatePlane();
            background.Texture = Renderer.Textures["grid.png"];
            background.SetColor(new Vector3(1, 1, 0.5f));
            background.Transform.Position = new Vector3(0, 0, -5f);
            float size = 50;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUv.Size = size;
            Entity back = new Entity(scene, new Vector2(0f, 0f));
            back.Name = "Background";
            back.AddModel(background);
            #endregion

            HashSet<IDeepClone> toClone = new HashSet<IDeepClone>();
            ICamera2 camera = level.ActiveCamera;
            //toClone.Add(camera);
            foreach (EditorObject e in level.EditorObjects)
            {
                if (e is EditorEntity)
                {
                    EditorEntity editorEntity = (EditorEntity)e;
                    //toClone.Add(editorEntity.Entity);
                }
                else if (e is EditorPortal)
                {
                    EditorPortal editorPortal = (EditorPortal)e;
                }
                else if (e is EditorActor)
                {
                    EditorActor editorActor = (EditorActor)e;
                    Actor actor = ActorFactory.CreateEntityBox(new Entity(scene), Transform2.GetPosition(editorActor));
                    actor.SetTransform(editorActor.GetTransform());
                }
                else if (e is EditorWall)
                {
                    EditorWall editorWall = (EditorWall)e;
                    Actor actor = ActorFactory.CreateEntityPolygon(scene, editorWall.GetTransform(), editorWall.Vertices);
                    ((Entity)actor.Children[0]).ModelList[0].SetTexture(Renderer.Textures["default.png"]);
                    FixturePortal portal0 = new FixturePortal(scene, new FixtureEdgeCoord(actor.Body.FixtureList[0], 0, 0.2f));
                    //Transform2.SetSize(portal0, 2);
                    
                    FixturePortal portal1 = new FixturePortal(scene, new FixtureEdgeCoord(actor.Body.FixtureList[0], 0, 0.8f));
                    //portal0.SetLinked(portal1);
                }
            }

            Dictionary<IDeepClone, IDeepClone> dictionary = DeepClone.Clone(toClone);
            /*Cast all the cloned instances to SceneNode.  
            There should not be any types other than SceneNode or its derived types.*/
            List<SceneNode> cloned = dictionary.Values.Cast<SceneNode>().ToList();

            SceneNode.SetScene(cloned, scene);
            //scene.SetActiveCamera((Camera2)dictionary[camera]);
            return scene;
        }
    }
}
