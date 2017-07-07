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
using ClipperLib;
using Game;
using Game.Models;

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
            var currentTime = _scene.CurrentInstant.Time - (1 - t);

            foreach (var box in GetTimelineBoxes(currentTime))
            {
                var xValues = new[] { box.StartTime - 0.25, box.StartTime, box.EndTime, box.EndTime + 0.25 }
                    .Select(item => (float)MathEx.LerpInverse(MinTime, MaxTime, item) * size.X + topLeft.X)
                    .ToArray();
                var yValues = new[] { box.Row, box.Row + 1 }
                    .Select(item => (float)MathEx.LerpInverse(0, rowCount, item) * size.Y + topLeft.Y)
                    .ToArray();

                var color = new Color4(0.8f, 0.8f, 0f, 1f);

                var meshes = new List<IMesh>();
                if (box.FadeStart)
                {
					meshes.Add(ModelFactory.CreatePlaneMesh(
						new Vector2(xValues[0], yValues[0]),
						new Vector2(xValues[1], yValues[1]),
						color));    
                }
				meshes.Add(ModelFactory.CreatePlaneMesh(
					new Vector2(xValues[1], yValues[0]),
					new Vector2(xValues[2], yValues[1]),
                    color));
                if (box.FadeEnd)
                {
					meshes.Add(ModelFactory.CreatePlaneMesh(
						new Vector2(xValues[2], yValues[0]),
						new Vector2(xValues[3], yValues[1]),
						color));
				}

                var bounds = new[] { topLeft, topLeft + size.YOnly(), topLeft + size, topLeft + size.XOnly() }.ToArray(); 
				
                var clippedMeshes = meshes.Select(item => 
                {
                    var result = item;
                    for (int i = 0; i < bounds.Length; i++)
                    {
                        var iNext = (i + 1) % bounds.Length;
                        var bisector = new LineF(bounds[i], bounds[iNext]);
                        result = result.Bisect(bisector, Side.Right);
                    }
                    return result;
                });

				var model = new Model(IMeshEx.Combine(meshes.ToArray()));

                layer.Renderables.Add(new Renderable { Models = new[] { model }.ToList() });
			}

            var markerPos = topLeft + new Vector2((float)MathEx.LerpInverse(MinTime, MaxTime, currentTime), 0) * size;

            DrawTimeMarker(layer, markerPos, 1);

            layer.DrawLine(new LineF(markerPos, markerPos + size.YOnly()), Color4.Black);

            for (int i = (int)Math.Ceiling(MinTime - 0.01); i <= Math.Floor(MaxTime + 0.01); i++)
            {
                Vector2 pos = new Vector2((float)MathEx.LerpInverse(MinTime, MaxTime, i), 0) * size;
                var top = (topLeft + pos).Round(Vector2.One);
                layer.DrawText(_font, top, i.ToString(), new Vector2(0.5f, 1));
                layer.DrawLine(new LineF(top, top + size.YOnly()));
            }
        }

        public void DrawTimeMarker(IRenderLayer layer, Vector2 position, float uiScale)
        {
			layer.DrawTriangle(
				position,
				position + new Vector2(15, -18) * uiScale,
				position + new Vector2(-15, -18) * uiScale);
			layer.DrawTriangle(
                position + new Vector2(0, -2) * uiScale,
				position + new Vector2(11, -16) * uiScale,
				position + new Vector2(-11, -16) * uiScale,
                Color4.Green);
        }

        public List<TimelineBox> GetTimelineBoxes(double currentTime)
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
                    endTime = currentTime;
                }

                output.Add(new TimelineBox(row, entity.StartTime, endTime, i > 0, i < count));
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
            public bool FadeStart { get; }
            public bool FadeEnd { get; }

            public TimelineBox(int row, int startTime, double endTime, bool fadeStart, bool fadeEnd)
            {
                Row = row;
                StartTime = startTime;
                EndTime = endTime;
                FadeStart = fadeStart;
                FadeEnd = fadeEnd;
            }
        }
    }
}
