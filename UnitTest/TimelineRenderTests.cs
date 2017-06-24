using Game;
using Game.Rendering;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeLoopInc;

namespace GameTests
{
    [TestFixture]
    class TimelineRenderTests
    {
        TimeLoopInc.Scene _scene;
        TimelineRender _timelineRender;
        Layer _layer;

        [SetUp]
        public void SetUp()
        {
            _scene = new TimeLoopInc.Scene();
            _layer = new Layer();
            _timelineRender = new TimelineRender(_scene);
        }

        [Test]
        public void RenderTimeline()
        {
            //_timelineRender.Render(_layer);
        }
    }
}
