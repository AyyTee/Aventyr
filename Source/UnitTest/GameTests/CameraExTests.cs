using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Game;
using Game.Common;
using Game.Rendering;
using OpenTK;
using System.Runtime.Serialization;

namespace GameTests
{
    [TestFixture]
    public class CameraExTests
    {
        [Test]
        public void ScreenToWorldTest0()
        {
            var canvasSize = new Vector2i(800, 600);

            SimpleCamera2 camera = new SimpleCamera2();

            Vector2 result;
            result = camera.ScreenToWorld(new Vector2(), canvasSize);
            Assert.IsTrue(result == new Vector2(-0.5f, 0.5f));

            result = camera.ScreenToWorld(new Vector2(400, 300), canvasSize);
            Assert.IsTrue(Vector2Ex.AlmostEqual(result, new Vector2(), 0.00001f));

            result = camera.ScreenToWorld(new Vector2(800, 600), canvasSize);
            Assert.IsTrue(Vector2Ex.AlmostEqual(result, new Vector2(0.5f, -0.5f), 0.00001f));
        }

        [Test]
        public void ScreenToWorldTest1()
        {
            SimpleCamera2 camera = new SimpleCamera2();

            Vector2 result = camera.ScreenToClip(new Vector2(), new Vector2i(800, 600));
            Assert.IsTrue(result == new Vector2(-1f, 1f));
        }

        [Test]
        public void GetWorldVertsTest0()
        {
            var camera = new SimpleCamera2();

            var result = camera.GetWorldVerts();
            var expected = new[] {
                new Vector2(-0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, -0.5f),
                new Vector2(-0.5f, -0.5f),
            };
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetWorldVertsTest1()
        {
            var offset = new Vector2(5, 1);
            var camera = new SimpleCamera2 { WorldTransform = new Transform2(offset, 0, 1, true) };

            var result = camera.GetWorldVerts();
            var expected = new[] {
                new Vector2(0.5f, 0.5f),
                new Vector2(-0.5f, 0.5f),
                new Vector2(-0.5f, -0.5f),
                new Vector2(0.5f, -0.5f),
            }.Select(v => v + offset);
            Assert.AreEqual(expected, result);
        }

        [Test]
        [Ignore("GetViewMatrix currently doesn't support world space near/far planes.")]
        public void ClipPlaneTest0()
        {
            var camera = new SimpleCamera2();
            camera.WorldTransform = camera.WorldTransform.WithSize(20);

            var viewMatrix = camera.GetViewMatrix(false);

            var z = camera.GetWorldZ();
            var worldZNear = Math.Min(z - 0.1, 10);
            var worldZFar = -10.0;

            var mInverse = viewMatrix.Inverted();
            var cube = new[] {
                new Vector3(1, 1, 1),
                new Vector3(-1, 1, 1),
                new Vector3(1, -1, 1),
                new Vector3(-1, -1, 1),
                new Vector3(1, 1, -1),
                new Vector3(-1, 1, -1),
                new Vector3(1, -1, -1),
                new Vector3(-1, -1, -1),
            }.Select(item => Vector3Ex.Transform(item, mInverse)).ToArray();

            var maxError = 0.001;
            Assert.IsTrue(cube.Take(4).All(item => Math.Abs(item.Z - worldZFar) < maxError));
            Assert.IsTrue(cube.Skip(4).All(item => Math.Abs(item.Z - worldZNear) < maxError));
        }
    }
}
