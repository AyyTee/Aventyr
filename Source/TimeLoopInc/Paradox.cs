using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Equ;
using Game.Common;

namespace TimeLoopInc
{
    public class Paradox : MemberwiseEquatable<Paradox>
    {
        public int Time { get; }
        public ImmutableHashSet<IGridEntity> Affected { get; }

        public Paradox(int time, ISet<IGridEntity> affected)
        {
            Time = time;
            Affected = affected.ToImmutableHashSet();
        }
    }
}
