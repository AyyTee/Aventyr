namespace Game.Portals
{
    /// <summary>
    /// Callback data often provided when entering a portal.
    /// </summary>
    public struct EnterCallbackData
    {
		/// <summary>
		/// Portal being entered (not exited).
		/// </summary>
        public IPortal EntrancePortal;
		/// <summary>
		/// Instance entering portal.
		/// </summary>
        public IPortalCommon Instance;
		/// <summary>
		/// Intersection t value for the portal.
		/// </summary>
        public double PortalT;

        public EnterCallbackData(IPortal entrancePortal, IPortalCommon instance, double portalT)
        {
            EntrancePortal = entrancePortal;
            Instance = instance;
            PortalT = portalT;
        }
    }
}
