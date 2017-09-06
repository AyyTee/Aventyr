using Game.Common;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Models;

namespace TimeLoopInc
{
    public class TimelineRender
    {
        public IGridEntity Selected { get; set; }
        readonly Scene _scene;
        readonly Font _font;

        public float MinTime { get; set; } = 0;
        public float MaxTime { get; set; } = 10;
        public float MinRow { get; set; } = 0;
        public float MaxRow { get; set; } = 3;

        public TimelineRender(Scene scene, Font font)
        {
            _scene = scene;
            _font = font;
        }

        public Timeline GetTimeline()
        {
            if (Selected == null)
            {
                return new Timeline();
            }
            return _scene.GetTimelines().First(item => item.Path.Contains(Selected));
        }

        public void Render(IRenderLayer layer, Vector2 topLeft, Vector2 size, float dpiScale, double t)
        {
            DebugEx.Assert(size.X > 0 && size.Y > 0);
            Vector2 gridSize = new Vector2(50, 20) * dpiScale;

            var output = new List<IRenderable>();
            layer.Renderables.Add(Draw.Rectangle(topLeft, topLeft + size, new Color4(0.8f, 0.8f, 0.8f, 0.8f)));

            var bounds = new ClipPath(new[] {
                    topLeft,
                    topLeft + size.YOnly(),
                    topLeft + size,
                    topLeft + size.XOnly()
                });

            layer.Renderables.Add(Draw.Text(_font, topLeft, GetTimeline().Name));
            output.AddRange(DrawTimelines(topLeft + new Vector2(10, 60), size - new Vector2(20, 60), t));
            foreach (var renderable in output)
            {
                renderable.ClipPaths.Add(bounds);
            }
            layer.Renderables.AddRange(output);
        }

        List<IRenderable> DrawTimelines(Vector2 topLeft, Vector2 size, double t)
        {
            var output = new List<IRenderable>();
            var currentTime = _scene.CurrentTime - (1 - t);

            var boxes = GetTimelineBoxes(currentTime);
            output.AddRange(DrawTimelineBoxes(boxes, topLeft, size));

            var markerPos = topLeft + new Vector2((float)MathEx.LerpInverse(MinTime, MaxTime, currentTime), 0) * size;

            output.AddRange(DrawTimeMarker(markerPos, 1));

            output.Add(Draw.Line(new LineF(markerPos, markerPos + size.YOnly()), Color4.Black));

            for (int i = (int)Math.Ceiling(MinTime - 0.01); i <= Math.Floor(MaxTime + 0.01); i++)
            {
                Vector2 pos = new Vector2((float)MathEx.LerpInverse(MinTime, MaxTime, i), 0) * size;
                var top = topLeft + pos;
                var text = i.ToString();
                output.Add(Draw.Text(_font, top - (Vector2)_font.GetSize(text) * new Vector2(0.5f, 1), text));
                output.Add(Draw.Line(new LineF(top, top + size.YOnly()), Color4.Black));
            }

            output.AddRange(DrawParadoxes(boxes, topLeft, size, 1));

            return output;
        }

        float TimeToX(double time, Vector2 topLeft, Vector2 size)
        {
            return (float)MathEx.LerpInverse(MinTime, MaxTime, time) * size.X + topLeft.X;
        }

        float RowToY(double row, Vector2 topLeft, Vector2 size)
        {
            return (float)MathEx.LerpInverse(MinRow, MaxRow, row) * size.Y + topLeft.Y;
        }

        public List<IRenderable> DrawTimelineBoxes(List<TimelineBox> boxes, Vector2 topLeft, Vector2 size)
        {
            var output = new List<IRenderable>();
            foreach (var box in boxes)
            {
                var xValues = new[] { box.StartTime - 0.9, box.StartTime, box.EndTime, box.EndTime + 0.9 }
                    .Select(item => TimeToX(item, topLeft, size))
                    .ToArray();
                var yValues = new[] { box.Row, box.Row + 1 }
                    .Select(item => RowToY(item, topLeft, size))
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
                    .Select(item => (IRenderable)new Renderable(new Model(item) { IsDithered = true }))
                    .ToArray());
            }

            return output;
        }

        public IRenderable[] DrawTimeMarker(Vector2 position, float uiScale)
        {
            return new[] {
                Draw.Triangle(
                    position,
                    position + new Vector2(15, -18) * uiScale,
                    position + new Vector2(-15, -18) * uiScale,
                    Color4.Black),
                Draw.Triangle(
                    position + new Vector2(0, -2) * uiScale,
                    position + new Vector2(11, -16) * uiScale,
                    position + new Vector2(-11, -16) * uiScale,
                    Color4.Green)
            };
        }

        List<IRenderable> DrawParadoxes(List<TimelineBox> boxes, Vector2 topLeft, Vector2 size, float uiScale)
        {
            var output = new List<IRenderable>();
            var paradoxes = _scene.GetParadoxes();
            foreach (var box in boxes)
            {
                var result = paradoxes.Where(item => item.Affected.Contains(box.Entity));
                foreach (var paradox in result)
                {
                    var v0 = new Vector2(
                        TimeToX(paradox.Time, topLeft, size),
                        RowToY(box.Row + 0.5, topLeft, size));

                    output.Add(Draw.Triangle(
                        v0 + new Vector2(0, -10) * uiScale,
                        v0 + new Vector2(-10, 8) * uiScale,
                        v0 + new Vector2(10, 8) * uiScale,
                        Color4.Yellow));
                }
            }
            return output;
        }

        public List<TimelineBox> GetTimelineBoxes(double currentTime)
        {
            var output = new List<TimelineBox>();

            var timeline = GetTimeline();
            int row = 0;
            var count = timeline.Path.Count;
            var maxEndTime = _scene.ChangeEndTime();
            for (int i = 0; i < count; i++)
            {
                var entity = timeline.Path[i];

                var startTime = entity.StartTime;
                if (i > 0)
                {
                    var previousEndTime = _scene.EntityEndTime(timeline.Path[i - 1]) ?? maxEndTime;
                    if (entity.StartTime < previousEndTime)
                    {
                        row++;
                    }
                }

                double endTime = _scene.EntityEndTime(entity) ?? maxEndTime;
                if (i + 1 == count)
                {
                    endTime = Math.Max(startTime, currentTime);
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
            var boxes = GetTimelineBoxes(_scene.CurrentTime);

            var timelineMax = boxes.MaxOrNull(item => item.EndTime + (item.FadeEnd ? 0.5 : 0)) ?? 10;
            var timelineMin = boxes.MinOrNull(item => item.StartTime + (item.FadeStart ? -0.5 : 0)) ?? 0;

            var targetMaxTime = (float)Math.Max(timelineMax, MathEx.Ceiling(_scene.CurrentTime, 5));
            var targetMinTime = (float)timelineMin;

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
    }
}
