using Game.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TimeLoopInc;

namespace TimeLoopIncTests
{
    [TestFixture]
    public class SceneTests
    {
        [Test]
        public void EmptySceneStartTime()
        {
            Assert.AreEqual(0, new Scene().StartTime);
        }

        [Test]
        public void GetStateInstantTest0()
        {
            var block = new Block(new Transform2i(), 0);
            var scene = new Scene(new HashSet<Vector2i>(), new List<TimePortal>(), new[] { block });

            var result = scene.GetSceneInstant(0).Entities.Count;
            Assert.AreEqual(1, result);
        }

        [Test]
        public void GetStateInstantTest1()
        {
            var block = new Block(new Transform2i(), 0);
            var scene = new Scene(new HashSet<Vector2i>(), new List<TimePortal>(), new[] { block });

            var result = scene.GetSceneInstant(1).Entities.Count;
            Assert.AreEqual(1, result);
        }

        [Test]
        public void GetStateInstantTest2()
        {
            var block = new Block(new Transform2i(), 0);
            var scene = new Scene(new HashSet<Vector2i>(), new List<TimePortal>(), new[] { block });

            var result = scene.GetSceneInstant(-1).Entities.Count;
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetStateInstantTest3()
        {
            var block = new Block(new Transform2i(), 2);
            var scene = new Scene(new HashSet<Vector2i>(), new List<TimePortal>(), new[] { block });

            Assert.AreEqual(0, scene.GetSceneInstant(1).Entities.Count);
        }

        [Test]
        public void GetStateInstantTest4()
        {
            var blocks = new[] {
                new Block(new Transform2i(new Vector2i(2, 0)), 0),
                new Block(new Transform2i(new Vector2i(2, 1)), 1),
                new Block(new Transform2i(new Vector2i(2, 2)), 2),
            };
            var scene = new Scene(new HashSet<Vector2i>(), new List<TimePortal>(), blocks);

            Assert.AreEqual(3, scene.GetSceneInstant(10).Entities.Count);
        }

        [Test]
        public void RotationRoundingBug()
        {
            var player = new Player(new Transform2i(gridRotation: new GridAngle(5)), 0);
            var scene = new Scene(new HashSet<Vector2i>(), new List<TimePortal>(), new[] { player });

            scene.Step(new MoveInput(new GridAngle()));

            Assert.AreEqual(5, scene.CurrentInstant.Entities[player].Transform.Direction.Value);
            Assert.AreEqual(5 * Math.PI / 2, scene.CurrentInstant.Entities[player].Transform.Angle, 0.0000001);
        }

        public TimePortal[] CreateTwoPortals(int timeOffset = 5)
        {
            var portal0 = new TimePortal(new Vector2i(1, 0), GridAngle.Right);
            var portal1 = new TimePortal(new Vector2i(-10, 0), GridAngle.Left);

            portal0.SetLinked(portal1);
            portal0.SetTimeOffset(timeOffset);

            return new[] { portal0, portal1 };
        }

        [Test]
        public void PlayerMovesForwardInTime()
        {
            var scene = new Scene(
                new HashSet<Vector2i>(),
                CreateTwoPortals(),
                new[] { new Player(new Transform2i(new Vector2i(1, 0)), 0) });

            scene.Step(new MoveInput(GridAngle.Right));

            Assert.AreEqual(6, scene.CurrentTime);
        }

        [Test]
        public void PlayerMovesBackwardInTime()
        {
            var scene = new Scene(
                new HashSet<Vector2i>(),
                CreateTwoPortals(-5),
                new[] { new Player(new Transform2i(new Vector2i(1, 0)), 0) });

            scene.Step(new MoveInput(GridAngle.Right));

            Assert.AreEqual(-4, scene.CurrentTime);
        }

        [Test]
        public void BoxIsPushedForwardInTime()
        {
            var entities = new[]
            {
                (IGridEntity)new Player(new Transform2i(), 0),
                new Block(new Transform2i(new Vector2i(1, 0)), 0)
            };

            var scene = new Scene(
                new HashSet<Vector2i>(),
                CreateTwoPortals(),
                entities);

            Assert.AreEqual(1, scene.GetEntities().OfType<Block>().Count());
            Assert.AreEqual(1, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());

            scene.Step(new MoveInput(GridAngle.Right));

            Assert.AreEqual(2, scene.GetEntities().OfType<Block>().Count());
            Assert.AreEqual(0, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());

            for (int i = 0; i < scene.Portals[0].TimeOffset - 1; i++)
            {
                scene.Step(new MoveInput(null));

                Assert.AreEqual(2, scene.GetEntities().OfType<Block>().Count());
                Assert.AreEqual(0, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());
            }

            scene.Step(new MoveInput(null));

            Assert.AreEqual(2, scene.GetEntities().OfType<Block>().Count());
            Assert.AreEqual(1, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());
        }

        [Test]
        public void BoxIsPushedBackwardInTime()
        {
            var entities = new[]
            {
                (IGridEntity)new Player(new Transform2i(), 0),
                new Block(new Transform2i(new Vector2i(1, 0)), 0)
            };

            var portals = CreateTwoPortals(-5);

            var scene = new Scene(
                new HashSet<Vector2i>(),
                portals,
                entities);

            Assert.AreEqual(1, scene.GetEntities().OfType<Block>().Count());
            Assert.AreEqual(1, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());

            scene.Step(new MoveInput(GridAngle.Right));

            Assert.AreEqual(2, scene.GetEntities().OfType<Block>().Count());
            Assert.AreEqual(1, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());

            for (int i = 0; i < portals[0].TimeOffset; i++)
            {
                scene.Step(new MoveInput(null));

                Assert.AreEqual(2, scene.GetEntities().OfType<Block>().Count());
                Assert.AreEqual(1, scene.CurrentInstant.Entities.Keys.OfType<Block>().Count());
            }

            scene.Step(new MoveInput(null));

            Assert.AreEqual(2, scene.GetEntities().OfType<Block>().Count());
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
                new IGridEntity[0]);

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
                new IGridEntity[0]);

