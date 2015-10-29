using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;
using System.Collections;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class MathExtTests
    {
        #region LineInRectangle tests
        [TestMethod]
        public void LineInRectangle0()
        {
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2(0, 0), new Vector2(10, 10), new Vector2(1, 1), new Vector2(2, 2)));
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2d(0, 0), new Vector2d(10, 10), new Vector2d(1, 1), new Vector2d(2, 2)));
        }
        [TestMethod]
        public void LineInRectangle1()
        {
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2(0, 0), new Vector2(-10, -10), new Vector2(-1, -1), new Vector2(-2, -2)));
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2d(0, 0), new Vector2d(-10, -10), new Vector2d(-1, -1), new Vector2d(-2, -2)));
        }
        [TestMethod]
        public void LineInRectangle2()
        {
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(4, 6), new Vector2(20, 4)));
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(4, 6), new Vector2d(20, 4)));
        }
        [TestMethod]
        public void LineInRectangle3()
        {
            Assert.IsFalse(MathExt.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(0, 0), new Vector2(20, 1)));
            Assert.IsFalse(MathExt.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(0, 0), new Vector2d(20, 1)));
        }
        [TestMethod]
        public void LineInRectangle4()
        {
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(5, 5), new Vector2(0, 0)));
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(5, 5), new Vector2d(0, 0)));
        }
        [TestMethod]
        public void LineInRectangle5()
        {
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(0, 0), new Vector2(5, 5)));
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(0, 0), new Vector2d(5, 5)));
        }
        [TestMethod]
        public void LineInRectangle6()
        {
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(6, 5), new Vector2(0, 0)));
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(6, 5), new Vector2d(0, 0)));
        }
        [TestMethod]
        public void LineInRectangle7()
        {
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(0, 0), new Vector2(6, 5)));
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(0, 0), new Vector2d(6, 5)));
        }
        [TestMethod]
        public void LineInRectangle8()
        {
            Assert.IsFalse(MathExt.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(0, 0), new Vector2(0, 0)));
            Assert.IsFalse(MathExt.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(0, 0), new Vector2d(0, 0)));
        }
        #endregion
        #region ComputeConvexHull tests
        [TestMethod]
        public void ComputeConvexHullTest0()
        {
            List<Vector2> listIn = new List<Vector2>();
            listIn.AddRange(new Vector2[3] {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0)
            });
            List<Vector2> listOut;

            listOut = MathExt.ComputeConvexHull(listIn);

            Assert.IsTrue(
                listOut[0] == new Vector2(0,0) &&
                listOut[1] == new Vector2(0,1) &&
                listOut[2] == new Vector2(1,0)
                );
        }
        [TestMethod]
        public void ComputeConvexHullTest1()
        {
            List<Vector2> listIn = new List<Vector2>();
            listIn.AddRange(new Vector2[4] {
                new Vector2(-5, -5),
                new Vector2(5, -5),
                new Vector2(5, 5),
                new Vector2(-5, 5),
            });
            List<Vector2> listOut;

            listOut = MathExt.ComputeConvexHull(listIn);

            Assert.IsTrue(
                listOut[0] == new Vector2(-5, -5) &&
                listOut[1] == new Vector2(-5, 5) &&
                listOut[2] == new Vector2(5, 5) &&
                listOut[3] == new Vector2(5, -5)
                );
        }
        [TestMethod]
        public void ComputeConvexHullTest2()
        {
            List<Vector2> listIn = new List<Vector2>();
            listIn.AddRange(new Vector2[5] {
                new Vector2(-5, -5),
                new Vector2(5, -5),
                new Vector2(0, 0),
                new Vector2(5, 5),
                new Vector2(-5, 5),
            });
            List<Vector2> listOut;

            listOut = MathExt.ComputeConvexHull(listIn);

            Assert.IsTrue(
                listOut[0] == new Vector2(-5, -5) &&
                listOut[1] == new Vector2(-5, 5) &&
                listOut[2] == new Vector2(5, 5) &&
                listOut[3] == new Vector2(5, -5)
                );
        }
        [TestMethod]
        public void ComputeConvexHullTest3()
        {
            List<Vector2> listIn = new List<Vector2>();
            listIn.AddRange(new Vector2[7] {
                new Vector2(123, 34),
                new Vector2(135, 50),
                new Vector2(303, 101),
                new Vector2(15, 51),
                new Vector2(60, 59),
                new Vector2(198, 32),
                new Vector2(198, 200),
            });
            List<Vector2> listOut;

            listOut = MathExt.ComputeConvexHull(listIn);

            Assert.IsTrue(
                listOut[0] == new Vector2(15, 51) &&
                listOut[1] == new Vector2(198, 200) &&
                listOut[2] == new Vector2(303, 101) &&
                listOut[3] == new Vector2(198, 32) &&
                listOut[4] == new Vector2(123, 34)
                );
        }
        #endregion

        #region IsClockwise tests
        [TestMethod]
        public void IsClockwiseTest0()
        {
            Vector2[] v = new Vector2[] {
                new Vector2(),
                new Vector2(0, 1),
                new Vector2(1, 0)
            };
            Assert.IsTrue(MathExt.IsClockwise(v));
        }
        [TestMethod]
        public void IsClockwiseTest1()
        {
            Vector2[] v = new Vector2[] {
                new Vector2(),
                new Vector2(1, 0),
                new Vector2(0, 1)
            };
            Assert.IsFalse(MathExt.IsClockwise(v));
        }
        [TestMethod]
        public void IsClockwiseTest2()
        {
            Vector2[] v = new Vector2[] {
                new Vector2(1f, 9f),
                new Vector2(10f, 10f),
                new Vector2(11f, -5f),
                new Vector2(-6f, -6.3f)
            };
            Assert.IsTrue(MathExt.IsClockwise(v));
        }
        [TestMethod]
        public void IsClockwiseTest3()
        {
            Vector2[] v = new Vector2[] {
                new Vector2(1f, 1f),
                new Vector2(1f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
            };
            Assert.IsTrue(MathExt.IsClockwise(v));
        }
        [TestMethod]
        public void IsClockwiseTest4()
        {
            Vector2[] v = new Vector2[] {
                new Vector2(1f, 1f),
                new Vector2(1f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
            };
            List<Vector2> vList = new List<Vector2>(v);
            vList.Reverse();
            Assert.IsFalse(MathExt.IsClockwise(vList));
        }
        #endregion
    }
}
