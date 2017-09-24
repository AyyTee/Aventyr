using Game;
using Game.Common;
using Game.Rendering;
using NUnit.Framework;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTests
{
    [TestFixture]
    public class IVirtualWindowExTests
    {
        FakeVirtualWindow CreateWindowWithInput(IEnumerable<Key> input = null)
        {
            input = input ?? new[] { Key.A, Key.ControlLeft };
            var window = new FakeVirtualWindow(new Resources(), () => new Vector2i(100, 100));
            window.Update(
                "", 
                new HashSet<Key>(input), 
                new HashSet<MouseButton>());
            return window;
        }

        [Test]
        public void HotkeyTest0()
        {
            var window = CreateWindowWithInput();
            Assert.IsTrue(window.ButtonDown(Key.A));
            Assert.IsTrue(window.ButtonPress(Key.A));
        }

        [Test]
        public void HotkeyTest1()
        {
            var hotkey = new Hotkey(Key.A, true);
            var window = CreateWindowWithInput();
            Assert.IsTrue(window.HotkeyDown(hotkey));
            Assert.IsTrue(window.HotkeyPress(hotkey));
        }

        [Test]
        public void HotkeyTest2()
        {
            var hotkey = new Hotkey(Key.A, true, true);
            var window = CreateWindowWithInput();
            Assert.IsFalse(window.HotkeyDown(hotkey));
            Assert.IsFalse(window.HotkeyPress(hotkey));
        }

        [Test]
        public void HotkeyTest3()
        {
            var hotkey = new Hotkey(Key.A, true);
            var window = CreateWindowWithInput(new[] { Key.A, Key.ControlLeft, Key.ShiftLeft});
            Assert.IsFalse(window.HotkeyDown(hotkey));
            Assert.IsFalse(window.HotkeyPress(hotkey));
        }

        [Test]
        public void HotkeyTest4()
        {
            var window = CreateWindowWithInput();
            window.Update("", new HashSet<Key>(), new HashSet<MouseButton>());
            Assert.IsFalse(window.ButtonDown(Key.A));
            Assert.IsTrue(window.ButtonRelease(Key.A));
        }
    }
}
