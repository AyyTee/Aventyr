using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Equ;
using Game.Common;
using System.Runtime.Serialization;

namespace TimeLoopInc.Editor
{
    [DataContract]
    public class PortalLink : IMemberwiseEquatable<PortalLink>
    {
        [DataMember]
        public ImmutableArray<PortalBuilder> Portals { get; }
        [DataMember]
        public int TimeOffset { get; }

        public PortalLink(IList<PortalBuilder> portals, int timeOffset = 0)
        {
            DebugEx.Assert(portals.Count <= 2);
            DebugEx.Assert(portals.All(item => item != null));
            DebugEx.Assert(portals.Count <= 1 || portals[0] != portals[1]);
            Portals = portals.ToImmutableArray();
            TimeOffset = timeOffset;
        }

        public bool Equals(PortalLink other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (TimeOffset == other.TimeOffset && Portals.Length == other.Portals.Length)
            {
                for (int i = 0; i < Portals.Length; i++)
                {
                    if (Portals[i] != other.Portals[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
