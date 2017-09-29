using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui;
using Ui.Elements;

namespace UiTests
{
    [TestFixture]
    public class StyleTests
    {
        [Test]
        public void GetParentXFunc()
        {
            var style = new Style
            {
                new StyleElement<Element, float>(nameof(Element.X), _ => 10)
            };

            Frame child;
            new Frame(style: style)
            {
                (child = new Frame())
            };

            Assert.AreEqual(10, child.X);
        }

        [Test]
        public void OverrideXFunc()
        {
            var style = new Style
            {
                new StyleElement<Element, float>(nameof(Element.X), _ => 10)
            };

            Frame child;
            new Frame(style: style)
            {
                (child = new Frame(_ => 15))
            };

            Assert.AreEqual(15, child.X);
        }

        [Test]
        public void ToImmutable()
        {
            var style = new Style
            {
                new StyleElement<Element, float>(nameof(Element.X), _ => 0)
            };

            var result = style.ToImmutable().Count;
            Assert.AreEqual(1, result);
        }
    }
}
