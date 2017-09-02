using Game.Common;

namespace Game.Portals
{
    public interface IPortalCommon : ITreeNode<IPortalCommon>, IGetTransformVelocity, IGetWorldTransformVelocity
    {
        IScene Scene { get; }
        PortalPath Path { get; set; }
        /// <summary>
        /// The previously set world transform.
        /// </summary>
        new Transform2 WorldTransform { get; set; }
        /// <summary>
        /// The previously set world velocity.
        /// </summary>
        new Transform2 WorldVelocity { get; set; }
        /// <summary>
        /// Whether or not this instance can interact with portals.  
        /// If false, EnterPortal will never be called and collisions with portals will be ignored.
        /// </summary>
        bool IsPortalable { get; }
    }
}
