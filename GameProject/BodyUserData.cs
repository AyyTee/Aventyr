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
        private Entity _linkedEntity;
        public ResourceID<Entity> EntityID;
        [XmlIgnore]
        public Entity LinkedEntity
        {
            get 
            {
                return _linkedEntity;//Entity.IDMap[EntityID];
            }
            set
            {
                _linkedEntity = value;
            }
        }

        public BodyUserData()
        {
        }

        public BodyUserData(Entity linkedEntity)
        {
            _linkedEntity = linkedEntity;
            //EntityID = linkedEntity.ID;
        }
    }
}
