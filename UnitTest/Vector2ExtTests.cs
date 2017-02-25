using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Common;
using OpenTK;

namespace GameTests
{
    [TestClass]
    public class Vector2ExtTests
    {
        #region Project tests
        [TestMethod]
        public void ProjectTest0()
        {
            var v0 = new Vector2d(1, 0);
            var v1 = new Vector2d(1, 0);

            var result = v0.Project(v1);
            Assert.IsTrue(result == new Vector2d(1, 0));
        }

        [TestMethod]
        public void ProjectTest1()
        {
            var v0 = new Vector2d(1, 1);
            var v1 = new Vector2d(1, 0);

            var result = v0.Project(v1);
            Assert.IsTrue(result == new Vector2d(1, 0));
        }

        [TestMethod]
        public void ProjectTest2()
        {
            var v0 = new Vector2d(1, 1);
            var v1 = new Vector2d(-5, 0);

            var result = v0.Project(v1);
            Assert.IsTrue(result == new Vector2d(1, 0));
        }

        [TestMethod]
        public void ProjectTest3()
        {
            var v0 = new Vector2d(0, 0);
            var v1 = new Vector2d(2, 1);

            var result = v0.Project(v1);
            Assert.IsTrue(result == new Vector2d(0, 0));
        }

        [TestMethod]
        public void ProjectTest4()
        {
            var v0 = new Vector2d(1, 0);
            var v1 = new Vector2d(2, 1);

            var result = v0.Project(v1);
            Assert.IsTrue((result - new Vector2d(0.8, 0.4)).Length < 0.0001);
        }
        #endregion

        #region Mirror tests
        [TestMethod]
        public void MirrorTest0()
        {
            var v0 = new Vector2d(1, 0);
            var normal = new Vector2d(1, 0);

            var result = v0.Mirror(normal);
            Assert.IsTrue(result == v0);
        }

        [TestMethod]
        public void MirrorTest1()
        {
            var v0 = new Vector2d(1, 1);
            var normal = new Vector2d(1, 0);

            var result = v0.Mirror(normal);
            Assert.IsTrue(result == new Vector2d(1, -1));
        }

        [TestMethod]
        public void MirrorTest2()
        {
            var v0 = new Vector2d(1, 1);
            var normal = new Vector2d(-4, 0);

            var result = v0.Mirror(normal);
            Assert.IsTrue(result == new Vector2d(1, -1));
        }

        [TestMethod]
        public void MirrorTest3()
        {
            var v0 = new Vector2d(5, 1);
            var normal = new Vector2d(2, 2);

            var result = v0.Mirror(normal);
            Assert.IsTrue((result - new Vector2d(1, 5)).Length < 0.0001);
        }
        #endregion
    }
}