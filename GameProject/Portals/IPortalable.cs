using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Portals
{
    /// <summary>
    /// An object that can travel through portals.
    /// </summary>
    public interface IPortalable : ITransformable2, IGetTransformVelocity, IPortalCommon
    {
        /// <summary>
        /// Whether or not this instance can interact with portals.  
        /// If false, EnterPortal will never be called and collisions with portals will be ignored.
        /// </summary>
        bool IsPortalable { get; }
        /// <summary>
        /// Is called when entering a portal.  
        /// EnterPortal(IPortal enter, Transform2 transform, Transform2 velocity)
        /// </summary>
        Action<EnterCallbackData, Transform2, Transform2> EnterPortal { get; set; }
        /// <summary>
        /// Get/Set transform without any side effects.
        /// </summary>
        Transform2 Transform { get; set; }
        /// <summary>
        /// Get/Set velocity without any side effects.
        /// </summary>
        Transform2 Velocity { get; set; }
        /// <summary>Replaces the local velocity with a copy of the passed argument.</summary>
        void SetVelocity(Transform2 velocity);
        List<IPortal> GetPortalChildren();
        
    }
}
