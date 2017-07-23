using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Game.Common;

namespace TimeLoopInc.Editor
{
    public class PortalLink
    {
        public ImmutableArray<PortalBuilder> Portals;
        public int TimeOffset { get; }

        public PortalLink(IList<PortalBuilder> portals, int timeOffset = 0)
        {
            DebugEx.Assert(portals.Count <= 2);
            DebugEx.Assert(portals.All(item => item != null));
            Portals = portals.ToImmutableArray();
            TimeOffset = timeOffset;
        }
    }
}
