using System;
using Game.Common;

namespace Game.Portals
{
    /// <summary>
    /// Callback data often provided when entering a portal.
    /// </summary>
    public struct EnterCallbackData : IGetTransformVelocity
    {
		/// <summary>
		/// Portal being entered (not exited).
		/// </summary>
        public readonly IPortal EntrancePortal;
		/// <summary>
		/// Instance entering portal.
		/// </summary>
        public readonly IPortalCommon Instance;
		/// <summary>
		/// Intersection t value for the portal.
		/// </summary>
        public readonly double PortalT;

        readonly Transform2 _transform;
        readonly Transform2 _velocity;

        public EnterCallbackData(IPortal entrancePortal, IPortalCommon instance, Transform2 transform, Transform2 velocity, double portalT)
        {
            EntrancePortal = entrancePortal;
            Instance = instance;
            _transform = transform.ShallowClone();
            _velocity = velocity.ShallowClone();
            PortalT = portalT;
        }

        public Transform2 GetTransform() => _transform.ShallowClone();
        public Transform2 GetVelocity() => _velocity.ShallowClone();
    }
}
