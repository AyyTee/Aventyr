using System.Runtime.Serialization;
using FarseerPhysics.Dynamics;
using Game.Common;

namespace Game.Physics
{
    /// <summary>
    /// Stores the relavent state of a FarseerPhysics body to be used for serialization.
    /// </summary>
    [DataContract]
    public class BodyMemento
    {
        [DataMember]
        public Transform2 Transform;
        [DataMember]
        public Transform2 Velocity;

        public BodyMemento()
        {
        }

        public BodyMemento(Body body)
        {
            Transform = BodyExt.GetTransform(body);
            Velocity = BodyExt.GetVelocity(body);
        }
    }
}
