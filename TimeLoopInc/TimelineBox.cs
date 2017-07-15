using System;
using Equ;
using Game.Common;

namespace TimeLoopInc
{
    public class TimelineBox : MemberwiseEquatable<TimelineBox>
    {
        public int Row { get; }
        public int StartTime { get; }
        public double EndTime { get; }
        public bool FadeStart { get; }
        public bool FadeEnd { get; }
        public IGridEntity Entity { get; }

        public TimelineBox(
            int row,
            int startTime,
            double endTime,
            bool fadeStart,
            bool fadeEnd,
            IGridEntity entity)
        {
            DebugEx.Assert(startTime <= endTime);
            Row = row;
            StartTime = startTime;
            EndTime = endTime;
            FadeStart = fadeStart;
            FadeEnd = fadeEnd;
            Entity = entity;
        }
    }
}
