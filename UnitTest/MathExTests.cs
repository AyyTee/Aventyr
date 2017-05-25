using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Game.Common;
using Game.Models;
using Game.Portals;
using Game.Rendering;

namespace GameTests
{
    [TestClass]
    public class MathExTests
    {
        const double ErrorMargin = 0.0000001;

        #region LineInRectangle tests
        [TestMethod]
        public void LineInRectangle0()
        {
            Assert.IsTrue(MathEx.LineInRectangle(new Vector2(0, 0), new Vector2(10, 10), new Vector2(1, 1), new Vector2(2, 2)));
            Assert.IsTrue(MathEx.LineInRectangle(new Vector2d(0, 0), new Vector2d(10, 10), new Vector2d(1, 1), new Vector2d(2, 2)));
        }
        [TestMethod]
        public void LineInRectangle1()
        {
            Assert.IsTrue(MathEx.LineInRectangle(new Vector2(0, 0), new Vector2(-10, -10), new Vector2(-1, -1), new Vector2(-2, -2)));
            Assert.IsTrue(MathEx.LineInRectangle(new Vector2d(0, 0), new Vector2d(-10, -10), new Vector2d(-1, -1), new Vector2d(-2, -2)));
        }
        [TestMethod]
        public void LineInRectangle2()
        {
            Assert.IsTrue(MathEx.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(4, 6), new Vector2(20, 4)));
            Assert.IsTrue(MathEx.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(4, 6), new Vector2d(20, 4)));
        }
        [TestMethod]
        public void LineInRectangle3()
        {
            Assert.IsFalse(MathEx.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(0, 0), new Vector2(20, 1)));
            Assert.IsFalse(MathEx.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(0, 0), new Vector2d(20, 1)));
        }
        [TestMethod]
        public void LineInRectangle4()
        {
            Assert.IsTrue(MathEx.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(5, 5), new Vector2(0, 0)));
            Assert.IsTrue(MathEx.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(5, 5), new Vector2d(0, 0)));
        }
        [TestMethod]
        public void LineInRectangle5()
        {
            Assert.IsTrue(MathEx.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(0, 0), new Vector2(5, 5)));
            Assert.IsTrue(MathEx.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(0, 0), new Vector2d(5, 5)));
        }
        [TestMethod]
        public void LineInRectangle6()
        {
            Assert.IsTrue(MathEx.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(6, 5), new Vector2(0, 0)));
            Assert.IsTrue(MathEx.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(6, 5), new Vector2d(0, 0)));
        }
        [TestMethod]
        public void LineInRectangle7()
        {
            Assert.IsTrue(MathEx.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(0, 0), new Vector2(6, 5)));
            Assert.IsTrue(MathEx.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(0, 0), new Vector2d(6, 5)));
        }
        [TestMethod]
        public void LineInRectangle8()
        {
            Assert.IsFalse(MathEx.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(0, 0), new Vector2(0, 0)));
            Assert.IsFalse(MathEx.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(0, 0), new Vector2d(0, 0)));
        }
        #endregion
        #region ComputeConvexHull tests
        [TestMethod]
        public void ComputeConvexHullTest0()
        {
            List<Vector2> listIn = new List<Vector2>();
            listIn.AddRange(new[] {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0)
            });

            List<Vector2> listOut = MathEx.GetConvexHull(listIn);

            Assert.IsTrue(
                listOut[0] == new Vector2(0, 0) &&
                listOut[1] == new Vector2(0, 1) &&
                listOut[2] == new Vector2(1, 0)
                );
        }
        [TestMethod]
        public void ComputeConvexHullTest1()
        {
            List<Vector2> listIn = new List<Vector2>();
            listIn.AddRange(new[] {
                new Vector2(-5, -5),
                new Vector2(5, -5),
                new Vector2(5, 5),
                new Vector2(-5, 5),
            });

            List<Vector2> listOut = MathEx.GetConvexHull(listIn);

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
            listIn.AddRange(new[] {
                new Vector2(-5, -5),
                new Vector2(5, -5),
                new Vector2(0, 0),
                new Vector2(5, 5),
                new Vector2(-5, 5),
            });

            List<Vector2> listOut = MathEx.GetConvexHull(listIn);

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
            listIn.AddRange(new[] {
                new Vector2(123, 34),
                new Vector2(135, 50),
                new Vector2(303, 101),
                new Vector2(15, 51),
                new Vector2(60, 59),
                new Vector2(198, 32),
                new Vector2(198, 200),
            });

            List<Vector2> listOut = MathEx.GetConvexHull(listIn);

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
            Vector2[] v = {
                new Vector2(),
                new Vector2(0, 1),
                new Vector2(1, 0)
            };
            Assert.IsTrue(MathEx.IsClockwise(v));
        }
        [TestMethod]
        public void IsClockwiseTest1()
        {
            Vector2[] v = {
                new Vector2(),
                new Vector2(1, 0),
                new Vector2(0, 1)
            };
            Assert.IsFalse(MathEx.IsClockwise(v));
        }
        [TestMethod]
        public void IsClockwiseTest2()
        {
            Vector2[] v = {
                new Vector2(1f, 9f),
                new Vector2(10f, 10f),
                new Vector2(11f, -5f),
                new Vector2(-6f, -6.3f)
            };
            Assert.IsTrue(MathEx.IsClockwise(v));
        }
        [TestMethod]
        public void IsClockwiseTest3()
        {
            Vector2[] v = {
                new Vector2(1f, 1f),
                new Vector2(1f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
            };
            Assert.IsTrue(MathEx.IsClockwise(v));
        }
        [TestMethod]
        public void IsClockwiseTest4()
        {
            Vector2[] v = {
                new Vector2(1f, 1f),
                new Vector2(1f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
            };
            List<Vector2> vList = new List<Vector2>(v);
            vList.Reverse();
            Assert.IsFalse(MathEx.IsClockwise(vList));
        }
        #endregion
        #region SetWinding tests
        [TestMethod]
        public void SetWindingTest0()
        {
            Vector2[] v = {
                new Vector2(),
                new Vector2(0, 1),
                new Vector2(1, 0)
            };
            Debug.Assert(MathEx.IsClockwise(v));
            Vector2[] result = MathEx.SetWinding(v, false);
            Vector2[] expected = {
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2()
            };

            Assert.IsTrue(result.SequenceEqual(expected));
        }
        [TestMethod]
        public void SetWindingTest1()
        {
            var v = new List<Vector2>
            {
                new Vector2(),
                new Vector2(0, 1),
                new Vector2(1, 0)
            };

            Debug.Assert(MathEx.IsClockwise(v));
            List<Vector2> result = MathEx.SetWinding(v, false);
            Vector2[] expected = {
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2()
            };

            Assert.IsTrue(result.SequenceEqual(expected));
        }
        #endregion
        #region LineCircleIntersect tests
        [TestMethod]
        public void LineCircleIntersectTest0()
        {
            //test degernate line outside of circle
            LineF line = new LineF(new Vector2(1, 0), new Vector2(1, 0));
            Assert.IsTrue(MathEx.LineCircleIntersect(new Vector2(1, 0), 0.1f, line, true).Length == 0);
        }
        [TestMethod]
        public void LineCircleIntersectTest1()
        {
            //test degenerate line inside of circle
            LineF line = new LineF(new Vector2(1, 0), new Vector2(1f, 0));
            Assert.IsTrue(MathEx.LineCircleIntersect(new Vector2(1, 0), 0.1f, line, true).Length == 0);
        }
        [TestMethod]
        public void LineCircleIntersectTest2()
        {
            //test degernate line outside of circle
            LineF line = new LineF(new Vector2(1, 0), new Vector2(1, 0));
            Assert.IsTrue(MathEx.LineCircleIntersect(new Vector2(1, 0), 0.1f, line, false).Length == 0);
        }
        [TestMethod]
        public void LineCircleIntersectTest3()
        {
            //test degenerate line inside of circle
            LineF line = new LineF(new Vector2(1, 0), new Vector2(1f, 0));
            Assert.IsTrue(MathEx.LineCircleIntersect(new Vector2(1, 0), 0.1f, line, false).Length == 0);
        }
        [TestMethod]
        public void LineCircleIntersectTest4()
        {
            LineF line = new LineF(new Vector2(-1f, 1f), new Vector2(2f, 1f));
            IntersectCoord[] intersects = MathEx.LineCircleIntersect(new Vector2(1, 1), 0.1f, line, true);
            Assert.IsTrue(intersects[0] != null && intersects[1] != null);
            Assert.IsTrue(
                ((intersects[0].Position - new Vector2d(1.1, 1)).Length < ErrorMargin && intersects[0].First - 0.7 < ErrorMargin &&
                (intersects[1].Position - new Vector2d(0.9, 1)).Length < ErrorMargin) && intersects[1].First - 0.63333333333 < ErrorMargin ||
                ((intersects[0].Position - new Vector2d(0.9, 1)).Length < ErrorMargin && intersects[0].First - 0.63333333333 < ErrorMargin &&
                (intersects[1].Position - new Vector2d(1.1, 1)).Length < ErrorMargin) && intersects[1].First - 0.7 < ErrorMargin
                );
        }

        [TestMethod]
        public void LineCircleIntersectTest5()
        {
            LineF line = new LineF(new Vector2(-100f, 1f), new Vector2(20f, 100f));
            IntersectCoord[] intersections = MathEx.LineCircleIntersect(new Vector2(1, 1), 1f, line, true);
            Assert.IsTrue(intersections.Length == 0);
        }

        [TestMethod]
        public void LineCircleIntersectTest6()
        {
            LineF line = new LineF(new Vector2(-100f, 1f), new Vector2(20f, 100f));
            IntersectCoord[] intersections = MathEx.LineCircleIntersect(new Vector2(1, 1), 1000f, line, true);
            Assert.IsTrue(intersections.Length == 0);
        }
        #endregion
        #region GetHomography tests

        Vector2[] GetSquare()
        {
            return new[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
        }

        Camera2 GetCamera()
        {
            Camera2 camera = new Camera2(new Scene(), new Transform2(), 1.3333f);
            camera.WorldTransform = PortalCommon.GetWorldTransform(camera);
            return camera;
        }

        [TestMethod]
        public void GetHomographyTest0()
        {
            Vector2[] source = GetSquare();
            Vector2[] destination = GetSquare();
            Matrix4d homography = MathEx.GetHomography(source, destination);
            Vector3[] vectors = {
                new Vector3(source[0]),
                new Vector3(source[1]),
                new Vector3(source[2]),
                new Vector3(source[3])
            };
            vectors = Vector3Ex.Transform(vectors, homography);

            ICamera2 camera = GetCamera();
            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i].Z = camera.UnitZToWorld(vectors[i].Z);
            }

            for (int i = 0; i < vectors.Length; i++)
            {
                Vector2 offset = camera.GetOverlapOffset(vectors[i], new Vector3(destination[i]));
                Assert.IsTrue(offset.Length < 0.001f);
            }
        }

        [TestMethod]
        public void GetHomographyTest1()
        {
            Vector2[] source = GetSquare();
            Vector2[] destination = {
                new Vector2(0, 0),
                new Vector2(1, -1),
                new Vector2(1, 2),
                new Vector2(0, 1)
            };
            Matrix4d homography = MathEx.GetHomography(source, destination);
            Vector3[] vectors = {
                new Vector3(source[0]),
                new Vector3(source[1]),
                new Vector3(source[2]),
                new Vector3(source[3])
            };
            vectors = Vector3Ex.Transform(vectors, homography);

            Camera2 camera = GetCamera();
            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i].Z = camera.UnitZToWorld(vectors[i].Z);
            }

            for (int i = 0; i < vectors.Length; i++)
            {
                Vector2 offset = camera.GetOverlapOffset(vectors[i], new Vector3(destination[i]));
                Assert.IsTrue(offset.Length < 0.001f);
            }
        }

        /// <summary>
        /// Correctly throw ExceptionInvalidPolygon if source and destination quads aren't both convex or concave.
        /// </summary>
        [TestMethod]
        public void GetHomographyTest2()
        {
            Vector2[] source = {
                new Vector2(12, 50),
                new Vector2(11, 0.01f),
                new Vector2(-15, 11),
                new Vector2(0, 100.5f)
            };
            Vector2[] destination = {
                new Vector2(10, -10),
                new Vector2(41, -41),
                new Vector2(0, 0),
                new Vector2(0, 24)
            };
            try
            {
                MathEx.GetHomography(source, destination);
            }
            catch (ExceptionInvalidPolygon)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void GetHomographyTest3()
        {
            Vector2[] source = {
                new Vector2(15, -10),
                new Vector2(10, 100),
                new Vector2(-50, 95),
                new Vector2(-50, -40)
            };
            Vector2[] destination = {
                new Vector2(0, 0),
                new Vector2(1, -1),
                new Vector2(1, 2),
                new Vector2(0, 1)
            };
            Matrix4d homography = MathEx.GetHomography(source, destination);
            Vector3[] vectors = {
                new Vector3(source[0]),
                new Vector3(source[1]),
                new Vector3(source[2]),
                new Vector3(source[3])
            };
            vectors = Vector3Ex.Transform(vectors, homography);

            Camera2 camera = GetCamera();
            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i].Z = camera.UnitZToWorld(vectors[i].Z);
            }

            for (int i = 0; i < vectors.Length; i++)
            {
                Vector2 offset = camera.GetOverlapOffset(vectors[i], new Vector3(destination[i]));
                Assert.IsTrue(offset.Length < 0.001f);
            }
        }
        #endregion
        #region BisectTriangle tests

        Triangle GetDefaultTriangle()
        {
            return new Triangle(new Vertex(0, 0), new Vertex(1, 0), new Vertex(0, 1));
        }

        Triangle GetDefaultComparison()
        {
            return new Triangle(new Vertex(0.5f, 0.5f), new Vertex(0, 1), new Vertex(0, 0.5f));
        }

        LineF GetDefaultBisector()
        {
            return new LineF(new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
        }

        [TestMethod]
        public void BisectTriangleTest0()
        {
            Triangle triangle = GetDefaultTriangle();
            Triangle[] result = triangle.Bisect(GetDefaultBisector());

            Assert.IsTrue(result.Length == 1);
            Triangle comparison = GetDefaultComparison();
            Assert.IsTrue(Triangle.IsIsomorphic(result[0], comparison));
        }

        [TestMethod]
        public void BisectTriangleTest1()
        {
            Triangle triangle = GetDefaultTriangle().Reverse();
            Triangle[] result = triangle.Bisect(GetDefaultBisector());

            Assert.IsTrue(result.Length == 1);
            Triangle comparison = GetDefaultComparison().Reverse();
            Assert.IsTrue(Triangle.IsIsomorphic(result[0], comparison));
        }

        [TestMethod]
        public void BisectTriangleTest2()
        {
            Triangle triangle = GetDefaultTriangle();
            Triangle[] result = triangle.Bisect(GetDefaultBisector());

            Assert.IsTrue(result.Length == 1);
            Triangle comparison = GetDefaultComparison();
            Assert.IsTrue(Triangle.IsIsomorphic(result[0], comparison));
        }

        [TestMethod]
        public void BisectTriangleTest3()
        {
            Triangle triangle = GetDefaultTriangle();
            Triangle[] result = triangle.Bisect(GetDefaultBisector(), Side.Right);

            Assert.IsTrue(result.Length == 2);
            Assert.IsTrue(Triangle.IsIsomorphic(result[0], new Triangle(new Vertex(0, 0), new Vertex(1, 0), new Vertex(0.5f, 0.5f))));
            Assert.IsTrue(Triangle.IsIsomorphic(result[1], new Triangle(new Vertex(0, 0), new Vertex(0.5f, 0.5f), new Vertex(0, 0.5f))));
        }

        [TestMethod]
        public void BisectTriangleTest4()
        {
            Triangle triangle = GetDefaultTriangle();
            Triangle[] result = triangle.Bisect(new LineF(new Vector2(-1, 1), new Vector2(1, 1)));

            Assert.IsTrue(result.Length == 0);
        }

        [TestMethod]
        public void BisectTriangleTest5()
        {
            Triangle triangle = GetDefaultTriangle();
            Triangle[] result = triangle.Bisect(new LineF(new Vector2(1, 1), new Vector2(-1, 1)), Side.Right);

            Assert.IsTrue(result.Length == 0);
        }

        [TestMethod]
        public void BisectTriangleTest6()
        {
            Triangle triangle = GetDefaultTriangle();
            Triangle[] result = triangle.Bisect(new LineF(new Vector2(-1, 1), new Vector2(1, 1)), Side.Right);

            Assert.IsTrue(result.Length == 1);
            Assert.IsTrue(result[0].Equals(triangle));
        }

        [TestMethod]
        public void BisectTriangleTest7()
        {
            Triangle triangle = GetDefaultTriangle();
            Triangle[] result = triangle.Bisect(new LineF(new Vector2(1, 1), new Vector2(-1, 1)));

            Assert.IsTrue(result.Length == 1);
            Assert.IsTrue(result[0].Equals(triangle));
        }

        [TestMethod]
        public void BisectTriangleTest8()
        {
            Triangle triangle = new Triangle(new Vertex(0, 0, 5f), new Vertex(1, 0), new Vertex(0, 1, 2f));
            Triangle[] result = triangle.Bisect(new LineF(new Vector2(-1, 0.6f), new Vector2(1, 0.6f)));

            Assert.IsTrue(result.Length == 1);
            Triangle comparison = new Triangle(new Vertex(0.399999976f, 0.6f, 1.2f), new Vertex(0f, 1f, 2f), new Vertex(0f, 0.6f, 3.19999981f));
            Assert.IsTrue(Triangle.IsIsomorphic(result[0], comparison));
        }
        #endregion
        #region LinePolygonDistance tests
        [TestMethod]
        public void LinePolygonDistanceTest0()
        {
            LineF line = new LineF(new Vector2(), new Vector2(4, 0));
            Vector2[] polygon = PolygonFactory.CreateRectangle(2, 2);
            double distance = MathEx.LinePolygonDistance(line, polygon);
            Assert.IsTrue(distance == 0);
        }

        [TestMethod]
        public void LinePolygonDistanceTest1()
        {
            LineF line = new LineF(new Vector2(), new Vector2(0.5f, 0.5f));
            Vector2[] polygon = PolygonFactory.CreateRectangle(2, 2);
            double distance = MathEx.LinePolygonDistance(line, polygon);
            Assert.IsTrue(Math.Abs(distance + 0.5) <= ErrorMargin);
        }

        [TestMethod]
        public void LinePolygonDistanceTest2()
        {
            LineF line = new LineF(new Vector2(0f, 3f), new Vector2(3f, 0f));
            Vector2[] polygon = PolygonFactory.CreateRectangle(2, 2);
            double distance = MathEx.LinePolygonDistance(line, polygon);
            Assert.IsTrue(Math.Abs(distance - Math.Sqrt(0.5)) <= ErrorMargin);
        }
        #endregion
        #region MovingPointLineIntersect tests
        [TestMethod]
        public void MovingPointLineIntersectTest0()
        {
            LineF point = new LineF(Vector2.Zero, Vector2.Zero);
            LineF lineStart = new LineF(new Vector2(-1, -1), new Vector2(1, -1));
            LineF lineEnd = new LineF(new Vector2(-1, 1), new Vector2(1, 1));
            var result = MathEx.MovingPointLineIntersect(point, lineStart, lineEnd);
            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(result[0].AcrossProportion, 0.5, 0.000001);
            Assert.AreEqual(result[0].TimeProportion, 0.5, 0.000001);
        }

        [TestMethod]
        public void MovingPointLineIntersectTest1()
        {
            LineF point = new LineF(Vector2.Zero, new Vector2(3, 0));
            LineF lineStart = new LineF(new Vector2(-1, -1), new Vector2(1, -1));
            LineF lineEnd = new LineF(new Vector2(-1, 1), new Vector2(1, 1));
            var result = MathEx.MovingPointLineIntersect(point, lineStart, lineEnd);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void MovingPointLineIntersectTest2()
        {
            LineF point = new LineF(Vector2.Zero, new Vector2(-3, 0));
            LineF lineStart = new LineF(new Vector2(-1, -1), new Vector2(1, -1));
            LineF lineEnd = new LineF(new Vector2(-1, 1), new Vector2(1, 1));
            var result = MathEx.MovingPointLineIntersect(point, lineStart, lineEnd);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void MovingPointLineIntersectTest3()
        {
            LineF point = new LineF(Vector2.Zero, new Vector2(0, 1.5f));
            LineF lineStart = new LineF(new Vector2(-1, -1), new Vector2(1, -1));
            LineF lineEnd = new LineF(new Vector2(-1, 1), new Vector2(1, 1));
            var result = MathEx.MovingPointLineIntersect(point, lineStart, lineEnd);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void MovingPointLineIntersectTest4()
        {
            LineF point = new LineF(Vector2.Zero, new Vector2(10f, 0));
            LineF lineStart = new LineF(new Vector2(5, -1), new Vector2(5, 9));
            LineF lineEnd = new LineF(new Vector2(5, -1), new Vector2(5, 9));
            var result = MathEx.MovingPointLineIntersect(point, lineStart, lineEnd);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0.5f, result[0].TimeProportion, 0.00001);
            Assert.AreEqual(0.1f, result[0].AcrossProportion, 0.00001);
        }

        [TestMethod]
        public void MovingPointLineIntersectTest5()
        {
            LineF point = new LineF(new Vector2(-1.965838f, 0.5397364f), new Vector2(-1.965838f, 0.4839308f));
            //point = point.Reverse();
            LineF lineStart = new LineF(new Vector2(-2.529442f, 0.5394384f), new Vector2(-1.530722f, 0.4888392f));
            LineF lineEnd = new LineF(new Vector2(-2.529442f, 0.5394384f), new Vector2(-1.530722f, 0.4888392f));
            var result = MathEx.MovingPointLineIntersect(point, lineStart, lineEnd);
            Assert.AreEqual(1, result.Count);
            /*Assert.AreEqual(0.5f, result[0].TimeProportion, 0.00001);
            Assert.AreEqual(0.1f, result[0].AcrossProportion, 0.00001);*/
        }
        #endregion

        [TestMethod]
        public void AngleToVectorTest0()
        {
            var result = MathEx.VectorToAngle(new Vector2d(1, 0));
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void AngleToVectorTest1()
        {
            var result = MathEx.VectorToAngle(new Vector2d(0, -1));
            Assert.AreEqual(Math.PI / 2, result, 0.0000001);
        }

        [TestMethod]
        public void VectorToAngleTest0()
        {
            var result = MathEx.AngleToVector(0.0);
            Assert.IsTrue((new Vector2d(1, 0) - result).Length < 0.00001);
        }

        [TestMethod]
        public void VectorToAngleTest1()
        {
            var result = MathEx.AngleToVector(Math.PI / 2);
            Assert.IsTrue((new Vector2d(0, -1) - result).Length < 0.00001);
        }

        [TestMethod]
        public void RoundTest0()
        {
            var result = (int)MathEx.Round(100001, 10);
            Assert.AreEqual(100000, result);
        }
    }
}
