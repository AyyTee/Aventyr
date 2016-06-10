using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UnitTest
{
    [TestClass]
    public class MathExtTests
    {
        const double ErrorMargin = 0.0000001;

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

            listOut = MathExt.GetConvexHull(listIn);

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
            listIn.AddRange(new Vector2[4] {
                new Vector2(-5, -5),
                new Vector2(5, -5),
                new Vector2(5, 5),
                new Vector2(-5, 5),
            });
            List<Vector2> listOut;

            listOut = MathExt.GetConvexHull(listIn);

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

            listOut = MathExt.GetConvexHull(listIn);

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

            listOut = MathExt.GetConvexHull(listIn);

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
        #region SetWinding tests
        [TestMethod]
        public void SetWindingTest0()
        {
            Vector2[] v = new Vector2[] {
                new Vector2(),
                new Vector2(0, 1),
                new Vector2(1, 0)
            };
            Debug.Assert(MathExt.IsClockwise(v));
            Vector2[] result = MathExt.SetWinding(v, false);
            Vector2[] expected = new Vector2[]
            {
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2()
            };

            Assert.IsTrue(Enumerable.SequenceEqual(result, expected));
        }
        [TestMethod]
        public void SetWindingTest1()
        {
            List<Vector2> v = new List<Vector2>();
            v.Add(new Vector2());
            v.Add(new Vector2(0, 1));
            v.Add(new Vector2(1, 0));
            
            Debug.Assert(MathExt.IsClockwise(v));
            List<Vector2> result = MathExt.SetWinding(v, false);
            Vector2[] expected = new Vector2[]
            {
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2()
            };

            Assert.IsTrue(Enumerable.SequenceEqual(result, expected));
        }
        #endregion
        #region LineCircleIntersect tests
        [TestMethod]
        public void LineCircleIntersectTest0()
        {
            //test degernate line outside of circle
            Line line = new Line(new Vector2(1, 0), new Vector2(1, 0));
            Assert.IsTrue(MathExt.LineCircleIntersect(new Vector2(1, 0), 0.1f, line, true).Length == 0);
        }
        [TestMethod]
        public void LineCircleIntersectTest1()
        {
            //test degenerate line inside of circle
            Line line = new Line(new Vector2(1, 0), new Vector2(1f, 0));
            Assert.IsTrue(MathExt.LineCircleIntersect(new Vector2(1, 0), 0.1f, line, true).Length == 0);
        }
        [TestMethod]
        public void LineCircleIntersectTest2()
        {
            //test degernate line outside of circle
            Line line = new Line(new Vector2(1, 0), new Vector2(1, 0));
            Assert.IsTrue(MathExt.LineCircleIntersect(new Vector2(1, 0), 0.1f, line, false).Length == 0);
        }
        [TestMethod]
        public void LineCircleIntersectTest3()
        {
            //test degenerate line inside of circle
            Line line = new Line(new Vector2(1, 0), new Vector2(1f, 0));
            Assert.IsTrue(MathExt.LineCircleIntersect(new Vector2(1, 0), 0.1f, line, false).Length == 0);
        }
        [TestMethod]
        public void LineCircleIntersectTest4()
        {
            Line line = new Line(new Vector2(-1f, 1f), new Vector2(2f, 1f));
            IntersectCoord[] intersects = MathExt.LineCircleIntersect(new Vector2(1, 1), 0.1f, line, true);
            Assert.IsTrue(intersects[0].Exists == true && intersects[1].Exists == true);
            Assert.IsTrue(
                ((intersects[0].Position - new Vector2d(1.1, 1)).Length < ErrorMargin && intersects[0].TFirst - 0.7 < ErrorMargin &&
                (intersects[1].Position - new Vector2d(0.9, 1)).Length < ErrorMargin) && intersects[1].TFirst - 0.63333333333 < ErrorMargin ||
                ((intersects[0].Position - new Vector2d(0.9, 1)).Length < ErrorMargin && intersects[0].TFirst - 0.63333333333 < ErrorMargin &&
                (intersects[1].Position - new Vector2d(1.1, 1)).Length < ErrorMargin) && intersects[1].TFirst - 0.7 < ErrorMargin
                );
        }

        [TestMethod]
        public void LineCircleIntersectTest5()
        {
            Line line = new Line(new Vector2(-100f, 1f), new Vector2(20f, 100f));
            IntersectCoord[] intersections = MathExt.LineCircleIntersect(new Vector2(1, 1), 1f, line, true);
            Assert.IsTrue(intersections.Length == 0);
        }

        [TestMethod]
        public void LineCircleIntersectTest6()
        {
            Line line = new Line(new Vector2(-100f, 1f), new Vector2(20f, 100f));
            IntersectCoord[] intersections = MathExt.LineCircleIntersect(new Vector2(1, 1), 1000f, line, true);
            Assert.IsTrue(intersections.Length == 0);
        }
        #endregion
        #region GetHomography tests
        public Vector2[] GetSquare()
        {
            return new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
        }

        public Camera2 GetCamera()
        {
            return new Camera2(new Scene(), new Transform2(), 1.3333f);
        }

        [TestMethod]
        public void GetHomographyTest0()
        {
            Vector2[] source = GetSquare();
            Vector2[] destination = GetSquare();
            Matrix4d homography = MathExt.GetHomography(source, destination);
            Vector3[] vectors = new Vector3[] {
                new Vector3(source[0]),
                new Vector3(source[1]),
                new Vector3(source[2]),
                new Vector3(source[3])
            };
            vectors = Vector3Ext.Transform(vectors, homography);

            ICamera2 camera = GetCamera();
            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i].Z = CameraExt.UnitZToWorld(camera, vectors[i].Z);
            }

            for (int i = 0; i < vectors.Length; i++)
            {
                Vector2 offset = CameraExt.GetOverlapOffset(camera, vectors[i], new Vector3(destination[i]));
                Assert.IsTrue(offset.Length < 0.001f);
            }
        }

        [TestMethod]
        public void GetHomographyTest1()
        {
            Vector2[] source = GetSquare();
            Vector2[] destination = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, -1),
                new Vector2(1, 2),
                new Vector2(0, 1)
            };
            Matrix4d homography = MathExt.GetHomography(source, destination);
            Vector3[] vectors = new Vector3[] {
                new Vector3(source[0]),
                new Vector3(source[1]),
                new Vector3(source[2]),
                new Vector3(source[3])
            };
            vectors = Vector3Ext.Transform(vectors, homography);

            Camera2 camera = GetCamera();
            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i].Z = CameraExt.UnitZToWorld(camera, vectors[i].Z);
            }

            for (int i = 0; i < vectors.Length; i++)
            {
                Vector2 offset = CameraExt.GetOverlapOffset(camera, vectors[i], new Vector3(destination[i]));
                Assert.IsTrue(offset.Length < 0.001f);
            }
        }

        /// <summary>
        /// Correctly throw ExceptionInvalidPolygon if source and destination quads aren't both convex or concave.
        /// </summary>
        [TestMethod]
        public void GetHomographyTest2()
        {
            Vector2[] source = new Vector2[] {
                new Vector2(12, 50),
                new Vector2(11, 0.01f),
                new Vector2(-15, 11),
                new Vector2(0, 100.5f)
            };
            Vector2[] destination = new Vector2[] {
                new Vector2(10, -10),
                new Vector2(41, -41),
                new Vector2(0, 0),
                new Vector2(0, 24)
            };
            try
            {
                Matrix4d homography = MathExt.GetHomography(source, destination);
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
            Vector2[] source = new Vector2[] {
                new Vector2(15, -10),
                new Vector2(10, 100),
                new Vector2(-50, 95),
                new Vector2(-50, -40)
            };
            Vector2[] destination = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, -1),
                new Vector2(1, 2),
                new Vector2(0, 1)
            };
            Matrix4d homography = MathExt.GetHomography(source, destination);
            Vector3[] vectors = new Vector3[] {
                new Vector3(source[0]),
                new Vector3(source[1]),
                new Vector3(source[2]),
                new Vector3(source[3])
            };
            vectors = Vector3Ext.Transform(vectors, homography);

            Camera2 camera = GetCamera();
            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i].Z = CameraExt.UnitZToWorld(camera, vectors[i].Z);
            }

            for (int i = 0; i < vectors.Length; i++)
            {
                Vector2 offset = CameraExt.GetOverlapOffset(camera, vectors[i], new Vector3(destination[i]));
                Assert.IsTrue(offset.Length < 0.001f);
            }
        }
        #endregion
        #region BisectTriangle tests
        private Triangle GetDefaultTriangle()
        {
            return new Triangle(new Vertex(0, 0), new Vertex(1, 0), new Vertex(0, 1));
        }

        private Triangle GetDefaultComparison()
        {
            return new Triangle(new Vertex(0.5f, 0.5f), new Vertex(0, 1), new Vertex(0, 0.5f));
        }

        private Line GetDefaultBisector()
        {
            return new Line(new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
        }
        
        [TestMethod]
        public void BisectTriangleTest0()
        {
            Triangle triangle = GetDefaultTriangle();
            Triangle[] result = MathExt.BisectTriangle(triangle, GetDefaultBisector());

            Assert.IsTrue(result.Length == 1);
            Triangle comparison = GetDefaultComparison();
            Assert.IsTrue(Triangle.IsIsomorphic(result[0], comparison));
        }

        [TestMethod]
        public void BisectTriangleTest1()
        {
            Triangle triangle = GetDefaultTriangle().Reverse();
            Triangle[] result = MathExt.BisectTriangle(triangle, GetDefaultBisector());

            Assert.IsTrue(result.Length == 1);
            Triangle comparison = GetDefaultComparison().Reverse();
            Assert.IsTrue(Triangle.IsIsomorphic(result[0], comparison));
        }

        [TestMethod]
        public void BisectTriangleTest2()
        {
            Triangle triangle = GetDefaultTriangle();
            Triangle[] result = MathExt.BisectTriangle(triangle, GetDefaultBisector(), Side.Left);

            Assert.IsTrue(result.Length == 1);
            Triangle comparison = GetDefaultComparison();
            Assert.IsTrue(Triangle.IsIsomorphic(result[0], comparison));
        }

        [TestMethod]
        public void BisectTriangleTest3()
        {
            Triangle triangle = GetDefaultTriangle();
            Triangle[] result = MathExt.BisectTriangle(triangle, GetDefaultBisector(), Side.Right);

            Assert.IsTrue(result.Length == 2);
            Assert.IsTrue(Triangle.IsIsomorphic(result[0], new Triangle(new Vertex(0, 0), new Vertex(1, 0), new Vertex(0.5f, 0.5f))));
            Assert.IsTrue(Triangle.IsIsomorphic(result[1], new Triangle(new Vertex(0, 0), new Vertex(0.5f, 0.5f), new Vertex(0, 0.5f))));
        }

        [TestMethod]
        public void BisectTriangleTest4()
        {
            Triangle triangle = GetDefaultTriangle();
            Triangle[] result = MathExt.BisectTriangle(triangle, new Line(new Vector2(-1, 1), new Vector2(1, 1)));

            Assert.IsTrue(result.Length == 0);
        }

        [TestMethod]
        public void BisectTriangleTest5()
        {
            Triangle triangle = GetDefaultTriangle();
            Triangle[] result = MathExt.BisectTriangle(triangle, new Line(new Vector2(1, 1), new Vector2(-1, 1)), Side.Right);

            Assert.IsTrue(result.Length == 0);
        }

        [TestMethod]
        public void BisectTriangleTest6()
        {
            Triangle triangle = GetDefaultTriangle();
            Triangle[] result = MathExt.BisectTriangle(triangle, new Line(new Vector2(-1, 1), new Vector2(1, 1)), Side.Right);

            Assert.IsTrue(result.Length == 1);
            Assert.IsTrue(result[0].Equals(triangle));
        }

        [TestMethod]
        public void BisectTriangleTest7()
        {
            Triangle triangle = GetDefaultTriangle();
            Triangle[] result = MathExt.BisectTriangle(triangle, new Line(new Vector2(1, 1), new Vector2(-1, 1)));

            Assert.IsTrue(result.Length == 1);
            Assert.IsTrue(result[0].Equals(triangle));
        }

        [TestMethod]
        public void BisectTriangleTest8()
        {
            Triangle triangle = new Triangle(new Vertex(0, 0, 5f), new Vertex(1, 0, 0), new Vertex(0, 1, 2f));
            Triangle[] result = MathExt.BisectTriangle(triangle, new Line(new Vector2(-1, 0.6f), new Vector2(1, 0.6f)));

            Assert.IsTrue(result.Length == 1);
            Triangle comparison = new Triangle(new Vertex(0.399999976f, 0.6f, 1.2f), new Vertex(0f, 1f, 2f), new Vertex(0f, 0.6f, 3.19999981f));
            Assert.IsTrue(Triangle.IsIsomorphic(result[0], comparison));
        }
        #endregion
        #region BisectMesh tests
        [TestMethod]
        public void BisectMestTest0()
        {
            Model m = ModelFactory.CreatePlane();
            Line bisector = new Line(new Vector2(0.4f, -1f), new Vector2(0.4f, 0));
            Mesh mesh = MathExt.BisectMesh(m.Mesh, bisector);
        }
        #endregion
        #region LinePolygonDistance tests
        [TestMethod]
        public void LinePolygonDistanceTest0()
        {
            Line line = new Line(new Vector2(), new Vector2(4, 0));
            Vector2[] polygon = PolygonFactory.CreateRectangle(2, 2);
            double distance = MathExt.LinePolygonDistance(line, polygon);
            Assert.IsTrue(distance == 0);
        }

        [TestMethod]
        public void LinePolygonDistanceTest1()
        {
            Line line = new Line(new Vector2(), new Vector2(0.5f, 0.5f));
            Vector2[] polygon = PolygonFactory.CreateRectangle(2, 2);
            double distance = MathExt.LinePolygonDistance(line, polygon);
            Assert.IsTrue(Math.Abs(distance + 0.5) <= ErrorMargin);
        }

        [TestMethod]
        public void LinePolygonDistanceTest2()
        {
            Line line = new Line(new Vector2(0f, 3f), new Vector2(3f, 0f));
            Vector2[] polygon = PolygonFactory.CreateRectangle(2, 2);
            double distance = MathExt.LinePolygonDistance(line, polygon);
            Assert.IsTrue(Math.Abs(distance - Math.Sqrt(0.5)) <= ErrorMargin);
        }
        #endregion
    }
}
