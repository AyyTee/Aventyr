using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class ActorExtTests
    {
        public Vector2[] GetVertices()
        {
            return new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
        }

        public void GetFixtureContourAssert(Actor actor)
        {
            Vector2[] worldVertices = actor.GetWorldVertices().ToArray();
            List<Vector2> fixtureVertices = ActorExt.GetFixtureContour(actor);
            Assert.IsTrue(worldVertices.SequenceEqual(fixtureVertices));
        }

        /// <summary>
        /// Fixture contour and world vertices should be equal if rotation and position are 0.
        /// </summary>
        [TestMethod]
        public void GetFixtureContourTest0()
        {
            Actor actor = new Actor(new Scene(), GetVertices());

            GetFixtureContourAssert(actor);
        }
        [TestMethod]
        public void GetFixtureContourTest1()
        {
            Actor actor = new Actor(new Scene(), GetVertices());
            actor.SetTransform(new Transform2(new Vector2(), 2.2f));

            GetFixtureContourAssert(actor);
        }
        [TestMethod]
        public void GetFixtureContourTest2()
        {
            Actor actor = new Actor(new Scene(), GetVertices());
            actor.SetTransform(new Transform2(new Vector2(), -3.2f));

            GetFixtureContourAssert(actor);
        }
        [TestMethod]
        public void GetFixtureContourTest3()
        {
            Actor actor = new Actor(new Scene(), GetVertices());
            actor.SetTransform(new Transform2(new Vector2(), 2.2f, 0, true));

            GetFixtureContourAssert(actor);
        }
        [TestMethod]
        public void GetFixtureContourTest4()
        {
            Actor actor = new Actor(new Scene(), GetVertices());
            actor.SetTransform(new Transform2(new Vector2(), -2.2f, 0, true));

            GetFixtureContourAssert(actor);
        }
    }
}
