using Game;
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
            
            HashSet<IDeepClone> toClone = new HashSet<IDeepClone>();
            Camera2 camera = level.Scene.ActiveCamera;
            toClone.Add(camera);
            foreach (EditorObject e in level.Children)
            {
                if (e is EditorEntity)
                {
                    EditorEntity editorEntity = (EditorEntity)e;
                    toClone.Add(editorEntity.Entity);
                }
                else if (e is EditorPortal)
                {
                    EditorPortal editorPortal = (EditorPortal)e;
                    toClone.Add(editorPortal.PortalEntity);
                    toClone.Add(editorPortal.Portal);
                }
                else if (e is EditorActor)
                {
                    EditorActor editorActor = (EditorActor)e;
                    ActorFactory.CreateEntityBox(new Entity(scene), Transform2.GetPosition(editorActor));
                }
            }

            Dictionary<IDeepClone, IDeepClone> dictionary = DeepClone.Clone(toClone);
            /*Cast all the cloned instances to SceneNode.  
            There should not be any types other than SceneNode and any derived types.*/
            List<SceneNode> cloned = dictionary.Values.Cast<SceneNode>().ToList();

            SceneNode.SetScene(cloned, scene);
            scene.SetActiveCamera((Camera2)dictionary[camera]);
            return scene;
        }
    }
}
