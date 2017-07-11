using Game.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTests
{
    [TestFixture]
    public class Vector2iTests
    {
        [Test]
        public void EqualsTest0()
        {
            Assert.AreEqual(new Vector2i(1, 2), new Vector2i(1, 2));
        }

        [Test]
        public void EqualsTest1()
        {
            Assert.AreNotEqual(new Vector2i(2, 2), new Vector2i(1, 2));
        }

        [Test]
        public void EqualsTest2()
        {
            Assert.IsTrue(new Vector2i(1, 2) == new Vector2i(1, 2));
        }

        [Test]
        public void EqualsTest3()
        {
            Assert.IsFalse(new Vector2i(2, 2) == new Vector2i(1, 2));
        }
    }
}
