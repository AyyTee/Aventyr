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
    public class EditorScene : IRenderLayer
    {
        [DataMember]
        public List<EditorObject> _children = new List<EditorObject>();
        [DataMember]
        public ICamera2 ActiveCamera { get; set; }
        public List<EditorObject> EditorObjects { get { return new List<EditorObject>(_children); } }

        public EditorScene()
        {
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

        public void Clear()
        {
            foreach (EditorObject e in EditorObjects)
            {
                e.Remove();
            }
        }

        public List<IRenderable> GetRenderList()
        {
            
            return FindAll().OfType<IRenderable>().ToList();
        }

        public List<IPortal> GetPortalList()
        {
            return FindAll().OfType<IPortal>().ToList();
        }

        public ICamera2 GetCamera()
        {
            return ActiveCamera;
        }
    }
}
