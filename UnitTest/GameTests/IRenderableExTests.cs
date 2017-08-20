using Game.Common;
using Game.Rendering;
using NUnit.Framework;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTests
{
    [TestFixture]
    public class IRenderableExTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void PixelAlignedWorldTransformTest0(bool pixelAlign)
        {
            var camera = new HudCamera2(new Vector2i(800, 600));
            var renderable = new Renderable() { PixelAlign = pixelAlign };

            var result = renderable.PixelAlignedWorldTransform(camera, camera.CanvasSize);
            Assert.AreEqual(renderable.WorldTransform, result);
        }

        [Test]
        public void PixelAlignedWorldTransformTest1()
        {
            var camera = new HudCamera2(new Vector2i(800, 600));
            var renderable = new Renderable(new Transform2(new Vector2(0.4f, -0.3f))) { PixelAlign = true };

            var result = renderable.PixelAlignedWorldTransform(camera, camera.CanvasSize);
            var expected = new Transform2();
            Assert.AreEqual(expected, result);
        }

        [TestCase(1.5f)]
        [TestCase(0f, 1.4f)]
        public void PixelAlignedWorldTransformTest2(float rotation, float size = 1)
        {
            var camera = new HudCamera2(new Vector2i(800, 600));
            var renderable = new Renderable(new Transform2(new Vector2(0.4f, -0.3f), rotation, size)) { PixelAlign = true };

            var result = renderable.PixelAlignedWorldTransform(camera, camera.CanvasSize);
            Assert.AreEqual(renderable.WorldTransform, result);
        }
    }
}
