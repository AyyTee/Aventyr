using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    [DataContract]
    public class EditorEntity : EditorObject
    {
        [DataMember]
        public List<Model> _models = new List<Model>();

        public EditorEntity(EditorScene editorScene)
            : base(editorScene)
        {
        }

        public override IDeepClone ShallowClone()
        {
            EditorEntity clone = new EditorEntity(Scene);
            ShallowClone(clone);
            return clone;
        }

        protected void ShallowClone(EditorEntity destination)
        {
            base.ShallowClone(destination);
        }

        public void AddModel(Model model)
        {
            _models.Add(model);
        }

        public void RemoveModel(Model model)
        {
            _models.Remove(model);
        }

        public void RemoveAllModels()
        {
            _models.Clear();
        }

        public override List<Model> GetModels()
        {
            List<Model> models = new List<Model>(_models);
            models.AddRange(base.GetModels());
            return models;
        }
    }
}
