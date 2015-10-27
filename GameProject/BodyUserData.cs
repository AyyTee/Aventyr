using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Game
{
    
    public class BodyUserData
    {
        public int EntityID;
        [XmlIgnore]
        public Entity LinkedEntity { get; private set; }

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
