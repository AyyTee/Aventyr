using Game;
using GameTests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui;

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
                new DataTemplate<string>(
                    () => new OrderedSet<string> { "a", "ab", "abc" }, 
                    text => new TextBlock(font: _ => font, text: _ => text))
            };

            Assert.AreEqual(3, stackFrame.Count());
            Assert.IsTrue(stackFrame.All(item => item is TextBlock));
            Assert.IsTrue(stackFrame.All(item => item.Width > 0 && item.Height > 0));
        }
    }
}
