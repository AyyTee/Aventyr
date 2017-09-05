using Game;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Debug = System.Diagnostics.Debugger;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui;
using Ui.Elements;

namespace UiTests
{
    [TestFixture]
    public class DetectLoopTests
    {
        [Test]
        public void TryExecuteTest0()
        {
            var frame = new Frame(args => args.Self.First().X)
            {
                new Frame(args => args.Parent.First().X)
            };

            var result = DetectLoop.TryExecute(() => frame.X, out float value);

            Assert.AreEqual(false, result);
            Assert.AreEqual(0, value);
        }

        [Test]
        public void TryExecuteTest1()
        {
            var expectedValue = 50;
            var frame = new Frame(_ => expectedValue)
            {
                new Frame(args => args.Parent.First().X)
            };

            var result = DetectLoop.TryExecute(() => frame.X, out float value);

            Assert.AreEqual(true, result);
            Assert.AreEqual(expectedValue, value);
        }

        public Style GetStyle()
        {
            return new Style
            {
                new StyleElement<Element, float>(nameof(Element.X), _ => 0),
                new StyleElement<Element, float>(nameof(Element.Y), _ => 0f),
                new StyleElement<Element, float>(nameof(Element.Width), args => args.Parent.Width),
                new StyleElement<Element, float>(nameof(Element.Height), args => args.Parent.Height),
            };
        }

        [Test]
        public void StackFrameTest0()
        {
            Console.SetOut(TestContext.Progress);
            const float expected = 40;
            var stackFrame = new StackFrame(thickness: ElementEx.ChildrenMaxX(), style: GetStyle())
            {
                new Frame(width: _ => expected, height: _ => 50),
                new Frame(height: _ => 50)
            };

            var result = stackFrame.Width;

            // If we make it this far then we haven't caused a stack overflow.

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void StackFrameTest1()
        {
            const float expected = 40;
            var stackFrame = new StackFrame(_ => 0, thickness: ElementEx.ChildrenMaxY(), isVertical: false, style: GetStyle())
            {
                new Frame(_ => 0, width: _ => 50, height: _ => expected),
                new Frame(_ => 0, width: _ => 50)
            };

            var result = stackFrame.Height;

            // If we make it this far then we haven't caused a stack overflow.

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void StackFrameTest2()
        {
            //const float expected = 40;
            //var stackFrame = new StackFrame(thickness: ElementEx.ChildrenMaxY(), isVertical: false)
            //{
            //    new Frame(width: _ => 50, height: _ => expected),
            //    new Frame(width: _ => 50)
            //};

            //var result = stackFrame.Height;

            //// If we make it this far then we haven't caused a stack overflow.

            //Assert.AreEqual(expected, result);
        }

        [Explicit]
        [Test]
        public void DebuggerHandlesRecursiveLoops()
        {
            if (!Debug.IsAttached)
            {
                Assert.Inconclusive("Debugger must be attached for this test to be meaningful.");
            }

            var frame = new Frame(args => args.Self.First().X)
            {
                new Frame(args => args.Parent.First().X)
            };

            Debug.Break();

            // Inspecting the frame's x value should return a stack overflow but should not crash the debugger.
        }
    }
}
