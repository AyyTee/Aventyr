using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Portals
{
    public interface IPortalCommon : ITreeNode<IPortalCommon>, IGetTransformVelocity
    {
        PortalPath Path { get; set; }
        /// <summary>
        /// Computes the world transform.
        /// </summary>
        Transform2 GetWorldTransform(bool ignorePortals = false);
        /// <summary>
        /// Computes the velocity transform.
        /// </summary>
        Transform2 GetWorldVelocity(bool ignorePortals = false);
        /// <summary>
        /// The previously set world transform.
        /// </summary>
        Transform2 WorldTransformPrevious { get; set; }
        /// <summary>
        /// The previously set world velocity.
        /// </summary>
        Transform2 WorldVelocityPrevious { get; set; }
    }
}
