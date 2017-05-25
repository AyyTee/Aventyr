using Game.Common;
using NUnit.Framework;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeLoopInc;

namespace GameTests
{
    [TestFixture]
    public class Transform2iTests
    {
        [Test]
        public void RoundTransform2Test0()
        {
            var result = Transform2i.RoundTransform2d(new Transform2d());
            Assert.AreEqual(new Transform2i(), result);
        }

        [Test]
        public void RoundTransform2Test1()
        {
            var result = Transform2i.RoundTransform2d(new Transform2d(new Vector2d(100000001, 0)));
            Assert.AreEqual(new Transform2i(new Vector2i(100000001, 0)), result);
        }

        [Test]
        public void Equals()
        {
            var transform0 = new Transform2i(new Vector2i(5, 1), new GridAngle(1), 2, true);
            var transform1 = new Transform2i(new Vector2i(5, 1), new GridAngle(1), 2, true);
            Assert.AreEqual(transform0, transform1);
        }

        [Test]
        public void NotEqualsTest0()
        {
            var transform0 = new Transform2i(new Vector2i(3, 1), new GridAngle(1), 2, true);
            var transform1 = new Transform2i(new Vector2i(5, 1), new GridAngle(1), 2, true);
            Assert.AreNotEqual(transform0, transform1);
        }

        [Test]
        public void NotEqualsTest1()
        {
            var transform0 = new Transform2i(new Vector2i(5, 1), new GridAngle(-3), 2, true);
            var transform1 = new Transform2i(new Vector2i(5, 1), new GridAngle(1), 2, true);
            Assert.AreNotEqual(transform0, transform1);
        }

        [Test]
        public void NotEqualsTest2()
        {
            var transform0 = new Transform2i(new Vector2i(5, 1), new GridAngle(1), 3, true);
            var transform1 = new Transform2i(new Vector2i(5, 1), new GridAngle(1), 2, true);
            Assert.AreNotEqual(transform0, transform1);
        }

        [Test]
        public void NotEqualsTest3()
        {
            var transform0 = new Transform2i(new Vector2i(5, 1), new GridAngle(-3), 2, false);
            var transform1 = new Transform2i(new Vector2i(5, 1), new GridAngle(1), 2, true);
            Assert.AreNotEqual(transform0, transform1);
        }
    }
}
