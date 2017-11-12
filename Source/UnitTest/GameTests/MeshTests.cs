using System;
using System.Linq;
using Game.Models;
using Game.Rendering;
using NUnit.Framework;
using OpenTK;

namespace GameTests
{
    [TestFixture]
    public class MeshTests
    {
        [Test]
        public void CombineTest0()
        {
            var mesh0 = ModelFactory.CreatePlaneMesh(new Vector2(), Vector2.One);
            var mesh1 = ModelFactory.CreatePlaneMesh(new Vector2(2, 2), new Vector2(3, 3));

            var offset = mesh0.GetVertices().Count;
            var result = IMeshEx.Combine(mesh0, mesh1).GetIndices();
            var expected = mesh0
                .GetIndices()
                .Concat(mesh1.GetIndices().Select(item => item + offset));

            Assert.IsTrue(expected.SequenceEqual(result));
        }

        [Test]
        public void CombineTest1()
        {
            var mesh0 = ModelFactory.CreatePlaneMesh(new Vector2(), Vector2.One);
            var mesh1 = ModelFactory.CreatePlaneMesh(new Vector2(2, 2), new Vector2(3, 3));

            var result = IMeshEx.Combine(mesh0, mesh1).GetVertices();
            var expected = mesh0
                .GetVertices()
                .Concat(mesh1.GetVertices());

            Assert.IsTrue(expected.SequenceEqual(result));
        }

        [Test]
        public void CombineTest2()
        {
            var mesh0 = ModelFactory.CreatePlaneMesh(new Vector2(), Vector2.One);
            var mesh1 = ModelFactory.CreatePlaneMesh(new Vector2(2, 2), new Vector2(3, 3));
            var mesh2 = ModelFactory.CreatePlaneMesh(new Vector2(4, 4), new Vector2(5, 5));

            var offset0 = mesh0.GetVertices().Count;
            var offset1 = offset0 + mesh1.GetVertices().Count;

            var result = IMeshEx.Combine(mesh0, mesh1, mesh2).GetIndices();
            var expected = mesh0
                .GetIndices()
                .Concat(mesh1.GetIndices().Select(item => item + offset0))
                .Concat(mesh2.GetIndices().Select(item => item + offset1));

            Assert.IsTrue(expected.SequenceEqual(result));
        }

        [Test]
        public void TriangleIntersectionsTest0()
        {
            var triangle = new[]
            {
                new Vertex(new Vector2()),
                new Vertex(new Vector2(1, 0)),
                new Vertex(new Vector2(0, 1))
            };

            var result = IMeshEx.TriangleIntersections(triangle, new Vector3(0, 0.5f, 0), new Vector3(0, 1, 0));

            var expected = new[] { (Vector3?)null, new Vector3(0.5f, 0.5f, 0), new Vector3(0, 0.5f, 0) };
            Assert.AreEqual(expected, result.Select(item => item?.Position));
        }

        /// <summary>
        /// Reversing triangle winding order reverses result.
        /// </summary>
        [Test]
        public void TriangleIntersectionsTest1()
        {
            var triangle = new[]
            {
                new Vertex(new Vector2()),
                new Vertex(new Vector2(0, 1)),
                new Vertex(new Vector2(1, 0))
            };

            var result = IMeshEx.TriangleIntersections(triangle, new Vector3(0, 0.5f, 0), new Vector3(0, 1, 0));

            var expected = new[] { new Vector3(0, 0.5f, 0), new Vector3(0.5f, 0.5f, 0), (Vector3?)null };
            Assert.AreEqual(expected, result.Select(item => item?.Position));
        }


        /// <summary>
        /// Intersections very close to a corner should snap to that corner.
        /// </summary>
        /// <param name="offset"></param>
        [TestCase(-0.00001f)]
        [TestCase(0f)]
        [TestCase(0.00001f)]
        public void TriangleIntersectionsTest2(float offset)
        {
            var triangle = new[]
            {
                new Vertex(new Vector2(-1, 0)),
                new Vertex(new Vector2(0, 1)),
                new Vertex(new Vector2(1, 0))
            };

            var result = IMeshEx.TriangleIntersections(triangle, new Vector3(offset, 0, 0), new Vector3(1, 0, 0));

            var expected = new Vector3(0, 1, 0);
            Assert.AreEqual(expected, result[0].Position);
        }

        /// <summary>
        /// Triangle is parallel with plane.
        /// </summary>
        [TestCase(-1, 0)]
        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(-1, 1000000)]
        [TestCase(0, 1000000)]
        [TestCase(1, 1000000)]
        public void TriangleIntersectionsTest3(float zOffset, float xOffset)
        {
            var triangle = new[]
            {
                new Vertex(new Vector2(-1, 0)),
                new Vertex(new Vector2(0, 1)),
                new Vertex(new Vector2(1, 0))
            };

            var result = IMeshEx.TriangleIntersections(triangle, new Vector3(xOffset, 0, zOffset), new Vector3(0, 0, 1));

            var expected = new Vertex[] { null, null, null };
            Assert.AreEqual(expected, result);
        }
    }
}
