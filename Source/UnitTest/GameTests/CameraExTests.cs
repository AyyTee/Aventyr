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

namespace GameTests
{
    [TestFixture]
    public class CameraExTests
    {
        /// <summary>
        /// Simple ICamera2 implementation for unit testing.
        /// </summary>
        class SimpleCamera2 : ICamera2
        {
            public float Aspect { get; set; } = 1;
            public double Fov { get; set; } = Math.PI / 4;
            public Vector2 ViewOffset { get; set; }
            public Transform2 WorldTransform { get; set; } = new Transform2();
            public Transform2 WorldVelocity { get; set; } = Transform2.CreateVelocity();
            public string Name { get; set; } = nameof(SimpleCamera2);
            public bool IsOrtho { get; set; } = true;
        }

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
