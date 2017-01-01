using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using Game.Models;

namespace UnitTest
{
    [TestClass]
    public class TriangleTests
    {
        [TestMethod]
        public void EqualsTest0()
        {
            Triangle t0 = new Triangle(new Vertex(), new Vertex(), new Vertex());
            Triangle t1 = new Triangle(new Vertex(), new Vertex(), new Vertex());
            Assert.IsTrue(t0.Equals(t1));
            Assert.IsTrue(t1.Equals(t0));
            Assert.IsTrue(t0.GetHashCode() == t1.GetHashCode());
        }

        [TestMethod]
        public void EqualsTest1()
        {
            Random random = new Random();
            Triangle[] triangles = GetRandomTrianglePair(random);
            Triangle triangleRotated = new Triangle(triangles[0][1], triangles[0][2], triangles[0][1]);

            Assert.IsFalse(triangles[0].Equals(triangleRotated));
            Assert.IsFalse(triangleRotated.Equals(triangles[0]));
        }

        [TestMethod]
        public void EqualsTest2()
        {
            Random random = new Random();
            Triangle[] triangles = GetRandomTrianglePair(random);
            Triangle triangleModified = new Triangle(new Vertex(), triangles[0][2], triangles[0][1]);

            Assert.IsFalse(triangles[0].Equals(triangleModified));
            Assert.IsFalse(triangleModified.Equals(triangles[0]));
        }

        [TestMethod]
        public void EqualsTest3()
        {
            Random random = new Random();
            for (int i = 0; i < 100; i++)
            {
                Triangle[] triangles = GetRandomTrianglePair(random);
                Assert.IsTrue(triangles[0].GetHashCode() == triangles[1].GetHashCode());
                Assert.IsTrue(triangles[0].Equals(triangles[1]));
                Assert.IsTrue(triangles[1].Equals(triangles[0]));
            }
        }

        /// <summary>
        /// Randomly creates two triangles that are identical.
        /// </summary>
        /// <param name="random">Random number generator used to create triangles.</param>
        /// <returns>Array containing two triangles.</returns>
        public static Triangle[] GetRandomTrianglePair(Random random)
        {
            Vertex[][] vertices = new Vertex[][]
            {
                VertexTests.GetRandomVertexPair(random),
                VertexTests.GetRandomVertexPair(random),
                VertexTests.GetRandomVertexPair(random)
            };
            return new Triangle[]
            {
                new Triangle(vertices[0][0], vertices[1][0], vertices[2][0]),
                new Triangle(vertices[0][1], vertices[1][1], vertices[2][1])
            };
        }
    }
}