            var start = new Transform2i(new Vector2i(2, 0));
            var result = scene.Move(start, new Vector2i(1, 0), 0);
            var expected = new Transform2i(new Vector2i(-10, 0), GridAngle.Left, 1, true);
            Assert.AreEqual(expected, result.Transform);
        }

        [Test]
        public void GetInstantTest()
        {
            var portal0 = new TimePortal(new Vector2i(2, 0), GridAngle.Right);
            var portal1 = new TimePortal(new Vector2i(0, 0), GridAngle.Left);
            portal0.SetLinked(portal1);
            portal0.SetTimeOffset(10);

            var Portals = new[]
            {
                portal0,
                portal1,
            };

            var entities = new[]
            {
                (IGridEntity)new Player(new Transform2i(), 0),
                new Block(new Transform2i(new Vector2i(2, 2)), 0),
            };

            var scene = new Scene(new HashSet<Vector2i>(), Portals, entities);

            scene.Step(new MoveInput(GridAngle.Left));

            Assert.AreEqual(-9, scene.CurrentTime);

            var instant = scene.GetSceneInstant(1);

            Assert.AreEqual(1, instant.Entities.Keys.OfType<Block>().Count());
        }

        [Test]
        public void PushBlockThroughPortal()
        {
            var block = new Block(new Transform2i(new Vector2i(1, 0)), 0);
            var entities = new[]
            {
                new Player(new Transform2i(new Vector2i(-10, 0)), 0),
                (IGridEntity)block
            };

            var scene = new Scene(
                new HashSet<Vector2i>(),
                CreateTwoPortals(-5),
                entities);

            scene.Step(new MoveInput(GridAngle.Left));

            Assert.AreEqual(6, scene.CurrentTime);
            Assert.AreEqual(new Vector2i(1, 0), scene.CurrentInstant.Entities[scene.CurrentPlayer].Transform.Position);
            Assert.AreEqual(new Vector2i(0, 0), scene.CurrentInstant.Entities[block].Transform.Position);
        }

        [Test]
        public void ParadoxTest0()
        {
            var scene = new Scene();

            var block0 = new Block(new Transform2i(), 0);
            var block1 = new Block(new Transform2i().WithRotation(new GridAngle(1)), 0);

            scene.AddEntity(block0);
            scene.AddEntity(block1);

            var result = scene.GetParadoxes(0);
            var expected = new[] { new Paradox(0, new HashSet<IGridEntity> { block0, block1 }) };
            Assert.AreEqual(expected, result);
        }

        Scene CreateDefaultScene()
        {
            var walls = new HashSet<Vector2i>
            {
            };
            var player = new Player(new Transform2i(), 0);

            var portal0 = new TimePortal(new Vector2i(1, 0), GridAngle.Right);
            var portal1 = new TimePortal(new Vector2i(-1, 0), GridAngle.Left);

            portal0.SetLinked(portal1);
            portal0.SetTimeOffset(10);

            var Portals = new[]
            {
                portal0,
                portal1,
            };

            var entities = new[] {
                (IGridEntity)player,
                new Block(new Transform2i(new Vector2i(1, 0)), 0),
            };

            return new Scene(walls, Portals, entities);
        }

        [Test]
        public void GetTimelinesTest0()
        {
            var scene = CreateDefaultScene();

            // Move right and push block through portal.
            scene.Step(new MoveInput(GridAngle.Right));

            var result = scene.GetTimelines().ToList();
            Assert.AreEqual(2, result.Count());
            var playerTimeline = result.First(item => item.Path[0].GetType() == typeof(Player));
            var blockTimeline = result.First(item => item.Path[0].GetType() == typeof(Block));
            Assert.AreEqual(2, blockTimeline.Path.Count);
            Assert.AreEqual(1, playerTimeline.Path.Count);
        }

        [Test]
        public void GetTimelinesTest1()
        {
            var scene = CreateDefaultScene();

            scene.Step(new MoveInput(GridAngle.Left));
            scene.Step(new MoveInput(GridAngle.Left));

            var result = scene.GetTimelines().ToList();
            Assert.AreEqual(2, result.Count);
            var playerTimeline = result.First(item => item.Path[0].GetType() == typeof(Player));
            var blockTimeline = result.First(item => item.Path[0].GetType() == typeof(Block));
            Assert.AreEqual(1, blockTimeline.Path.Count);
            Assert.AreEqual(2, playerTimeline.Path.Count);
        }

        [Test]
        public void GetTimelinesTest2()
        {
            var scene = CreateDefaultScene();

            // Go back in time and then travel back to the present.
            scene.Step(new MoveInput(GridAngle.Left));
            scene.Step(new MoveInput(GridAngle.Left));
            scene.Step(new MoveInput(GridAngle.Right));

            var result = scene.GetTimelines().ToList();
            Assert.AreEqual(2, result.Count);
			var playerTimeline = result.First(item => item.Path[0].GetType() == typeof(Player));
			var blockTimeline = result.First(item => item.Path[0].GetType() == typeof(Block));
			Assert.AreEqual(1, blockTimeline.Path.Count);
			Assert.AreEqual(3, playerTimeline.Path.Count);
        }
    }
}
