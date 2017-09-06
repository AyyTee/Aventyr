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
    }
}
