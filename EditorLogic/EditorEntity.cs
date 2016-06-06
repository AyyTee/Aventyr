using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EditorLogic
{
    [DataContract, AffineMember]
    public sealed class EditorEntity : EditorObject
    {
        [DataMember]
        public List<Model> Models = new List<Model>();

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

        private void ShallowClone(EditorEntity destination)
        {
            base.ShallowClone(destination);
            destination.Models = new List<Model>(Models);
            /*for (int i = 0; i < _models.Count; i++)
            {
                destination._models.Add(_models[i].DeepClone());
            }*/
        }

        public void AddModel(Model model)
        {
            Models.Add(model);
        }

        public void RemoveModel(Model model)
        {
            Models.Remove(model);
        }

        public void RemoveAllModels()
        {
            Models.Clear();
        }

        public override List<Model> GetModels()
        {
            List<Model> models = new List<Model>(Models);
            models.AddRange(base.GetModels());
            return models;
        }
    }
}
