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
            Dictionary<EditorObject, SceneNode> dictionary = new Dictionary<EditorObject, SceneNode>();

            List<EditorObject> editorObjects = level.EditorObjects;
            foreach (EditorObject e in editorObjects)
            {
                if (e is EditorPortal)
                {
                    EditorPortal cast = (EditorPortal)e;
                    if (cast.OnEdge)
                    {
                        dictionary.Add(cast, new FixturePortal(scene));
                    }
                    else
                    {
                        dictionary.Add(cast, new FloatPortal(scene));
                    }
                }
                else if (e is EditorEntity)
                {
                    EditorEntity cast = (EditorEntity)e;
                    dictionary.Add(cast, new Entity(scene));
                }
                else if (e is EditorWall)
                {
                    EditorWall cast = (EditorWall)e;
                    Body body = ActorFactory.CreatePolygon(scene.World, cast.GetTransform(), cast.Vertices);
                    dictionary.Add(cast, new Actor(scene, body));
                }
            }

            foreach (EditorObject e in editorObjects)
            {
                SceneNode parent = dictionary[e.Parent];
                SceneNode clone = dictionary[e];
                clone.SetParent(parent);
                if (clone is FixturePortal)
                {
                    FixturePortal cast = (FixturePortal)clone;
                    
                    //cast.SetFixtureParent(((EditorPortal)e).PolygonTransform);
                }
            }

            //Dictionary<IDeepClone, IDeepClone> dictionary = DeepClone.Clone(toClone);
            /*Cast all the cloned instances to SceneNode.  
            There should not be any types other than SceneNode or its derived types.*/
            List<SceneNode> cloned = dictionary.Values.Cast<SceneNode>().ToList();

            SceneNode.SetScene(cloned, scene);
            scene.SetActiveCamera(level.ActiveCamera);
            return scene;
        }
    }
}
