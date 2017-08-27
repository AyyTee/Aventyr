using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Elements;
using static Ui.ElementEx;
using Ui;
using Game;

namespace UiTests
{
    [TestFixture]
    public class MiscTests
    {
        [Test]
        [Ignore("Temporary test for fixing a bug.")]
        public void TypeOfTest()
        {
			var centerText = new Style
			{
				new StyleElement(typeof(TextBlock), nameof(TextBlock.X), args => AlignX(0.5f)(args)),
				new StyleElement(typeof(TextBlock), nameof(TextBlock.Y), args => AlignY(0.5f)(args))
			};

            var text = new[] { "test", "test test test", "blah blah" };

            var stack = new StackFrame(thickness: _ => 120, spacing: _ => 2, style: centerText)
            {
                new DataTemplate<string>(
                    () => new OrderedSet<string>(text),
                    data => new Button(
                        height: ChildrenMaxY())
                    {
                        new TextBlock(
                            text: _ => data,
                            maxWidth: args => (int)args.Parent.Width - 10,
                            textAlignment: _ => 0.5f)
                    })
            };

            var result = stack.Length;
        }
    }
}
