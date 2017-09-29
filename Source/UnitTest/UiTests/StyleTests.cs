using NUnit.Framework;
using OpenTK.Graphics;
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

        [Test]
        public void GenericElementStyle()
        {
            var expected = new Color4(0.5f, 0.5f, 0.5f, 1f);
            var style = new Style
            {
                new StyleElement(typeof(Radio<>), nameof(Radio<object>.SelectedColor), _ => expected)
            };

            var radio = new Radio<int>(style: style);

            var result = radio.SelectedColor;
            Assert.AreEqual(expected, result);
        }
    }
}
