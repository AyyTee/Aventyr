using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xna = Microsoft.Xna.Framework;

namespace Game
{
    
    public class BodyUserData
    {
        public int EntityID;
        [XmlIgnore]
        public Entity LinkedEntity { get; private set; }
        public Xna.Vector2 PreviousPosition { get; set; }

        public BodyUserData()
        {
        }

        public BodyUserData(Entity linked)
        {
            LinkedEntity = linked;
            EntityID = linked.Id;
        }
    }
}
