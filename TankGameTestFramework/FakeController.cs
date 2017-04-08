using Game;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Rendering;

namespace TankGameTestFramework
{
    public class FakeController : IGameController
    {
        public Size CanvasSize => new Size(800, 600);
        public FakeInput Input { get; private set; } = new FakeInput();

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void Render()
        {
            throw new NotImplementedException();
        }

        public void OnLoad(IResourceController resourceController, VirtualWindow window)
        {
            throw new NotImplementedException();
        }
    }
}
