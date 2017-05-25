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
    public class GridAngleTests
    {
        [Test]
        public void LeftTest()
        {
            var v = MathEx.AngleToVector(GridAngle.Left.Radians);
            Assert.IsTrue((v - (Vector2d)GridAngle.Left.Vector).Length < 0.0001);
        }
    }
}
