using Game.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Elements;

namespace UiTests
{
    [TestFixture]
    public class GridTests
    {
        [Test]
        public void GetGridCellTest()
        {
            Frame frame00, frame10, frame01, frame11, frame21;
            var grid = new Grid(_ => new[] { 50f, 100f, 30f })
            {
                new GridRow(_ => 20f)
                {
                    (frame00 = new Frame()),
                    (frame10 = new Frame()),
                },
                new GridRow(_ => 30f)
                {
                    (frame01 = new Frame()),
                    (frame11 = new Frame()),
                    (frame21 = new Frame()),
                }
            };

            Assert.AreEqual(frame00, grid.GetCell(new Vector2i(0, 0)));
            Assert.AreEqual(frame10, grid.GetCell(new Vector2i(1, 0)));
            Assert.AreEqual(frame01, grid.GetCell(new Vector2i(0, 1)));
            Assert.AreEqual(frame11, grid.GetCell(new Vector2i(1, 1)));
            Assert.AreEqual(frame21, grid.GetCell(new Vector2i(2, 1)));

            try
            {
                grid.GetCell(new Vector2i(2, 0));
                Assert.Fail("Accessing a cell that doesn't exist should have thrown an exception.");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        [Test]
        public void CellLayoutTest()
        {
            Frame frame;
            var grid = new Grid(_ => new[] { 50f, 100f, 30f })
            {
                new GridRow(_ => 20f)
                {
                    new Frame(),
                    new Frame(),
                },
                new GridRow(_ => 30f)
                {
                    new Frame(),
                    (frame = new Frame()),
                    new Frame(),
                }
            };

            Assert.AreEqual(100f, frame.Width);
            Assert.AreEqual(30f, frame.Height);
            Assert.AreEqual(50f, frame.X);
            Assert.AreEqual(20f, frame.Y);
        }
    }
}
