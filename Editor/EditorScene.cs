using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class EditorScene
    {
        public readonly Scene Scene;
        //public readonly EditorObject Root;
        public List<EditorObject> Children = new List<EditorObject>();

        public EditorScene(Scene scene)
        {
            Scene = scene;
            //Root = new EditorObject(this);
        }

        public List<T> FindByType<T>() where T : EditorObject
        {
            List<T> list = new List<T>();
            foreach (EditorObject e in Children)
            {
                list.AddRange(Tree<EditorObject>.FindByType<T>(e));
                //list.Remove(Root as T);
            }
            return list;
        }

        public void Step()
        {
            Scene.Step();
        }

        /*public SceneNode FindByName(string name)
        {
            return Root.FindByName(name);
        }*/
    }
}
