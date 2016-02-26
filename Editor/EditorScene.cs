using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    [DataContract]
    public class EditorScene
    {
        [DataMember]
        public Scene Scene;
        [DataMember]
        public List<EditorObject> _children = new List<EditorObject>();
        public List<EditorObject> Children { get { return new List<EditorObject>(_children); } }

        public EditorScene(Scene scene)
        {
            Scene = scene;
        }

        public List<EditorObject> FindAll()
        {
            return FindByType<EditorObject>();
        }

        public List<T> FindByType<T>() where T : EditorObject
        {
            List<T> list = new List<T>();
            foreach (EditorObject e in _children)
            {
                list.AddRange(Tree<EditorObject>.FindByType<T>(e));
            }
            return list;
        }

        public void Step()
        {
            Scene.Step();
        }

        public void Clear()
        {
            foreach (EditorObject e in Children)
            {
                e.Remove();
            }
        }
    }
}
