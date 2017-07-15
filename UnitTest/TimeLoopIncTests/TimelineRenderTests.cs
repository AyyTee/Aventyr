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
        public void GetTimelineBoxesTest0()
        {
            var scene = SceneTests.CreateDefaultScene();
            scene.SetEntities(new[] { new Player(new Transform2i(), 0) });

            _timelineRender = new TimelineRender(scene, null);
            _timelineRender.Selected = scene.CurrentPlayer;

            scene.Step(new MoveInput(GridAngle.Right));
            scene.Step(new MoveInput(GridAngle.Right));
            scene.Step(new MoveInput(GridAngle.Left));

            var result = _timelineRender.GetTimelineBoxes(scene.CurrentTime);

            var entities = scene.GetEntities();
            var expected = new[]
            {
                new TimelineBox(0, 0, 1, false, true, entities[0]),
                new TimelineBox(0, 12, 12, true, true, entities[1]),
                new TimelineBox(1, 3, 3, true, false, entities[2]),
            };
            Assert.AreEqual(expected, result);
        }
    }
}
