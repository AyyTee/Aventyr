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
        public readonly EditorObject Root;

        public EditorScene(Scene scene)
        {
            Scene = scene;
            Root = new EditorObject(this);
            Root.SetVisible(false);
        }

        public List<T> FindByType<T>() where T : EditorObject
        {
            List<T> list = Tree<EditorObject>.FindByType<T>(Root);
            list.Remove(Root as T);
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
