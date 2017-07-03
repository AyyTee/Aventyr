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

        public void Render(IRenderLayer layer, Vector2 topLeft, Vector2 size, float dpiScale, double t)
        {
            DebugEx.Assert(size.X > 0 && size.Y > 0);
            Vector2 gridSize = new Vector2(50, 20) * dpiScale;

            layer.DrawRectangle(topLeft, topLeft + size, new Color4(0.8f, 0.8f, 0.8f, 0.8f));

            if (Timeline == null)
            {
                return;
            }

            layer.DrawText(_font, topLeft, Timeline.Name);
            DrawTimelines(layer, topLeft + new Vector2(10, 60), size - new Vector2(20, 60), t);
        }

        void DrawTimelines(IRenderLayer layer, Vector2 topLeft, Vector2 size, double t)
        {
            float rowCount = 3;

            foreach (var box in GetTimelineBoxes(t))
            {
                Vector2 start = Vector2.Clamp(
                    new Vector2(
                        (float)MathEx.LerpInverse(MinTime, MaxTime, box.StartTime),
                        box.Row / rowCount),
                    new Vector2(),
                    Vector2.One);
                start *= size;

                Vector2 end = Vector2.Clamp(
                    new Vector2(
                        (float)MathEx.LerpInverse(MinTime, MaxTime, box.EndTime),
                        (box.Row + 1) / rowCount),
                    new Vector2(),
                    Vector2.One);
                end *= size;

                layer.DrawRectangle(topLeft + start, topLeft + end, new Color4(0.8f, 0.8f, 0f, 1f));
            }

            //_scene.CurrentInstant.Time - (1 - t);

            for (int i = (int)Math.Ceiling(MinTime - 0.01); i <= Math.Floor(MaxTime + 0.01); i++)
            {
                Vector2 pos = new Vector2((i - MinTime) / (MaxTime - MinTime), 0) * size;
                var top = (topLeft + pos).Round(Vector2.One);
                layer.DrawText(_font, top, i.ToString(), new Vector2(0.5f, 1));
                layer.DrawLine(new LineF(top, top + size.YOnly()));
            }
        }

        public List<TimelineBox> GetTimelineBoxes(double t)
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

                double endTime = entity.EndTime;
                if (i + 1 == count)
                {
                    endTime = _scene.CurrentInstant.Time - (1 - t);
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
            public double EndTime { get; }

            public TimelineBox(int row, int startTime, double endTime)
            {
                Row = row;
                StartTime = startTime;
                EndTime = endTime;
            }
        }
    }
}
