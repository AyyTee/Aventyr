using System;
using Game.Common;
using Game.Rendering;
using OpenTK.Input;
using Ui;

namespace TimeLoopInc.Editor
{
    public class EditorController
    {
        readonly IVirtualWindow _window;
        readonly UiController _menu;
        readonly GridCamera _camera;
        readonly Controller _controller;
        readonly SceneBuilder _sceneBuilder = new SceneBuilder();
        Scene _scene;

        public EditorController(IVirtualWindow window, Controller controller)
        {
            _window = window;
            _controller = controller;
            _menu = new UiController(_window);
            _camera = new GridCamera(new Transform2(), (float)_window.CanvasSize.XRatio);

            _scene = _sceneBuilder.CreateScene();
        }

        public void Update()
        {
            var mousePos = _window.MouseWorldPos(_camera);

            _menu.Update(1);
            if (_menu.Hover == null)
            {
                if (_window.ButtonPress(MouseButton.Left))
                {
                    
                }
            }
            var move = MoveInput.CreateFromKeyboard(_window).Direction;
        }

        public void Render()
        {
            //_window.Layers.Add(_sceneRender.Render(1));
            _window.Layers.Add(_menu.Render());
        }
    }
}
