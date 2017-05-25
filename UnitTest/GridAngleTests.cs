using Game.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeLoopInc;

namespace GameTests
{
    [TestClass]
    public class GridAngleTests
    {
        [TestMethod]
        public void LeftTest()
        {
            var v = MathEx.AngleToVector(GridAngle.Left.Radians);
            Assert.IsTrue((v - (Vector2d)GridAngle.Left.Vector).Length < 0.0001);
        }
    }
}
