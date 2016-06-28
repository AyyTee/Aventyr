using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    /// <summary>
    /// An object that can travel through portals.
    /// </summary>
    public interface IPortalable : ITransformable2
    {
        /// <summary>
        /// Whether or not this instance can interact with portals.  
        /// If false, EnterPortal will never be called and collisions with portals will be ignored.
        /// </summary>
        bool IsPortalable { get; }
        /// <summary>Is called when entering a portal.  
        /// The first Transform2 is the previous Transform and the second Transform2 is the previous velocity.</summary>
        Action<IPortal, Transform2, Transform2> EnterPortal { get; set; }
        /// <summary>Returns a copy of the local velocity.</summary>
        Transform2 GetVelocity();
        /// <summary>Replaces the local velocity with a copy of the passed argument.</summary>
        void SetVelocity(Transform2 velocity);
    }
}
