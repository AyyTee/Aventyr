using Game;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGameTests
{
    public class FakeController : IController
    {
        Size IController.CanvasSize { get { return CanvasSize; } }
        public Size CanvasSize { get { return new Size(800, 600); } }
        IInput IController.Input { get { return Input; } }
        public FakeInput Input { get; private set; } = new FakeInput();
    }
}
