using Game;
using GameTests;
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
    public class DataTemplateTests
    {
        [Test]
        public void TemplateElementsAppearCorrectly()
        {
            var (font, _) = FontTests.GetFont();
            var stackFrame = new StackFrame()
            {
                new DataTemplate<string, Element>(
                    () => new OrderedSet<string> { "a", "ab", "abc" }, 
                    text => new TextBlock(text: _ => text, font: _ => font, maxWidth: _ => null, textAlignment: _ => 0))
            };

            Assert.AreEqual(3, stackFrame.Count());
            Assert.IsTrue(stackFrame.All(item => item is TextBlock));
            Assert.IsTrue(stackFrame.All(item => item.Width > 0 && item.Height > 0));
        }
    }
}
