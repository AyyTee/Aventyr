using Game;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui
{
    [TestFixture]
    public class StackFrameTests
    {
        UiController controller;

        [SetUp]
        public void SetUp()
        {
            controller = new UiController(new FakeVirtualWindow());
        }

        [Test]
        public void DetectEndlessRecursion()
        {
            var stackFrame = new StackFrame(width: args => args.Self.MaxOrNull(item => item.GetWidth()) ?? 0)
            {
                new Frame(width: _ => 50, height: _ => 50),
                new Frame(height: _ => 50)
            };

            stackFrame.GetWidth();
        }
    }
}
