using Game.Rendering;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeLoopInc;

namespace TimeLoopIncTests
{
    [TestFixture]
    class TimelineRenderTests
    {
        Scene _scene;
        TimelineRender _timelineRender;
        Layer _layer;

        [SetUp]
        public void SetUp()
        {
            _scene = new Scene();
            _layer = new Layer();
            _timelineRender = new TimelineRender(_scene, null);
        }

        [Test]
        public void RenderTimeline()
        {
            //_timelineRender.Render(_layer);
        }

        [Test]
        public void TimelineBoxesTest0()
        {
            var path = new List<Player> 
            { 
                new Player(new Transform2i(), 0), 
                new Player(new Transform2i(), -10)
            };
            _timelineRender.Timeline = new Timeline<Player>();
        }
    }
}
