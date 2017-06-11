using Game.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TimeLoopInc;

namespace GameTests
{
    [TestFixture]
    public class SceneStateTests
    {
        [Test]
        public void EmptySceneStateStartTime()
        {
            Assert.AreEqual(0, new Scene().StartTime);
        }

        [Test]
        public void GetStateInstantTest0()
        {
            var block = new Block(new Transform2i(), 0);
            var scene = new Scene(new HashSet<Vector2i>(), new List<TimePortal>(), null, new[] { block });

            var result = scene.GetStateInstant(0).Entities.Count;
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetStateInstantTest1()
        {
            var block = new Block(new Transform2i(), 0);
            var scene = new Scene(new HashSet<Vector2i>(), new List<TimePortal>(), null, new[] { block });

            var result = scene.GetStateInstant(1).Entities.Count;
            Assert.AreEqual(1, result);
        }

        [Test]
        public void GetStateInstantTest2()
        {
            var block = new Block(new Transform2i(), 0);
            var scene = new Scene(new HashSet<Vector2i>(), new List<TimePortal>(), null, new[] { block });

            var result = scene.GetStateInstant(-1).Entities.Count;
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetStateInstantTest3()
        {
            var block = new Block(new Transform2i(), 2);
            var scene = new Scene(new HashSet<Vector2i>(), new List<TimePortal>(), null, new[] { block });

            Assert.AreEqual(0, scene.GetStateInstant(1).Entities.Count);
        }

        [Test]
        public void GetStateInstantTest4()
        {
            var blocks = new[] {
                new Block(new Transform2i(new Vector2i(2, 0)), 0, 1),
                new Block(new Transform2i(new Vector2i(2, 1)), 1, 1),
                new Block(new Transform2i(new Vector2i(2, 2)), 2, 1),
            };
            var scene = new Scene(new HashSet<Vector2i>(), new List<TimePortal>(), null, blocks);

            Assert.AreEqual(3, scene.GetStateInstant(10).Entities.Count);
        }

        [Test]
        public void RotationRoundingBug()
        {
            var player = new Player(new Transform2i(gridRotation: new GridAngle(5)), 0);
            var scene = new Scene(new HashSet<Vector2i>(), new List<TimePortal>(), player, new List<Block>());

            scene.Step(new Input(new GridAngle()));

            Assert.AreEqual(5, scene.CurrentInstant.Entities[player].Transform.Direction.Value);
            Assert.AreEqual(5 * Math.PI / 2, scene.CurrentInstant.Entities[player].Transform.Angle, 0.0000001);
        }
    }
}
