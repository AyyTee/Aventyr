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
        public float MinRow { get; private set; } = 0;
        public float MaxRow { get; private set; } = 3;

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

			var bounds = new ClipPath(new[] {
					topLeft,
					topLeft + size.YOnly(),
					topLeft + size,
					topLeft + size.XOnly()
				});

            layer.Renderables.Add(IRenderLayerEx.DrawText(_font, topLeft, Timeline.Name));
            var output = DrawTimelines(topLeft + new Vector2(10, 60), size - new Vector2(20, 60), t);
            foreach (var renderable in output)
            {
                renderable.ClipPaths.Add(bounds);
            }
            layer.Renderables.AddRange(output);
        }

        List<IRenderable> DrawTimelines(Vector2 topLeft, Vector2 size, double t)
        {
            var output = new List<IRenderable>();
            var currentTime = _scene.CurrentInstant.Time - (1 - t);

            var boxes = GetTimelineBoxes(currentTime);
            output.AddRange(DrawTimelineBoxes(boxes, topLeft, size));
            output.AddRange(DrawParadoxes(boxes, topLeft, size));
			
            var markerPos = topLeft + new Vector2((float)MathEx.LerpInverse(MinTime, MaxTime, currentTime), 0) * size;

            output.AddRange(DrawTimeMarker(markerPos, 1));

            output.Add(IRenderLayerEx.DrawLine(new LineF(markerPos, markerPos + size.YOnly()), Color4.Black));

            for (int i = (int)Math.Ceiling(MinTime - 0.01); i <= Math.Floor(MaxTime + 0.01); i++)
            {
                Vector2 pos = new Vector2((float)MathEx.LerpInverse(MinTime, MaxTime, i), 0) * size;
                var top = (topLeft + pos).Round(Vector2.One);
                output.Add(IRenderLayerEx.DrawText(_font, top, i.ToString(), new Vector2(0.5f, 1)));
                output.Add(IRenderLayerEx.DrawLine(new LineF(top, top + size.YOnly())));
            }

            return output;
        }

        public List<IRenderable> DrawTimelineBoxes(List<TimelineBox> boxes, Vector2 topLeft, Vector2 size)
        {
            var output = new List<IRenderable>();
            foreach (var box in boxes)
            {
                var xValues = new[] { box.StartTime - 0.5, box.StartTime, box.EndTime, box.EndTime + 0.5 }
                    .Select(item => (float)MathEx.LerpInverse(MinTime, MaxTime, item) * size.X + topLeft.X)
                    .ToArray();
                var yValues = new[] { box.Row, box.Row + 1 }
                    .Select(item => (float)MathEx.LerpInverse(MinRow, MaxRow, item) * size.Y + topLeft.Y)
                    .ToArray();

                var color = new Color4(0.8f, 0f, 0.8f, 1f);
                var colorTransparent = new Color4(0.8f, 0f, 0.8f, 0f);

                var meshes = new List<IMesh>();
                if (box.FadeStart)
                {
                    meshes.Add(ModelFactory.CreatePlaneMesh(
                        new Vector2(xValues[0], yValues[0]),
                        new Vector2(xValues[1], yValues[1]),
                        colorTransparent, color, color, colorTransparent));
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
                        color, colorTransparent, colorTransparent, color));
                }

				output.AddRange(meshes
					.Select(item => (IRenderable)new Renderable(new Model(item) { IsTransparent = true }))
					.ToArray());
            }

            return output;
        }

        public IRenderable[] DrawTimeMarker(Vector2 position, float uiScale)
        {
            return new[] {
                IRenderLayerEx.DrawTriangle(
                    position,
                    position + new Vector2(15, -18) * uiScale,
                    position + new Vector2(-15, -18) * uiScale),
                IRenderLayerEx.DrawTriangle(
                    position + new Vector2(0, -2) * uiScale,
                    position + new Vector2(11, -16) * uiScale,
                    position + new Vector2(-11, -16) * uiScale,
                    Color4.Green)
            };
        }

        List<IRenderable> DrawParadoxes(List<TimelineBox> boxes, Vector2 topLeft, Vector2 size)
        {
            var paradoxes = _scene.GetParadoxes();
            foreach (var box in boxes)
            {
                var result = paradoxes.Where(item => item.Affected.Contains(box.Entity));

            }
            return new List<IRenderable>();
            //_scene.GetParadoxes().Where();
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

                var box = new TimelineBox(
                    row, 
                    entity.StartTime, 
                    endTime, 
                    i > 0, 
                    i + 1 < count,
                    entity);
                output.Add(box);
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
            public IGridEntity Entity { get; }

            public TimelineBox(int row, int startTime, double endTime, bool fadeStart, bool fadeEnd, IGridEntity entity)
            {
                Row = row;
                StartTime = startTime;
                EndTime = endTime;
                FadeStart = fadeStart;
                FadeEnd = fadeEnd;
                Entity = entity;
            }
        }
    }
}
