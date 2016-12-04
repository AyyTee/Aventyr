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
        public Size CanvasSize { get { return new Size(800, 600); } }
        public IInput Input { get; private set; } = new FakeInput();
    }
}
