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
        //private Entity _linkedEntity;
        /// <summary>
        /// This will cause XMLSerializer to store the Guid of the Entity.  This is neccesary to preserve references to the Entity.
        /// </summary>
        public ResourceID<Entity> EntityID;
        public Entity LinkedEntity
        {
            get 
            { 
                return Entity.IDMap[EntityID];
            }
        }

        private BodyUserData()
        {
        }

        public BodyUserData(Entity linkedEntity)
        {
            //_linkedEntity = linkedEntity;
            EntityID = linkedEntity.ID;
        }
    }
}
