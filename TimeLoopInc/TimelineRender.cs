using Game.Common;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class TimelineRender
    {
        public ITimeline Timeline { get; set; }
        readonly Scene _scene;
        readonly Font _font;
        public float MinTime { get; private set; } = 0;
        public float MaxTime { get; private set; } = 10;

        public TimelineRender(Scene scene, Font font)
        {
            _scene = scene;
            _font = font;
        }

        public void Render(IRenderLayer layer, Vector2 topLeft, Vector2 size, float dpiScale)
        {
            Debug.Assert(size.X > 0 && size.Y > 0);
            Vector2 gridSize = new Vector2(50, 20) * dpiScale;

            layer.DrawRectangle(topLeft, topLeft + size, new Color4(0.8f, 0.8f, 0.8f, 0.8f));

            if (Timeline == null)
            {
                return;
            }

            float rowCount = 3;

            foreach (var box in GetTimelineBoxes())
            {
                Vector2 start = new Vector2((box.StartTime - MinTime) / (MaxTime - MinTime), box.Row / rowCount) * size;
                Vector2 end = new Vector2((box.EndTime - MinTime) / (MaxTime - MinTime), (box.Row + 1) / rowCount) * size;
                layer.DrawRectangle(topLeft + start, topLeft + end, new Color4(0.8f, 0.8f, 0f, 1f));
            }

            for (int i = (int)((MinTime - 0.001) + 1); i <= (int)(MaxTime + 0.001); i++)
            {
                Vector2 pos = new Vector2((i - MinTime) / (MaxTime - MinTime), 0) * size;
                layer.DrawText(_font, topLeft + pos, i.ToString(), new Vector2(0.5f, 1));
            }
        }

        public List<TimelineBox> GetTimelineBoxes()
        {
            var output = new List<TimelineBox>();

            int row = 0;
            var count = Timeline.Path.Count;
            for (int i = 0; i < count; i++)
            {
                var entity = Timeline.Path[i];

                var startTime = entity.StartTime;
                if (i > 0 && entity.StartTime < Timeline.Path[i - 1].EndTime)
                {
                    row++;
                }

                var endTime = entity.EndTime;
                if (i + 1 == count)
                {
                    endTime = _scene.CurrentInstant.Time;
                }

                output.Add(new TimelineBox(row, entity.StartTime, endTime));
            }

            return output;
        }

        public void Update(double timeDelta)
        {
            var targetMaxTime = (float)MathEx.Ceiling(Math.Max(Timeline.MaxTime(), _scene.CurrentInstant.Time), 5);
            var targetMinTime = (float)MathEx.Floor(Timeline.MinTime(), 5);

            if (Math.Abs(targetMinTime - MinTime) > 0.0001)
            {
                MinTime = (MinTime - targetMinTime) * (float)Math.Pow(0.01, timeDelta) + targetMinTime;
            }
            else
            {
                MinTime = targetMinTime;
            }
            if (Math.Abs(targetMaxTime - MaxTime) > 0.0001)
            {
                MaxTime = (MaxTime - targetMaxTime) * (float)Math.Pow(0.01, timeDelta) + targetMaxTime;
            }
            else
            {
                MaxTime = targetMaxTime;
            }
            if (MaxTime - MinTime < 10)
            {
                MaxTime = MinTime + 10;
            }
        }

        public class TimelineBox
        {
            public int Row { get; }
            public int StartTime { get; }
            public int EndTime { get; }

            public TimelineBox(int row, int startTime, int endTime)
            {
                Row = row;
                StartTime = startTime;
                EndTime = endTime;
            }
        }
    }
}
