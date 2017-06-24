using Game.Common;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class TimelineRender
    {
        public ITimeline Timeline { get; set; }
        readonly Scene _scene;

        public TimelineRender(Scene scene)
        {
            _scene = scene;
        }

        public void Render(IRenderLayer layer, Vector2 topLeft, Vector2 bottomRight, float dpiScale)
        {
            Vector2 gridSize = new Vector2(50, 20) * dpiScale;

            layer.DrawRectangle(topLeft, bottomRight, new Color4(0.8f, 0.8f, 0.8f, 0.8f));

            if (Timeline == null)
            {
                return;
            }

            Vector2 start = new Vector2();
            var count = Timeline.Path.Count;
            for (int i = 0; i < count; i++)
            {
                var entity = Timeline.Path[i];

                Vector2 end = new Vector2();

                start.X = entity.StartTime * gridSize.X;
                if (i > 0 && entity.StartTime < Timeline.Path[i - 1].EndTime)
                {
                    start.Y -= 1;
                }

                end.X = entity.EndTime;
                if (i + 1 == count)
                {
                    end.X = _scene.CurrentInstant.Time;
                }

                end.Y = start.Y - 1;

                layer.DrawRectangle(topLeft + start * gridSize, topLeft + end * gridSize, new Color4(0.8f, 0.8f, 0f, 1f));
            }
        }

        public void Update()
        {
            
        }
    }
}
