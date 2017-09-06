using System;
using Game.Common;
using NUnit.Framework;
using TimeLoopInc;
using TimeLoopInc.Editor;
using System.Linq;
using System.Collections.Immutable;

namespace TimeLoopIncTests
{
    [TestFixture]
    public class LinkToolTests
    {
        [Test]
        public void LinkPortalsTest0()
        {
            var portal0 = new PortalBuilder(new Vector2i(), GridAngle.Left);
            var portal1 = new PortalBuilder(new Vector2i(2, 0), GridAngle.Left);
            var links = new[] { new PortalLink(new[] { portal0, portal1 }) };

            var result = LinkTool.LinkPortals(portal0, portal1, links);
            Assert.AreEqual(links, result);
        }

        [Test]
        public void LinkPortalsTest1()
        {
            var portal0 = new PortalBuilder(new Vector2i(), GridAngle.Left);
            var portal1 = new PortalBuilder(new Vector2i(2, 0), GridAngle.Left);
            var portal2 = new PortalBuilder(new Vector2i(3, 1), GridAngle.Left);
            var links = new[]
            {
                new PortalLink(new[] { portal0, portal1 }),
                new PortalLink(new[] { portal2 })
            };

            var result = LinkTool.LinkPortals(portal2, portal1, links);

            var expected = new[]
            {
                new PortalLink(new[] { portal0, }),
                new PortalLink(new[] { portal2, portal1 })
            };
            Assert.AreEqual(expected, result);
        }
    }
}
