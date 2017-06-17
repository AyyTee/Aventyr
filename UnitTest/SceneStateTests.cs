﻿using Game.Common;
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
            Assert.AreEqual(1, result);
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
                new Block(new Transform2i(new Vector2i(2, 0)), 0),
                new Block(new Transform2i(new Vector2i(2, 1)), 1),
                new Block(new Transform2i(new Vector2i(2, 2)), 2),
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

        [Test]
        public void BoxIsPushedForwardInTime()
        {
            var portal0 = new TimePortal(new Vector2i(1, 0), GridAngle.Right);
            var portal1 = new TimePortal(new Vector2i(-10, 0), GridAngle.Left);

            portal0.SetLinked(portal1);
            portal0.SetTimeOffset(5);

            var scene = new Scene(
                new HashSet<Vector2i>(),
                new[] { portal0, portal1 },
                new Player(new Transform2i(), 0),
                new[] { new Block(new Transform2i(new Vector2i(1, 0)), 0) });

            Assert.AreEqual(1, scene.BlockTimelines.Count);
            Assert.AreEqual(1, scene.BlockTimelines[0].Path.Count);
            Assert.AreEqual(1, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());

            scene.Step(new Input(GridAngle.Right));

            Assert.AreEqual(1, scene.BlockTimelines.Count);
            Assert.AreEqual(2, scene.BlockTimelines[0].Path.Count);
            Assert.AreEqual(0, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());

            for (int i = 0; i < portal0.TimeOffset - 1; i++)
            {
                scene.Step(new Input(null));

                Assert.AreEqual(1, scene.BlockTimelines.Count);
                Assert.AreEqual(2, scene.BlockTimelines[0].Path.Count);
                Assert.AreEqual(0, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());
            }

            scene.Step(new Input(null));

            Assert.AreEqual(1, scene.BlockTimelines.Count);
            Assert.AreEqual(2, scene.BlockTimelines[0].Path.Count);
            Assert.AreEqual(1, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());
        }

        [Test]
        public void BoxIsPushedBackwardInTime()
        {
            var portal0 = new TimePortal(new Vector2i(1, 0), GridAngle.Right);
            var portal1 = new TimePortal(new Vector2i(-10, 0), GridAngle.Left);

            portal0.SetLinked(portal1);
            portal0.SetTimeOffset(-5);

            var scene = new Scene(
                new HashSet<Vector2i>(),
                new[] { portal0, portal1 },
                new Player(new Transform2i(), 0),
                new[] { new Block(new Transform2i(new Vector2i(1, 0)), 0) });

            Assert.AreEqual(1, scene.BlockTimelines.Count);
            Assert.AreEqual(1, scene.BlockTimelines[0].Path.Count);
            Assert.AreEqual(1, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());

            scene.Step(new Input(GridAngle.Right));

            Assert.AreEqual(1, scene.BlockTimelines.Count);
            Assert.AreEqual(2, scene.BlockTimelines[0].Path.Count);
            Assert.AreEqual(1, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());

            for (int i = 0; i < portal0.TimeOffset; i++)
            {
                scene.Step(new Input(null));

                Assert.AreEqual(1, scene.BlockTimelines.Count);
                Assert.AreEqual(2, scene.BlockTimelines[0].Path.Count);
                Assert.AreEqual(1, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());
            }

            scene.Step(new Input(null));

            Assert.AreEqual(1, scene.BlockTimelines.Count);
            Assert.AreEqual(2, scene.BlockTimelines[0].Path.Count);
            Assert.AreEqual(1, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());
        }

        [Test]
        public void MoveIntoPortalHasCorrectTime()
        {
            var portal0 = new TimePortal(new Vector2i(2, 0), GridAngle.Right);
            var portal1 = new TimePortal(new Vector2i(-10, 0), GridAngle.Left);

            portal0.SetLinked(portal1);
            var timeOffset = 5;
            portal0.SetTimeOffset(timeOffset);

            var scene = new Scene(
                new HashSet<Vector2i>(),
                new[] { portal0, portal1 },
                null,
                new List<Block>());

            var result = scene.Move(new Transform2i(new Vector2i(2, 0)), new Vector2i(1, 0), 0);
            Assert.AreEqual(timeOffset, result.Time);
        }

        [Test]
        public void MoveIntoPortalHasCorrectTransform()
        {
            var portal0 = new TimePortal(new Vector2i(2, 0), GridAngle.Right);
            var portal1 = new TimePortal(new Vector2i(-10, 0), GridAngle.Left);

            portal0.SetLinked(portal1);
            portal0.SetTimeOffset(5);

            var scene = new Scene(
                new HashSet<Vector2i>(),
                new[] { portal0, portal1 },
                null,
                new List<Block>());

            var start = new Transform2i(new Vector2i(2, 0));
            var result = scene.Move(start, new Vector2i(1, 0), 0);
            var expected = new Transform2i(new Vector2i(-10, 0), GridAngle.Left, 1, true);
            Assert.AreEqual(expected, result.Transform);
        }
    }
}
