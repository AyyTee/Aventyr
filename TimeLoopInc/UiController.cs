using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Common;
using Game.Rendering;
using OpenTK.Input;

namespace TimeLoopInc
{
    public class UiController : IUpdateable
    {
        public List<Button> Elements { get; set; } = new List<Button>();
        readonly IVirtualWindow _window;
        public ICamera2 Camera { get; set; }

        public UiController(IVirtualWindow window)
        {
            _window = window;
            Camera = new HudCamera2(_window.CanvasSize);
        }

        public void Update(double timeDelta)
        {
            var mousePos = Camera.ScreenToWorld(_window.MousePosition, _window.CanvasSize);

            var buttonHover = Elements
                .FirstOrDefault(item => MathEx.PointInRectangle(item.TopLeft, item.BottomRight, mousePos));
            if (_window.ButtonPress(MouseButton.Left))
            {
                buttonHover.Click();
            }
        }

        public void Render(double timeDelta)
        {

        }
    }
}
