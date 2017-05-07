using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Models;
using Game.Rendering;
using System.Runtime.Serialization;

namespace EditorLogic
{
    [DataContract]
    public class Doodad : Renderable, ISceneObject
    {
        [DataMember]
        public string Name { get; set; } = "Doodad";

        public Doodad(string name)
        {
            Name = name;
        }

        public void Remove()
        {
        }
    }
}
