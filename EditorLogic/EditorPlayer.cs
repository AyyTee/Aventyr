using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using OpenTK;
using System.Runtime.Serialization;

namespace EditorLogic
{
    [DataContract]
    public class EditorPlayer : EditorObject
    {
        public EditorPlayer(EditorScene editorScene) 
            : base(editorScene)
        {
        }

        public override List<Model> GetModels()
        {
            List<Model> models = base.GetModels();
            models.Add(ModelFactory.CreatePlayer());
            return models;
        }
    }
}
