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
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void VectorMatchesAngle(int angle)
        {
            var gridAngle = new GridAngle(angle);
            var v = MathEx.AngleToVectorReversed(gridAngle.Radians);
            Assert.IsTrue((v - (Vector2d)gridAngle.Vector).Length < 0.0001);
        }
    }
}
