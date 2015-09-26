using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class BodyUserData
    {
        Entity _linkedEntity;

        public Entity LinkedEntity
        {
            get { return _linkedEntity; }
        }

        public BodyUserData(Entity linkedEntity)
        {
            _linkedEntity = linkedEntity;
        }
    }
}
