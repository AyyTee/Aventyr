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
        /// <summary>Is called when entering a portal.  
        /// The first Transform2 is the previous Transform and the second Transform2 is the previous velocity.</summary>
        Action<IPortal, Transform2, Transform2> enterPortal { get; set; }
        /// <summary>Returns a copy of the local velocity.</summary>
        Transform2 GetVelocity();
        /// <summary>Replaces the local velocity with a copy of the passed argument.</summary>
        void SetVelocity(Transform2 velocity);
    }
}
