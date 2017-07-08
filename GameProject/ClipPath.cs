using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTK;

namespace Game
{
    public class ClipPath
    {
        public ImmutableList<Vector2> Path { get; }

        public ClipPath(IEnumerable<Vector2> path)
        {
            Path = path.ToImmutableList();
        }
    }
}
