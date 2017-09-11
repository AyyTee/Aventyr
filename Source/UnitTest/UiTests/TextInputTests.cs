using Game;
using NUnit.Framework;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui;

namespace UiTests
{
    [TestFixture]
    public class TextInputTests
    {
        FakeVirtualWindow _window;

        [SetUp]
        public void SetUp()
        {
            _window = new FakeVirtualWindow(Config.Resources);
        }

        void SetInput(IEnumerable<Key> keys = null, string keyString = "")
        {
            _window.Update(keyString, new HashSet<Key>(keys ?? new Key[0]), new HashSet<MouseButton>());
        }

        [Test]
        public void CursorMoveTest0()
        {
            var expected = new CursorText("test", 0);
            var result = TextInput.Update(_window, expected);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CursorMoveTest1()
        {
            SetInput(new[] { Key.Left });
            var expected = new CursorText("test", 0);
            var result = TextInput.Update(_window, expected);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CursorMoveTest2()
        {
            SetInput(new[] { Key.Right });
            var expected = new CursorText("test", 4);
            var result = TextInput.Update(_window, expected);
            Assert.AreEqual(expected, result);
        }

        [TestCase(0)]
        [TestCase(2)]
        [TestCase(3)]
        public void CursorMoveTest3(int cursorIndex)
        {
            SetInput(new[] { Key.Right });
            var expected = new CursorText("test", cursorIndex + 1);
            var result = TextInput.Update(_window, new CursorText("test", cursorIndex));
            Assert.AreEqual(expected, result);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        public void CursorMoveTest4(int cursorIndex)
        {
            SetInput(new[] { Key.Left });
            var expected = new CursorText("test", cursorIndex - 1);
            var result = TextInput.Update(_window, new CursorText("test", cursorIndex));
            Assert.AreEqual(expected, result);
        }

        public void CursorJumpTest0()
        {
            SetInput(new[] { Key.Left, Key.ControlLeft });
            var text = " test";
            var expected = new CursorText(text, 0);
            var result = TextInput.Update(_window, new CursorText(text, 1));
            Assert.AreEqual(expected, result);
        }

        [TestCase(2)]
        [TestCase(5)]
        public void CursorJumpTest1(int cursorIndex)
        {
            SetInput(new[] { Key.Left, Key.ControlLeft });
            var text = " test";
            var expected = new CursorText(text, 1);
            var result = TextInput.Update(_window, new CursorText(text, cursorIndex));
            Assert.AreEqual(expected, result);
        }

        public void CursorJumpTest2()
        {
            SetInput(new[] { Key.Right, Key.ControlLeft });
            var text = " test";
            var expected = new CursorText(text, 0);
            var result = TextInput.Update(_window, new CursorText(text, 1));
            Assert.AreEqual(expected, result);
        }

        [TestCase(1)]
        [TestCase(2)]
        public void CursorJumpTest3(int cursorIndex)
        {
            SetInput(new[] { Key.Right, Key.ControlLeft });
            var text = " test";
            var expected = new CursorText(text, text.Length);
            var result = TextInput.Update(_window, new CursorText(text, cursorIndex));
            Assert.AreEqual(expected, result);
        }

        [TestCase(1)]
        [TestCase(2)]
        public void CursorJumpTest5(int cursorIndex)
        {
            SetInput(new[] { Key.Right, Key.ControlLeft });
            var text = " test  ";
            var expected = new CursorText(text, text.Length);
            var result = TextInput.Update(_window, new CursorText(text, cursorIndex));
            Assert.AreEqual(expected, result);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        public void EnterTextTest0(int cursorIndex)
        {
            var text = "TEST";
            var newText = "insert";
            SetInput(keyString: newText);

            var result = TextInput.Update(_window, new CursorText(text, cursorIndex));
            var expected = new CursorText(text.Insert(cursorIndex, newText), cursorIndex + newText.Length);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void EnterTextTest1()
        {
            var text = "TEST";
            var newText = "\b";
            SetInput(keyString: newText);

            var result = TextInput.Update(_window, new CursorText(text, 0));
            var expected = new CursorText(text, 0);
            Assert.AreEqual(expected, result);
        }

        [TestCase(1)]
        [TestCase(4)]
        public void EnterTextTest2(int cursorIndex)
        {
            var text = "TEST";
            var newText = "\b";
            SetInput(keyString: newText);

            var result = TextInput.Update(_window, new CursorText(text, cursorIndex));
            var expected = new CursorText(text.Remove(cursorIndex - 1, 1), cursorIndex - 1);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void EnterTextTest3()
        {
            var text = "TEST";
            var newText = "\bb";
            SetInput(keyString: newText);

            var result = TextInput.Update(_window, new CursorText(text, 1));
            var expected = new CursorText("bEST", 1);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void EnterTextTest4()
        {
            var text = "A TEST";
            var newText = "\b";
            SetInput(new[] { Key.ControlLeft }, newText);

            var result = TextInput.Update(_window, new CursorText(text, 4));
            var expected = new CursorText("A ST", 2);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void EnterTextTest5()
        {
            var text = "A TEST";
            var newText = "\b";
            SetInput(new[] { Key.ControlLeft }, newText);

            var result = TextInput.Update(_window, new CursorText(text, 2));
            var expected = new CursorText("TEST", 0);
            Assert.AreEqual(expected, result);
        }
    }
}
