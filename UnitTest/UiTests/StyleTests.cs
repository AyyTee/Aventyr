using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui;

namespace UnitTest.UiTests
{
    [TestFixture]
    public class StyleTests
    {
        [Test]
        public void GetParentXFunc()
        {
            var style = new Dictionary<(Type, string), ElementFunc<object>>
            {
                { (typeof(Element), nameof(Element.X)), _ => 10f }
            }.ToImmutableDictionary();

            new Frame(style: style)
            {
                new Frame(out Frame child)
            };

            Assert.AreEqual(10, child.X);
        }

        [Test]
        public void OverrideXFunc()
        {
            var style = new Dictionary<(Type, string), ElementFunc<object>>
            {
                { (typeof(Element), nameof(Element.X)), _ => 10f }
            }.ToImmutableDictionary();

            new Frame(style: style)
            {
                new Frame(out Frame child, _ => 15)
            };

            Assert.AreEqual(15, child.X);
        }
    }
}
