using System;
using Game.Common;

namespace TimeLoopInc.Editor
{
    public class PortalLink
    {
        public PortalBuilder Portal0 { get; }
        public PortalBuilder Portal1 { get; }
        public int TimeOffset { get; }

        public PortalLink(PortalBuilder portal0, PortalBuilder portal1 = null, int timeOffset = 0)
        {
            DebugEx.Assert(portal0 != null);
            DebugEx.Assert(portal0 != portal1);

            Portal0 = portal0;
            Portal1 = portal1;
            TimeOffset = timeOffset;
        }
    }
}
