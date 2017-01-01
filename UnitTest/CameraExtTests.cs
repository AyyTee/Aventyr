using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using Game.Common;
using Game.Rendering;
using OpenTK;

namespace UnitTest
{
    [TestClass]
    public class CameraExtTests
    {
        /// <summary>
        /// Simple ICamera2 implementation for unit testing.
        /// </summary>
        class SimpleCamera2 : ICamera2
        {
            public float Aspect { get; set; } = 1;
            public double Fov { get; set; } = Math.PI / 4;
            public Vector2 ViewOffset { get; set; }
            public float ZFar { get; set; } = 10000;
            public float ZNear { get; set; } = 0.1f;
            public Transform2 WorldTransform { get; set; } = new Transform2();
            public Transform2 WorldVelocity { get; set; } = Transform2.CreateVelocity();

            public Transform2 GetWorldTransform(bool ignorePortals = false)
            {
                return WorldTransform.ShallowClone();
            }

            public Transform2 GetWorldVelocity(bool ignorePortals = false)
            {
                return WorldVelocity.ShallowClone();
            }

            public void Remove()
            {
            }
        }

        [TestMethod]
        public void ScreenToWorldTest0()
        {
            var canvasSize = new Vector2(800, 600);

            SimpleCamera2 camera = new SimpleCamera2();

            Vector2 result;
            result = CameraExt.ScreenToWorld(camera, new Vector2(), canvasSize);
            Assert.IsTrue(result == new Vector2(-0.5f, 0.5f));

            result = CameraExt.ScreenToWorld(camera, new Vector2(400, 300), canvasSize);
            Assert.IsTrue(Vector2Ext.AlmostEqual(result, new Vector2(), 0.00001f));

            result = CameraExt.ScreenToWorld(camera, new Vector2(800, 600), canvasSize);
            Assert.IsTrue(Vector2Ext.AlmostEqual(result, new Vector2(0.5f, -0.5f), 0.00001f));
        }

        [TestMethod]
        public void ScreenToWorldTest1()
        {
            SimpleCamera2 camera = new SimpleCamera2();

            Vector2 result = CameraExt.ScreenToClip(camera, new Vector2(), new Vector2(800, 600));
            Assert.IsTrue(result == new Vector2(-1f, 1f));
        }
    }
}
