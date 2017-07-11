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
    }
}
