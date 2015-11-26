using Game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;

namespace Editor
{
    public class EntityProperty
    {
        Entity _entity;

        [TypeConverter(typeof(PropertyGridConverter)), BrowsableAttribute(true)]
        public Transform2DProperty Transform { get { return new Transform2DProperty(_entity.Transform); } }

        [TypeConverter(typeof(PropertyGridConverter)), BrowsableAttribute(true)]
        public Transform2DProperty Velocity { get { return new Transform2DProperty(_entity.Velocity); } }

        [BrowsableAttribute(true)]
        public bool IsPortalable { get { return _entity.IsPortalable; } set { _entity.IsPortalable = value; } }

        [BrowsableAttribute(true)]
        public string Name { get { return _entity.Name; } set { _entity.Name = value; } }

        public EntityProperty(Entity entity)
        {
            _entity = entity;
        }
    }
}
