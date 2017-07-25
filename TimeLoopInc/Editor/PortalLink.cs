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
    public class PortalLink
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

        public static bool operator ==(PortalLink left, PortalLink right) => Equals(left, right);
        public static bool operator !=(PortalLink left, PortalLink right) => !Equals(left, right);

        public override bool Equals(object obj)
        {
            // Equality method has been explicity defined because MemberwiseEquatable doesn't correctly handle ImmutableArray.
            if (obj is PortalLink link)
            {
                return Equals(this, link);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = base.GetHashCode() ^ TimeOffset;
            for (int i = 0; i < Portals.Length; i++)
            {
                hashCode ^= Portals[i].GetHashCode();
            }
            return hashCode;
        }

        public static bool Equals(PortalLink obj0, PortalLink obj1)
        {
            if (ReferenceEquals(obj0, obj1))
            {
                return true;
            }
            if (ReferenceEquals(obj0, null) || ReferenceEquals(obj1, null))
            {
                return false;
            }
            
            if (obj0.TimeOffset == obj1.TimeOffset && obj0.Portals.Length == obj1.Portals.Length)
            {
                for (int i = 0; i < obj0.Portals.Length; i++)
                {
                    if (obj0.Portals[i] != obj1.Portals[i])
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
