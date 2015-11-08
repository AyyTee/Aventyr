using FarseerPhysics.Dynamics;
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

        public List<Body> BodyChildren = new List<Body>();

        public struct ChildBody
        {
            public Body Body;
            public Portal Portal;
            public ChildBody(Body body, Portal portal)
            {
                Body = body;
                Portal = portal;
            }
        }

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
