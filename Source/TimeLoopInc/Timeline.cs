using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Game.Common;
using System.Collections.Immutable;

namespace TimeLoopInc
{
    public class Timeline
    {
        public ImmutableList<IGridEntity> Path { get; private set; } = new List<IGridEntity>().ToImmutableList();

        public string Name => "Timeline";

        public bool IsClosed { get; }

        public void Add(IGridEntity entity)
        {
            DebugEx.Assert(Path.Count == 0 || entity.GetType() == Path.First().GetType());
            Path = Path.Add(entity);    
        }

        public Timeline()
        {
        }

        public Timeline(IList<IGridEntity> path, bool isClosed)
        {
            Path = path.Cast<IGridEntity>().ToImmutableList();
            IsClosed = isClosed;
        }
    }
}
