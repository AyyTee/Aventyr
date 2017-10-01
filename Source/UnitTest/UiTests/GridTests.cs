using Game;
using Game.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui;
using Ui.Elements;

namespace UiTests
{
    [TestFixture]
    public class GridTests
    {
        [Test]
        public void CellLayoutTest()
        {
            Frame frame;
            var grid = new Grid(
                columnWidths: _ => new[] { 50f, 100f, 30f }, 
                rowHeights: _ => new[] { 20f, 30f })
            {
                new Frame(),
                new Frame(),
                new Frame(),
                new Frame(),
                (frame = new Frame()),
                new Frame(),
            };

            Assert.AreEqual(100f, frame.Width);
            Assert.AreEqual(30f, frame.Height);
            Assert.AreEqual(50f, frame.X);
            Assert.AreEqual(20f, frame.Y);
        }

        [Test]
        public void GridDataTemplate()
        {
            var grid = new Grid()
            {
                new DataTemplate<int, Element>(() => new OrderedSet<int>(new[] { 0, 1, 2 }), data => new TextBlock(text: _ => data.ToString()))
            };

            Assert.AreEqual(3, grid.Count());
            for (int i = 0; i < grid.Count(); i++)
            {
                var textBlock = (TextBlock)grid.ElementAt(i);
                Assert.AreEqual(i.ToString(), textBlock.Text);
            }
        }
    }
}
