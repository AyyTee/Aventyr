using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Portals
{
    public interface IPortalCommon : ITreeNode<IPortalCommon>, IGetTransformVelocity
    {
        IScene Scene { get; }
        PortalPath Path { get; set; }
        /// <summary>
        /// The previously set world transform.
        /// </summary>
        Transform2 WorldTransform { get; set; }
        /// <summary>
        /// The previously set world velocity.
        /// </summary>
        Transform2 WorldVelocity { get; set; }
    }
}
