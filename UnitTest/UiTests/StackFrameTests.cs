using Game;
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
    public class StackFrameTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void DetectStackOverflowTest0()
        {
            const float expected = 50;
            var stackFrame = new StackFrame(thickness: ElementEx.ChildWidth())
            {
                new Frame(width: _ => expected, height: _ => 50),
                new Frame(height: _ => 50)
            };

            var result = stackFrame.GetWidth();

            // If we make it this far then we haven't caused a stack overflow.

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void DetectStackOverflowTest1()
        {
            const float expected = 50;
            var stackFrame = new StackFrame(thickness: ElementEx.ChildHeight(), isVertical: false)
            {
                new Frame(width: _ => 50, height: _ => expected),
                new Frame(width: _ => 50)
            };

            var result = stackFrame.GetHeight();

            // If we make it this far then we haven't caused a stack overflow.

            Assert.AreEqual(expected, result);
        }
    }
}
